#include "pch.h"
#include "VLCD2dImageSource.h"
#include <wincodec.h>
//#include <atlbase.h>

using namespace VLC_Wrapper;
using namespace Platform;
using namespace Microsoft::WRL;


VLCD2dImageSource::VLCD2dImageSource(int pixelWidth, int pixelHeight, bool isOpaque) :
    SurfaceImageSource(pixelWidth, pixelHeight, isOpaque)
{
    m_width = pixelWidth;
    m_height = pixelHeight;

    CreateDeviceIndependentResources();
    CreateDeviceResources();
}

// Initialize resources that are independent of hardware.
void VLCD2dImageSource::CreateDeviceIndependentResources()
{
    // Query for ISurfaceImageSourceNative interface.
    reinterpret_cast<IUnknown*>(this)->QueryInterface(IID_PPV_ARGS(&m_sisNative));


	HRESULT hr = CoCreateInstance(
		CLSID_WICImagingFactory,
		NULL,
		CLSCTX_INPROC_SERVER,
		IID_IWICImagingFactory,
		(LPVOID*)&m_WicFactory
		);
}

// Initialize hardware-dependent resources.
void VLCD2dImageSource::CreateDeviceResources()
{
    // This flag adds support for surfaces with a different color channel ordering
    // than the API default. It is required for compatibility with Direct2D.
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

    // Create the Direct3D 11 API device object.
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
        nullptr
        );

    // Get the Direct3D 11.1 API device.
    ComPtr<IDXGIDevice> dxgiDevice;
    m_d3dDevice.As(&dxgiDevice);

    // Create the Direct2D device object and a corresponding context.
    D2D1CreateDevice(
        dxgiDevice.Get(),
        nullptr,
        &m_d2dDevice
        );

    
    m_d2dDevice->CreateDeviceContext(
        D2D1_DEVICE_CONTEXT_OPTIONS_NONE,
        &m_d2dContext
        );

    // Set DPI to the display's current DPI.
    SetDpi(Windows::Graphics::Display::DisplayProperties::LogicalDpi);

    // Associate the DXGI device with the SurfaceImageSource.
   
    m_sisNative->SetDevice(dxgiDevice.Get());
}

//Only works for 32 bits per pixel at the moment
void VLCD2dImageSource::DrawFrame(UINT height, UINT width, byte* sourceData, UINT pitch, Windows::Foundation::Rect updateRect){

		D2D1_BITMAP_PROPERTIES props;
		D2D1_PIXEL_FORMAT pixFormat;
		D2D1_SIZE_U size;
		ID2D1Bitmap* d2dbmp;

		float dpi = Windows::Graphics::Display::DisplayProperties::LogicalDpi;
		size.height = height;
		size.width = width;

		pixFormat.alphaMode = D2D1_ALPHA_MODE_IGNORE;

		//pixFormat.format = DXGI_FORMAT_420_OPAQUE;
		pixFormat.format = DXGI_FORMAT_B8G8R8A8_UNORM;

		props.pixelFormat = pixFormat;
		props.dpiX = dpi;
		props.dpiY = dpi;

		m_d2dContext->CreateBitmap(size,sourceData, pitch, props, &d2dbmp);

		m_d2dContext->DrawBitmap(d2dbmp,D2D1::RectF(static_cast<LONG>(updateRect.Left),
													static_cast<LONG>(updateRect.Top),
													static_cast<LONG>(updateRect.Right),
													static_cast<LONG>(updateRect.Bottom)));
}

// Sets the current DPI.
void VLCD2dImageSource::SetDpi(float dpi)
{
    // Update Direct2D's stored DPI.
    m_d2dContext->SetDpi(dpi, dpi);
}

// Begins drawing, allowing updates to content in the specified area.
void VLCD2dImageSource::BeginDraw(Windows::Foundation::Rect updateRect)
{    
    POINT offset;
    ComPtr<IDXGISurface> surface;

    // Express target area as a native RECT type.
    RECT updateRectNative; 
    updateRectNative.left = static_cast<LONG>(updateRect.Left);
    updateRectNative.top = static_cast<LONG>(updateRect.Top);
    updateRectNative.right = static_cast<LONG>(updateRect.Right);
    updateRectNative.bottom = static_cast<LONG>(updateRect.Bottom);

    // Begin drawing - returns a target surface and an offset to use as the top left origin when drawing.
    HRESULT beginDrawHR = m_sisNative->BeginDraw(updateRectNative, &surface, &offset);
 
    if (beginDrawHR == DXGI_ERROR_DEVICE_REMOVED || beginDrawHR == DXGI_ERROR_DEVICE_RESET)
    {
        // If the device has been removed or reset, attempt to recreate it and continue drawing.
        CreateDeviceResources();
        BeginDraw(updateRect);
    }
    else
    {
        // Notify the caller by throwing an exception if any other error was encountered.
		DX::ThrowIfFailed(beginDrawHR);
    }

    // Create render target.
    ComPtr<ID2D1Bitmap1> bitmap;
    
    m_d2dContext->CreateBitmapFromDxgiSurface(
        surface.Get(),
        nullptr,
        &bitmap
        );
    
    // Set context's render target.
    m_d2dContext->SetTarget(bitmap.Get());

    // Begin drawing using D2D context.
    m_d2dContext->BeginDraw();

    // Apply a clip and transform to constrain updates to the target update area.
    // This is required to ensure coordinates within the target surface remain
    // consistent by taking into account the offset returned by BeginDraw, and
    // can also improve performance by optimizing the area that is drawn by D2D.
    // Apps should always account for the offset output parameter returned by 
    // BeginDraw, since it may not match the passed updateRect input parameter's location.
    m_d2dContext->PushAxisAlignedClip(
        D2D1::RectF(
            static_cast<float>(offset.x),  
            static_cast<float>(offset.y),  
            static_cast<float>(offset.x + updateRect.Width),
            static_cast<float>(offset.y + updateRect.Height)  
            ),  
        D2D1_ANTIALIAS_MODE_ALIASED  
        );

    m_d2dContext->SetTransform(
        D2D1::Matrix3x2F::Translation(
            static_cast<float>(offset.x),
            static_cast<float>(offset.y)
            )
        );
}

// Ends drawing updates started by a previous BeginDraw call.
void VLCD2dImageSource::EndDraw()
{
    // Remove the transform and clip applied in BeginDraw since
    // the target area can change on every update.
    m_d2dContext->SetTransform(D2D1::IdentityMatrix());
    m_d2dContext->PopAxisAlignedClip();

    // Remove the render target and end drawing.
    m_d2dContext->EndDraw();
    m_d2dContext->SetTarget(nullptr);
    m_sisNative->EndDraw();
}

// Clears the background with the given color.
void VLCD2dImageSource::Clear(Windows::UI::Color color)
{
    m_d2dContext->Clear(DX::ConvertToColorF(color));
}

// Draws a filled rectangle with the given color and position.
void VLCD2dImageSource::FillSolidRect(Windows::UI::Color color, Windows::Foundation::Rect rect)
{
    // Create a solid color D2D brush.
    ComPtr<ID2D1SolidColorBrush> brush;
    m_d2dContext->CreateSolidColorBrush(
        DX::ConvertToColorF(color),
        &brush
        );

    // Draw a filled rectangle.
    m_d2dContext->FillRectangle(DX::ConvertToRectF(rect), brush.Get());
}