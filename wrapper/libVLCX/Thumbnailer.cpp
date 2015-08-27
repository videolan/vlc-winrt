/*****************************************************************************
* Copyright © 2014 VideoLAN
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

#include "Thumbnailer.h"

using namespace libVLCX;

//TODO: dynamic size
#define SEEK_POSITION 0.5f
#define PIXEL_SIZE 4 /* BGRA */

enum thumbnail_state{
    THUMB_SEEKING,
    THUMB_SEEKED,
    THUMB_RENDERED
};

struct thumbnailer_sys_t
{
    thumbnail_state                         state;
    task_completion_event<PreparseResult^>  screenshotCompleteEvent;
    concurrency::task<void>                 cancellationTask;
    concurrency::cancellation_token_source  timeoutCts;
    char                                    *thumbData;
    unsigned                                thumbHeight;
    unsigned                                thumbWidth;
    size_t                                  thumbSize;
    HANDLE                                  hLock;
    libvlc_media_player_t*                  mp;
    libvlc_event_manager_t*                 eventMgr;
};

Thumbnailer::Thumbnailer()
{
    const char *argv [] = {
        "-I", "dummy",            // Only use options needed for snapshots
        "--verbose=5",
        "--no-video-title-show",
        "--no-stats",
    };
    p_instance = libvlc_new(sizeof(argv) / sizeof(*argv), argv);
    if (!p_instance) {
        //TODO: better error code
        throw ref new Platform::Exception(-1, "Could not initialise libvlc!");
    }
}



static void onError(const libvlc_event_t*, void* data)
{
    auto sys = reinterpret_cast<thumbnailer_sys_t*>(data);
    sys->screenshotCompleteEvent.set(nullptr);
}

/**
* Thumbnailer vout lock
**/
static void *Lock(void *opaque, void **pixels)
{
    thumbnailer_sys_t *sys = (thumbnailer_sys_t*)opaque;

    WaitForSingleObjectEx(sys->hLock, INFINITE, TRUE);
    *pixels = sys->thumbData;
    if (sys->mp && sys->state == THUMB_SEEKING
        && libvlc_media_player_get_position(sys->mp) >= SEEK_POSITION) {
        sys->state = THUMB_SEEKED;
    }
    return NULL;
}

static WriteableBitmap^ CopyToBitmap(thumbnailer_sys_t* sys)
{
    WriteableBitmap^ bmp = ref new WriteableBitmap(sys->thumbWidth, sys->thumbHeight);
    IBuffer^ pixelBuffer = bmp->PixelBuffer;
    ComPtr<IInspectable> pixelBufferInspectable(reinterpret_cast<IInspectable*>(pixelBuffer));
    ComPtr<IBufferByteAccess> pixelBytes;
    HRESULT hr = pixelBufferInspectable.As(&pixelBytes);
    byte* modifyablePixels(nullptr);
    hr = pixelBytes->Buffer(&modifyablePixels);

    for (unsigned int i = 0; i < sys->thumbSize; i += 4) {
        //B
        modifyablePixels[i] = sys->thumbData[i];
        //G
        modifyablePixels[i + 1] = sys->thumbData[i + 1];
        //R
        modifyablePixels[i + 2] = sys->thumbData[i + 2];
        //Alpha
        modifyablePixels[i + 3] = sys->thumbData[i + 3];
    }

    bmp->Invalidate();
    return bmp;
}

/**
* Thumbnailer vout unlock
**/
static void Unlock(void *opaque, void *picture, void *const *pixels){
    thumbnailer_sys_t* sys = (thumbnailer_sys_t*) opaque;

    if (sys->state != THUMB_SEEKED)
    {
        SetEvent(sys->hLock);
        return;
    }

    CoreWindow^ window = CoreApplication::MainView->CoreWindow;
    auto dispatcher = window->Dispatcher;
    auto priority = CoreDispatcherPriority::Low;
    dispatcher->RunAsync(priority, ref new DispatchedHandler([=]()
    {
        auto res = ref new PreparseResult;
        res->bitmap = CopyToBitmap(sys);
        sys->screenshotCompleteEvent.set(res);
        sys->state = THUMB_RENDERED;
        SetEvent(sys->hLock);
    }));
}

