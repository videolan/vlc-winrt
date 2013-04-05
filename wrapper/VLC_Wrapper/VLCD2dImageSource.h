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

#include "pch.h"
#include "DirectXHelper.h"

namespace VLC_Wrapper {
    public ref class VLCD2dImageSource sealed : Windows::UI::Xaml::Media::Imaging::SurfaceImageSource
    {
      public:
        VLCD2dImageSource(int pixelWidth, int pixelHeight, bool isOpaque);

        void BeginDraw(Windows::Foundation::Rect updateRect);
        void BeginDraw()    { BeginDraw(Windows::Foundation::Rect(0, 0, (float)m_width, (float)m_height)); }
        void EndDraw();

        void SetDpi(float dpi);

        void Clear(Windows::UI::Color color);
        void VLCD2dImageSource::DrawFrame(UINT height, UINT width, byte* sourceData, UINT pitch, Windows::Foundation::Rect updateRect);

    private protected:
        void CreateDeviceIndependentResources();
        void CreateDeviceResources();

        Microsoft::WRL::ComPtr<ISurfaceImageSourceNative>   m_sisNative;

        // Direct3D device
        Microsoft::WRL::ComPtr<ID3D11Device>                m_d3dDevice;

        // Direct2D objects
        Microsoft::WRL::ComPtr<ID2D1Device>                 m_d2dDevice;
        Microsoft::WRL::ComPtr<ID2D1DeviceContext>          m_d2dContext;

        int                                                 m_width;
        int                                                 m_height;

        ID2D1Bitmap*                                        d2dbmp;
    };
};

