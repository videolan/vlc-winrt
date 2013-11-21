/*****************************************************************************
* Copyright © 2013 VideoLAN
*
* Authors: Kellen Sunderland <kellen _DOT_ sunderland _AT_ gmail _DOT_ com>
*
* This code is based on directfb.c, vmem.c, thanks VideoLAN team.
*
* This program is free software; you can redistribute it and/or modify it
* under the terms of the GNU Lesser General Public License as published by
* the Free Software Foundation; either version 2.1 of the License, or
* (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU Lesser General Public License for more details.
*
* You should have received a copy of the GNU Lesser General Public License
* along with this program; if not, write to the Free Software Foundation,
* Inc., 51 Franklin Street, Fifth Floor, Boston MA 02110-1301, USA.
*****************************************************************************/


/*****************************************************************************
* Preamble
*****************************************************************************/

#include "pch.h"
#include "targetver.h"
#include <ppltasks.h>
#include "D2DPanel.h"

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN /* Exclude rarely-used stuff from Windows headers */
#endif

#ifdef _MSC_VER /* help visual studio compile vlc headers */
#define inline __inline
#define strdup _strdup
#define ssize_t SSIZE_T
#define N_(x) x
#endif

using namespace concurrency;
using namespace Platform;
using namespace Microsoft::WRL;
using namespace Windows::Graphics::Display;


#include <vlc_common.h>
#include <vlc_plugin.h>
#include <vlc_vout_display.h>
#include <vlc_picture_pool.h>

/*****************************************************************************
* Module descriptor
*****************************************************************************/
static int  Open(vlc_object_t *);
static void Close(vlc_object_t *);

vlc_module_begin()
    set_description(N_("Windows 8.1 video output"))
	set_shortname(N_("Video winrt"))

    set_category(CAT_VIDEO)
	set_subcategory(SUBCAT_VIDEO_VOUT)
	set_capability("vout display", 60)

    set_callbacks(Open, Close)
vlc_module_end()

/*****************************************************************************
* Local prototypes
*****************************************************************************/
static picture_pool_t *Pool(vout_display_t *, unsigned);
static void           Display(vout_display_t *vd, picture_t *picture, subpicture_t *subpicture);
static int            Control(vout_display_t *vd, int query, va_list args);
static void           Manage(vout_display_t *vd);
static void           Prepare(vout_display_t *vd, picture_t *picture, subpicture_t *subpicture);
static void           CreateDeviceResources(vout_display_t* vd);

/* */
struct vout_display_sys_t {
	/* */
	libdirect2d_winrt_plugin::D2DPanel^ d2dPanel;

	/* */
	int width;
	int height;

	/* */
	//TODO: check to see if these are all needed
	picture_pool_t             *pool;
	ID2D1Bitmap                *d2dbmp;
	ComPtr<ID3D11Device>       d3dDevice;
	ComPtr<ID2D1Device>        d2dDevice;
	ComPtr<ID2D1DeviceContext> d2dContext;
};


/**
* Renders video to a SwapChainPanel in WinRT Environments
* Currently this module only supports RGB
*/
static int Open(vlc_object_t *object)
{
	//TODO: error handling, cleanup
	vout_display_t *vd = (vout_display_t *) object;
	vout_display_sys_t *sys;

	vd->sys = sys = (vout_display_sys_t*) calloc(1, sizeof(*sys));
	if (!sys)
		return VLC_ENOMEM;

	//Todo: check on double click, hide mouse
	vout_display_info_t info = vd->info;
	info.is_slow = false;
	info.has_double_click = true;
	info.has_hide_mouse = false;
	info.has_pictures_invalid = false;
	vd->info = info;

	vd->fmt.i_chroma = VLC_CODEC_RGB32; /* masks change this to BGR32 for ID2D1Bitmap */
	vd->fmt.i_rmask = 0x0000ff00;
	vd->fmt.i_gmask = 0x00ff0000;
	vd->fmt.i_bmask = 0xff000000;

	vd->pool = Pool;
	vd->prepare = Prepare;
	vd->display = Display;
	vd->manage = Manage;
	vd->control = Control;

	CreateDeviceResources(vd);

	return 0;
}

static void Close(vlc_object_t * object){
	vout_display_t *vd = (vout_display_t *) object;

	if (vd->sys->pool)
		picture_pool_Delete(vd->sys->pool);

	free(vd->sys);

	return;
}

static int OpenDisplay(vout_display_t *vd)
{
	//TODO: call callback and populate SwapChainPanel
	//TODO: intialize dependant and independant resources

	vout_display_sys_t *sys = vd->sys;
}

static void CloseDisplay(vout_display_t *vd)
{

}

static void Display(vout_display_t *vd, picture_t *picture, subpicture_t *subpicture)
{
	vout_display_sys_t *sys = vd->sys;
	//TODO: d2d magic
}

