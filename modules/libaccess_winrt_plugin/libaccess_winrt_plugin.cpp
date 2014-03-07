/*****************************************************************************
* Copyright © 2013 VideoLAN
*
* Authors: Kellen Sunderland <kellen _DOT_ sunderland _AT_ gmail _DOT_ com>
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


/*****************************************************************************
* Preamble
*****************************************************************************/

#include "pch.h"
#include <ppltasks.h>

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN /* Exclude rarely-used stuff from Windows headers */
#endif

#ifdef _MSC_VER /* help visual studio compile vlc headers */
#define inline __inline
#define strdup _strdup
#define ssize_t SSIZE_T
#define N_(x) x
int poll(struct pollfd *, unsigned, int);
#endif

using namespace concurrency;
using namespace Platform;
using namespace Windows::Storage;
using namespace Windows::Storage::AccessCache;
using namespace Windows::Storage::Streams;

#include <vlc_common.h>
#include <vlc_plugin.h>
#include <vlc_access.h>
#include <vlc_input.h>

static int					Open(vlc_object_t *);
static void					Close(vlc_object_t *);
static ssize_t				Read(access_t *access, uint8_t *buffer, size_t size);
static int					Seek(access_t *access, uint64_t position);
static int					Control(access_t *access, int query, va_list args);
static String^				GetString(char* in);
static IRandomAccessStream^	readStream;
static DataReader^			dataReader;

/*****************************************************************************
* Module descriptor
*****************************************************************************/

vlc_module_begin()
    set_shortname(N_("WinRTInput"))
    set_description(N_("WinRT input"))
    set_category(CAT_INPUT)
    set_subcategory(SUBCAT_INPUT_ACCESS)
    set_capability("access", 60)
    add_shortcut("winrt")
    set_callbacks(&Open, &Close)
vlc_module_end()

/*****************************************************************************
* Local prototypes
*****************************************************************************/

/**
 * Handles the file opening
*/
static int OpenFileAsync(String^ token)
{
    auto openTask = create_task(StorageApplicationPermissions::FutureAccessList->GetFileAsync(token))
        .then([](StorageFile^ newFile){
            create_task(newFile->OpenReadAsync())
                .then([](task<IRandomAccessStreamWithContentType^> task){
                    readStream = task.get();
                    dataReader = ref new DataReader(readStream);
                }).wait();
        });

    try
    {
        openTask.wait();  /* block with wait since we're in a worker thread */
    }
    catch(Exception^)
    {
        OutputDebugString(L"Failed to open file.");
        return VLC_EGENERIC;
    }
    return VLC_SUCCESS;
}


/**
 * Open a WinRT StorageFile that has been added to the FutureAccessList
 * This allows for generic file loading from a Library, Picker, SkyDrive, SMB, USB etc.
 * psz_location: contains the GUID for the StorageFile in the FutureAccessList
 */
int Open(vlc_object_t *object)
{
    access_t *access    = (access_t *) object;
    access->pf_read     = &Read;
    access->pf_seek     = &Seek;
    access->pf_control  = &Control;

    String^ futureAccesToken = GetString(access->psz_location);
    if ( OpenFileAsync(futureAccesToken) != VLC_SUCCESS ){
        OutputDebugStringW(L"Error Opening File");

        if (dataReader != nullptr){
            delete dataReader;
            dataReader = nullptr;
        }

        if (readStream != nullptr){
            delete readStream;
            readStream = nullptr;
        }

        return VLC_EGENERIC;
    }

    return VLC_SUCCESS;
}

/* */
void Close(vlc_object_t *object)
{
    access_t     *access = (access_t *) object;

    if (dataReader != nullptr){
        delete dataReader;
        dataReader = nullptr;
    }
    if (readStream != nullptr){
        delete readStream;
        readStream = nullptr;
    }
}

/* */
ssize_t Read(access_t *access, uint8_t *buffer, size_t size)
{
    unsigned int totalRead = 0;

    auto readTask = create_task(dataReader->LoadAsync(size)).then([&totalRead, &buffer](unsigned int numBytesLoaded)
    {
        WriteOnlyArray<unsigned char, 1U>^ bufferArray = ref new Array<unsigned char, 1U>(numBytesLoaded);
        dataReader->ReadBytes(bufferArray);
        memcpy(buffer, bufferArray->begin(), bufferArray->end() - bufferArray->begin());
        totalRead = numBytesLoaded;
    });

    try
    {
        readTask.wait(); /* block with wait since we're in a worker thread */
    }
    catch(Exception^ ex)
    {
        OutputDebugString(L"Failure while reading block");
        if (ex->HResult == HRESULT_FROM_WIN32(ERROR_OPLOCK_HANDLE_CLOSED)){
            if (OpenFileAsync(GetString(access->psz_location)) == VLC_SUCCESS){
                return 0;
            }
            OutputDebugString(L"Failed to reopen file");
        }
        return -1;
    }

    access->info.i_pos += totalRead;
    access->info.b_eof = readStream->Position >= readStream->Size;
    if (access->info.b_eof){
        OutputDebugString(L"End of file reached");
    }

    return totalRead;
}

/* */
int Seek(access_t *access, uint64_t position)
{
    try
    {
        readStream->Seek(position);
        access->info.i_pos = position;
        access->info.b_eof = readStream->Position >= readStream->Size;
    }
    catch (int ex)
    {
        return VLC_EGENERIC;
    }

    return VLC_SUCCESS;
}

/* */
int Control(access_t *access, int query, va_list args)
{
    VLC_UNUSED(access);
    switch (query)
    {
    case ACCESS_CAN_FASTSEEK:
    case ACCESS_CAN_PAUSE:
    case ACCESS_CAN_SEEK:
    case ACCESS_CAN_CONTROL_PACE: {
                                      bool *b = va_arg(args, bool*);
                                      *b = true;
                                      return VLC_SUCCESS;
    }

    case ACCESS_GET_PTS_DELAY: {
                                   int64_t *d = va_arg(args, int64_t *);
                                   *d = DEFAULT_PTS_DELAY;
                                   return VLC_SUCCESS;
    }

    case ACCESS_SET_PAUSE_STATE:
        return VLC_SUCCESS;

    case ACCESS_GET_SIZE: {
        *va_arg( args, uint64_t *) = readStream->Size;
        return VLC_SUCCESS;
    }
    default:
        return VLC_EGENERIC;
    }
}

/**
 * Helper to return a WinRT string from a char*
 */
String^ GetString(char* in)
{
    std::string s_str = std::string(in);
    std::wstring wid_str = std::wstring(s_str.begin(), s_str.end());
    const wchar_t* w_char = wid_str.c_str();
    return ref new String(w_char);
}
