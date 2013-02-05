#pragma once
#include <vlc/vlc.h>
#include "VLCD2dImageSource.h"

namespace VLC_Wrapper {
	public ref class Player sealed
	{
	public:
		Player::Player(Windows::UI::Xaml::Media::ImageBrush^ brush);
		void TestMedia();

	private:
		libvlc_instance_t* p_instance;
		static void *Lock(void* opaque, void** planes);
		static void Unlock(void* opaque, void* picture, void** planes);
		static void Display(void* opaque, void* picture);
		
	};
}


