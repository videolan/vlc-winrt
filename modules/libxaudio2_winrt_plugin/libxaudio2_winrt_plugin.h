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
#include "xaudio2.h"
#include "SoundCallbackHandler.h"

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
static int  Start(audio_output_t *p_aout, audio_sample_format_t *__restrict fmt);
static void Stop(audio_output_t *);
static void Play(audio_output_t *, block_t *);
static int  MuteSet(audio_output_t *, bool);
static void Flush(audio_output_t *, bool);
static void Pause(audio_output_t *, bool, mtime_t);
static int  VolumeSet(audio_output_t *p_aout, float volume);
static int  TimeGet(audio_output_t *, mtime_t *);

struct aout_sys_t
{
	IXAudio2* audioEngine;
	IXAudio2MasteringVoice* masteringVoice;
	IXAudio2SourceVoice* sourceVoice;
	int sampleRate;
	int channels;
	SoundCallbackHander* callbackHandler;
	bool isPlaying;

	struct
	{
		float            volume;
		LONG             mb;
		bool             mute;
	} volume;
};

static const uint32_t chans_in[] = {
	SPEAKER_FRONT_LEFT, SPEAKER_FRONT_RIGHT,
	SPEAKER_SIDE_LEFT, SPEAKER_SIDE_RIGHT,
	SPEAKER_BACK_LEFT, SPEAKER_BACK_RIGHT, SPEAKER_BACK_CENTER,
	SPEAKER_FRONT_CENTER, SPEAKER_LOW_FREQUENCY, 0
};

static void vlc_ToWave(WAVEFORMATEXTENSIBLE * __restrict wf,
	audio_sample_format_t *__restrict audio)
{
	switch (audio->i_format)
	{
	case VLC_CODEC_FL64:
		audio->i_format = VLC_CODEC_FL32;
	case VLC_CODEC_FL32:
		wf->SubFormat = KSDATAFORMAT_SUBTYPE_IEEE_FLOAT;
		break;

	case VLC_CODEC_U8:
		audio->i_format = VLC_CODEC_S16N;
	case VLC_CODEC_S16N:
		wf->SubFormat = KSDATAFORMAT_SUBTYPE_PCM;
		break;

	default:
		audio->i_format = VLC_CODEC_FL32;
		wf->SubFormat = KSDATAFORMAT_SUBTYPE_IEEE_FLOAT;
		break;
	}
	aout_FormatPrepare(audio);

	wf->Format.wFormatTag = WAVE_FORMAT_EXTENSIBLE;
	wf->Format.nChannels = audio->i_channels;
	wf->Format.nSamplesPerSec = audio->i_rate;
	wf->Format.nAvgBytesPerSec = audio->i_bytes_per_frame * audio->i_rate;
	wf->Format.nBlockAlign = audio->i_bytes_per_frame;
	wf->Format.wBitsPerSample = audio->i_bitspersample;
	wf->Format.cbSize = sizeof (*wf) - sizeof (wf->Format);

	wf->Samples.wValidBitsPerSample = audio->i_bitspersample;

	wf->dwChannelMask = 0;

	//TODO: step through this.
	for (unsigned i = 0; pi_vlc_chan_order_wg4[i]; i++)
	if (audio->i_physical_channels & pi_vlc_chan_order_wg4[i])
		wf->dwChannelMask |= chans_in[i];
}

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

	/* Setup Callbacks */
	aout->start = Start;
	aout->stop = Stop;
	aout->volume_set = NULL;
	aout->mute_set = MuteSet;
	aout->pause = Pause;
	aout->play = Play;
	aout->flush = Flush;
	//aout->time_get = TimeGet;

	return VLC_SUCCESS;
}

static void Close(vlc_object_t * object){
	audio_output_t *aout = (audio_output_t *) object;
	free(aout->sys);

	return;
}

