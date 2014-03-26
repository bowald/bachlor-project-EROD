// Pixel shader that applies a Poisson disc blur filter.
// Samples pixels within a circle. The good thing about this 
// blur filter is that it is dynamic. You can grow/shrink the
// circle however you like to achieve the desired effect.

#define SAMPLE_COUNT 12

uniform extern texture SceneTex;
uniform extern float DiscRadius;
uniform extern float2 TexelSize;
uniform extern float2 Taps[SAMPLE_COUNT];
float2 halfPixel;

sampler2D TextureSampler : register(s0)
{
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MaxAnisotropy = 8;
	MipFilter = POINT;
	MinFilter = POINT;
	MagFilter = POINT;
	MipFilter = POINT;

	AddressU = CLAMP;
	AddressV = CLAMP;
};
struct VertexShaderInput
{
	float3 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = float4(input.Position, 1);
	output.TexCoord = input.TexCoord - halfPixel;

	return output;
}
float4 PoissonDiscBlurPS(float2 texCoord : TEXCOORD0) : COLOR0
{
	texCoord -= halfPixel;
	// Take a sample at the disc’s center
	float4 base = tex2D(TextureSampler, texCoord);
		float4 sampleAccum = base;

		// Take 12 samples in disc
	for (int nTapIndex = 0; nTapIndex < SAMPLE_COUNT; nTapIndex++)
	{
		// Compute new texture coord inside disc
		float2 vTapCoord = texCoord - TexelSize * Taps[nTapIndex] * DiscRadius;

			// Accumulate samples
			sampleAccum += tex2D(TextureSampler, saturate(vTapCoord));
	}

	return sampleAccum * 0.0769f; // Return average, divide by 13
}


technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PoissonDiscBlurPS();
	}
}