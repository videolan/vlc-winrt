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
#include "Player.h"

#include <exception>

using namespace VLC_Wrapper;
using namespace Platform;
using namespace Windows::UI::Xaml::Media::Imaging;
using namespace Windows::UI::Core;
using namespace Windows::UI;
using namespace Windows::Foundation;
using namespace Windows::System::Threading;


Player::Player(SwapChainPanel^ panel)
{
    OutputDebugStringW(L"Hello, Player!");
	p_panel = panel;
}

//Todo: don't block UI during initialization
void Player::Initialize(){
	HRESULT hr;
	ComPtr<IDXGIFactory2> dxgiFactory;
	ComPtr<IDXGIAdapter> dxgiAdapter;
	ComPtr<IDXGIDevice1> dxgiDevice;
	ComPtr<ID3D11Device> d3dDevice;
	ComPtr<IDXGISwapChain1> swapChain1;
	ComPtr<ID2D1Device> d2dDevice;

	UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;
	float dpi = Windows::Graphics::Display::DisplayProperties::LogicalDpi;

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
	swapChainDesc.Width = p_panel->ActualWidth;      // Match the size of the panel.
	swapChainDesc.Height = p_panel->ActualHeight;
	swapChainDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;                  // This is the most common swap chain format.
	swapChainDesc.Stereo = false;
	swapChainDesc.SampleDesc.Count = 1;                                 // Don't use multi-sampling.
	swapChainDesc.SampleDesc.Quality = 0;
	swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
	swapChainDesc.BufferCount = 2;                                      // Use double buffering to enable flip.
	swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL;        // All Windows Store apps must use this SwapEffect.
	swapChainDesc.Flags = 0;
	swapChainDesc.AlphaMode = DXGI_ALPHA_MODE_UNSPECIFIED;

	ComPtr<IDXGISwapChain1> swapChain;
	hr = dxgiFactory->CreateSwapChainForComposition(
		d3dDevice.Get(),
		&swapChainDesc,
		nullptr,
		&swapChain
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
	hr = reinterpret_cast<IUnknown*>(p_panel)->QueryInterface(IID_PPV_ARGS(&panelNative));
	if (hr != S_OK) {
		throw new std::exception("Could not initialise libvlc!", hr);
	}

	// Associate swap chain with SwapChainPanel.  This must be done on the UI thread.
	hr = panelNative->SetSwapChain(swapChain.Get());
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

	this->InitializeVLC();
}

void Player::InitializeVLC(){
	ComPtr<MMDeviceLocator> audioReg = Make<MMDeviceLocator>();
	audioReg->m_AudioClient = NULL;
	audioReg->RegisterForWASAPI();

	void *addr = NULL;
	while (!WaitOnAddress(&audioReg->m_AudioClient, &addr, sizeof(void*), 1000)) {
		OutputDebugStringW(L"Waiting for audio\n");
	}

	char ptr_astring[40];
	sprintf_s(ptr_astring, "--mmdevice-audioclient=0x%p", audioReg->m_AudioClient);

	char ptr_vstring[40];
	sprintf_s(ptr_vstring, "--winrt-d2dcontext=0x%p", cp_d2dContext);

	/* Don't add any invalid options, otherwise it causes LibVLC to fail */
	const char *argv[] = {
		"-I", "dummy",
		"--no-osd",
		"--verbose=2",
		"--no-video-title-show",
		"--no-stats",
		"--no-drop-late-frames",
		ptr_vstring
		//"--aout=mmdevice",
		//ptr_astring,
		//"--avcodec-fast"
	};

	p_instance = libvlc_new(sizeof(argv) / sizeof(*argv), argv);
	if (!p_instance) {
		throw new std::exception("Could not initialise libvlc!", 1);
		return;
	}
}

void Player::Open(Platform::String^ mrl) {

    size_t len = WideCharToMultiByte (CP_UTF8, 0, mrl->Data(), -1, NULL, 0, NULL, NULL);
    char* p_mrl = new char[len];
    WideCharToMultiByte (CP_UTF8, 0, mrl->Data(), -1, p_mrl, len, NULL, NULL);
    
    libvlc_media_t* m = libvlc_media_new_location(this->p_instance, p_mrl);
    p_mp = libvlc_media_player_new_from_media(m);

    libvlc_media_release (m);
    delete[](p_mrl);
}

void Player::Stop(){
    libvlc_media_player_stop(p_mp);
    return;
}

void Player::Pause(){
    libvlc_media_player_pause(p_mp);
    return;
}

void Player::Play(){
    libvlc_media_player_play(p_mp);
    return;
}

void Player::Seek(float position){
	libvlc_media_player_set_position(p_mp, position);
}

float Player::GetPosition(){
	return libvlc_media_player_get_position(p_mp);
}

int64 Player::GetLength(){
	return libvlc_media_player_get_length(p_mp);
}

Player::~Player(){
	libvlc_media_player_stop(p_mp);
	libvlc_media_player_release(p_mp);
	libvlc_release(p_instance);
}


