/*****************************************************************************
* Copyright © 2013 VideoLAN
*
* Authors: Kellen Sunderland <kellen _DOT_ sunderland _AT_ gmail _DOT_ com>
*
* This code is based on directfb.c, vmem.c, direct2d thanks VideoLAN team.
* Code also based on Microsoft DirectX / Xaml interop samples, thanks MS.
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
#include <windows.ui.xaml.media.dxinterop.h>

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN /* Exclude rarely-used stuff from Windows headers */
#endif

#ifdef _MSC_VER /* help visual studio compile vlc headers */
#define inline __inline
#define strdup _strdup
#define ssize_t SSIZE_T
#define N_(x) x
int poll(struct pollfd *, unsigned, int);
#endif

using namespace concurrency;
using namespace Platform;
using namespace Microsoft::WRL;
using namespace Windows::Graphics::Display;
using namespace Windows::ApplicationModel::Core;
using namespace Windows::UI::Core;

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
    set_description(N_("Windows 8 video output"))
	set_shortname(N_("Video winrt"))

    set_category(CAT_VIDEO)
	set_subcategory(SUBCAT_VIDEO_VOUT)
	set_capability("vout display", 60)
	add_integer("winrt-d2dcontext", 0x0, NULL, NULL, true);
	add_integer("winrt-swapchain", 0x0, NULL, NULL, true);
	add_float("winrt-width", 0x0, NULL, NULL, true);
	add_float("winrt-height", 0x0, NULL, NULL, true);

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
static int            CreateDeviceResources(vout_display_t* vd);
static const vlc_fourcc_t d2d_subpicture_chromas[] = {
	VLC_CODEC_RGBA,
	0
};

/* */
struct vout_display_sys_t {
	/* */
	float                         displayWidth;
	float                         displayHeight;

	//TODO: check to see if these are all needed
	picture_pool_t              *pool;
	ID2D1Bitmap                 *d2dbmp;
	ComPtr<ID2D1DeviceContext>  d2dContext;
	ComPtr<IDXGISwapChain1>     swapChain;
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
	info.subpicture_chromas = d2d_subpicture_chromas;
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

	sys->displayWidth = var_CreateGetFloat(vd, "winrt-width");
	sys->displayHeight = var_CreateGetFloat(vd, "winrt-height");

	unsigned int panelInt = var_CreateGetInteger(vd, "winrt-d2dcontext");
	reinterpret_cast<IUnknown*>(panelInt)->QueryInterface(IID_PPV_ARGS(&sys->d2dContext));

	unsigned int swapChainInt = var_CreateGetInteger(vd, "winrt-swapchain");
	reinterpret_cast<IUnknown*>(swapChainInt)->QueryInterface(IID_PPV_ARGS(&sys->swapChain));

	return VLC_SUCCESS;
}

static void Close(vlc_object_t * object){
	vout_display_t *vd = (vout_display_t *) object;

	if (vd->sys->pool)
		picture_pool_Delete(vd->sys->pool);

	free(vd->sys);

	return;
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

static void Prepare(vout_display_t *vd, picture_t *picture, subpicture_t *subpicture)
{
	vout_display_sys_t     *sys = vd->sys;
	D2D1_BITMAP_PROPERTIES props;
	D2D1_PIXEL_FORMAT      pixFormat;
	D2D1_SIZE_U            size;
	float                  dpi = DisplayProperties::LogicalDpi;
	ComPtr<ID2D1Effect>    scaleEffect;
	float                  scale;

	if (sys->d2dbmp){
		// cleanup previous bmp
		sys->d2dbmp->Release();
		sys->d2dbmp = nullptr;
	}
	
	sys->d2dContext->BeginDraw();
	sys->d2dContext->Clear(D2D1::ColorF(D2D1::ColorF::Black));

	size.width = picture->format.i_width;
	size.height = picture->format.i_height;
	pixFormat.alphaMode = D2D1_ALPHA_MODE_IGNORE;
	pixFormat.format = DXGI_FORMAT_B8G8R8X8_UNORM;
	props.pixelFormat = pixFormat;
	props.dpiX = dpi;
	props.dpiY = dpi;
	sys->d2dContext->CreateBitmap(size, picture->p[0].p_pixels, picture->p[0].i_pitch, props, &sys->d2dbmp);

	scale = (double) sys->displayWidth / (double) picture->format.i_width;
	sys->d2dContext->CreateEffect(CLSID_D2D1Scale, &scaleEffect);
	scaleEffect->SetInput(0, sys->d2dbmp);
	scaleEffect->SetValue(D2D1_SCALE_PROP_CENTER_POINT, D2D1::Vector2F(0.0f,0.0f));
	scaleEffect->SetValue(D2D1_SCALE_PROP_SCALE, D2D1::Vector2F(scale, scale));
	D2D1_RECT_F displayRect = { 0.0f, (double) sys->displayHeight, (double) sys->displayWidth, 0.0f };
	D2D1_RECT_F pictureRect = { 0.0f, picture->format.i_height, (double) picture->format.i_width, 0.0f };
	D2D1_POINT_2F offset = {0.0f, ((double) sys->displayHeight - (((double)picture->format.i_height)*scale))/2.0f};

	vd->sys->d2dContext->DrawImage(scaleEffect.Get(), offset);
	vd->sys->d2dContext->EndDraw();

	VLC_UNUSED(subpicture);
}

static void Display(vout_display_t *vd, picture_t *picture, subpicture_t *subpicture)
{
	DXGI_PRESENT_PARAMETERS parameters = { 0 };
	parameters.DirtyRectsCount = 0;
	parameters.pDirtyRects = nullptr;
	parameters.pScrollRect = nullptr;
	parameters.pScrollOffset = nullptr;

	HRESULT hr = vd->sys->swapChain->Present1(1, 0, &parameters);

	picture_Release(picture);
	VLC_UNUSED(subpicture);
}