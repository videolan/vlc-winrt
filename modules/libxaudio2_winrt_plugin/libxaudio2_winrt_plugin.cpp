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
#include "targetver.h"
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
using namespace Windows::Graphics::Display;
using namespace Windows::ApplicationModel::Core;
using namespace Windows::UI::Core;

#include <vlc_common.h>
#include <vlc_plugin.h>
#include <vlc_aout.h>

/*****************************************************************************
* Module descriptor
*****************************************************************************/
static int  Open(vlc_object_t *);
static void Close(vlc_object_t *);

vlc_module_begin()
set_description(N_("XAudio2 audio output"))
set_shortname(N_("XAudio2"))

set_category(CAT_AUDIO)
set_subcategory(SUBCAT_AUDIO_AOUT)
set_capability("audio output", 60)

set_callbacks(Open, Close)
vlc_module_end()

/*****************************************************************************
* Local prototypes
*****************************************************************************/
struct aout_sys_t
{
	struct
	{
		float            volume;
		LONG             mb;
		bool             mute;
	} volume;
};


/**
* 
*/
static int Open(vlc_object_t *object)
{
	audio_output_t *aout = (audio_output_t *) object;
	aout_sys_t *sys;

	aout->sys = sys = (aout_sys_t*) calloc(1, sizeof(*sys));
	if (!sys)
		return VLC_ENOMEM;

	return VLC_SUCCESS;
}

static void Close(vlc_object_t * object){
	audio_output_t *aout = (audio_output_t *) object;
	free(aout->sys);

	return;
}
