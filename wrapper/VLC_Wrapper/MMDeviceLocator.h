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

#pragma once

#include <AudioClient.h>
#include <mmdeviceapi.h>

using namespace Microsoft::WRL;
using namespace Windows::Media::Devices;

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

