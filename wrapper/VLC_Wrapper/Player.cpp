#include "pch.h"
#include "Player.h"

using namespace VLC_Wrapper;
using namespace Platform;
using namespace Windows::UI::Xaml::Media::Imaging;



Player::Player(void)
{
	libvlc_instance_t *inst;
	libvlc_media_player_t *mp;
	libvlc_media_t *m;

	/* Don't add any invalid options, otherwise it causes LibVLC to fail */
	static const char *argv[] = {
		"-I", "dummy",
		"--no-osd",
		"--verbose=2",
		"--no-video-title-show",
		"--no-stats",
		"--no-drop-late-frames",
	};
	inst = libvlc_new(sizeof(argv) / sizeof(*argv), argv);

	// add a hard-coded http source for videos to play
	m = libvlc_media_new_location(inst, "http://media.ch9.ms/ch9/e869/9d2a7276-5398-4fdd-9639-263a3a08e869/4-103.mp4");
	mp = libvlc_media_player_new_from_media (m);
	libvlc_media_release (m);
	libvlc_media_player_play (mp);
}
