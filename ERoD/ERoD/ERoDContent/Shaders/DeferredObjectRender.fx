float4x4 World;
float4x4 wvp : WorldViewProjection;

float3 color = 1;

bool mask = false;

bool textureEnabled;
texture diffuseTexture;
sampler diffuseSampler = sampler_state
{
	texture = <diffuseTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

float bumpConstant = 1.0f;
texture bumpMap;
sampler BumpMapSampler = sampler_state
{
	Texture = <bumpMap>;
	AddressU = Wrap;
	AddressV = Wrap;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

texture specularMap;
sampler SpecularSampler = sampler_state
{
	Texture = <specularMap>;
	AddressU = Wrap;
	AddressV = Wrap;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

//texture glowMap;
//sampler GlowSampler = sampler_state
//{
//	Texture = <glowMap>;
//	AddressU = Wrap;
//	AddressV = Wrap;
//	MipFilter = LINEAR;
//	MinFilter = LINEAR;
//	MagFilter = LINEAR;
//};

//texture reflectionMap;
//sampler ReflectionMap = sampler_state
//{
//	Texture = <reflectionMap>;
//	AddressU = Wrap;
//	AddressV = Wrap;
//	MipFilter = LINEAR;
//	MinFilter = LINEAR;
//	MagFilter = LINEAR;
//};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	float3 Tangent	: TANGENT0;
	float3 Binormal : BINORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float3 Tangent : TANGENT0;
	float3 Binormal : BINORMAL0;
	float2 TexCoord : TEXCOORD0;
	float Depth : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, wvp);

	output.TexCoord = input.TexCoord;

	output.Depth = 1 - (output.Position.z / output.Position.w);

	output.Normal = mul(input.Normal, World);

	output.Tangent = mul(input.Tangent, World);
	output.Binormal = mul(input.Binormal, World);

    return output;
}

struct PixelShaderOutput
{
	float4 Color : COLOR0;
	float4 Normal : COLOR1;
	float4 Depth : COLOR2;
	float4 SGR : COLOR3;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	PixelShaderOutput output = (PixelShaderOutput)0;

	if (textureEnabled)
	{
		output.Color = tex2D(diffuseSampler, input.TexCoord) * float4(color, 1);
	}
	else
	{
		output.Color = float4(color, 1);
	}

	if (mask)
	{
		output.Color.a = 0;
	}
	else 
	{
		output.Color.a = 1;
	}

	float3 bumpValue = bumpConstant * tex2D(BumpMapSampler, input.TexCoord);

	float3 bumpNormal = input.Normal + (bumpValue.x * input.Tangent + bumpValue.y * input.Binormal);

	output.Normal.xyz = (normalize(bumpNormal).xyz / 2) + 0.5f;
	output.Normal.a = 1;

	output.Depth = float4(input.Depth.x, 0, 0, 1);

	output.SGR.r = tex2D(SpecularSampler, input.TexCoord);
	output.SGR.g = 0;//tex2D(GlowSampler, input.TexCoord);
	output.SGR.b = 0;// tex2D(ReflectionMap, input.TexCoord);
	output.SGR.w = 0;

	return output;
}

technique Deferred
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
