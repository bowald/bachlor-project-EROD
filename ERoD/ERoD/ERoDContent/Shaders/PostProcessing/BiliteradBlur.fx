// Pixel shader applies a one dimensional gaussian blur filter.
// This is used twice by the bloom postprocess, first to
// blur horizontally, and then again to blur vertically.

#define SAMPLE_COUNT 11

uniform extern float4 SampleOffsets[SAMPLE_COUNT];
uniform extern float SampleWeights[SAMPLE_COUNT];
//uniform extern texture SceneTex;

float2 HalfPixel;

sampler TextureSampler : register(s0);

texture NormalMap;
sampler normalSampler = sampler_state
{
	Texture = (NormalMap);
};
texture DepthMap;
sampler depthSampler = sampler_state
{
	Texture = <DepthMap>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};

float3 getNormal(in float2 uv)
{
	return normalize(tex2D(normalSampler, uv).xyz * 2.0f - 1.0f);
}
float getDepth(in float2 uv){
	return 1 - tex2D(depthSampler, uv).r;
}

float4 BiliteradBlurPS(float2 texCoord : TEXCOORD0) : COLOR0
{
	texCoord -= HalfPixel;
	float4 c = 0;

	float3 centerNormal = getNormal(texCoord);
	float centerDepth = getDepth(texCoord);
		// Combine a number of weighted image filter taps.
	for (int i = 0; i < SAMPLE_COUNT; i++)
	{
		float SampleDepth = getDepth(texCoord + SampleOffsets[i].xy);
		float weight = SampleWeights[i];
		float3 SampleNormal = getNormal(saturate(texCoord + SampleOffsets[i].xy));
		if (dot(SampleNormal, centerNormal) < 0.9f || abs(centerDepth - SampleDepth) > 0.01f){
			weight = 0.0f;
		}	

		c += tex2D(TextureSampler, saturate(texCoord + SampleOffsets[i].xy)) * weight;
	}

	return c;
}


technique BiliteradBlur
{
	pass P0
	{
		PixelShader = compile ps_3_0 BiliteradBlurPS();

	}
}