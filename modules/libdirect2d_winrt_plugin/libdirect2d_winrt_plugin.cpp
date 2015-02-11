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
#include <sstream>
#include <ppltasks.h>
#include <windows.ui.xaml.media.dxinterop.h>
#include <d2d1_1helper.h>
#include <d2d1helper.h>
#include <objbase.h>

#include "I420Effect.h"

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
static void           UpdateResourcesFromWindowSizeChanged(vout_display_t *vd);

static const vlc_fourcc_t d2d_subpicture_chromas[] = {
    VLC_CODEC_RGBA,
    0
};

/* */
struct vout_display_sys_t {
    /* */
    uint32_t*                    displayWidth;
    uint32_t*                    displayHeight;

    uint32_t                     lastDisplayWidth;
    uint32_t                     lastDisplayHeight;

    D2D1_POINT_2U                offset;
    D2D1_SIZE_U                  size;
    D2D1_SIZE_U                  halfSize;
    D2D1_SIZE_U                  renderSize;

    //TODO: check to see if these are all needed
    picture_pool_t               *pool;
    ID2D1Bitmap1                 *yBitmap;
    ID2D1Bitmap1                 *uBitmap;
    ID2D1Bitmap1                 *vBitmap;
    ID2D1Effect                  *yuvEffect;
    ComPtr<ID2D1Factory2>        d2dFactory;
    ComPtr<ID2D1DeviceContext1>  d2dContext;
    ComPtr<IDXGISwapChain1>      swapChain;
    ComPtr<IDXGISurface>         backBuffer;
    ComPtr<ID2D1Bitmap1>         targetBitmap;
};

static void Debug(const wchar_t *fmt, ...)
{
    wchar_t buf[255];
    va_list args;
    va_start(args, fmt);
    vswprintf_s(buf, fmt, args);
    va_end(args);
    OutputDebugStringW(buf);
}

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

    vd->fmt.i_chroma = VLC_CODEC_I420; // YUV NV12 Codec
    vd->pool         = Pool;
    vd->prepare      = Prepare;
    vd->display      = Display;
    vd->manage       = Manage;
    vd->control      = Control;

   sys->displayWidth = (uint32_t*)var_CreateGetInteger(vd, "winrt-width");
   sys->displayHeight = (uint32_t*)var_CreateGetInteger(vd, "winrt-height");

    uintptr_t panelInt = (uintptr_t)var_CreateGetInteger(vd, "winrt-d2dcontext");
    reinterpret_cast<IUnknown*>(panelInt)->QueryInterface(IID_PPV_ARGS(&sys->d2dContext));

    uintptr_t swapChainInt = (uintptr_t)var_CreateGetInteger(vd, "winrt-swapchain");
    reinterpret_cast<IUnknown*>(swapChainInt)->QueryInterface(IID_PPV_ARGS(&sys->swapChain));

    if (sys->d2dContext == NULL || sys->swapChain == NULL) {
        free(sys);
        return VLC_EGENERIC;
    }
    sys->d2dContext->GetFactory((ID2D1Factory**)sys->d2dFactory.GetAddressOf());
    HRESULT hr_create = I420Effect::Register(sys->d2dFactory.Get());

    return VLC_SUCCESS;
}

static void ClearBuffers(vout_display_sys_t* p_sys)
{
    DXGI_PRESENT_PARAMETERS parameters = { 0 };
    DXGI_SWAP_CHAIN_DESC1 desc;

    if (p_sys->swapChain->GetDesc1(&desc) != S_OK)
        return;
    for (unsigned int i = 0; i < desc.BufferCount; ++i)
    {
        p_sys->d2dContext->BeginDraw();
        p_sys->d2dContext->Clear();
        p_sys->d2dContext->EndDraw();
        p_sys->swapChain->Present1(1, 0, &parameters);
    }
}

