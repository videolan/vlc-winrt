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

#include "pch.h"
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

    UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;
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
        &d3dDevice,
        nullptr,
        nullptr
        );
    CheckDXOperation(hr, "Could not D3D11CreateDevice");
    CheckDXOperation(d3dDevice.As(&dxgiDevice), "Could not transform to DXGIDevice");
    CheckDXOperation(dxgiDevice->GetAdapter(&dxgiAdapter), "Could not  get adapter");
    CheckDXOperation(dxgiAdapter->GetParent(IID_PPV_ARGS(&dxgiFactory)), "Could not get adapter parent");


    D2D1_FACTORY_OPTIONS options;
    ZeroMemory(&options, sizeof(D2D1_FACTORY_OPTIONS));

#if defined(_DEBUG)
    // If the project is in a debug build, enable Direct2D debugging via SDK Layers.
    options.debugLevel = D2D1_DEBUG_LEVEL_INFORMATION;
#endif


    D2D1CreateFactory(
        D2D1_FACTORY_TYPE_MULTI_THREADED,
        __uuidof(ID2D1Factory2),
        &options,
        &d2dFactory
        );


    // Create the Direct2D device object and a corresponding context.
    d2dFactory->CreateDevice(dxgiDevice.Get(), &(d2dDevice));
    CheckDXOperation(hr, "Could not create D2D1 device");

    hr = d2dDevice->CreateDeviceContext(
        D2D1_DEVICE_CONTEXT_OPTIONS_NONE,
        &cp_d2dContext
        );
    CheckDXOperation(hr, "Could not create device context");

    // Set DPI to the display's current DPI.
    cp_d2dContext->SetDpi(displayInfo->RawDpiX, displayInfo->RawDpiY);
    cp_d2dContext->SetUnitMode(D2D1_UNIT_MODE_PIXELS);

    //Create the swapchain
    DXGI_SWAP_CHAIN_DESC1 swapChainDesc = { 0 };
#if WINAPI_FAMILY == WINAPI_FAMILY_PHONE_APP
    double rpp = displayInfo->RawPixelsPerViewPixel;
    swapChainDesc.Width = (UINT) (panel->ActualWidth * rpp);
    swapChainDesc.Height = (UINT) (panel->ActualHeight * rpp);
#else
    swapChainDesc.Width = (UINT) (panel->ActualWidth * (double) displayInfo->ResolutionScale / 100.0f);      // Match the size of the panel.
    swapChainDesc.Height = (UINT) (panel->ActualHeight * (double) displayInfo->ResolutionScale / 100.0f);
#endif
    swapChainDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    swapChainDesc.Stereo = false;
    swapChainDesc.SampleDesc.Count = 1;
    swapChainDesc.SampleDesc.Quality = 0;
    swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    // This seems to NOT include the front buffer http://www.gamedev.net/topic/633807-swap-chain-buffer-count/
    swapChainDesc.BufferCount = 1;
    swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL;
    swapChainDesc.Flags = 0;
    swapChainDesc.AlphaMode = DXGI_ALPHA_MODE_UNSPECIFIED;

    hr = dxgiFactory->CreateSwapChainForComposition(
        d3dDevice.Get(),
        &swapChainDesc,
        nullptr,
        &cp_swapChain
        );
    CheckDXOperation(hr, "Could not create swapChain");
    CheckDXOperation(dxgiDevice->SetMaximumFrameLatency(1), "Could not set maximum Frame Latency");

    // Configurer l'échelle inverse sur la chaîne de permutation
    DXGI_MATRIX_3X2_F inverseScale = { 0 };
    inverseScale._11 = 1.0f / panel->CompositionScaleX;
    inverseScale._22 = 1.0f / panel->CompositionScaleY;
    ComPtr<IDXGISwapChain2> spSwapChain2;
    CheckDXOperation(cp_swapChain.As<IDXGISwapChain2>(&spSwapChain2), L"Could not retrieve SwapChain");
    CheckDXOperation(spSwapChain2->SetMatrixTransform(&inverseScale), L"Failed to set matrix transform");
    
    //TODO: perform the next 2 calls on the UI thread
    ComPtr<ISwapChainPanelNative> panelNative;
    hr = reinterpret_cast<IUnknown*>(panel)->QueryInterface(IID_PPV_ARGS(&panelNative));
    CheckDXOperation(hr, "Could not initialise the native panel");

    // Associate swap chain with SwapChainPanel.  This must be done on the UI thread.
    CheckDXOperation(panelNative->SetSwapChain(cp_swapChain.Get()), "Could not associate the swapChain");
}

void DirectXManger::Trim()
{
    if (cp_dxgiDev3 != nullptr)
        cp_dxgiDev3->Trim();
}
