#pragma once

#include <Windows.h>
#include <mfapi.h>
#include <AudioClient.h>
#include <mmdeviceapi.h>
#include <wrl\implements.h>

using namespace Microsoft::WRL;
using namespace Windows::Media::Devices;
using namespace Windows::Storage::Streams;

template <class T> void SafeRelease(T **ppT)
{
	if (*ppT)
	{
		(*ppT)->Release();
		*ppT = NULL;
	}
}

class MMDeviceLocator :
	public RuntimeClass< RuntimeClassFlags< ClassicCom >, FtmBase, IActivateAudioInterfaceCompletionHandler >
{
public:
	STDMETHOD(RegisterForWASAPI)();
	STDMETHOD(ActivateCompleted)(IActivateAudioInterfaceAsyncOperation *operation);
	IAudioClient           *m_AudioClient;
};