static void Close(vlc_object_t * object){
    vout_display_t *vd = (vout_display_t *) object;

    ClearBuffers(vd->sys);

    if (vd->sys->pool)
        picture_pool_Release(vd->sys->pool);

    if (vd->sys->yBitmap)
        vd->sys->yBitmap->Release();

    if (vd->sys->uBitmap)
        vd->sys->uBitmap->Release();

    if (vd->sys->vBitmap)
        vd->sys->vBitmap->Release();

    if (vd->sys->yuvEffect)
        vd->sys->yuvEffect->Release();

    if (vd->sys->d2dContext)
    {
        vd->sys->d2dContext->Flush();
        vd->sys->d2dContext = nullptr;
    }
    vd->sys->d2dFactory = nullptr;
    vd->sys->swapChain = nullptr;
    vd->sys->backBuffer = nullptr;
    vd->sys->targetBitmap = nullptr;

    free(vd->sys);
    return;
}

static void UpdateResourcesFromWindowSizeChanged(vout_display_t *vd)
{
    if (vd->sys->yBitmap)
    {
        vd->sys->yBitmap->Release();
        vd->sys->yBitmap = nullptr;
    }

    if (vd->sys->uBitmap)
    {
        vd->sys->uBitmap->Release();
        vd->sys->uBitmap = nullptr;
    }

    if (vd->sys->vBitmap)
    {
        vd->sys->vBitmap->Release();
        vd->sys->vBitmap = nullptr;
    }

    if (vd->sys->yuvEffect)
    {
        vd->sys->yuvEffect->Release();
        vd->sys->yuvEffect = nullptr;
    }
}
static int Control(vout_display_t *vd, int query, va_list args)
{
    switch (query) 
    {
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

static bool ResizeBuffers(vout_display_sys_t* p_sys, float dpiX, float dpiY)
{
    p_sys->d2dContext->SetTarget(nullptr);
    p_sys->backBuffer = nullptr;
    p_sys->targetBitmap = nullptr;

    D2D1_PIXEL_FORMAT pixelFormat = {
        DXGI_FORMAT_B8G8R8A8_UNORM,
        D2D1_ALPHA_MODE_PREMULTIPLIED
    };

    D2D1_BITMAP_PROPERTIES1 bitmapProperties = {
        pixelFormat,
        dpiX,
        dpiY,
        D2D1_BITMAP_OPTIONS_TARGET | D2D1_BITMAP_OPTIONS_CANNOT_DRAW,
    };

    p_sys->swapChain->ResizeBuffers(0, *p_sys->displayWidth, *p_sys->displayHeight, DXGI_FORMAT_UNKNOWN, 0);

    if (p_sys->swapChain->GetBuffer(0, IID_PPV_ARGS(&p_sys->backBuffer)) != S_OK)
        return false;

    //set d2d target
    if (p_sys->d2dContext->CreateBitmapFromDxgiSurface(p_sys->backBuffer.Get(), &bitmapProperties, &p_sys->targetBitmap) != S_OK)
        return false;

    p_sys->d2dContext->SetTarget(p_sys->targetBitmap.Get());
    return true;
}

static void Prepare(vout_display_t *vd, picture_t *picture, subpicture_t *subpicture)
{

    vout_display_sys_t     *sys = vd->sys;

    if (*sys->displayWidth != sys->lastDisplayWidth || *sys->displayHeight != sys->lastDisplayHeight)
    {
        vout_display_place_t place;
        vout_display_cfg_t cfg = *vd->cfg;
        cfg.display.width = *sys->displayWidth;
        cfg.display.height = *sys->displayHeight;
        vout_display_PlacePicture(&place, &vd->source, &cfg, true);
        sys->renderSize.width = place.width;
        sys->renderSize.height = place.height;
        sys->offset.x = place.x;
        sys->offset.y = place.y;

        // Get the size from the current picture
        sys->size.width = picture->format.i_visible_width;
        sys->size.height = picture->format.i_visible_height;

        sys->halfSize.width = picture->p[1].i_visible_pitch / picture->p[1].i_pixel_pitch;
        sys->halfSize.height = picture->p[1].i_visible_lines;

        float dpiX;
        float dpiY;
        sys->d2dContext->GetDpi(&dpiX, &dpiY);
        sys->lastDisplayWidth = *sys->displayWidth;
        sys->lastDisplayHeight = *sys->displayHeight;

        UpdateResourcesFromWindowSizeChanged(vd);

        if (ResizeBuffers(sys, dpiX, dpiY) == false)
            return;

        HRESULT hrx = sys->d2dContext->CreateEffect(CLSID_CustomI420Effect, &sys->yuvEffect);

        sys->yuvEffect->SetInputCount(2);

        if (sys->yuvEffect->SetValue(I420_PROP_DISPLAYEDFRAME_WIDTH, sys->renderSize.width) != S_OK)
            Debug(TEXT("Failed to set effect width parameter"));
        if (sys->yuvEffect->SetValue(I420_PROP_DISPLAYEDFRAME_HEIGHT, sys->renderSize.height) != S_OK)
            Debug(TEXT("Failed to set effect heigth parameter"));


        // Init bitmap properties in which will store the y (lumi) plane
        D2D1_BITMAP_PROPERTIES1 props;
        D2D1_PIXEL_FORMAT       pixFormat;

        pixFormat.alphaMode = D2D1_ALPHA_MODE_STRAIGHT;
        pixFormat.format = DXGI_FORMAT_A8_UNORM;
        props.pixelFormat = pixFormat;
        props.dpiX = dpiX;
        props.dpiY = dpiY;
        props.bitmapOptions = D2D1_BITMAP_OPTIONS_NONE;
        props.colorContext = nullptr;

        if (S_OK != sys->d2dContext->CreateBitmap(sys->size, NULL, 0, props, &sys->yBitmap))
            return;

        // Create or copy uv (chroma) plane
        if (S_OK != sys->d2dContext->CreateBitmap(sys->halfSize, NULL, 0, props, &sys->uBitmap))
            return;

        // Create or copy uv (chroma) plane
        if (S_OK != sys->d2dContext->CreateBitmap(sys->halfSize, NULL, 0, props, &sys->vBitmap))
            return;
    }


    if (sys->yuvEffect->SetValue(I420_PROP_VISIBLE_WIDTH, picture->format.i_visible_width) != S_OK)
        Debug(TEXT("Failed to set effect visible width parameter"));
    if (sys->yuvEffect->SetValue(I420_PROP_VISIBLE_HEIGHT, picture->format.i_visible_height) != S_OK)
        Debug(TEXT("Failed to set effect visible heigth parameter"));

    sys->d2dContext->SetTransform(D2D1::Matrix3x2F::Translation((float)sys->offset.x, (float)sys->offset.y));

    // Init and clear d2dContext render target
    sys->d2dContext->BeginDraw();

    D2D1_RECT_F pushRect = D2D1::RectF(0, 0, sys->renderSize.width, sys->renderSize.height);
    sys->d2dContext->PushAxisAlignedClip(&pushRect, D2D1_ANTIALIAS_MODE_ALIASED);
    sys->d2dContext->Clear(D2D1::ColorF(D2D1::ColorF::Black));

    //D2D1_RECT_U destRect = D2D1::RectU(0, 0, sys->size.width, sys->size.height);
    sys->yBitmap->CopyFromMemory(NULL, picture->p[0].p_pixels, picture->p[0].i_pitch);
    sys->yuvEffect->SetInput(0, sys->yBitmap);


    //destRect = D2D1::RectU(0, 0, sys->halfSize.width, sys->halfSize.height);
    sys->uBitmap->CopyFromMemory(NULL, picture->p[1].p_pixels, picture->p[1].i_pitch);

    sys->yuvEffect->SetInput(1, sys->uBitmap);

    //destRect = D2D1::RectU(0, 0, sys->halfSize.width, sys->halfSize.height);
    sys->vBitmap->CopyFromMemory(NULL, picture->p[2].p_pixels, picture->p[2].i_pitch);

    sys->yuvEffect->SetInput(2, sys->vBitmap);

    sys->d2dContext->DrawImage(sys->yuvEffect, D2D1_INTERPOLATION_MODE_CUBIC);

    sys->d2dContext->PopAxisAlignedClip();
    HRESULT hr_draw = vd->sys->d2dContext->EndDraw();

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
