float4x4 World;
float4x4 View;
float4x4 Projection;

float Alpha;

float FarPlane;
float2 HalfPixel;

//float ModColor;

texture Diffuse;
sampler TextureSampler = sampler_state
{
	Texture = <Diffuse>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
};

texture DepthMap;
sampler DepthSampler = sampler_state
{
	Texture = <DepthMap>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float4 ScreenPosition : TEXCOORD1;
	float Depth : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.ScreenPosition = output.Position;
	output.TexCoord = input.TexCoord;
	output.Depth = viewPosition.z;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float depth = (-input.Depth / FarPlane);

	input.ScreenPosition.xy /= input.ScreenPosition.w;

	float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
	texCoord += HalfPixel;

	float envDepth = 1 - tex2D(DepthSampler, texCoord).r;

	clip(envDepth - depth);

	float4 color = tex2D(TextureSampler, input.TexCoord);
	return float4(color.rgb, Alpha);
}

technique Particles
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
