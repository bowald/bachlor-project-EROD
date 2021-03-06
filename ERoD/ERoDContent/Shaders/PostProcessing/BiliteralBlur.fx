// Pixel shader applies a one dimensional BiliteralBlur.
// This is used twice by the fullSSAO postprocess, first to
// blur horizontally, and then again to blur vertically.

#define SAMPLE_COUNT 11

float4 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];
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
	output.TexCoord = input.TexCoord - HalfPixel;

	return output;
}

float4 BiliteralBlurPS(VertexShaderOutput input) : COLOR0
{
	input.TexCoord -= HalfPixel;
	float4 c = 0;

	float3 centerNormal = getNormal(input.TexCoord);
	float centerDepth = getDepth(input.TexCoord);
		// Combine a number of weighted image filter taps.
	
	for (int i = 0; i < SAMPLE_COUNT; i++)
	{
		float SampleDepth = getDepth(input.TexCoord + SampleOffsets[i].xy);
		float weight = SampleWeights[i];
		
		float3 SampleNormal = getNormal(saturate(input.TexCoord + SampleOffsets[i].xy));

		if (dot(SampleNormal, centerNormal) < 0.9f || abs(centerDepth - SampleDepth) > 0.007f)
		{
			c += tex2D(TextureSampler, input.TexCoord) * weight;
		}
		else
		{
			c += tex2D(TextureSampler, input.TexCoord + SampleOffsets[i].xy) * weight;
		}
	}
	return c;
}


technique BiliteralBlur
{
	pass P0
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 BiliteralBlurPS();

	}
}