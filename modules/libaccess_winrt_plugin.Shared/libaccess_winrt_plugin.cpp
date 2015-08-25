/*****************************************************************************
 * Copyright © 2013-2014 VideoLAN
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
# define WIN32_LEAN_AND_MEAN /* Exclude rarely-used stuff from Windows headers */
#endif

#ifdef _MSC_VER /* help visual studio compile vlc headers */
# define inline __inline
# define strdup _strdup
# define ssize_t SSIZE_T
# define N_(x) x
# define _(x) x
int poll(struct pollfd *, unsigned, int);
# define restrict __restrict
#endif

# define VLC_MODULE_COPYRIGHT "Copyright";
# define VLC_MODULE_LICENSE  VLC_LICENSE_LGPL_2_1_PLUS;

using namespace concurrency;
using namespace Platform;
using namespace Windows::Storage;
using namespace Windows::Storage::AccessCache;
using namespace Windows::Storage::Streams;
using namespace Windows::Foundation;

#include <vlc_common.h>
#include <vlc_plugin.h>
#include <vlc_access.h>
#include <vlc_input.h>
#define GetACP() (0)
#include <vlc_charset.h>
#include <memory>

static int                    Open(vlc_object_t *);
static void                   Close(vlc_object_t *);

static ssize_t                Read(access_t *access, uint8_t *buffer, size_t size);
static int                    Seek(access_t *access, uint64_t position);
static int                    Control(access_t *access, int query, va_list args);

/*****************************************************************************
* Module descriptor
*****************************************************************************/

vlc_module_begin()
set_shortname(N_("WinRTInput"))
set_description(N_("WinRT input"))
set_category(CAT_INPUT)
set_subcategory(SUBCAT_INPUT_ACCESS)
set_capability("access", 80)
add_shortcut("winrt", "file")
set_callbacks(&Open, &Close)
vlc_module_end()

struct access_sys_t
{
    IRandomAccessStream^   readStream;
    DataReader^            dataReader;
};


void Debug(const wchar_t *fmt, ...)
{
    wchar_t buf[255];
    va_list args;
    va_start(args, fmt);
    vswprintf_s(buf, 255, fmt, args);
    va_end(args);
    OutputDebugStringW(buf);
}

void replaceAll( std::string& str, const std::string& from, const std::string& to ) {
    if( from.empty() )
        return;
    size_t start_pos = 0;
    while( ( start_pos = str.find( from, start_pos ) ) != std::string::npos ) {
        str.replace( start_pos, from.length(), to );
        start_pos += to.length(); // In case 'to' contains 'from', like replacing 'x' with 'yx'
    }
}

Platform::String^
GetString(char* in)
{
    std::string sin( in );
    replaceAll( sin, "\\\\", "\\" );
    std::unique_ptr<wchar_t, void(*)(void*)> str( ToWide(sin.c_str()), free );
    return ref new Platform::String(str.get());
}

bool IsTokenValid(Platform::String^ futureAccesToken) {
    auto charBegin = futureAccesToken->Begin()[0];
    auto charEnd = (futureAccesToken->End() - 1)[0];
    return !((charBegin != '{') || ((charEnd != '}') || futureAccesToken->Length() < 32));
}

/*****************************************************************************
* Local prototypes
*****************************************************************************/

/**
 * Handles the file opening
 */
static int OpenFileAsync(access_sys_t *p_sys, String^ path)
{
    auto openTask = create_task(StorageFile::GetFileFromPathAsync(path)).then([=](StorageFile^ storageFile)
    {
        create_task(storageFile->OpenReadAsync()).then([p_sys](task<IRandomAccessStreamWithContentType^> task)
        {
            p_sys->readStream = task.get();
            p_sys->dataReader = ref new DataReader(p_sys->readStream);
        }).wait();
    });

    try
    {
        openTask.wait();  /* block with wait since we're in a worker thread */
    }
    catch( Exception^ )
    {
        OutputDebugString(L"Failed to open file.");
        return VLC_EGENERIC;
    }
    return VLC_SUCCESS;
}

