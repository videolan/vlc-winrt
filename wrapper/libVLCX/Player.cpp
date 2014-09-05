/*****************************************************************************
 * Copyright © 2013-2014 VideoLAN
 *
 * Authors: Kellen Sunderland
 *          Jean-Baptiste Kempf
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
#include "Helpers.h"
#include <map>
using namespace libVLCX;
using namespace Windows::Graphics::Display;

Player::Player(SwapChainPanel^ panel) :p_mp(NULL), p_instance(NULL)
{
    //OutputDebugStringW(L"Hello, Player!");
    p_panel = panel;
    p_dxManager = new DirectXManger();

    UpdateSize(p_panel->ActualWidth, p_panel->ActualHeight);
}

//Todo: don't block UI during initialization
IAsyncAction^ Player::Initialize()
{
    p_dxManager->CreateSwapPanel(p_panel);

    IAsyncAction^ vlcInitTask = ThreadPool::RunAsync(ref new WorkItemHandler([=](IAsyncAction^ operation)
    {
        this->InitializeVLC();
    }, Platform::CallbackContext::Any), WorkItemPriority::High, WorkItemOptions::TimeSliced);
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
    sprintf_s(ptr_astring, "--winstore-audioclient=0x%p", audioReg->m_AudioClient);

    char ptr_d2dstring[40];
    sprintf_s(ptr_d2dstring, "--winrt-d2dcontext=0x%p", p_dxManager->cp_d2dContext);

    char ptr_scstring[40];
    sprintf_s(ptr_scstring, "--winrt-swapchain=0x%p", p_dxManager->cp_swapChain);

    char widthstring[40];
    sprintf_s(widthstring, "--winrt-width=0x%p", &m_displayWidth);

    char heightstring[40];
    sprintf_s(heightstring, "--winrt-height=0x%p", &m_displayHeight);

    char fontstring[128 + 28];
    char packagePath[128];

    Windows::ApplicationModel::Package^ currentPackage = Windows::ApplicationModel::Package::Current;
    ToCharArray(currentPackage->InstalledLocation->Path, packagePath, 128);
    sprintf_s(fontstring, "--freetype-font=%s\\segoeui.ttf", packagePath);

    /* Don't add any invalid options, otherwise it causes LibVLC to fail */
    const char *argv[] = {
        "-I",
        "dummy",
        "--no-osd",
        "--verbose=3",
        "--no-stats",
        ptr_d2dstring,
        ptr_scstring,
        widthstring,
        heightstring,
        "--avcodec-fast",
        "--no-avcodec-dr",
        fontstring
    };

    p_instance = libvlc_new(sizeof(argv) / sizeof(*argv), argv);
    if (!p_instance) {
        throw ref new Platform::Exception(E_FAIL, "Could not initialise libvlc!");
    }
}

void Player::UpdateSize(unsigned int x, unsigned int y)
{
    m_displayWidth = (float)(x * (float)DisplayProperties::ResolutionScale / 100.0f);
    m_displayHeight = (float)(y * (float)DisplayProperties::ResolutionScale / 100.0f);

    p_dxManager->UpdateSwapChain(m_displayWidth, m_displayHeight);
}

void Player::MediaEndedCall(){
    MediaEnded();
}

void vlc_event_callback(const libvlc_event_t *ev, void *data)
{
    Player ^player = ((PlayerPointerWrapper*)data)->player;
    if (ev->type == libvlc_MediaPlayerEndReached)
    {
        player->DetachEvent();
        player->MediaEndedCall();
    }
}

void Player::DetachEvent(){
    libvlc_event_manager_t *ev = libvlc_media_player_event_manager(p_mp);
    static const libvlc_event_type_t mp_events[] = {
        libvlc_MediaPlayerPlaying,
        libvlc_MediaPlayerPaused,
        libvlc_MediaPlayerEndReached,
        libvlc_MediaPlayerStopped,
        libvlc_MediaPlayerVout,
        libvlc_MediaPlayerPositionChanged,
        libvlc_MediaPlayerEncounteredError
    };
    for (int i = 0; i < (sizeof(mp_events) / sizeof(*mp_events)); i++)
        libvlc_event_detach(ev, mp_events[i], vlc_event_callback, new PlayerPointerWrapper(this));
}

