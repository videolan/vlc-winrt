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

//Dont begin copying new frames into memory until the old one is rendered on the UI thread.
//start 2 signalled.
static HANDLE displayLock = CreateSemaphoreExW( NULL,           // default security attributes
                                    1,  // initial count
                                    4,  // maximum count
                                    L"displaysem",0,SYNCHRONIZE|SEMAPHORE_MODIFY_STATE);
static HANDLE xamlLock = CreateSemaphoreExW( NULL,           // default security attributes
                                    1,  // initial count
                                    4,  // maximum count
                                    L"xamlsem",0,SYNCHRONIZE|SEMAPHORE_MODIFY_STATE);
static byte* pixelData;
static VLCD2dImageSource^ vlcImageSource;
static UINT frameWidth = 1024;
static UINT frameHeight = 768;
static UINT pitch;
static int pixelBufferSize;
Windows::UI::Xaml::Media::ImageBrush^ target;


void *Player::Lock(void* opqaue, void** planes){
    //Wait for multiple
    WaitForSingleObjectEx(displayLock, INFINITE, false);
    WaitForSingleObjectEx(xamlLock, INFINITE, false);

    *planes = pixelData;
    return NULL;
}

void Player::Unlock(void* opaque, void* picture, void** planes){
    //signal
    ReleaseSemaphore(displayLock, 1, NULL);
    return;
}
void Player::Display(void* opaque, void* picture){
    //do even more stuff
    vlcImageSource->Dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler( []
    {
        vlcImageSource->BeginDraw(Rect(0, 0, (float)frameWidth, (float)frameHeight));
        vlcImageSource->DrawFrame(frameHeight, frameWidth, pixelData, pitch, Rect(0, 0, (float)frameWidth, (float)frameHeight));
        vlcImageSource->EndDraw();
        //signal
        ReleaseSemaphore(xamlLock, 1, NULL);
    }));

    return;
}

Player::Player(Windows::UI::Xaml::Media::ImageBrush^ brush)
{
    OutputDebugStringW(L"Hello, Player!");

    ComPtr<MMDeviceLocator> audioReg = Make<MMDeviceLocator>();
    audioReg->m_AudioClient = NULL;
    audioReg->RegisterForWASAPI();

    void *addr = NULL;
    while (!WaitOnAddress(&audioReg->m_AudioClient, &addr, sizeof(void*), 1000)) {
        OutputDebugStringW(L"Waiting for audio\n");
    }

    char ptr_string[40];
    sprintf_s(ptr_string, "--mmdevice-audioclient=0x%p", audioReg->m_AudioClient);

    /* Don't add any invalid options, otherwise it causes LibVLC to fail */
    const char *argv[] = {
        "-I", "dummy",
        "--no-osd",
        "--verbose=2",
        "--no-video-title-show",
        "--no-stats",
        "--no-drop-late-frames",
        "--aout=mmdevice",
        ptr_string,
        //"--avcodec-fast"
    };

    p_instance = libvlc_new(sizeof(argv) / sizeof(*argv), argv);
    if(!p_instance) {
        throw new std::exception("Could not initialise libvlc!",1);
        return;
    }

	target = brush;
  
}

void Player::Open(Platform::String^ mrl, int width, int height) {

	frameHeight = height;
	frameWidth = width;

    size_t len = WideCharToMultiByte (CP_UTF8, 0, mrl->Data(), -1, NULL, 0, NULL, NULL);
    char* p_mrl = new char[len];
    WideCharToMultiByte (CP_UTF8, 0, mrl->Data(), -1, p_mrl, len, NULL, NULL);
    
    libvlc_media_t* m = libvlc_media_new_location(this->p_instance, p_mrl);
    p_mp = libvlc_media_player_new_from_media(m);

	vlcImageSource = ref new VLCD2dImageSource(frameWidth, frameHeight, true);
	target->ImageSource = vlcImageSource;

    //we're using vmem so format is always RGB
    unsigned int bitsPerPixel = 32; // hard coded for RV32 videos
    pitch = (frameWidth*bitsPerPixel)/8;

    //hard coded pixel data allocation for sample video
    pixelBufferSize = (frameWidth*frameHeight*bitsPerPixel)/8;
    pixelData = new byte[pixelBufferSize];

    libvlc_video_set_format(p_mp, "RV32", frameWidth, frameHeight, pitch);
    libvlc_video_set_callbacks(p_mp, (libvlc_video_lock_cb)(this->Lock),
        (libvlc_video_unlock_cb)(this->Unlock),
        (libvlc_video_display_cb)(this->Display), NULL);

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
    delete(pixelData);
}


