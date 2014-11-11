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
    if (SUCCEEDED(hr) && SUCCEEDED(hrActivateResult) && audioInterface != nullptr)
    {
        audioInterface->QueryInterface(IID_PPV_ARGS(&m_AudioClient));
        if (nullptr == m_AudioClient)
        {
            hr = E_FAIL;
        }
        else
        {
            SetEvent(m_audioClientReady);
        }
    }

    return hr;
}

