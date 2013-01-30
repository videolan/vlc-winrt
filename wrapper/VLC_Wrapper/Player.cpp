#include "pch.h"
#include "Player.h"

#include <exception>

using namespace VLC_Wrapper;
using namespace Platform;
using namespace Windows::UI::Xaml::Media::Imaging;

void *Player::Lock(void* opqaue, void** planes){
	//do some stuff
	int k = 234+32;

	return NULL;
}

void Player::Unlock(void* opaque, void* picture, void** planes){
	//do some more stuff
	int i = 5+3;

	return;
}
void Player::Display(void* opaque, void* picture){
	//do even more stuff

	int v= 324+32;
	

	return;
}
Player::Player(void)
{
	/* Don't add any invalid options, otherwise it causes LibVLC to fail */
	static const char *argv[] = {
		"-I", "dummy",
		"--no-osd",
        //"--verbose=2",
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
	unsigned int pitch = width*bitsPerPixel / 8;


	libvlc_video_set_format(mp, "RV32", width, height, pitch);
	libvlc_video_set_callbacks(mp, (libvlc_video_lock_cb)(this->Lock), 
		(libvlc_video_unlock_cb)(this->Unlock), 
		(libvlc_video_display_cb)(this->Display), NULL);
	
	libvlc_media_release (m);
	libvlc_media_player_play (mp);
}


