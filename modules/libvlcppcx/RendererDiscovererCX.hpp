/*****************************************************************************
* RendererDiscoverer.hpp: RendererDiscoverer API
*****************************************************************************
* Copyright © 2015 libvlcpp authors & VideoLAN
*
* Authors: Alexey Sokolov <alexey+vlc@asokolov.org>
*          Hugo Beauzée-Luyssen <hugo@beauzee.fr>
*          Bastien Penavayre <bastienPenava@gmail.com>
*
* This program is free software; you can redistribute it and/or modify
* it under the terms of the GNU Lesser General Public License as published by
* the Free Software Foundation; either version 2.1 of the License, or
* (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Lesser General Public License for more details.
*
* You should have received a copy of the GNU Lesser General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston MA 02110-1301, USA.
*****************************************************************************/

#pragma once

#include "vlcpp\RendererDiscoverer.hpp"

namespace libVLCX
{
    ref class RendererDiscovererEventManager;
    ref class Instance;

    public ref class RendererItem sealed
    {
  
    internal:
        VLC::RendererDiscoverer::Item m_item;
        RendererItem(const VLC::RendererDiscoverer::Item& item);

    public:  
        Platform::String^ name();
        Platform::String^ type();
        Platform::String^ iconUri();
        bool canRenderVideo();
        bool canRenderAudio();
    };

    public ref class RendererDiscoverer sealed
    {
        VLC::RendererDiscoverer m_discoverer;

    public:
        RendererDiscoverer(Instance^ inst, Platform::String^ name);

        bool start();

        void stop();

        RendererDiscovererEventManager^ eventManager();

    private:
        RendererDiscovererEventManager ^ m_eventManager;
    };
}