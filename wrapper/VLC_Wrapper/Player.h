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

using namespace Microsoft::WRL;
using namespace Windows::Media::Devices;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::System::Threading;
using namespace Windows::Foundation;

namespace VLC_Wrapper {
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
		int           SetSubtitleTrack(int track);
        virtual       ~Player();

    private:
		void			         InitializeVLC();
        libvlc_instance_t        *p_instance;
        libvlc_media_player_t    *p_mp;
		SwapChainBackgroundPanel ^p_panel;
		DirectXManger            *p_dxManager;
		float                    m_displayWidth;
		float                    m_displayHeight;
    };
}

