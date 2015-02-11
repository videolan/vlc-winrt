#include "pch.h"
#include <initguid.h>

#include "I420Effect.h"

#define XML(X) TEXT(#X)

I420Effect::I420Effect() :
m_refCount(1)
{
    m_displayedFrame = D2D1::SizeU(0, 0);
    m_visibleSize = D2D1::SizeU(0, 0);
}

HRESULT I420Effect::UpdateConstants()
{
    m_constants.displayedFrameWidth = m_displayedFrame.width;
    m_constants.displayedFrameHeight = m_displayedFrame.height;

    m_constants.visibleFrameWidth = m_visibleSize.width /*- 1*/;
    m_constants.visibleFrameHeight = m_visibleSize.height /*- 1*/;

    return  m_drawInfo->SetVertexShaderConstantBuffer(reinterpret_cast<BYTE*>(&m_constants), sizeof(m_constants));
}

uint32 I420Effect::GetDisplayedFrameWidth() const		{ return m_displayedFrame.width; }
HRESULT I420Effect::SetDisplayedFrameWidth(uint32 width)	
{
    m_displayedFrame.width = width;
    return S_OK;
}

uint32 I420Effect::GetDisplayedFrameHeight() const		{ return m_displayedFrame.height; }
HRESULT I420Effect::SetDisplayedFrameHeight(uint32 height)	
{
    m_displayedFrame.height = height; 
    return S_OK;
}

uint32 I420Effect::GetVisibleWidth() const		{ return m_visibleSize.width; }
HRESULT I420Effect::SetVisibleWidth(uint32 width)
{
    m_visibleSize.width = width;
    return S_OK;
}

uint32 I420Effect::GetVisibleHeight() const		{ return m_visibleSize.height; }
HRESULT I420Effect::SetVisibleHeight(uint32 height)
{
    m_visibleSize.height = height;
    return S_OK;
}

HRESULT I420Effect::Register(_In_ ID2D1Factory1* pFactory)
{
    // Format Effect metadata in XML as expected
    PCWSTR pszXml =
        XML(
        <?xml version='1.0' ?>
        <Effect>
        <!--System Properties-->
        <Property name='DisplayName' type='string' value='I420' />
        <Property name='Author' type='string' value='VideoLan' />
        <Property name='Category' type='string' value='CODEC' />
        <Property name='Description' type='string' value='Adds a I420 Support' />
        <Inputs>
            <Input name='ySource' />
            <Input name='uSource' />
            <Input name='vSource' />
        </Inputs>
        <Property name='DisplayedFrameWidth' type='uint32' value='0'>
            <Property name='DisplayName' type='string' value='RenderTarget Size X' />
            <Property name='Default' type='uint32' value='0' />
        </Property>
        <Property name='DisplayedFrameHeight' type='uint32' value='0'>
            <Property name='DisplayName' type='string' value='RenderTarget Size Y' />
            <Property name='Default' type='uint32' value='0' />
        </Property>
        <Property name='VisibleFrameWidth' type='uint32' value='0'>
            <Property name='DisplayName' type = 'string' value = 'Visible Width' />
            <Property name='Default' type='uint32' value='0' />
        </Property>
        <Property name='VisibleFrameHeight' type='uint32' value='0'>
            <Property name='DisplayName' type='string' value='Visible Height' />
            <Property name='Default' type='uint32' value='0' />
        </Property>
        </Effect>
        );

    // Create the binding for the differents properties
    const D2D1_PROPERTY_BINDING bindings[] =
    {
        D2D1_VALUE_TYPE_BINDING(L"DisplayedFrameWidth", &SetDisplayedFrameWidth, &GetDisplayedFrameWidth),
        D2D1_VALUE_TYPE_BINDING(L"DisplayedFrameHeight", &SetDisplayedFrameHeight, &GetDisplayedFrameHeight),
        D2D1_VALUE_TYPE_BINDING(L"VisibleFrameWidth", &SetVisibleWidth, &GetVisibleWidth),
        D2D1_VALUE_TYPE_BINDING(L"VisibleFrameHeight", &SetVisibleHeight, &GetVisibleHeight),
    };

    // Register the effect in the factory
    return pFactory->RegisterEffectFromString(
        CLSID_CustomI420Effect,
        pszXml,
        bindings,
        ARRAYSIZE(bindings),
        CreateRippleImpl
        );
}

