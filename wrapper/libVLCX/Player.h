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

#pragma once

#include <vlc/vlc.h>
#include "MMDeviceLocator.h"
#include "DirectXManager.h"

#include <exception>
#include <collection.h>

using namespace Windows::UI::Xaml::Controls;
using namespace Windows::System::Threading;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;

namespace libVLCX {
    public delegate void MediaEndedHandler();

    [Windows::Foundation::Metadata::WebHostHidden]
    public ref class Player sealed
    {
    public:
        Player(SwapChainBackgroundPanel^ panel);
        IAsyncAction^ Initialize();

        void          Open(Platform::String^ mrl);

        void          Stop();
        void          Pause();
        void          Play();

        void          Seek(float position);
        float         GetPosition();
        int64         GetLength();
        int64         GetTime();

        float         GetRate();
        int           SetRate(float rate);

        int           GetSubtitleCount();
        int           GetSubtitleDescription(IMap<int, Platform::String ^> ^tracks);
        int           SetSubtitleTrack(int track);

        int           GetAudioTracksCount();
        int           GetAudioTracksDescription(IMap<int, Platform::String ^> ^tracks);
        int           SetAudioTrack(int track);

        int           SetVolume(int volume);
        int           GetVolume();

        virtual       ~Player();
        void          DetachEvent();
        void          UpdateSize(unsigned int x, unsigned int y);

        void OpenSubtitle( Platform::String ^mrl);
    public:
        event MediaEndedHandler^ MediaEnded;

    internal:
        void MediaEndedCall();

    private:
        void                     InitializeVLC();
        libvlc_instance_t        *p_instance;
        libvlc_media_player_t    *p_mp;
        SwapChainBackgroundPanel ^p_panel;
        DirectXManger            *p_dxManager;
        float                    m_displayWidth;
        float                    m_displayHeight;
    };

    class PlayerPointerWrapper
    {
    public:
        Player^ player;

    public:
        PlayerPointerWrapper(Player^ player)
        {
            this->player = player;
        }
    };
}

