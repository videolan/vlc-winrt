#include "pch.h"
#include "Player.h"

#include <exception>

using namespace VLC_Wrapper;
using namespace Platform;
using namespace Windows::UI::Xaml::Media::Imaging;
using namespace Windows::UI::Core;
using namespace Windows::UI;
using namespace Windows::Foundation;

static HANDLE displayMutex = CreateMutexEx(NULL,NULL,0,MUTEX_ALL_ACCESS);
static byte* pixelData;
static VLCD2dImageSource^ vlcImageSource;
static const UINT frameWidth = 300;
static const UINT frameHeight = 240;
static UINT pitch;
static int pixelBufferSize;

void *Player::Lock(void* opqaue, void** planes){
	WaitForSingleObjectEx(displayMutex, 5000L, true);
	
	*planes = pixelData;


	return NULL;
}

void Player::Unlock(void* opaque, void* picture, void** planes){
	ReleaseMutex(displayMutex);
	return;
}
void Player::Display(void* opaque, void* picture){

	WaitForSingleObjectEx(displayMutex, 5000L, true);
	//do even more stuff
	vlcImageSource->Dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler( []
	{
		vlcImageSource->BeginDraw(Rect(0, 0, (float)frameWidth, (float)frameHeight));
		vlcImageSource->DrawFrame(frameHeight, frameWidth, pixelData, pitch, Rect(0, 0, (float)frameWidth, (float)frameHeight));
		vlcImageSource->EndDraw();
		ReleaseMutex(displayMutex);
	}));

	
	return;
}

Player::Player(Windows::UI::Xaml::Media::ImageBrush^ brush)
{
	OutputDebugStringA("Hello, Player!");
	/* Don't add any invalid options, otherwise it causes LibVLC to fail */
	static const char *argv[] = {
		"-I", "dummy",
		"--no-osd",
		"--verbose=0",
		"--no-video-title-show",
		"--no-stats",
		"--no-drop-late-frames",
		//"--avcodec-fast"
	};

	p_instance = libvlc_new(sizeof(argv) / sizeof(*argv), argv);
	if(!p_instance) {
		throw new std::exception("Could not initialise libvlc!",1);
		return;
	}

	vlcImageSource = ref new VLCD2dImageSource(frameWidth, frameHeight, true);
	brush->ImageSource = vlcImageSource;
}

void Player::TestMedia() {
	libvlc_media_player_t *mp;
	libvlc_media_t *m;

	// add a hard-coded http source for videos to play
	m = libvlc_media_new_location(this->p_instance, "http://localhost/xfactor.mp4");
	mp = libvlc_media_player_new_from_media(m);


	//we're using vmem so format is always RGB
	unsigned int bitsPerPixel = 32; // hard coded for RV32 videos
	pitch = (frameWidth*bitsPerPixel)/8;

	//hard coded pixel data allocation for sample video
	pixelBufferSize = (frameWidth*frameHeight*bitsPerPixel)/8;
	pixelData = new byte[pixelBufferSize];

	libvlc_video_set_format(mp, "RV32", frameWidth, frameHeight, pitch);
	//libvlc_video_set_format_callbacks(
	libvlc_video_set_callbacks(mp, (libvlc_video_lock_cb)(this->Lock),
		(libvlc_video_unlock_cb)(this->Unlock),
		(libvlc_video_display_cb)(this->Display), NULL);

	libvlc_media_release (m);
	libvlc_media_player_play (mp);
}