static int OpenFileAsyncWithToken(access_sys_t *p_sys, String^ token)
{
    auto openTask = create_task(StorageApplicationPermissions::FutureAccessList->GetFileAsync(token))
        .then([p_sys](StorageFile^ newFile) {
        create_task(newFile->OpenReadAsync())
            .then([p_sys](task<IRandomAccessStreamWithContentType^> task) {
            p_sys->readStream = task.get();
            p_sys->dataReader = ref new DataReader(p_sys->readStream);
        }).wait();
    });
    try
    {
        openTask.wait();  /* block with wait since we're in a worker thread */
    }
    catch (Exception^)
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
    access_t *access = (access_t *) object;
    String^ futureAccesToken;
    int (*pf_open)(access_sys_t *, String^);

    if (strncmp(access->psz_access, "winrt", 5) == 0) {
        futureAccesToken = GetString(access->psz_location);
        if(!IsTokenValid(futureAccesToken))
            return VLC_EGENERIC;
        pf_open = OpenFileAsyncWithToken;
    }
    else if (strncmp(access->psz_access, "file", 4) == 0) {
#if 0
        if (strcmp(access->psz_demux, "subtitle") == 0)
#endif
        {
            char* pos = strstr(access->psz_filepath, "winrt:\\\\");
            if (pos && strlen(pos) > 8) {
                futureAccesToken = GetString(pos + 8);
                if (!IsTokenValid(futureAccesToken))
                    return VLC_EGENERIC;
                pf_open = OpenFileAsyncWithToken;
            }
            else
            {
                pf_open = OpenFileAsync;
                futureAccesToken = GetString(access->psz_filepath);
            }
        }
#if 0
        else
        {
            pf_open = OpenFileAsync;
            futureAccesToken = GetString(access->psz_filepath);
        }
#endif
    }
    else
        return VLC_EGENERIC;

    access_sys_t *p_sys = access->p_sys = new(std::nothrow) access_sys_t();
    if (p_sys == nullptr)
        return VLC_EGENERIC;

    if (pf_open(p_sys, futureAccesToken ) != VLC_SUCCESS) {
        OutputDebugStringW(L"Error opening file with Path");
        Close(object);
        return VLC_EGENERIC;
    }

    access->pf_read = &Read;
    access->pf_seek = &Seek;
    access->pf_control = &Control;

    return VLC_SUCCESS;
}

/* */
void Close(vlc_object_t *object)
{
    access_t     *access = (access_t *) object;
    access_sys_t *p_sys = access->p_sys;
    if( p_sys->dataReader != nullptr ){
        delete p_sys->dataReader;
        p_sys->dataReader = nullptr;
    }
    if( p_sys->readStream != nullptr ){
        delete p_sys->readStream;
        p_sys->readStream = nullptr;
    }
    delete p_sys;
}

/* */
ssize_t Read(access_t *access, uint8_t *buffer, size_t size)
{
    access_sys_t *p_sys = access->p_sys;

    unsigned int totalRead = 0;

    auto readTask = create_task(p_sys->dataReader->LoadAsync(size)).then([&totalRead, buffer, p_sys](unsigned int numBytesLoaded)
    {
        p_sys->dataReader->ReadBytes( Platform::ArrayReference<uint8_t>( buffer, numBytesLoaded ) );
        totalRead = numBytesLoaded;
    });

    try
    {
        readTask.wait(); /* block with wait since we're in a worker thread */
    }
    catch( Exception^ ex )
    {
        OutputDebugString(L"Failure while reading block");
        if( ex->HResult == HRESULT_FROM_WIN32(ERROR_OPLOCK_HANDLE_CLOSED) ){
            if( OpenFileAsync(p_sys, GetString(access->psz_location)) == VLC_SUCCESS ){
                p_sys->readStream->Seek(access->info.i_pos);
                return Read(access, buffer, size);
            }
            OutputDebugString(L"Failed to reopen file");
        }
        return -1;
    }

    access->info.i_pos += totalRead;
    access->info.b_eof = p_sys->readStream->Position >= p_sys->readStream->Size;
    if( access->info.b_eof ){
        OutputDebugString(L"End of file reached");
    }

    return totalRead;
}

/* */
int Seek(access_t *access, uint64_t position)
{
    access_sys_t *p_sys = access->p_sys;

    try
    {
        p_sys->readStream->Seek(position);
        access->info.i_pos = position;
        access->info.b_eof = p_sys->readStream->Position >= p_sys->readStream->Size;
    }
    catch( int ex )
    {
        Debug(L"Exception: 0x%x", ex);
        return VLC_EGENERIC;
    }

    return VLC_SUCCESS;
}

/* */
int Control(access_t *access, int query, va_list args)
{
    VLC_UNUSED(access);
    switch( query )
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
        *va_arg(args, uint64_t *) = access->p_sys->readStream->Size;
        return VLC_SUCCESS;
    }
    default:
        return VLC_EGENERIC;
    }
}
