cbuffer Direct2DTransforms : register(b0)
{
	float2x1 sceneToOutputX;
	float2x1 sceneToOutputY;
	float2x1 sceneToInput0X;
	float2x1 sceneToInput0Y;
};

cbuffer constants : register(b1)
{
	// size in pixel of the frame to draw to the screen
	float2 displayedFrameSize;

	// size in pixel of the original frame (native video source resolution)
	float2 originalFrameSize;
};

struct VSOut
{
	float4 clipSpaceOutput  : SV_POSITION;
	float4 sceneSpaceOutput : SCENE_POSITION;
	float4 texelSpaceInput0 : TEXCOORD0;
};

struct VSIn
{
	// Vertices position
	float2 position : MESH_POSITION;
};

VSOut main(VSIn input)
{
	VSOut output;

	// Compute Scene-space output (vertex simply passed-through here). 
	output.sceneSpaceOutput.x = displayedFrameSize.x * input.position.x;
	output.sceneSpaceOutput.y = displayedFrameSize.y * input.position.y;
	output.sceneSpaceOutput.z = 0.0f;
	output.sceneSpaceOutput.w = 1.0f;

	// Generate standard Clip-space output coordinates.
	output.clipSpaceOutput.x =  (output.sceneSpaceOutput.x * sceneToOutputX[0] + sceneToOutputX[1]);
	output.clipSpaceOutput.y = (output.sceneSpaceOutput.y *sceneToOutputY[0] + sceneToOutputY[1]);

	output.clipSpaceOutput.z = output.sceneSpaceOutput.z;
	output.clipSpaceOutput.w = output.sceneSpaceOutput.w;

	// Generate standard Texel-space input coordinates.
	output.texelSpaceInput0.x = (originalFrameSize.x * input.position.x * sceneToInput0X[0]) + sceneToInput0X[1];
	output.texelSpaceInput0.y = (originalFrameSize.y * input.position.y * sceneToInput0Y[0]) + sceneToInput0Y[1];
	output.texelSpaceInput0.z = 1 - input.position.y;
	output.texelSpaceInput0.w = 1;

	return output;
}