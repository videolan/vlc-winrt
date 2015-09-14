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

#include <atomic>
#include <memory>

using namespace libVLCX;
#define FAST_COPY 0

//TODO: dynamic size
#define SEEK_POSITION 0.5f
#define PIXEL_SIZE 4 /* BGRA */

enum thumbnail_state{
    THUMB_SEEKING,
    THUMB_SEEKED,
    THUMB_RENDERED,
    THUMB_CANCELLED
};

struct thumbnailer_sys_t
{
    std::atomic<int>                        state;
    task_completion_event<PreparseResult^>  screenshotCompleteEvent;
    concurrency::task<void>                 cancellationTask;
    concurrency::cancellation_token_source  timeoutCts;
    std::unique_ptr<char[]>                 thumbData;
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
        "--no-audio",
        "--no-spu",
        "--no-osd",
        "--aout=none",
        "--no-video-title-show",
        "--no-stats",
    };
    p_instance = libvlc_new(sizeof(argv) / sizeof(*argv), argv);
    if (!p_instance) {
        //TODO: better error code
        throw ref new Platform::Exception(-1, "Could not initialise libvlc!");
    }
}

static void CancelPreparse(const libvlc_event_t*, void* data)
{
    auto sys = reinterpret_cast<thumbnailer_sys_t*>(data);
    sys->state = THUMB_CANCELLED;
    sys->screenshotCompleteEvent.set(nullptr);
}

static void onSeekChanged( const libvlc_event_t*ev, void* data )
{
    auto sys = reinterpret_cast<thumbnailer_sys_t*>( data );
    if ( ev->u.media_player_position_changed.new_position >= SEEK_POSITION )
        sys->state = THUMB_SEEKED;
}

/**
* Thumbnailer vout lock
**/
static void *Lock(void *opaque, void **pixels)
{
    thumbnailer_sys_t *sys = (thumbnailer_sys_t*)opaque;
    *pixels = sys->thumbData.get();
    return nullptr;
}

static WriteableBitmap^ CopyToBitmap(thumbnailer_sys_t* sys)
{
    WriteableBitmap^ bmp = ref new WriteableBitmap(sys->thumbWidth, sys->thumbHeight);
#if FAST_COPY
    InMemoryRandomAccessStream^ stream = ref new InMemoryRandomAccessStream();
    DataWriter^ dataWriter = ref new DataWriter( stream->GetOutputStreamAt( 0 ) );
    dataWriter->WriteBytes( Platform::ArrayReference<unsigned char>( (unsigned char*) sys->thumbData, sys->thumbSize ) );
    bmp->SetSource( stream );
#else /* FAST_COPY */
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
#endif /* FAST_COPY */
    bmp->Invalidate();
    return bmp;
}

/**
* Thumbnailer vout unlock
**/
static void Unlock(void *opaque, void *picture, void *const *pixels){
    thumbnailer_sys_t* sys = (thumbnailer_sys_t*) opaque;

    if (sys->state != THUMB_SEEKED)
        return;

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
    // Wait for the signaling to be done before resuming. Otherwise we could have
    // another call to lock/unlock while we're wrapping up
    WaitForSingleObjectEx(sys->hLock, INFINITE, TRUE);
}

