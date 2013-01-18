#include "pch.h"
#include "VLCD3dImageSource.h"

using namespace Platform;
using namespace Microsoft::WRL;
using namespace Windows::UI;
using namespace VLC_Wrapper;

VLCD3dImageSource::VLCD3dImageSource(int pixelWidth, int pixelHeight, bool isOpaque):
    SurfaceImageSource(pixelWidth, pixelHeight, isOpaque)
{
	m_width = pixelWidth;
	m_height = pixelHeight;

	CreateDeviceIndependentResources();
    CreateDeviceResources();
}


// Initialize resources that are independent of hardware.
void VLCD3dImageSource::CreateDeviceIndependentResources()
{
    // Query for ISurfaceImageSourceNative interface.
	reinterpret_cast<IUnknown*>(this)->QueryInterface(IID_PPV_ARGS(&m_sisNative));
}


// Initialize hardware-dependent resources.
void VLCD3dImageSource::CreateDeviceResources()
{
    // This flag adds support for surfaces with a different color channel ordering
    // than the API default.
    UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT; 

#if defined(_DEBUG)    
    // If the project is in a debug build, enable debugging via SDK Layers.
    creationFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif

    // This array defines the set of DirectX hardware feature levels this app will support.
    // Note the ordering should be preserved.
    // Don't forget to declare your application's minimum required feature level in its
    // description.  All applications are assumed to support 9.1 unless otherwise stated.
    const D3D_FEATURE_LEVEL featureLevels[] =
    {
        D3D_FEATURE_LEVEL_11_1,
        D3D_FEATURE_LEVEL_11_0,
        D3D_FEATURE_LEVEL_10_1,
        D3D_FEATURE_LEVEL_10_0,
        D3D_FEATURE_LEVEL_9_3,
        D3D_FEATURE_LEVEL_9_2,
        D3D_FEATURE_LEVEL_9_1,
    };

    // Create the DX11 API device object, and get a corresponding context.
    D3D11CreateDevice(
        nullptr,                        // Specify nullptr to use the default adapter.
        D3D_DRIVER_TYPE_HARDWARE,
        nullptr,
        creationFlags,                  // Set debug and Direct2D compatibility flags.
        featureLevels,                  // List of feature levels this app can support.
        ARRAYSIZE(featureLevels),
        D3D11_SDK_VERSION,              // Always set this to D3D11_SDK_VERSION for Metro style apps.
        &m_d3dDevice,                   // Returns the Direct3D device created.
        nullptr,
        &m_d3dContext                   // Returns the device immediate context.
        );

    // Get the Direct3D 11.1 API device.
    ComPtr<IDXGIDevice> dxgiDevice;
	m_d3dDevice.As(&dxgiDevice);

    // Associate the DXGI device with the SurfaceImageSource.
	m_sisNative->SetDevice(dxgiDevice.Get());

   

    // Create the constant buffer.
    const CD3D11_BUFFER_DESC constantBufferDesc = CD3D11_BUFFER_DESC(sizeof(ModelViewProjectionConstantBuffer), D3D11_BIND_CONSTANT_BUFFER);
    m_d3dDevice->CreateBuffer(
        &constantBufferDesc,
        nullptr,
        &m_constantBuffer
        );
  

    // Calculate the aspect ratio and field of view.
    float aspectRatio = (float)m_width / (float)m_height;
    
    float fovAngleY = 70.0f * XM_PI / 180.0f;
    if (aspectRatio < 1.0f)
    {
        fovAngleY /= aspectRatio;
    }

    // Set right-handed perspective projection based on aspect ratio and field of view.
    m_constantBufferData.projection = XMMatrixTranspose(
        XMMatrixPerspectiveFovRH(
            fovAngleY,
            aspectRatio,
            0.01f,
            100.0f
            )
        );

    // Start animating at frame 0.
    m_frameCount = 0;
}


// Begins drawing.
void VLCD3dImageSource::BeginDraw()
{
    POINT offset;
    ComPtr<IDXGISurface> surface;

    // Express target area as a native RECT type.
    RECT updateRectNative; 
    updateRectNative.left = 0;
    updateRectNative.top = 0;
    updateRectNative.right = m_width;
    updateRectNative.bottom = m_height;

    // Begin drawing - returns a target surface and an offset to use as the top left origin when drawing.
    HRESULT beginDrawHR = m_sisNative->BeginDraw(updateRectNative, &surface, &offset);

    if (beginDrawHR == DXGI_ERROR_DEVICE_REMOVED || beginDrawHR == DXGI_ERROR_DEVICE_RESET)
    {
        // If the device has been removed or reset, attempt to recreate it and continue drawing.
        CreateDeviceResources();
        BeginDraw();
    }
    else
    {
        // Notify the caller by throwing an exception if any other error was encountered.
        beginDrawHR;
    }

    // QI for target texture from DXGI surface.
    ComPtr<ID3D11Texture2D> d3DTexture;
    surface.As(&d3DTexture);

    // Create render target view.
    m_d3dDevice->CreateRenderTargetView(d3DTexture.Get(), nullptr, &m_renderTargetView);

    // Set viewport to the target area in the surface, taking into account the offset returned by BeginDraw.
    D3D11_VIEWPORT viewport;
    viewport.TopLeftX = static_cast<float>(offset.x);
    viewport.TopLeftY = static_cast<float>(offset.y);
    viewport.Width = static_cast<float>(m_width);
    viewport.Height = static_cast<float>(m_height);
    viewport.MinDepth = D3D11_MIN_DEPTH;
    viewport.MaxDepth = D3D11_MAX_DEPTH;
    m_d3dContext->RSSetViewports(1, &viewport);

    // Create depth/stencil buffer descriptor.
    CD3D11_TEXTURE2D_DESC depthStencilDesc(
        DXGI_FORMAT_D24_UNORM_S8_UINT, 
        m_width,
        m_height,
        1,
        1,
        D3D11_BIND_DEPTH_STENCIL
        );

    // Allocate a 2-D surface as the depth/stencil buffer.
    ComPtr<ID3D11Texture2D> depthStencil;
    m_d3dDevice->CreateTexture2D(&depthStencilDesc, nullptr, &depthStencil);

    // Create depth/stencil view based on depth/stencil buffer.
    const CD3D11_DEPTH_STENCIL_VIEW_DESC depthStencilViewDesc = CD3D11_DEPTH_STENCIL_VIEW_DESC(D3D11_DSV_DIMENSION_TEXTURE2D);    
	
	m_d3dDevice->CreateDepthStencilView(
            depthStencil.Get(),
            &depthStencilViewDesc,
            &m_depthStencilView
            );
}


// Clears the surface with the given color.  This must be called 
// after BeginDraw and before EndDraw for a given update.
void VLCD3dImageSource::Clear(Color color)
{
    // Convert color values.
    const float clearColor[4] = { color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f };
    // Clear render target view with given color.
    m_d3dContext->ClearRenderTargetView(m_renderTargetView.Get(), clearColor);
}

// Ends drawing updates started by a previous BeginDraw call.
void VLCD3dImageSource::EndDraw()
{
	m_sisNative->EndDraw();
}