HRESULT __stdcall I420Effect::CreateRippleImpl(_Outptr_ IUnknown** ppEffectImpl)
{
    // Since the object's refcount is initialized to 1, we don't need to AddRef here.
    *ppEffectImpl = static_cast<ID2D1EffectImpl*>(new (std::nothrow) I420Effect());

    if (*ppEffectImpl == nullptr)
        return E_OUTOFMEMORY;
    return S_OK;
}


IFACEMETHODIMP I420Effect::Initialize(
    _In_ ID2D1EffectContext* pEffectContext,
    _In_ ID2D1TransformGraph* pTransformGraph
    )
{
    HRESULT hr = pEffectContext->LoadPixelShader(GUID_I420PixelShader, I420Effect_ByteCode, ARRAYSIZE(I420Effect_ByteCode));
    if (SUCCEEDED(hr))
    {
        hr = pEffectContext->LoadVertexShader(GUID_I420VertexShader, I420Effect_VS_ByteCode, ARRAYSIZE(I420Effect_VS_ByteCode));

        if (SUCCEEDED(hr))
        {
            // Only generate the vertex buffer if it has not already been initialized.
            pEffectContext->FindVertexBuffer(&GUID_I420VertexShader, &m_vertexBuffer);
            if (m_vertexBuffer == nullptr)
            {
                // Create two triangles to shape the rectangle
                const Vertex mesh[6] = {
                    { 0, 1 },
                    { 0, 0 },
                    { 1, 0 },
                    { 1, 0 },
                    { 1, 1 },
                    { 0, 1 }
                };

                D2D1_VERTEX_BUFFER_PROPERTIES vbProp = { 0 };
                vbProp.byteWidth = sizeof(Vertex) * 6;
                vbProp.data = reinterpret_cast<const BYTE*>(&mesh);
                vbProp.inputCount = 1;
                vbProp.usage = D2D1_VERTEX_USAGE_STATIC;

                // Define the inputLayout
                static const D2D1_INPUT_ELEMENT_DESC vertexLayout[] =
                {
                    { "MESH_POSITION", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 0 },
                };

                D2D1_CUSTOM_VERTEX_BUFFER_PROPERTIES cvbProp = { 0 };
                cvbProp.elementCount = ARRAYSIZE(vertexLayout);
                cvbProp.inputElements = vertexLayout;
                cvbProp.stride = sizeof(Vertex);
                cvbProp.shaderBufferWithInputSignature = I420Effect_VS_ByteCode;
                cvbProp.shaderBufferSize = ARRAYSIZE(I420Effect_VS_ByteCode);

                // The GUID is optional, and is provided here to register the geometry globally.
                // As mentioned above, this avoids duplication if multiple versions of the effect
                // are created.
                hr = pEffectContext->CreateVertexBuffer(
                    &vbProp,
                    &GUID_I420VertexShader,
                    &cvbProp,
                    &m_vertexBuffer
                    );
            }

            // This loads the shader into the Direct2D image effects system and associates it with the GUID passed in.
            // If this method is called more than once (say by other instances of the effect) with the same GUID,
            // the system will simply do nothing, ensuring that only one instance of a shader is stored regardless of how
            // many time it is used.
            if (SUCCEEDED(hr))
            {

                // The graph consists of a single transform. In fact, this class is the transform,
                // reducing the complexity of implementing an effect when all we need to
                // do is use a single pixel shader.
                hr = pTransformGraph->SetSingleTransformNode(this);

            }
        }
    }

    

    return S_OK;
}

IFACEMETHODIMP I420Effect::PrepareForRender(D2D1_CHANGE_TYPE changeType)
{
    return UpdateConstants();
}

// SetGraph is only called when the number of inputs changes. This never happens as we publish this effect
// as a single input effect.
IFACEMETHODIMP I420Effect::SetGraph(_In_ ID2D1TransformGraph* pGraph)
{
    return E_NOTIMPL;
}

// Called to assign a new render info class, which is used to inform D2D on
// how to set the state of the GPU.
IFACEMETHODIMP I420Effect::SetDrawInfo(_In_ ID2D1DrawInfo* pDrawInfo)
{
    HRESULT hr = S_OK;

    m_drawInfo = pDrawInfo;

    D2D1_VERTEX_RANGE range;
    range.startVertex = 0;
    range.vertexCount = 6;

    hr = m_drawInfo->SetVertexProcessing(m_vertexBuffer.Get(), D2D1_VERTEX_OPTIONS_USE_DEPTH_BUFFER, nullptr, &range, &GUID_I420VertexShader);

    hr = m_drawInfo->SetPixelShader(GUID_I420PixelShader);
    
    if (SUCCEEDED(hr))
    {
        // Providing this hint allows D2D to optimize performance when processing large images.
        m_drawInfo->SetInstructionCountHint(sizeof(I420Effect_ByteCode));
    }

    return hr;
}