void Player::Open(Platform::String^ mrl)
{

    const char *p_mrl = FromPlatformString(mrl);
    if (p_instance){
        libvlc_media_t* m = libvlc_media_new_location(this->p_instance, p_mrl);
        p_mp = libvlc_media_player_new_from_media(m);

        /* Connect the event manager */
        libvlc_event_manager_t *ev = libvlc_media_player_event_manager(p_mp);
        static const libvlc_event_type_t mp_events[] = {
            libvlc_MediaPlayerPlaying,
            libvlc_MediaPlayerPaused,
            libvlc_MediaPlayerEndReached,
            libvlc_MediaPlayerStopped,
            libvlc_MediaPlayerVout,
            libvlc_MediaPlayerPositionChanged,
            libvlc_MediaPlayerEncounteredError
        };

        for (int i = 0; i < (sizeof(mp_events) / sizeof(*mp_events)); i++)
        {
            libvlc_event_attach(ev, mp_events[i], vlc_event_callback, new PlayerPointerWrapper(this));
        }

        libvlc_media_release(m);
    }

    delete[](p_mrl);
    p_dxManager->ClearSwapChainBuffers();
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

int64 Player::GetTime()
{
    int64 time = 0;
    if (p_mp)
    {
        time = libvlc_media_player_get_time(p_mp);
    }
    return time;
}

int Player::GetSubtitleCount(){
    int subtitleTrackCount = 0;
    if (p_mp)
    {
        subtitleTrackCount = libvlc_video_get_spu_count(p_mp);
    }
    return subtitleTrackCount;
}

int Player::GetSubtitleDescription(IMap<int,Platform::String ^> ^tracks) {
    libvlc_track_description_t *subtitleTrackDesc = NULL;
    int count = 0;
    if (p_mp && tracks) {
        subtitleTrackDesc = libvlc_video_get_spu_description(p_mp);
        while (subtitleTrackDesc != NULL ) {
            tracks->Insert(subtitleTrackDesc->i_id, ToPlatformString(subtitleTrackDesc->psz_name));
            subtitleTrackDesc = subtitleTrackDesc->p_next;
            count++;
        }
    }
    libvlc_track_description_list_release(subtitleTrackDesc);
    return count;
}

int Player::SetSubtitleTrack(int track){
    int spuLoaded = 0;
    if (p_mp)
    {
        spuLoaded = libvlc_video_set_spu(p_mp, track);
    }
    return spuLoaded;
}

int Player::GetAudioTracksCount(){
    int audioTracksCount = 0;
    if (p_mp)
    {
        audioTracksCount = libvlc_audio_get_track_count (p_mp);
    }
    return audioTracksCount;
}

int Player::GetAudioTracksDescription(IMap<int,Platform::String ^> ^tracks) {
    libvlc_track_description_t *audioTrackDesc = NULL;
    int count = 0;
    if (p_mp) {
        audioTrackDesc = libvlc_audio_get_track_description(p_mp);
        while (audioTrackDesc != NULL) {
            tracks->Insert(audioTrackDesc->i_id, ToPlatformString(audioTrackDesc->psz_name));
            audioTrackDesc = audioTrackDesc->p_next;
            count++;
        }
    }
    libvlc_track_description_list_release(audioTrackDesc);
    return count;
}

int Player::SetAudioTrack(int track){
    int audioTrackLoaded = 0;
    if (p_mp)
    {
        audioTrackLoaded = libvlc_audio_set_track(p_mp, track);
    }
    return audioTrackLoaded;
}

Player::~Player()
{
    if (p_mp){
        libvlc_media_player_release(p_mp);
    }
    if (p_instance){
        libvlc_release(p_instance);
    }


    /*if (p_dxManager){
        delete p_dxManager;
        p_dxManager = nullptr;
        }*/
}

float
Player::GetRate() {
    float rate = 0.;
    if (p_mp) {
        rate = libvlc_media_player_get_rate(p_mp);
    }
    return rate;
}

int
Player::SetRate(float rate) {
    if (p_mp)
        return libvlc_media_player_set_rate(p_mp, rate);
    else
        return -1;
}

int
Player::SetVolume(int volume) {
    if(p_mp)
        return libvlc_audio_set_volume(p_mp, volume);
    else
        return -1;
}

int
Player::GetVolume() {
    if(p_mp)
        return libvlc_audio_get_volume(p_mp);
    else
        return 0;
}

void
Player::OpenSubtitle( Platform::String ^ mrl)
{
    int ret;
    if(p_mp)
        ret = libvlc_video_set_subtitle_file(p_mp, FromPlatformString(mrl));
    Debug( L"Subtitles %i\n", ret);
};


void Player::Trim()
{
    p_dxManager->Trim();
}
