/*****************************************************************************
* winstore.cpp : VLC for Windows Store audio output plugin
*****************************************************************************
* Copyright (C) 2012 Rémi Denis-Courmont
* Copyright © 2017 VLC Authors, VideoLAN and VideoLabs
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

#ifdef HAVE_CONFIG_H
# include <config.h>
#endif

#include <ppltasks.h>

#ifdef _MSC_VER /* help visual studio compile vlc headers */
# define inline __inline
# define strdup _strdup
# define strcasecmp _stricmp
# define ssize_t SSIZE_T
# define N_(x) x
# define _(x) x
int poll(struct pollfd *, unsigned, int);
# define restrict __restrict
#endif


#include <initguid.h>

#ifdef _MSC_VER
DEFINE_GUID(IID_IAudioClient, 0x1cb9ad4c, 0xdbfa, 0x4c32, 0xb1, 0x78, 0xc2, 0xf5, 0x68, 0xa7, 0x03, 0xb2);
DEFINE_GUID(IID_ISimpleAudioVolume, 0x87ce5498, 0x68d6, 0x44e5, 0x92, 0x15, 0x6d, 0xa4, 0x7e, 0xf8, 0x83, 0xd8);
#endif
DEFINE_GUID(GUID_VLC_AUD_OUT, 0x4533f59d, 0x59ee, 0x00c6, 0xad, 0xb2, 0xc6, 0x8b, 0x50, 0x1a, 0x66, 0x55);

#include <wrl.h>
#include <wrl/client.h>

#include <vlc_common.h>
#include <vlc_plugin.h>
#include <vlc_aout.h>
#include <vlc_modules.h>
#include <vlc_aout.h>

#include <audiopolicy.h>

#include "mmdevice.h"

#include <tchar.h>

using namespace Microsoft::WRL;

char* default_device = "";

struct aout_sys_t
{
	aout_stream_t *stream; /**< Underlying audio output stream */
	module_t *module;
	char* psz_requested_device;
	IAudioClient *client;
};


Platform::String^
ToPlatformString(const char *str) {
	if (str == NULL)
		return ref new Platform::String();
	size_t len = MultiByteToWideChar(CP_UTF8, 0, str, -1, NULL, 0);
	if (len == 0)
		return nullptr;
	std::unique_ptr<wchar_t[]> w_str(new wchar_t[len]);
	MultiByteToWideChar(CP_UTF8, 0, str, -1, w_str.get(), len);
	return ref new Platform::String(w_str.get());
}

Platform::String^
ToPlatformString(const std::string& str)
{
	return ToPlatformString(str.c_str() == NULL ? "" : str.c_str());
}

#include <AudioClient.h>
#include <mmdeviceapi.h>

using namespace Microsoft::WRL;
using namespace Windows::Media::Devices;

class MMDeviceLocator :
	public RuntimeClass< RuntimeClassFlags< ClassicCom >, FtmBase, IActivateAudioInterfaceCompletionHandler >
{
public:
	MMDeviceLocator();
	STDMETHOD(RegisterForWASAPI)(const char* psz_deviceId);
	STDMETHOD(ActivateCompleted)(IActivateAudioInterfaceAsyncOperation *operation);
	IAudioClient2* WaitForAudioClient();

private:
	IAudioClient2          *m_AudioClient;
	HANDLE                 m_audioClientReady;
};

MMDeviceLocator::MMDeviceLocator()
	: m_AudioClient( nullptr )
{
	m_audioClientReady = CreateEventEx(NULL, TEXT("AudioClientReady"), 0, EVENT_ALL_ACCESS);
}

