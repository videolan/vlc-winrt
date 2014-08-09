/*****************************************************************************
* Copyright © 2013-2014 VideoLAN
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
#include "../../wrapper/libVLCX/Helpers.h"

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN /* Exclude rarely-used stuff from Windows headers */
#endif

#ifdef _MSC_VER /* help visual studio compile vlc headers */
#define inline __inline
#define strdup _strdup
#define ssize_t SSIZE_T
#define N_(x) x
#define _(x) x
int poll(struct pollfd *, unsigned, int);
# define restrict __restrict
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
    add_integer("winrt-width", 0x0, NULL, NULL, true);
    add_integer("winrt-height", 0x0, NULL, NULL, true);

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
    float*                        displayWidth;
    float*                        displayHeight;

    //TODO: check to see if these are all needed
    picture_pool_t              *pool;
    ID2D1Bitmap                 *yBitmap;
    ID2D1Bitmap                 *uvBitmap;
    ID2D1Effect                    *yuvEffect;
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
    vout_display_info_t info  = vd->info;
    info.is_slow              = false;
    info.has_double_click     = true;
    info.has_hide_mouse       = false;
    info.has_pictures_invalid = false;
    //info.subpicture_chromas   = d2d_subpicture_chromas;
    vd->info                  = info;

    vd->fmt.i_chroma = VLC_CODEC_NV12; // YUV NV12 Codec

    vd->pool         = Pool;
    vd->prepare      = Prepare;
    vd->display      = Display;
    vd->manage       = Manage;
    vd->control      = Control;


   sys->displayWidth = (float*)var_CreateGetInteger(vd, "winrt-width");
   sys->displayHeight = (float*)var_CreateGetInteger(vd, "winrt-height");

    uintptr_t panelInt = (uintptr_t)var_CreateGetInteger(vd, "winrt-d2dcontext");
    reinterpret_cast<IUnknown*>(panelInt)->QueryInterface(IID_PPV_ARGS(&sys->d2dContext));

    uintptr_t swapChainInt = (uintptr_t)var_CreateGetInteger(vd, "winrt-swapchain");
    reinterpret_cast<IUnknown*>(swapChainInt)->QueryInterface(IID_PPV_ARGS(&sys->swapChain));

    if (sys->d2dContext == NULL || sys->swapChain == NULL) {
        free(sys);
        return VLC_EGENERIC;
    }
    return VLC_SUCCESS;
}

