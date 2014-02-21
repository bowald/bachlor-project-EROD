

float AmbientMag = .125;

texture colorMap;
sampler colorSampler = sampler_state
{
	Texture = (colorMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
};

texture lightMap;
sampler lightSampler = sampler_state
{
	Texture = (lightMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
};

float2 halfPixel;

struct VertexShaderInput
{
    float3 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
	float4 ScreenPosition : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = float4(input.Position, 1);
	output.TexCoord = input.TexCoord - halfPixel;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(colorSampler, input.TexCoord);
	color *= (tex2D(lightSampler, input.TexCoord) + AmbientMag);

    return color;
}

technique RenderScene
{
    pass Pass1
    {
		ZEnable = true;
		ZWriteEnable = true;
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
