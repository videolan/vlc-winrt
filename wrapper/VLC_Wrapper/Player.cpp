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


Player::Player(Windows::UI::Xaml::Controls::SwapChainPanel^ swapChainPanel)
{
    OutputDebugStringW(L"Hello, Player!");

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
	sprintf_s(ptr_vstring, "--winrt-swapchainpanel=0x%p", swapChainPanel);

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
    if(!p_instance) {
        throw new std::exception("Could not initialise libvlc!",1);
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


