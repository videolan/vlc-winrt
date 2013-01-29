#pragma once
#include "vlc/vlc.h"

namespace VLC_Wrapper {
	public ref class Player sealed
	{
	public:
		Player(void);
		void TestMedia();
	private:
		libvlc_instance_t* p_instance;
	};
}


