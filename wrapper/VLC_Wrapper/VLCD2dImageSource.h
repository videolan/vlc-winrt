#pragma once

#include "pch.h"
#include "DirectXHelper.h"

namespace VLC_Wrapper {
	public ref class VLCD2dImageSource sealed : Windows::UI::Xaml::Media::Imaging::SurfaceImageSource
	{
	  public:
		VLCD2dImageSource(int pixelWidth, int pixelHeight, bool isOpaque);

		void BeginDraw(Windows::Foundation::Rect updateRect);
		void BeginDraw()    { BeginDraw(Windows::Foundation::Rect(0, 0, (float)m_width, (float)m_height)); }
		void EndDraw();

		void SetDpi(float dpi);

		void Clear(Windows::UI::Color color);
		void FillSolidRect(Windows::UI::Color color, Windows::Foundation::Rect rect);
		void VLCD2dImageSource::DrawFrame(UINT height, UINT width, byte* sourceData, UINT pitch, Windows::Foundation::Rect updateRect);

	private protected:
		void CreateDeviceIndependentResources();
		void CreateDeviceResources();

		Microsoft::WRL::ComPtr<ISurfaceImageSourceNative>   m_sisNative;

		// Direct3D device
		Microsoft::WRL::ComPtr<ID3D11Device>                m_d3dDevice;

		// Direct2D objects
		Microsoft::WRL::ComPtr<ID2D1Device>                 m_d2dDevice;
		Microsoft::WRL::ComPtr<ID2D1DeviceContext>          m_d2dContext;

		int                                                 m_width;
		int                                                 m_height;
		
		ID2D1Bitmap*										d2dbmp;
    };
};

