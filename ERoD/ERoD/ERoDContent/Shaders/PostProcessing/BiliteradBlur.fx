
float2 halfPixel;
#define RADIUS  6
#define KERNEL_SIZE (RADIUS * 2 + 1)
float weights[KERNEL_SIZE];
float2 offsets[KERNEL_SIZE];

float blurDepthFalloff;

texture2D ssaoMap;
sampler2D ssaoSampler = sampler_state
{
	Texture = <ssaoMap>;
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
texture depthMap;
sampler depthSampler = sampler_state
{
	Texture = <depthMap>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
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

float4 PS_GaussianBlurTriple(float4 texCoord : TEXCOORD0) : COLOR0
{
	float3 color = 0;

	float depth = tex2D(depthSampler, texCoord.xy).x;

	float s = 0;
	for (int i = 0; i < KERNEL_SIZE; ++i)
	{
		float3 im = tex2D(ssaoSampler, texCoord.zw + offsets[i]);
			float d = tex2D(depthSampler, texCoord.xy + offsets[i]).x;
		float r2 = abs(depth - d) * blurDepthFalloff;
		float g = exp(-r2*r2);
		color += im* weights[i] * g;
		s += g* weights[i];
	}
	color = color / s;
	return float4(color, 1);
}

technique GAUSSTriple
{
	pass p0
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PS_GaussianBlurTriple();
	}
}
