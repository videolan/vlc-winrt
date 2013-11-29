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

using namespace VLC_Wrapper;
using namespace Windows::Graphics::Display;

Player::Player(SwapChainBackgroundPanel^ panel) :p_mp(NULL), p_instance(NULL)
{
    OutputDebugStringW(L"Hello, Player!");
	p_panel = panel;
	p_dxManager = new DirectXManger();
	
	m_displayWidth = (float)(p_panel->ActualWidth * (float)DisplayProperties::ResolutionScale / 100.0f);
	m_displayHeight = (float)(p_panel->ActualHeight * (float) DisplayProperties::ResolutionScale / 100.0f);
}

//Todo: don't block UI during initialization
IAsyncAction^ Player::Initialize()
{
	p_dxManager->CreateSwapPanel(p_panel);

	IAsyncAction^ vlcInitTask = ThreadPool::RunAsync(ref new WorkItemHandler([=](IAsyncAction^ operation)
	{
		this->InitializeVLC();
	}, Platform::CallbackContext::Any));
	return vlcInitTask;
}

void Player::InitializeVLC()
{
	ComPtr<MMDeviceLocator> audioReg = Make<MMDeviceLocator>();

	audioReg->m_AudioClient = NULL;
	audioReg->RegisterForWASAPI();

	void *addr = NULL;
	while (!WaitOnAddress(&audioReg->m_AudioClient, &addr, sizeof(void*), 1000)) {
		OutputDebugStringW(L"Waiting for audio\n");
	}

	char ptr_astring[40];
	sprintf_s(ptr_astring, "--mmdevice-audioclient=0x%p", audioReg->m_AudioClient);

	char ptr_d2dstring[40];
	sprintf_s(ptr_d2dstring, "--winrt-d2dcontext=0x%p", p_dxManager->cp_d2dContext);

	char ptr_scstring[40];
	sprintf_s(ptr_scstring, "--winrt-swapchain=0x%p", p_dxManager->cp_swapChain);

	char widthstring[40];
	sprintf_s(widthstring, "--winrt-width=%f", m_displayWidth);

	char heightstring[40];
	sprintf_s(heightstring, "--winrt-height=%f", m_displayHeight);

	/* Don't add any invalid options, otherwise it causes LibVLC to fail */
	const char *argv[] = {
		"-I", "dummy",
		"--no-osd",
		"--verbose=2",
		"--no-video-title-show",
		"--no-stats",
		"--no-drop-late-frames",
		ptr_d2dstring,
		ptr_scstring,
		widthstring,
		heightstring,
		"--aout=mmdevice",
		ptr_astring,
		"--avcodec-fast"
	};

	p_instance = libvlc_new(sizeof(argv) / sizeof(*argv), argv);
	if (!p_instance) {
		throw new std::exception("Could not initialise libvlc!", 1);
		return;
	}
}

void Player::Open(Platform::String^ mrl) 
{
    size_t len = WideCharToMultiByte (CP_UTF8, 0, mrl->Data(), -1, NULL, 0, NULL, NULL);
    char* p_mrl = new char[len];
    WideCharToMultiByte (CP_UTF8, 0, mrl->Data(), -1, p_mrl, len, NULL, NULL);
    
	if (p_instance){
		libvlc_media_t* m = libvlc_media_new_location(this->p_instance, p_mrl);
		p_mp = libvlc_media_player_new_from_media(m);
		libvlc_media_release(m);
	}
    
    delete[](p_mrl);
}

void Player::Stop()
{
	if (p_mp)
	{
		libvlc_media_player_stop(p_mp);
	}
    return;
}

void Player::Pause()
{
	if (p_mp)
	{
		libvlc_media_player_pause(p_mp);
	}
    return;
}

void Player::Play()
{
	if (p_mp)
	{
		libvlc_media_player_play(p_mp);
	}
    return;
}

void Player::Seek(float position)
{
	if (p_mp)
	{
		if (libvlc_media_player_is_seekable(p_mp))
		{
			libvlc_media_player_set_position(p_mp, position);
		}
	}
}

float Player::GetPosition()
{
	float position = 0.0f;
	if (p_mp)
	{
		position = libvlc_media_player_get_position(p_mp);
	}
	return position;
}

int64 Player::GetLength()
{
	int64 length = 0;
	if (p_mp)
	{
		length = libvlc_media_player_get_length(p_mp);
	}
	return length;
}

Player::~Player()
{
	libvlc_media_player_stop(p_mp);
	libvlc_media_player_release(p_mp);
	libvlc_release(p_instance);

	if (p_dxManager){
		delete p_dxManager;
		p_dxManager = nullptr;
	}
}


