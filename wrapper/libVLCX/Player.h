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

#pragma once

#include <vlc/vlc.h>
#include "MMDeviceLocator.h"
#include "DirectXManger.h"
#include <exception>
#include <map>

using namespace Microsoft::WRL;
using namespace Windows::Media::Devices;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::System::Threading;
using namespace Windows::Foundation;

namespace libVLCX {
    public delegate void MediaEndedHandler();
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

        int           GetSubtitleCount();
        int           GetSubtitleDescription();
        int           SetSubtitleTrack(int track);

        int           GetAudioTracksCount();
        int           GetAudioTracksDescription();
        int           SetAudioTrack(int track);

        virtual       ~Player();
        void          DetachEvent();
        void          UpdateSize(unsigned int x, unsigned int y);

    public:
        event MediaEndedHandler^ MediaEnded;

    internal:
        void MediaEndedCall();

    private:
        size_t      ToCharArray(Platform::String^ str, char* arr, size_t maxSize);

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