static int Control(vout_display_t *vd, int query, va_list args)
{
	//TODO: do we care about resizes?  Windows 8 should take care of it
	switch (query) {
	case VOUT_DISPLAY_CHANGE_FULLSCREEN:
	case VOUT_DISPLAY_CHANGE_DISPLAY_SIZE: {
											   const vout_display_cfg_t *cfg = va_arg(args, const vout_display_cfg_t *);
											   if (cfg->display.width != vd->fmt.i_width ||
												   cfg->display.height != vd->fmt.i_height)
												   return VLC_EGENERIC;
											   if (cfg->is_fullscreen)
												   return VLC_EGENERIC;
											   return VLC_SUCCESS;
	}
	default:
		return VLC_EGENERIC;
	}
}

static void Manage(vout_display_t *vd)
{
	VLC_UNUSED(vd);
}

/**
* Handles pool allocations for bitmaps
*/
static picture_pool_t *Pool(vout_display_t *vd, unsigned count)
{
	vout_display_sys_t *sys = vd->sys;

	if (!sys->pool) {
		sys->pool = picture_pool_NewFromFormat(&vd->fmt, count);
	}

	return sys->pool;
}


/**
* Performs set up of ID2D1Bitmap memory ready for blitting
*/
static void Prepare(vout_display_t *vd, picture_t *picture, subpicture_t *subpicture)
{
	vout_display_sys_t *sys = vd->sys;

	if (sys->d2dbmp) {
		HRESULT hr =  sys->d2dbmp->CopyFromMemory(NULL, picture->p[0].p_pixels, picture->p[0].i_pitch);

		if (hr != S_OK)
			msg_Err(vd, "Failed to copy bitmap memory (hr = 0x%x)!",
			(unsigned) hr);

	/*	HRESULT hr = sys->d2dbmp->CopyFromMemory(&D2D1::RectU(static_cast<UINT>(updateRect.Left),
			static_cast<UINT>(updateRect.Top),
			static_cast<UINT>(updateRect.Right),
			static_cast<UINT>(updateRect.Bottom)), picture->p[0].p_pixels, picture->p[0].i_pitch);*/

#ifndef NDEBUG
		msg_Dbg(vd, "Bitmap dbg: pitch = %d, bitmap = %p", picture->p[0].i_pitch, sys->d2dbmp);
#endif
	}

	VLC_UNUSED(subpicture);
}


// Initialize hardware-dependent resources.
static void CreateDeviceResources(vout_display_t* vd)
{
	vout_display_sys_t *sys = vd->sys;

	// This flag adds support for surfaces with a different color channel ordering
	// than the API default. It is required for compatibility with Direct2D.
	UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;

	// This array defines the set of DirectX hardware feature levels this app will support.
	// Note the ordering should be preserved.
	// Don't forget to declare your application's minimum required feature level in its
	// description.  All applications are assumed to support 9.1 unless otherwise stated.
	const D3D_FEATURE_LEVEL featureLevels[] =
	{
		D3D_FEATURE_LEVEL_11_1,
		D3D_FEATURE_LEVEL_11_0,
		D3D_FEATURE_LEVEL_10_1,
		D3D_FEATURE_LEVEL_10_0,
		D3D_FEATURE_LEVEL_9_3,
		D3D_FEATURE_LEVEL_9_2,
		D3D_FEATURE_LEVEL_9_1,
	};

	// Create the Direct3D 11 API device object.
	D3D11CreateDevice(
		nullptr,                        // Specify nullptr to use the default adapter.
		D3D_DRIVER_TYPE_HARDWARE,
		nullptr,
		creationFlags,                  // Set debug and Direct2D compatibility flags.
		featureLevels,                  // List of feature levels this app can support.
		ARRAYSIZE(featureLevels),
		D3D11_SDK_VERSION,              // Always set this to D3D11_SDK_VERSION for Metro style apps.
		&(sys->d3dDevice),                   // Returns the Direct3D device created.
		nullptr,
		nullptr
		);

	// Get the Direct3D 11.1 API device.
	ComPtr<IDXGIDevice> dxgiDevice;
	sys->d3dDevice.As(&dxgiDevice);

	// Create the Direct2D device object and a corresponding context.
	D2D1CreateDevice(
		dxgiDevice.Get(),
		nullptr,
		&(sys->d2dDevice)
		);


	sys->d2dDevice->CreateDeviceContext(
		D2D1_DEVICE_CONTEXT_OPTIONS_NONE,
		&(sys->d2dContext)
		);

	// Set DPI to the display's current DPI.
	sys->d2dContext->SetDpi(DisplayProperties::LogicalDpi, DisplayProperties::LogicalDpi);

	// Associate the DXGI device with the SurfaceImageSource.
	//TODO: this with SwapChainPanel
	//m_sisNative->SetDevice(dxgiDevice.Get());
}