IAsyncOperation<PreparseResult^>^ Thumbnailer::TakeScreenshot(Platform::String^ mrl, int width, int height, int timeoutMs)
{
    if (!p_instance)
        return nullptr;
    return concurrency::create_async([&] (concurrency::cancellation_token ct) {
        thumbnailer_sys_t *sys = new thumbnailer_sys_t();
        auto completionTask = concurrency::create_task(sys->screenshotCompleteEvent, ct);
        size_t len2 = WideCharToMultiByte( CP_UTF8, 0, mrl->Data(), -1, nullptr, 0, nullptr, nullptr );
        char* mrl_str = new char[len2];
        WideCharToMultiByte( CP_UTF8, 0, mrl->Data(), -1, mrl_str, len2, nullptr, nullptr );

        libvlc_media_t* m = libvlc_media_new_location(this->p_instance, mrl_str);
        delete[] mrl_str;

        if (m == nullptr)
        {
            sys->screenshotCompleteEvent.set(nullptr);
            delete sys;
            return completionTask;
        }

        /* Set media to fast with no options */
        libvlc_media_add_option(m, ":no-audio");
        libvlc_media_add_option(m, ":no-spu");
        libvlc_media_add_option(m, ":no-osd");
        libvlc_media_add_option(m, ":avcodec-hw=none");
        libvlc_media_add_option( m, ":aout=none" );

        libvlc_media_player_t* mp = libvlc_media_player_new_from_media(m);
        libvlc_media_release(m);

        if (mp == nullptr)
        {
            sys->screenshotCompleteEvent.set(nullptr);
            delete sys;
            return completionTask;
        }
        sys->eventMgr = libvlc_media_player_event_manager(mp);
        libvlc_event_attach(sys->eventMgr, libvlc_MediaPlayerEncounteredError, &CancelPreparse, sys);
        // Workaround some short and weird samples can reach the end without getting a snapshot generated.
        libvlc_event_attach(sys->eventMgr, libvlc_MediaPlayerEndReached, &CancelPreparse, sys);
        // to know when seeking to the right position is done
        libvlc_event_attach( sys->eventMgr, libvlc_MediaPlayerPositionChanged, &onSeekChanged, sys );

        /* Set the video format and the callbacks. */

        unsigned int pitch = width * PIXEL_SIZE;
        sys->thumbHeight = height;
        sys->thumbWidth = width;
        sys->thumbSize = pitch * sys->thumbHeight;
        sys->thumbData.reset(new char[sys->thumbSize]);
        sys->hLock = CreateEventEx(nullptr, nullptr, CREATE_EVENT_INITIAL_SET, EVENT_MODIFY_STATE);
        sys->mp = mp;

        libvlc_video_set_format(mp, "BGRA", sys->thumbWidth, sys->thumbHeight, pitch);
        libvlc_video_set_callbacks(mp, Lock, Unlock, nullptr, (void*) sys);
        sys->state = THUMB_SEEKING;

        // Create a local representation to allow it to be captured
        auto sce = sys->screenshotCompleteEvent;
        sys->cancellationTask = concurrency::create_task( [sce, timeoutMs] {
            concurrency::wait( timeoutMs );
            if( !concurrency::is_task_cancellation_requested() )
                sce.set( nullptr );
        }, sys->timeoutCts.get_token() );

        libvlc_media_player_play(mp);
        //TODO: Specify position
        libvlc_media_player_set_position(mp, SEEK_POSITION);

        completionTask.then([mp, sys](PreparseResult^ res) {
            if ( res != nullptr )
                res->length = libvlc_media_player_get_length(mp);
            sys->timeoutCts.cancel();
            // event attachment cleanup
            libvlc_event_detach( sys->eventMgr, libvlc_MediaPlayerPositionChanged, &onSeekChanged, sys );
            // We rendered at least one frame, no need to check for EOF anymore
            libvlc_event_detach(sys->eventMgr, libvlc_MediaPlayerEndReached, &CancelPreparse, sys);
            libvlc_event_detach(sys->eventMgr, libvlc_MediaPlayerEncounteredError, &CancelPreparse, sys);
            // We only block the unlock function if state == THUMB_SEEKED. In the worst case, we are rendering
            // a thumbnail now (even if we timed out but the rendering started just before the timeout task triggered)
            // If so, this will probably take a bit more time, but will not block, so we're safe stopping the media player.
            libvlc_media_player_stop(mp);
            libvlc_media_player_release(mp);
            CloseHandle(sys->hLock);
            delete sys;
        });
        return completionTask;
    });
}

Thumbnailer::~Thumbnailer()
{
    libvlc_release(p_instance);
}