static void Close(vlc_object_t * object){
    vout_display_t *vd = (vout_display_t *) object;

    if (vd->sys->pool)
        picture_pool_Delete(vd->sys->pool);

    if (vd->sys->yBitmap)
        vd->sys->yBitmap->Release();

    if (vd->sys->uvBitmap)
        vd->sys->uvBitmap->Release();

    if (vd->sys->yuvEffect)
        vd->sys->yuvEffect->Release();

    if (vd->sys->d2dContext)
        vd->sys->d2dContext->Flush();


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
    D2D1_SIZE_U            size;
    float                  dpi = DisplayProperties::LogicalDpi;

    // Init and clear d2dContext render target
    sys->d2dContext->BeginDraw();
    sys->d2dContext->Clear(D2D1::ColorF(D2D1::ColorF::Black));

    // Get the size avec the current picture
    size.width          = picture->format.i_visible_width;
    size.height         = picture->format.i_visible_height;


    // If needed, create the YCbCr effect
    if (sys->yuvEffect == nullptr)
        sys->d2dContext->CreateEffect(CLSID_D2D1YCbCr, &sys->yuvEffect);

    // Init bitmap properties in which will store the y (lumi) plane
    D2D1_BITMAP_PROPERTIES propsY;
    D2D1_PIXEL_FORMAT      pixFormatY;

    pixFormatY.alphaMode = D2D1_ALPHA_MODE_IGNORE;
    pixFormatY.format = DXGI_FORMAT_R8_UNORM;
    propsY.pixelFormat = pixFormatY;
    propsY.dpiX = dpi;
    propsY.dpiY = dpi;

    if (sys->yBitmap == nullptr)
    {
        if (S_OK != sys->d2dContext->CreateBitmap(size,
            picture->p[0].p_pixels,
            picture->p[0].i_pitch,
            propsY,
            &sys->yBitmap))
            return;
    }
    {
        D2D1_RECT_U destRect = D2D1::RectU(0, 0, size.width, size.height);
        sys->yBitmap->CopyFromMemory(&destRect, picture->p[0].p_pixels, picture->p[0].i_pitch);
    }

    sys->yuvEffect->SetInput(0, sys->yBitmap);

    // Init bitmap properties in which will store uv (chroma) plane
    D2D1_BITMAP_PROPERTIES propsCbCr;
    D2D1_PIXEL_FORMAT      pixFormatCbCr;

    pixFormatCbCr.alphaMode = D2D1_ALPHA_MODE_IGNORE;
    pixFormatCbCr.format = DXGI_FORMAT_R8G8_UNORM;
    propsCbCr.pixelFormat = pixFormatCbCr;
    propsCbCr.dpiX = dpi;
    propsCbCr.dpiY = dpi;

    D2D1_SIZE_U halfSize = size;
    halfSize.width = size.width / 2;
    halfSize.height = size.height / 2;

    // Create or copy uv (chroma) plane
    if (sys->uvBitmap == nullptr)
    {
        if (S_OK != sys->d2dContext->CreateBitmap(halfSize,
            picture->p[1].p_pixels,
            picture->p[1].i_pitch,
            propsCbCr,
            &sys->uvBitmap))
            return;
    }
    {
        D2D1_RECT_U destRect = D2D1::RectU(0, 0, halfSize.width, halfSize.height);
        sys->uvBitmap->CopyFromMemory(&destRect, picture->p[1].p_pixels, picture->p[1].i_pitch);
    }

    sys->yuvEffect->SetInput(1, sys->uvBitmap);



    float scaleW = *sys->displayWidth / (float)picture->format.i_visible_width;
    float scaleH = *sys->displayHeight / (float)picture->format.i_visible_height;

    // Compute offset and scale factor
    D2D1_POINT_2F offset;
    float scale;
    if( scaleH <= scaleW) {
        scale = scaleH;
        offset.x = (*sys->displayWidth - ((float)picture->format.i_visible_width * scale)) / 2.0f;
        offset.y =  0.0f; ;
    } else {
        scale = scaleW;
        offset.x =  0.0f;
        offset.y = (*sys->displayHeight - ((float)picture->format.i_visible_height * scale)) / 2.0f;
    }

    // Apply translate + scale transformation
    D2D1::Matrix3x2F transform =
        D2D1::Matrix3x2F::Scale(scale, scale) *
        D2D1::Matrix3x2F::Translation(offset.x, offset.y);

    sys->yuvEffect->SetValue(
        D2D1_YCBCR_PROP_TRANSFORM_MATRIX,
        transform
        );

    // Draw the result of the YCbCr effect
    sys->d2dContext->DrawImage(sys->yuvEffect, D2D1_INTERPOLATION_MODE_CUBIC);

    #if 0
    /* FIXME: look at the following example http://code.msdn.microsoft.com/windowsapps/Direct2D-Image-Effects-4819dc5b */
    if (subpicture) {
        for (subpicture_region_t *r = subpicture->p_region; r != NULL; r = r->p_next) {

            D2D1_SIZE_U            size2;
            size2.height = r->fmt.i_visible_height;
            size2.width  = r->fmt.i_visible_width;

            D2D1_BITMAP_PROPERTIES props2;
            D2D1_PIXEL_FORMAT      pixFormat2;
            pixFormat2.format = DXGI_FORMAT_R8G8B8A8_UNORM;
            pixFormat2.alphaMode = D2D1_ALPHA_MODE_IGNORE;
            props2.pixelFormat   = pixFormat2;
            props2.dpiX = dpi;
            props2.dpiY = dpi;

            /* This is awful */
            const int pixels_offset = r->fmt.i_y_offset * r->p_picture->p->i_pitch +
                                      r->fmt.i_x_offset * r->p_picture->p->i_pixel_pitch;
            for (int y = 0; y < r->fmt.i_visible_height; y++) {
               uint8_t *row = (uint8_t*)&r->p_picture->p->p_pixels[pixels_offset + y*r->p_picture->p->i_pitch];
               for (int x = 0; x < r->fmt.i_visible_width * 4; x += 4) {
                   uint8_t r = row[x + 0];
                   uint8_t g = row[x + 1];
                   uint8_t b = row[x + 2];
                   uint8_t a = row[x + 3];
                   if (a != 255)
                       r = g = b = a = 0;
                   row[x + 0] = r;
                   row[x + 1] = g;
                   row[x + 2] = b;
                   row[x + 3] = a;
               }
            }


            ID2D1Bitmap                 *d2dbmp2;
            HRESULT hr = sys->d2dContext->CreateBitmap(size2, r->p_picture->p[0].p_pixels, r->p_picture->p[0].i_pitch, props2, &d2dbmp2 );

            Debug( L"D2DD 0x%x\n", hr);

            ComPtr<ID2D1Effect> blendEffect;
            sys->d2dContext->CreateEffect(CLSID_D2D1Blend, &blendEffect);

            /* Incorrect if the video is rescaled*/
            D2D1_RECT_F dst_rect = { r->i_x, r->i_y, r->i_x + r->fmt.i_visible_width, r->i_y + r->fmt.i_visible_height};
            if(d2dbmp2)
                vd->sys->d2dContext->DrawBitmap(d2dbmp2, dst_rect);
        }
    }
#endif

    vd->sys->d2dContext->EndDraw();

    VLC_UNUSED(subpicture);
}

static void Display(vout_display_t *vd, picture_t *picture, subpicture_t *subpicture)
{
    DXGI_PRESENT_PARAMETERS parameters = { 0 };
    parameters.DirtyRectsCount = 0;
    parameters.pDirtyRects     = nullptr;
    parameters.pScrollRect     = nullptr;
    parameters.pScrollOffset   = nullptr;

    HRESULT hr = vd->sys->swapChain->Present1(1, 0, &parameters);

    picture_Release(picture);
    VLC_UNUSED(subpicture);
}