HRESULT MMDeviceLocator::RegisterForWASAPI(const char* psz_deviceId)
{
	ComPtr<IActivateAudioInterfaceAsyncOperation> asyncOp;

	if (psz_deviceId)
	{
		size_t len = MultiByteToWideChar(CP_UTF8, 0, psz_deviceId, -1, NULL, 0);
		if (len == 0)
			return ERROR_NOT_ENOUGH_MEMORY;
		std::unique_ptr<wchar_t[]> w_str(new wchar_t[len]);
		MultiByteToWideChar(CP_UTF8, 0, psz_deviceId, -1, w_str.get(), len);

		return ActivateAudioInterfaceAsync(w_str.get(), __uuidof(IAudioClient2), nullptr, this, asyncOp.GetAddressOf());
	}
	else
	{
		wchar_t* w_str;
		HRESULT hr = StringFromIID(DEVINTERFACE_AUDIO_RENDER, &w_str);
		if (FAILED(hr))
			return hr;
		return ActivateAudioInterfaceAsync(w_str, __uuidof(IAudioClient2), nullptr, this, asyncOp.GetAddressOf());
	}
}

HRESULT MMDeviceLocator::ActivateCompleted(IActivateAudioInterfaceAsyncOperation *operation)
{
	HRESULT hr = S_OK;
	HRESULT hrActivateResult = S_OK;
	IUnknown *audioInterface = nullptr;

    m_AudioClient = nullptr;
	hr = operation->GetActivateResult(&hrActivateResult, &audioInterface);
	if (SUCCEEDED(hr) && SUCCEEDED(hrActivateResult) && audioInterface != nullptr)
	{
		audioInterface->QueryInterface(IID_PPV_ARGS(&m_AudioClient));
		if (nullptr == m_AudioClient)
		{
			hr = E_FAIL;
		}
		else
		{
			// "BackgroundCapableMedia" does not work in UWP
			AudioClientProperties props = AudioClientProperties{
				sizeof(props),
				FALSE,
				AudioCategory_Movie,
				AUDCLNT_STREAMOPTIONS_NONE
			};
			auto res = m_AudioClient->SetClientProperties(&props);
			if (res != S_OK) {
				OutputDebugString(TEXT("Failed to set audio client properties"));
			}
		}
	}
    SetEvent(m_audioClientReady);
	return hr;
}

IAudioClient2* MMDeviceLocator::WaitForAudioClient()
{
	DWORD res;
	while ((res = WaitForSingleObjectEx(m_audioClientReady, 1000, TRUE)) == WAIT_TIMEOUT) {
		OutputDebugStringW(L"Waiting for audio\n");
	}
	CloseHandle(m_audioClientReady);

	if (res != WAIT_OBJECT_0) {
		OutputDebugString(TEXT("Failure while waiting for audio client"));
		return nullptr;
	}
	return m_AudioClient;
}

void EnterMTA(void)
{
	HRESULT hr = CoInitializeEx(NULL, COINIT_MULTITHREADED);
	if (unlikely(FAILED(hr)))
		abort();
}

void LeaveMTA(void)
{
	CoUninitialize();
}

int vlc_FromHR(audio_output_t *aout, HRESULT hr)
{
	aout_sys_t* sys = aout->sys;
	/* Select the default device (and restart) on unplug */
	if (unlikely(hr == AUDCLNT_E_DEVICE_INVALIDATED ||
		hr == AUDCLNT_E_RESOURCES_INVALIDATED))
	{
		sys->psz_requested_device = default_device;
		sys->client = NULL;
		aout_RestartRequest(aout, AOUT_RESTART_OUTPUT);
	}
	return SUCCEEDED(hr) ? 0 : -1;
}

int VolumeSet(audio_output_t *aout, float vol)
{
	aout_sys_t *sys = aout->sys;
	if (unlikely(sys->client == NULL))
		return VLC_EGENERIC;
	HRESULT hr;
	ISimpleAudioVolume *pc_AudioVolume = NULL;
	float gain = 1.f;

	vol = vol * vol * vol; /* ISimpleAudioVolume is tapered linearly. */

	if (vol > 1.f)
	{
		gain = vol;
		vol = 1.f;
	}

	aout_GainRequest(aout, gain);
	hr = sys->client->GetService(IID_ISimpleAudioVolume, reinterpret_cast<void**>( &pc_AudioVolume ) );
	if (FAILED(hr))
	{
		msg_Err(aout, "cannot get volume service (error 0x%lx)", hr);
		goto done;
	}

	hr = pc_AudioVolume->SetMasterVolume(vol, NULL);
	if (FAILED(hr))
	{
		msg_Err(aout, "cannot set volume (error 0x%lx)", hr);
		goto done;
	}

done:
	pc_AudioVolume->Release();

	return SUCCEEDED(hr) ? 0 : -1;
}

