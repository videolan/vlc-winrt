#pragma once

#include <wrl.h>
#include <d3d11_2.h>
#include <d2d1_2.h>
#include <d2d1effects_1.h>
#include <dwrite_2.h>
#include <wincodec.h>
#include <agile.h>
#include <math.h>
#include <d2d1effectauthor.h>
#include <d2d1effecthelpers.h>

#include "I420Effect_PS.h"
#include "I420Effect_VS.h"

// {3AB41678-D4BC-4BE1-8A91-07A63DEEFEA1}
DEFINE_GUID(GUID_I420PixelShader,0x3ab41678, 0xd4bc, 0x4be1, 0x8a, 0x91, 0x7, 0xa6, 0x3d, 0xee, 0xfe, 0xa1);
// {D2A2EF51-23D8-41FF-9DF0-1B5D7CC2C3C5}
DEFINE_GUID(GUID_I420VertexShader, 0xd2a2ef51, 0x23d8, 0x41ff, 0x9d, 0xf0, 0x1b, 0x5d, 0x7c, 0xc2, 0xc3, 0xc5);
// {DA637E40-44D4-4617-A3D3-A28344549EEA}
DEFINE_GUID(CLSID_CustomI420Effect, 0xda637e40, 0x44d4, 0x4617, 0xa3, 0xd3, 0xa2, 0x83, 0x44, 0x54, 0x9e, 0xea);


typedef enum I420_PROP
{
    I420_PROP_DISPLAYEDFRAME_WIDTH = 0,
    I420_PROP_DISPLAYEDFRAME_HEIGHT = 1,
    I420_PROP_VISIBLE_WIDTH = 2,
    I420_PROP_VISIBLE_HEIGHT = 3,
};

struct Vertex{
    float x;
    float y;
};

class I420Effect : public ID2D1EffectImpl, public ID2D1DrawTransform
{
public:
    // Declare effect registration methods.
    static HRESULT Register(_In_ ID2D1Factory1* pFactory);
    static HRESULT __stdcall CreateRippleImpl(_Outptr_ IUnknown** ppEffectImpl);

    uint32 GetDisplayedFrameWidth() const;
    HRESULT SetDisplayedFrameWidth(uint32 width);

    uint32 GetDisplayedFrameHeight() const;
    HRESULT SetDisplayedFrameHeight(uint32 height);

    uint32 GetVisibleWidth() const;
    HRESULT SetVisibleWidth(uint32 width);

    uint32 GetVisibleHeight() const;
    HRESULT SetVisibleHeight(uint32 height);

    // Declare ID2D1EffectImpl implementation methods.
    IFACEMETHODIMP Initialize(_In_ ID2D1EffectContext* pContextInternal,_In_ ID2D1TransformGraph* pTransformGraph);
    IFACEMETHODIMP PrepareForRender(D2D1_CHANGE_TYPE changeType);
    IFACEMETHODIMP SetGraph(_In_ ID2D1TransformGraph* pGraph);

    // Declare ID2D1DrawTransform implementation methods.
    IFACEMETHODIMP SetDrawInfo(_In_ ID2D1DrawInfo* pRenderInfo);

    // Declare ID2D1Transform implementation methods.
    IFACEMETHODIMP MapOutputRectToInputRects(
        _In_ const D2D1_RECT_L* pOutputRect,
        _Out_writes_(inputRectCount) D2D1_RECT_L* pInputRects,
        UINT32 inputRectCount
        ) const;

    IFACEMETHODIMP MapInputRectsToOutputRect(
        _In_reads_(inputRectCount) CONST D2D1_RECT_L* pInputRects,
        _In_reads_(inputRectCount) CONST D2D1_RECT_L* pInputOpaqueSubRects,
        UINT32 inputRectCount,
        _Out_ D2D1_RECT_L* pOutputRect,
        _Out_ D2D1_RECT_L* pOutputOpaqueSubRect
        );

    IFACEMETHODIMP MapInvalidRect(
        UINT32 inputIndex,
        D2D1_RECT_L invalidInputRect,
        _Out_ D2D1_RECT_L* pInvalidOutputRect
        ) const;

    // Declare ID2D1TransformNode implementation methods.
    IFACEMETHODIMP_(UINT32) GetInputCount() const;

    // Declare IUnknown implementation methods.
    IFACEMETHODIMP_(ULONG) AddRef();
    IFACEMETHODIMP_(ULONG) Release();
    IFACEMETHODIMP QueryInterface(_In_ REFIID riid, _Outptr_ void** ppOutput);
private:

    I420Effect();
    HRESULT UpdateConstants();

    Microsoft::WRL::ComPtr<ID2D1VertexBuffer>   m_vertexBuffer;
    UINT                                        m_numVertices;
    Microsoft::WRL::ComPtr<ID2D1DrawInfo>       m_drawInfo;

    LONG                                        m_refCount;
    D2D1_SIZE_U									m_displayedFrame;
    D2D1_SIZE_U									m_visibleSize;
    D2D1_RECT_L                                 m_inputRects[3];

    float										m_dpi;

    struct
    {
        uint32 displayedFrameWidth;
        uint32 displayedFrameHeight;
        uint32 visibleFrameWidth;
        uint32 visibleFrameHeight;
    } m_constants;
};