

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

texture skyMap;
sampler skySampler = sampler_state
{
	Texture = (skyMap);
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
texture SSAOMap;
sampler SSAOSampler = sampler_state
{
	Texture = (SSAOMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
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

float2 halfPixel;

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

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(colorSampler, input.TexCoord);
	color *= (tex2D(lightSampler, input.TexCoord) + AmbientMag);
	color *= tex2D(SSAOSampler, input.TexCoord);

	float depth = 1 - tex2D(depthSampler, input.TexCoord).r;
	if (depth > 0.999999){
		color = tex2D(skySampler, input.TexCoord);
	}	

    return color;
}

technique RenderScene
{
    pass Pass1
    {
		ZEnable = true;
		ZWriteEnable = true;
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