int MuteSet(audio_output_t *aout, bool mute)
{
	aout_sys_t *sys = aout->sys;
	if (unlikely(sys->client == NULL))
		return VLC_EGENERIC;
	HRESULT hr;
	ISimpleAudioVolume *pc_AudioVolume = NULL;

	hr = sys->client->GetService(IID_ISimpleAudioVolume, reinterpret_cast<void**>( &pc_AudioVolume ) );
	if (FAILED(hr))
	{
		msg_Err(aout, "cannot get volume service (error 0x%lx)", hr);
		goto done;
	}

	hr = pc_AudioVolume->SetMute(mute, NULL);
	if (FAILED(hr))
	{
		msg_Err(aout, "cannot set mute (error 0x%lx)", hr);
		goto done;
	}

done:
	pc_AudioVolume->Release();

	return SUCCEEDED(hr) ? 0 : -1;
}

int TimeGet(audio_output_t *aout, mtime_t *restrict delay)
{
	aout_sys_t *sys = aout->sys;
	HRESULT hr;

	EnterMTA();
	hr = aout_stream_TimeGet(sys->stream, delay);
	LeaveMTA();

	return SUCCEEDED(hr) ? 0 : -1;
}

void Play(audio_output_t *aout, block_t *block)
{
	aout_sys_t *sys = aout->sys;

	EnterMTA();
	HRESULT hr = aout_stream_Play(sys->stream, block);
	LeaveMTA();

	vlc_FromHR(aout, hr);
}

void Pause(audio_output_t *aout, bool paused, mtime_t date)
{
	aout_sys_t *sys = aout->sys;
	if (unlikely(sys->client == NULL))
		return;

	EnterMTA();
	HRESULT hr = aout_stream_Pause(sys->stream, paused);
	LeaveMTA();

	(void)date;
	vlc_FromHR(aout, hr);
}

void Flush(audio_output_t *aout, bool wait)
{
	aout_sys_t *sys = aout->sys;
	if (unlikely(sys->client == NULL))
		return;

	EnterMTA();
	HRESULT hr = aout_stream_Flush(sys->stream, wait);
	LeaveMTA();

	vlc_FromHR(aout, hr);
}

HRESULT ActivateDevice(void *opaque, REFIID iid, PROPVARIANT *actparms, void **restrict pv)
{
	IAudioClient *client = (IAudioClient*)opaque;

	if (actparms != NULL || client == NULL)
		return E_INVALIDARG;
	client->AddRef();
	*pv = client;
	return S_OK;
}

int aout_stream_Start(void * func, va_list ap)
{
	aout_stream_start_t start = (aout_stream_start_t)func;

	aout_stream_t *s = va_arg(ap, aout_stream_t *);
	audio_sample_format_t *fmt = va_arg(ap, audio_sample_format_t *);
	HRESULT *hr = va_arg(ap, HRESULT *);

	*hr = start(s, fmt, &GUID_VLC_AUD_OUT);
	return SUCCEEDED(*hr) ? VLC_SUCCESS : VLC_EGENERIC;
}

static void aout_stream_Stop(void *func, va_list ap)
{
	aout_stream_stop_t stop = (aout_stream_stop_t)func;
	aout_stream_t *s = va_arg(ap, aout_stream_t *);

	stop(s);
}

