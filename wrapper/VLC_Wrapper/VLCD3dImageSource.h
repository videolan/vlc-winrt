#pragma once

#include "pch.h"

namespace VLC_Wrapper
{
	struct ModelViewProjectionConstantBuffer
    {
        DirectX::XMMATRIX model;
        DirectX::XMMATRIX view;
        DirectX::XMMATRIX projection;
    };

	public ref class VLCD3dImageSource sealed : Windows::UI::Xaml::Media::Imaging::SurfaceImageSource
	{
		public:
			VLCD3dImageSource(int pixelWidth, int pixelHeight, bool isOpaque);

			void BeginDraw();
			void EndDraw();

			void Clear(Windows::UI::Color color);
		
		private protected:
			void CreateDeviceIndependentResources();
			void CreateDeviceResources();

			Microsoft::WRL::ComPtr<ISurfaceImageSourceNative>   m_sisNative;

			// Direct3D objects
			Microsoft::WRL::ComPtr<ID3D11Device>                m_d3dDevice;
			Microsoft::WRL::ComPtr<ID3D11DeviceContext>         m_d3dContext;
			Microsoft::WRL::ComPtr<ID3D11RenderTargetView>      m_renderTargetView;
			Microsoft::WRL::ComPtr<ID3D11DepthStencilView>      m_depthStencilView;
			Microsoft::WRL::ComPtr<ID3D11VertexShader>          m_vertexShader;
			Microsoft::WRL::ComPtr<ID3D11PixelShader>           m_pixelShader;
			Microsoft::WRL::ComPtr<ID3D11InputLayout>           m_inputLayout;
			Microsoft::WRL::ComPtr<ID3D11Buffer>                m_vertexBuffer;
			Microsoft::WRL::ComPtr<ID3D11Buffer>                m_indexBuffer;
			Microsoft::WRL::ComPtr<ID3D11Buffer>                m_constantBuffer;
		
			uint32                                              m_indexCount;

			ModelViewProjectionConstantBuffer                   m_constantBufferData;

			int                                                 m_width;
			int                                                 m_height;

			float												m_frameCount;
	};
}


