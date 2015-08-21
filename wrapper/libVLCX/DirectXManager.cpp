/*****************************************************************************
 * Copyright © 2013-2014 VideoLAN
 *
 * Authors: Kellen Sunderland
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

#include <wrl.h>
#include <wrl/client.h>

#include <dxgi.h>
#include <dxgi1_2.h>
#include <dxgi1_3.h>
#include <d3d11_1.h>
#include <d2d1_2.h>

#include "windows.ui.xaml.media.dxinterop.h"

#include "DirectXManager.h"

using namespace libVLCX;
using namespace Platform;
using namespace Windows::UI::Xaml::Media::Imaging;
using namespace Windows::UI::Core;
using namespace Windows::UI;
using namespace Windows::Foundation;
using namespace Windows::System::Threading;
using namespace D2D1;
using namespace Windows::Graphics::Display;

DirectXManger::DirectXManger()
{
}

void DirectXManger::CheckDXOperation(HRESULT hr, Platform::String^ message){
    if (hr != S_OK) {
        throw ref new Platform::Exception(hr, message);
    }
}

void DirectXManger::CreateSwapPanel(SwapChainPanel^ panel){
    HRESULT hr;
    ComPtr<IDXGIFactory2> dxgiFactory;
    ComPtr<IDXGIAdapter> dxgiAdapter;
    ComPtr<IDXGIDevice1> dxgiDevice;
    ComPtr<ID3D11Device> d3dDevice;
    ComPtr<ID2D1Device1> d2dDevice;
    ComPtr<ID2D1Factory2> d2dFactory;

    UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT | D3D11_CREATE_DEVICE_VIDEO_SUPPORT;
//#ifndef NDEBUG
//    creationFlags |= D3D11_CREATE_DEVICE_DEBUG;
//#endif
    const auto displayInfo = Windows::Graphics::Display::DisplayInformation::GetForCurrentView();

    D2D1_BITMAP_PROPERTIES1 bitmapProperties =
        BitmapProperties1(
        D2D1_BITMAP_OPTIONS_TARGET | D2D1_BITMAP_OPTIONS_CANNOT_DRAW,
        PixelFormat(DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE_PREMULTIPLIED),
        displayInfo->RawDpiX,
        displayInfo->RawDpiY);

    // Feature sets supported
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

    hr = D3D11CreateDevice(
        nullptr,
        D3D_DRIVER_TYPE_HARDWARE,
        nullptr,
        creationFlags,
        featureLevels,
        ARRAYSIZE(featureLevels),
        D3D11_SDK_VERSION,
        &cp_d3dDevice,
        nullptr,
        &cp_d3dContext
        );
    CheckDXOperation(hr, "Could not D3D11CreateDevice");
    CheckDXOperation(cp_d3dDevice.As(&dxgiDevice), "Could not transform to DXGIDevice");
    CheckDXOperation(dxgiDevice->GetAdapter(&dxgiAdapter), "Could not  get adapter");
    CheckDXOperation(dxgiAdapter->GetParent(IID_PPV_ARGS(&dxgiFactory)), "Could not get adapter parent");


    D2D1_FACTORY_OPTIONS options;
    ZeroMemory(&options, sizeof(D2D1_FACTORY_OPTIONS));

#if defined(_DEBUG)
    // If the project is in a debug build, enable Direct2D debugging via SDK Layers.
    options.debugLevel = D2D1_DEBUG_LEVEL_INFORMATION;
#endif

    //Create the swapchain
    DXGI_SWAP_CHAIN_DESC1 swapChainDesc = { 0 };
    swapChainDesc.Width  = (UINT) panel->ActualWidth;
    swapChainDesc.Height = (UINT) panel->ActualHeight;
    swapChainDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    swapChainDesc.Stereo = false;
    swapChainDesc.SampleDesc.Count = 1;
    swapChainDesc.SampleDesc.Quality = 0;
    swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    swapChainDesc.BufferCount = 2;
    swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL;
    swapChainDesc.Flags = 0;
    swapChainDesc.AlphaMode = DXGI_ALPHA_MODE_UNSPECIFIED;

    hr = dxgiFactory->CreateSwapChainForComposition(
        cp_d3dDevice.Get(),
        &swapChainDesc,
        nullptr,
        &cp_swapChain
        );
    CheckDXOperation(hr, "Could not create swapChain");
    CheckDXOperation(dxgiDevice->SetMaximumFrameLatency(1), "Could not set maximum Frame Latency");

    //TODO: perform the next 2 calls on the UI thread
    ComPtr<ISwapChainPanelNative> panelNative;
    hr = reinterpret_cast<IUnknown*>(panel)->QueryInterface(IID_PPV_ARGS(&panelNative));
    CheckDXOperation(hr, "Could not initialise the native panel");

    // Associate swap chain with SwapChainPanel.  This must be done on the UI thread.
    CheckDXOperation(panelNative->SetSwapChain(cp_swapChain.Get()), "Could not associate the swapChain");

    // This is necessary so we can call Trim() on suspend
    hr = dxgiDevice.As(&cp_dxgiDev3);
    CheckDXOperation(hr, "Failed to get the DXGIDevice3 from Dxgidevice1");
}

void DirectXManger::Trim()
{
    if (cp_dxgiDev3 != nullptr)
        cp_dxgiDev3->Trim();
}