static IAudioClient2* GetAudioClient(const char* deviceId)
{
	ComPtr<MMDeviceLocator> audioReg = Make<MMDeviceLocator>();

	Windows::ApplicationModel::Core::CoreApplication::MainView->CoreWindow->Dispatcher->RunAsync(
		Windows::UI::Core::CoreDispatcherPriority::Normal,
		ref new Windows::UI::Core::DispatchedHandler([=]()
	{
		audioReg->RegisterForWASAPI(deviceId);
	}));
	return audioReg->WaitForAudioClient();
}

int Start(audio_output_t *aout, audio_sample_format_t *restrict fmt)
{
	aout_sys_t *sys = aout->sys;
	HRESULT hr;
	int ret = VLC_SUCCESS;

	aout_stream_t *s = (aout_stream_t*)vlc_object_create(aout, sizeof(*s));

	if (unlikely(s == NULL))
		return -1;

	if (sys->psz_requested_device != nullptr)
    {
		sys->client = GetAudioClient(sys->psz_requested_device != default_device ? sys->psz_requested_device : NULL);
        if (sys->client == NULL)
        {
            vlc_object_release(s);
            return -1;
        }
    }
	s->owner.activate = ActivateDevice;

	EnterMTA();

	for (;;)
	{
		hr = S_OK;
		s->owner.device = sys->client;
		sys->module = vlc_module_load(s, "aout stream", NULL, false, aout_stream_Start, s, fmt, &hr);
		if (hr == AUDCLNT_E_ALREADY_INITIALIZED)
			sys->client = GetAudioClient(sys->psz_requested_device != default_device ? sys->psz_requested_device : NULL);
		else if (hr == AUDCLNT_E_DEVICE_INVALIDATED)
			sys->client = GetAudioClient(NULL);
		else
			break;
		if (sys->client == nullptr || sys->module != nullptr)
			break;
	}
	if (sys->psz_requested_device != default_device)
		free(sys->psz_requested_device);
	sys->psz_requested_device = NULL;

	LeaveMTA();

	if (sys->module == NULL)
	{
		vlc_object_release(s);
		return -1;
	}
	sys->client->AddRef();
	sys->stream = s;
	return 0;
}

void Stop(audio_output_t *aout)
{
	aout_sys_t *sys = aout->sys;

	EnterMTA();
	vlc_module_unload(sys->stream, sys->module, aout_stream_Stop, sys->stream);
	LeaveMTA();
	vlc_object_release(sys->stream);
	sys->stream = NULL;
	sys->client->Release();
}

int DeviceSelect(audio_output_t *aout, const char* psz_device)
{
	aout->sys->psz_requested_device = psz_device ? strdup(psz_device) : default_device;
	aout_RestartRequest(aout, AOUT_RESTART_OUTPUT);
	return VLC_SUCCESS;
}

int Open(vlc_object_t *obj)
{
	audio_output_t *aout = (audio_output_t *)obj;

	aout_sys_t *sys = (aout_sys_t*)malloc(sizeof(*sys));
	if (unlikely(sys == NULL))
		return VLC_ENOMEM;

	aout->sys = sys;
	sys->stream = NULL;
	sys->client = nullptr;
	sys->psz_requested_device = default_device;
	aout->start = Start;
	aout->stop = Stop;
	aout->time_get = TimeGet;
	aout->volume_set = VolumeSet;
	aout->mute_set = MuteSet;
	aout->play = Play;
	aout->pause = Pause;
	aout->flush = Flush;
	aout->device_select = DeviceSelect;

	return VLC_SUCCESS;
}

void Close(vlc_object_t *obj)
{
	audio_output_t *aout = (audio_output_t *)obj;
	aout_sys_t *sys = aout->sys;

	free(sys);
}

vlc_module_begin()
set_shortname("winstore")
set_description("Windows Store audio output")
set_capability("audio output", 0)
set_category(CAT_AUDIO)
set_subcategory(SUBCAT_AUDIO_AOUT)
add_shortcut("winstore")
set_callbacks(Open, Close)
vlc_module_end()