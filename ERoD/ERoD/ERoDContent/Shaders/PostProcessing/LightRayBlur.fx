float4x4 World;
float4x4 View;
float4x4 Projection;

float2 HalfPixel;
float2 LightCenter;
float TextureAspectRatio = 0.5625f;
float Spread = 0.005;
float Decay = 0.5f;
float Intensity = 1;

#define SAMPLE_COUNT 40
#define INV_SAMPLE_COUNT 0.025f

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};


struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 TexCoord : TEXCOORD0;

};

texture ShaftBuffer;
sampler2D shaftSampler = sampler_state
{
	Texture = <ShaftBuffer>;
	MipFilter = NONE;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};

VertexShaderOutput VertexShaderBlur(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.Position = input.Position;
	output.TexCoord.xy = input.TexCoord + HalfPixel;
	output.TexCoord.zw = input.TexCoord * 2 - 1;
	return output;
}

float4 PixelShaderBlur(VertexShaderOutput input) : COLOR0
{
	float4 color = 0;

	// Do a radial blur, with the center as the light's center in screen space
	float2 blurDirection = (LightCenter.xy - input.TexCoord.zw);
	// As our screen is not always a square, we should compensate to avoid blurring
	// too much in the shorter dimension
	blurDirection.y *= TextureAspectRatio;

	float2 texCoord = input.TexCoord;
		blurDirection *= (Spread*INV_SAMPLE_COUNT);
	for (int s = 0; s < SAMPLE_COUNT; ++s)
	{
		float weight = 1.0f - s*Decay;
		color += tex2D(shaftSampler, texCoord) * (weight);

		texCoord += blurDirection;
	}

	return float4(color.rgb*(INV_SAMPLE_COUNT* Intensity), color.a);
}

technique Blur	//1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderBlur();
		PixelShader = compile ps_3_0 PixelShaderBlur();
	}
}
