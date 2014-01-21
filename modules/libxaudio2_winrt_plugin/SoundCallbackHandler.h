#include "pch.h"
#include "xaudio2.h"

namespace
{
	//
	// Handler for XAudio source voice callbacks to maintain playing state
	//
	class SoundCallbackHander : public IXAudio2VoiceCallback
	{
	public:
		SoundCallbackHander(bool* isPlayingHolder) :
			m_isPlayingHolder(isPlayingHolder)
		{
		}

		~SoundCallbackHander()
		{
			m_isPlayingHolder = nullptr;
		}

		//
		// Voice callbacks from IXAudio2VoiceCallback
		//
		STDMETHOD_(void, OnVoiceProcessingPassStart) (THIS_ UINT32 bytesRequired);
		STDMETHOD_(void, OnVoiceProcessingPassEnd) (THIS);
		STDMETHOD_(void, OnStreamEnd) (THIS);
		STDMETHOD_(void, OnBufferStart) (THIS_ void* bufferContext);
		STDMETHOD_(void, OnBufferEnd) (THIS_ void* bufferContext);
		STDMETHOD_(void, OnLoopEnd) (THIS_ void* bufferContext);
		STDMETHOD_(void, OnVoiceError) (THIS_ void* bufferContext, HRESULT error);

	private:
		bool* m_isPlayingHolder;
	};

	//
	// Callback handlers, only implement the buffer events for maintaining play state
	//
	void SoundCallbackHander::OnVoiceProcessingPassStart(UINT32 /*bytesRequired*/)
	{
	}
	void SoundCallbackHander::OnVoiceProcessingPassEnd()
	{
	}
	void SoundCallbackHander::OnStreamEnd()
	{
	}
	void SoundCallbackHander::OnBufferStart(void* /*bufferContext*/)
	{
		*m_isPlayingHolder = true;
	}
	void SoundCallbackHander::OnBufferEnd(void* /*bufferContext*/)
	{
		*m_isPlayingHolder = false;
	}
	void SoundCallbackHander::OnLoopEnd(void* /*bufferContext*/)
	{
	}
	void SoundCallbackHander::OnVoiceError(void* /*bufferContext*/, HRESULT /*error*/)
	{
	}
}
