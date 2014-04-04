#define SAMPLE_COUNT 11

float4 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];
float2 HalfPixel;

sampler TextureSampler : register(s0);

float4 GuassianBlurPS(float2 texCoord : TEXCOORD0) : COLOR0
{
	texCoord -= HalfPixel;
	float4 c = 0;

	for (int i = 0; i < SAMPLE_COUNT; i++)
	{
		float weight = SampleWeights[i];
		c += tex2D(TextureSampler, texCoord + SampleOffsets[i].xy) * weight;
	}
	return c;
}


technique GuassianBlur
{
	pass P0
	{
		PixelShader = compile ps_2_0 GuassianBlurPS();

	}
}