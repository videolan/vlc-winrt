#include "pch.h"
#include "MMDeviceLocator.h"


HRESULT MMDeviceLocator::RegisterForWASAPI(){
	HRESULT hr = S_OK;
	IActivateAudioInterfaceAsyncOperation *asyncOp;

	Platform::String^ id = MediaDevice::GetDefaultAudioRenderId(Windows::Media::Devices::AudioDeviceRole::Default);
	hr = ActivateAudioInterfaceAsync(id->Data(), __uuidof(IAudioClient), nullptr, this, &asyncOp);

	SafeRelease(&asyncOp);

	return hr;
}

HRESULT MMDeviceLocator::ActivateCompleted(IActivateAudioInterfaceAsyncOperation *operation)
{
	HRESULT hr = S_OK;
	HRESULT hrActivateResult = S_OK;
	IUnknown *audioInterface = nullptr;

	hr = operation->GetActivateResult(&hrActivateResult, &audioInterface);
	if (SUCCEEDED(hr) && SUCCEEDED(hrActivateResult))
	{
		audioInterface->QueryInterface(IID_PPV_ARGS(&m_AudioClient));
		if (nullptr == m_AudioClient)
		{
			hr = E_FAIL;
		}
	}

	return hr;
}

