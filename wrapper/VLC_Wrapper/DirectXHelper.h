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

namespace DX
{
    inline void ThrowIfFailed(HRESULT hr)
    {
        if (FAILED(hr))
        {
            throw Platform::Exception::CreateException(hr);
        }
    }

    inline D2D1_COLOR_F ConvertToColorF(Windows::UI::Color color)
    {
        return D2D1::ColorF(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
    }

    inline D2D1_RECT_F ConvertToRectF(Windows::Foundation::Rect source)
    {
        return D2D1::RectF(source.Left, source.Top, source.Right, source.Bottom);
    }
}
