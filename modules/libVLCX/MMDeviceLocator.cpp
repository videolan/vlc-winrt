/*****************************************************************************
 * Copyright © 2013 VideoLAN
 *
 * Authors: Kellen Sunderland
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

#include <wrl.h>
#include <wrl/client.h>

#include <dxgi.h>
#include <dxgi1_2.h>
#include <dxgi1_3.h>
#include <d3d11_1.h>
#include <d2d1_2.h>

#include "windows.ui.xaml.media.dxinterop.h"

#include "MMDeviceLocator.h"

#include <tchar.h>

HRESULT MMDeviceLocator::RegisterForWASAPI(Platform::String^ deviceId){
    HRESULT hr = S_OK;
    IActivateAudioInterfaceAsyncOperation *asyncOp;

    hr = ActivateAudioInterfaceAsync(deviceId->Data(), __uuidof(IAudioClient2), nullptr, this, &asyncOp);

    SafeRelease(&asyncOp);

    return hr;
}

HRESULT MMDeviceLocator::ActivateCompleted(IActivateAudioInterfaceAsyncOperation *operation)
{
    HRESULT hr = S_OK;
    HRESULT hrActivateResult = S_OK;
    IUnknown *audioInterface = nullptr;

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
                AudioCategory_Movie, // AudioCategory_BackgroundCapableMedia
                AUDCLNT_STREAMOPTIONS_NONE
			};
            auto res = m_AudioClient->SetClientProperties(&props);
            if (res != S_OK) {
                OutputDebugString(TEXT("Failed to set audio client properties"));
            }
            SetEvent(m_audioClientReady);
        }
    }

    return hr;
}

namespace libVLCX
{
    AudioDeviceHandler::AudioDeviceHandler(Platform::String^ deviceId)
    {
        ComPtr<MMDeviceLocator> audioReg = Make<MMDeviceLocator>();

        audioReg->m_AudioClient = NULL;
        audioReg->m_audioClientReady = CreateEventEx(NULL, TEXT("AudioClientReady"), 0, EVENT_ALL_ACCESS);
        audioReg->RegisterForWASAPI(deviceId);

        void *addr = NULL;
        DWORD res;
        while ((res = WaitForSingleObjectEx(audioReg->m_audioClientReady, 1000, TRUE)) == WAIT_TIMEOUT) {
            OutputDebugStringW(L"Waiting for audio\n");
        }
        CloseHandle(audioReg->m_audioClientReady);
        if (res != WAIT_OBJECT_0) {
            OutputDebugString(TEXT("Failure while waiting for audio client"));
            throw ref new Platform::FailureException("Failure while waiting for audio client");
        }
        m_ptr = audioReg->m_AudioClient;
    }

    Platform::String^ AudioDeviceHandler::audioClient()
    {
        TCHAR buff[32];
        _sntprintf_s(buff, _TRUNCATE, TEXT("%p"), m_ptr.Get());
        return ref new Platform::String(buff);
    }
}