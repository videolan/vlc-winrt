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

#ifdef _MSC_VER
using ssize_t = long int;
#endif

#include <vlc/vlc.h>
#include <ppltasks.h>
#include "Robuffer.h"

using namespace Windows::Foundation;
using namespace Windows::UI::Xaml::Media::Imaging;
using namespace Windows::Storage::Streams;
using namespace Microsoft::WRL;
using namespace Windows::UI::Core;
using namespace concurrency;
using namespace Windows::ApplicationModel::Core;

namespace libVLCX {
    [Windows::Foundation::Metadata::WebHostHidden]
    public ref class PreparseResult sealed
    {
    public:
        PreparseResult() : length(0){}
        WriteableBitmap^ Bitmap() { return bitmap; }
        uint64 Length() { return length; }
    internal:
        WriteableBitmap^ bitmap;
        uint64 length;
    };

    [Windows::Foundation::Metadata::WebHostHidden]
    public ref class Thumbnailer sealed
    {
    public:
                                           Thumbnailer();
        IAsyncOperation<PreparseResult^>^  TakeScreenshot(Platform::String^ mrl, int width, int height, int timeoutMs);
        virtual                            ~Thumbnailer();

    private:
        libvlc_instance_t *p_instance;
    };
}