IAsyncOperation<PreparseResult^>^ Thumbnailer::TakeScreenshot(Platform::String^ mrl, int width, int height, int timeoutMs)
{
    return concurrency::create_async([&] (concurrency::cancellation_token ct) {

        libvlc_media_player_t* mp;
        thumbnailer_sys_t *sys = new thumbnailer_sys_t();
        auto completionTask = concurrency::create_task(sys->screenshotCompleteEvent, ct);
        sys->cancellationTask = concurrency::create_task([sys, timeoutMs] {
            concurrency::wait(timeoutMs);
            if (concurrency::is_task_cancellation_requested())
                concurrency::cancel_current_task();
            if (!sys->screenshotCompleteEvent._IsTriggered())
            {
                sys->screenshotCompleteEvent.set(nullptr);
            }
        }, sys->timeoutCts.get_token());

        size_t len2 = WideCharToMultiByte(CP_UTF8, 0, mrl->Data(), -1, NULL, 0, NULL, NULL);
        char* mrl_str = new char[len2];
        WideCharToMultiByte(CP_UTF8, 0, mrl->Data(), -1, mrl_str, len2, NULL, NULL);

        if (p_instance){
            libvlc_media_t* m = libvlc_media_new_location(this->p_instance, mrl_str);

            /* Set media to fast with no options */
            libvlc_media_add_option(m, ":no-audio");
            libvlc_media_add_option(m, ":no-spu");
            libvlc_media_add_option(m, ":no-osd");
            libvlc_media_add_option(m, ":avcodec-hw=none");

            mp = libvlc_media_player_new_from_media(m);
            libvlc_media_release(m);
            sys->eventMgr = libvlc_media_player_event_manager(mp);
            libvlc_event_attach(sys->eventMgr, libvlc_MediaPlayerEncounteredError, &onError, sys);
            // Workaround some short and weird samples can reach the end without getting a snapshot generated.
            libvlc_event_attach(sys->eventMgr, libvlc_MediaPlayerEndReached, &onError, sys);

            /* Set the video format and the callbacks. */
            unsigned int pitch = width * PIXEL_SIZE;
            sys->thumbHeight = height;
            sys->thumbWidth = width;
            sys->thumbSize = pitch * sys->thumbHeight;
            sys->thumbData = (char*) malloc(sys->thumbSize);
            sys->hLock = CreateEventEx(NULL, NULL, NULL, EVENT_ALL_ACCESS);
            sys->mp = mp;
            SetEvent(sys->hLock);

            libvlc_video_set_format(mp, "BGRA", sys->thumbWidth, sys->thumbHeight, pitch);
            libvlc_video_set_callbacks(mp, Lock, Unlock, NULL, (void*) sys);
            sys->state = THUMB_SEEKING;

            if (mp)
            {
                libvlc_media_player_play(mp);
                //TODO: Specify position
                libvlc_media_player_set_position(mp, SEEK_POSITION);
            }

            completionTask.then([mp, sys](PreparseResult^ res){
                if ( res != nullptr )
                    res->length = libvlc_media_player_get_length(mp);
                sys->timeoutCts.cancel();
                // We rendered at least one frame, no need to check for EOF anymore
                libvlc_event_detach(sys->eventMgr, libvlc_MediaPlayerEndReached, &onError, sys);
                libvlc_media_player_stop(mp);
                libvlc_media_player_release(mp);
                free(sys->thumbData);
                delete sys;
            });
        }

        delete [](mrl_str);
        return completionTask;
    });
}

Thumbnailer::~Thumbnailer()
{
    libvlc_release(p_instance);
}