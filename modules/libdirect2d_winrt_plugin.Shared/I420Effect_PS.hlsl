Texture2D yTexture : register(t0);
Texture2D uTexture : register(t1);
Texture2D vTexture : register(t2);

SamplerState ySampler : register(s0);
SamplerState uSampler : register(s1);
SamplerState vSampler : register(s2);

float4 main(
	float4 pos      : SV_POSITION,
	float4 posScene : SCENE_POSITION,
	float4 uv0 : TEXCOORD0
	) : SV_Target
{
	float4 Y = yTexture.Sample(ySampler, uv0) * 255;
	float4 U = uTexture.Sample(uSampler, uv0) * 255;
	float4 V = vTexture.Sample(vSampler, uv0) * 255;

	float4 color = float4(0, 0, 0, 1.0f);

	color.b = clamp(1.164*(Y.r - 16) + 2.018*(U.r - 128), 0, 255) / 255.0f;
	color.g = clamp(1.164*(Y.r - 16) - 0.813*(V.r - 128) - 0.391*(U.r - 128), 0, 255) / 255.0f;
	color.r = clamp(1.164*(Y.r - 16) + 1.596*(V.r - 128), 0, 255) / 255.0f;

	return color;
}