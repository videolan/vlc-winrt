#include "pch.h"
#include "Player.h"

#include <exception>

using namespace VLC_Wrapper;
using namespace Platform;
using namespace Windows::UI::Xaml::Media::Imaging;
using namespace Windows::UI::Core;
using namespace Windows::Foundation;

static HANDLE displayMutex = CreateMutexEx(NULL,NULL,0,MUTEX_ALL_ACCESS);
static byte* pixelData;
static VLCD2dImageSource^ vlcImageSource;

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
		vlcImageSource->BeginDraw(Windows::Foundation::Rect(0, 0, (float)480, (float)270));
		vlcImageSource->Clear(Windows::UI::Colors::HotPink);
		vlcImageSource->EndDraw();
	}));

	ReleaseMutex(displayMutex);
	return;
}

Player::Player(Windows::UI::Xaml::Media::ImageBrush^ brush, int height, int width)
{
	OutputDebugStringA("Hello, Player!");
	/* Don't add any invalid options, otherwise it causes LibVLC to fail */
	static const char *argv[] = {
		"-I", "dummy",
		"--no-osd",
		"--verbose=2",
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

	vlcImageSource = ref new VLCD2dImageSource(width, height, false);
	brush->ImageSource = vlcImageSource;
}

void Player::TestMedia() {
	libvlc_media_player_t *mp;
	libvlc_media_t *m;

	// add a hard-coded http source for videos to play
	m = libvlc_media_new_location(this->p_instance, "http://media.ch9.ms/ch9/e869/9d2a7276-5398-4fdd-9639-263a3a08e869/4-103.mp4");
	mp = libvlc_media_player_new_from_media(m);


	//we're using vmem so format is always RGB
	unsigned int width = 480;
	unsigned int height = 270;
	unsigned int bitsPerPixel = 32; // hard coded for RV32 videos
	unsigned int bytesPerPixel = bitsPerPixel/8;
	unsigned int pitch = width*bytesPerPixel;

	//hard coded pixel data allocation for sample video
	pixelData = new byte[width*height*bytesPerPixel];

	libvlc_video_set_format(mp, "RV32", width, height, pitch);
	libvlc_video_set_callbacks(mp, (libvlc_video_lock_cb)(this->Lock),
		(libvlc_video_unlock_cb)(this->Unlock),
		(libvlc_video_display_cb)(this->Display), NULL);

	libvlc_media_release (m);
	libvlc_media_player_play (mp);
}


