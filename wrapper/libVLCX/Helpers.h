/*****************************************************************************
* Copyright © 2014 VideoLAN
*
* Authors: Jean-Baptiste Kempf
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

#include <Windows.h>

using namespace Platform;

char *
FromPlatformString(Platform::String^ str) {
    size_t len = WideCharToMultiByte(CP_UTF8, 0, str->Data(), -1, NULL, 0, NULL, NULL);
    if(len == 0)
        return NULL;
    char* psz_str = new char[len];
    WideCharToMultiByte(CP_UTF8, 0, str->Data(), -1, psz_str, len, NULL, NULL);
    return psz_str;
}

Platform::String^
ToPlatformString(const char *str) {
    size_t len = MultiByteToWideChar(CP_UTF8, 0, str, -1, NULL, 0);
    if(len == 0)
        return nullptr;
    wchar_t* w_str = new wchar_t[len];
    MultiByteToWideChar(CP_UTF8, 0, str, -1, w_str, len);
    return ref new Platform::String(w_str);
}

/**
 * Helper to return a WinRT string from a char*
 */
Platform::String^
GetString(char* in)
{
    std::string s_str = std::string(in);
    std::wstring wid_str = std::wstring(s_str.begin(), s_str.end());
    const wchar_t* w_char = wid_str.c_str();
    return ref new String(w_char);
}

size_t
ToCharArray(Platform::String^ str, char *arr, size_t maxSize)
{
    size_t nbConverted = 0;
    wcstombs_s(&nbConverted, arr, 128, str->Data(), maxSize);
    return nbConverted;
}

void
Debug( const wchar_t *fmt, ...) {
    wchar_t buf[255];
    va_list args;
    va_start(args, fmt);
    vswprintf_s(buf, fmt, args);
    va_end(args);
    OutputDebugStringW(buf);
}