// Calculates the mapping between the output and input rects. In this case,
// we want to request an expanded region to account for pixels that the ripple
// may need outside of the bounds of the destination.
IFACEMETHODIMP I420Effect::MapOutputRectToInputRects(
    _In_ const D2D1_RECT_L* pOutputRect,
    _Out_writes_(inputRectCount) D2D1_RECT_L* pInputRects,
    UINT32 inputRectCount
    ) const
{
    if (inputRectCount != 3)
        return E_INVALIDARG;

    pInputRects[0] = m_inputRects[0];
    pInputRects[1] = m_inputRects[1];
    pInputRects[2] = m_inputRects[2];
    
    return S_OK;
}

IFACEMETHODIMP I420Effect::MapInputRectsToOutputRect(
    _In_reads_(inputRectCount) CONST D2D1_RECT_L* pInputRects,
    _In_reads_(inputRectCount) CONST D2D1_RECT_L* pInputOpaqueSubRects,
    UINT32 inputRectCount,
    _Out_ D2D1_RECT_L* pOutputRect,
    _Out_ D2D1_RECT_L* pOutputOpaqueSubRect
    )
{
    if (inputRectCount != 3)
        return E_INVALIDARG;
    *pOutputRect = D2D1::RectL(0, 0, m_displayedFrame.width, m_displayedFrame.height);
    memcpy(m_inputRects, pInputRects, sizeof(m_inputRects));
    // Indicate that entire output might contain transparency.
    ZeroMemory(pOutputOpaqueSubRect, sizeof(*pOutputOpaqueSubRect));

    return S_OK;
}

IFACEMETHODIMP I420Effect::MapInvalidRect(
    UINT32 inputIndex,
    D2D1_RECT_L invalidInputRect,
    _Out_ D2D1_RECT_L* pInvalidOutputRect
    ) const
{
    // Indicate that the entire output may be invalid.
    *pInvalidOutputRect = D2D1::RectL(0, 0, m_displayedFrame.width, m_displayedFrame.height);

    return S_OK;
}

IFACEMETHODIMP_(UINT32) I420Effect::GetInputCount() const
{
    return 3;
}

// D2D ensures that that effects are only referenced from one thread at a time.
// To improve performance, we simply increment/decrement our reference count
// rather than use atomic InterlockedIncrement()/InterlockedDecrement() functions.
IFACEMETHODIMP_(ULONG) I420Effect::AddRef()
{
    m_refCount++;
    return m_refCount;
}

IFACEMETHODIMP_(ULONG) I420Effect::Release()
{
    m_refCount--;

    if (m_refCount == 0)
    {
        delete this;
        return 0;
    }
    else
    {
        return m_refCount;
    }
}

// This enables the stack of parent interfaces to be queried. In the instance
// of the Ripple interface, this method simply enables the developer
// to cast a Ripple instance to an ID2D1EffectImpl or IUnknown instance.
IFACEMETHODIMP I420Effect::QueryInterface(
    _In_ REFIID riid,
    _Outptr_ void** ppOutput
    )
{
    *ppOutput = nullptr;
    HRESULT hr = S_OK;

    if (riid == __uuidof(ID2D1EffectImpl))
    {
        *ppOutput = reinterpret_cast<ID2D1EffectImpl*>(this);
    }
    else if (riid == __uuidof(ID2D1DrawTransform))
    {
        *ppOutput = static_cast<ID2D1DrawTransform*>(this);
    }
    else if (riid == __uuidof(ID2D1Transform))
    {
        *ppOutput = static_cast<ID2D1Transform*>(this);
    }
    else if (riid == __uuidof(ID2D1TransformNode))
    {
        *ppOutput = static_cast<ID2D1TransformNode*>(this);
    }
    else if (riid == __uuidof(IUnknown))
    {
        *ppOutput = this;
    }
    else
    {
        hr = E_NOINTERFACE;
    }

    if (*ppOutput != nullptr)
    {
        AddRef();
    }

    return hr;
}