static int  Start(audio_output_t *p_aout, audio_sample_format_t *__restrict fmt){
	aout_sys_t *asys = p_aout->sys;
	WAVEFORMATEXTENSIBLE wf;
	
	/* Check input format */
	vlc_ToWave(&wf, fmt);

	vlc_fourcc_t format = fmt->i_format;
	asys->sampleRate = fmt->i_rate;
	asys->channels = fmt->i_channels;
	asys->isPlaying = false;
	asys->callbackHandler = new SoundCallbackHander(&asys->isPlaying);

	//TODO: Set physical channels
	//TODO: Set maximum sample rate

	// Initialize XAudio2
	UINT32 flags = 0;
	HRESULT hr = XAudio2Create(&asys->audioEngine, flags);

	hr = asys->audioEngine->CreateMasteringVoice(&asys->masteringVoice,
		asys->channels, asys->sampleRate);

	hr = asys->audioEngine->CreateSourceVoice(
		&asys->sourceVoice,
		(WAVEFORMATEX*)&wf,
		0,
		XAUDIO2_DEFAULT_FREQ_RATIO,
		nullptr,
		nullptr,
		nullptr
		);

	hr = asys->audioEngine->StartEngine();
	
	if (hr == S_OK){
		return VLC_SUCCESS;
	}
	else
	{
		return VLC_EGENERIC;
	}
};
static void Stop(audio_output_t * p_aout){
	aout_sys_t *asys = p_aout->sys;

	// Stop any sounds that are playing
	HRESULT hr = asys->sourceVoice->Stop();
	if (SUCCEEDED(hr))
	{
		hr = asys->sourceVoice->FlushSourceBuffers();
		asys->audioEngine->StopEngine();
	}

	if (asys->sourceVoice != NULL)
	{
		asys->sourceVoice->DestroyVoice();
		asys->sourceVoice = NULL;
	}
	if (asys->masteringVoice != NULL)
	{
		asys->masteringVoice->DestroyVoice();
		asys->masteringVoice = NULL;
	}

	if (asys->audioEngine != NULL)
	{
		asys->audioEngine->Release();
		asys->audioEngine = NULL;
	}

	return;
}
static void Play(audio_output_t * p_aout, block_t * block){
	aout_sys_t *asys = p_aout->sys;

	XAUDIO2_BUFFER playBuffer = { 0 };
	playBuffer.AudioBytes = block->i_buffer;
	playBuffer.pAudioData = block->p_buffer;
	//playBuffer.pAudioData = audioData->Data;
	//playBuffer.Flags = XAUDIO2_END_OF_STREAM;

	HRESULT hr = asys->sourceVoice->SubmitSourceBuffer(&playBuffer);
	if (SUCCEEDED(hr))
	{
		hr = asys->sourceVoice->Start(0, XAUDIO2_COMMIT_NOW);
	}

	//block_Release(block);

	return;
}
static int  MuteSet(audio_output_t *, bool){
	return VLC_SUCCESS;
}
static void Flush(audio_output_t * p_aout, bool wait){
	if (!wait)
	{
		aout_sys_t *asys = p_aout->sys;
		HRESULT hr = asys->sourceVoice->FlushSourceBuffers();
	}
	
	return;
}
static void Pause(audio_output_t *, bool, mtime_t){
	return;
}

//static int TimeGet(audio_output_t * aout, mtime_t * delay)
//{
//	uint32_t read;
//	mtime_t size;
//
//	if (IDirectSoundBuffer_GetCurrentPosition(aout->sys->p_dsbuffer, (LPDWORD) &read, NULL) != DS_OK)
//		return 1;
//
//	read %= DS_BUF_SIZE;
//
//	size = (mtime_t) aout->sys->i_write - (mtime_t) read;
//	if (size < 0)
//		size += DS_BUF_SIZE;
//
//	*delay = (size / aout->sys->i_bytes_per_sample) * CLOCK_FREQ / aout->sys->i_rate;
//	return 0;
//}

static int TimeGet(audio_output_t * p_aout, mtime_t * delay)
{
	aout_sys_t *asys = p_aout->sys;
	uint32_t read;
	mtime_t size;


	XAUDIO2_VOICE_STATE state;
	asys->sourceVoice->GetState(&state);

	/*read %= DS_BUF_SIZE;

	size = (mtime_t) aout->sys->i_write - (mtime_t) read;
	if (size < 0)
		size += DS_BUF_SIZE;

	*delay = (size / aout->sys->i_bytes_per_sample) * CLOCK_FREQ / aout->sys->i_rate;*/

	return 0;
}
