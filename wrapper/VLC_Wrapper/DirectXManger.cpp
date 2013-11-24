/*****************************************************************************
* Copyright © 2013 VideoLAN
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
#include "DirectXManger.h"

using namespace VLC_Wrapper;
using namespace Platform;
using namespace Windows::UI::Xaml::Media::Imaging;
using namespace Windows::UI::Core;
using namespace Windows::UI;
using namespace Windows::Foundation;
using namespace Windows::System::Threading;
using namespace D2D1;

DirectXManger::DirectXManger()
{
}


void DirectXManger::CreateSwapPanel(SwapChainPanel^ panel){
	HRESULT hr;
	ComPtr<IDXGIFactory2> dxgiFactory;
	ComPtr<IDXGIAdapter> dxgiAdapter;
	ComPtr<IDXGIDevice1> dxgiDevice;
	ComPtr<ID3D11Device> d3dDevice;
	ComPtr<ID2D1Device> d2dDevice;
	ComPtr<ID2D1Bitmap1> d2dTargetBitmap;

	UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;
	float dpi = Windows::Graphics::Display::DisplayProperties::LogicalDpi;

	D2D1_BITMAP_PROPERTIES1 bitmapProperties =
		BitmapProperties1(
		D2D1_BITMAP_OPTIONS_TARGET | D2D1_BITMAP_OPTIONS_CANNOT_DRAW,
		PixelFormat(DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE_PREMULTIPLIED),
		dpi,
		dpi);

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
	if (hr != S_OK) {
		throw new std::exception("Could not initialise libvlc!", hr);
	}

	hr = d3dDevice.As(&dxgiDevice);
	if (hr != S_OK) {
		throw new std::exception("Could not initialise libvlc!", hr);
	}

	hr = dxgiDevice->GetAdapter(&dxgiAdapter);
	if (hr != S_OK) {
		throw new std::exception("Could not initialise libvlc!", hr);
	}

	hr = dxgiAdapter->GetParent(IID_PPV_ARGS(&dxgiFactory));
	if (hr != S_OK) {
		throw new std::exception("Could not initialise libvlc!", hr);
	}

	//Create the swapchain
	DXGI_SWAP_CHAIN_DESC1 swapChainDesc = { 0 };
	//TODO: try panel height
	swapChainDesc.Width = panel->ActualWidth;      // Match the size of the panel.
	swapChainDesc.Height = panel->ActualHeight;
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
		d3dDevice.Get(),
		&swapChainDesc,
		nullptr,
		&cp_swapChain
		);
	if (hr != S_OK) {
		throw new std::exception("Could not initialise libvlc!", hr);
	}

	hr = dxgiDevice->SetMaximumFrameLatency(1);
	if (hr != S_OK) {
		throw new std::exception("Could not initialise libvlc!", hr);
	}

	//TODO: perform the next 2 calls on the UI thread
	ComPtr<ISwapChainPanelNative> panelNative;
	hr = reinterpret_cast<IUnknown*>(panel)->QueryInterface(IID_PPV_ARGS(&panelNative));
	if (hr != S_OK) {
		throw new std::exception("Could not initialise libvlc!", hr);
	}

	// Associate swap chain with SwapChainPanel.  This must be done on the UI thread.
	hr = panelNative->SetSwapChain(cp_swapChain.Get());
	if (hr != S_OK) {
		throw new std::exception("Could not initialise libvlc!", hr);
	}

	// Create the Direct2D device object and a corresponding context.
	hr = D2D1CreateDevice(
		dxgiDevice.Get(),
		nullptr,
		&(d2dDevice)
		);
	if (hr != S_OK) {
		throw new std::exception("Could not initialise libvlc!", hr);
	}

	hr = d2dDevice->CreateDeviceContext(
		D2D1_DEVICE_CONTEXT_OPTIONS_NONE,
		&cp_d2dContext
		);
	if (hr != S_OK) {
		throw new std::exception("Could not initialise libvlc!", hr);
	}

	// Set DPI to the display's current DPI.
	cp_d2dContext->SetDpi(dpi, dpi);

	ComPtr<IDXGISurface> dxgiBackBuffer;
	hr = cp_swapChain->GetBuffer(0, IID_PPV_ARGS(&dxgiBackBuffer));
	if (hr != S_OK) {
		throw new std::exception("Could not initialise libvlc!", hr);
	}

	//set d2d target
	hr = cp_d2dContext->CreateBitmapFromDxgiSurface(
		dxgiBackBuffer.Get(),
		&bitmapProperties,
		&d2dTargetBitmap
		);

	if (hr != S_OK) {
		throw new std::exception("Could not initialise libvlc!", hr);
	}

	cp_d2dContext->SetTarget(d2dTargetBitmap.Get());
}