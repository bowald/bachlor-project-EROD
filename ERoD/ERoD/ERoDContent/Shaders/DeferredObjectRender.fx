float4x4 world;
float4x4 wvp : WorldViewProjection;

float4x4 view;
float4x4 proj;

float farPlane;

float3 color = 1;

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

	float4x4 WorldView = mul(world, view);
	float4 PositionVS = mul(input.Position, WorldView);
	output.Position = mul(PositionVS, proj);
	output.Depth = PositionVS.z;

	//output.Depth = 1 - (output.Position.z / output.Position.w);

	output.Normal = mul(input.Normal, world);

	output.Tangent = mul(input.Tangent, world);
	output.Binormal = mul(input.Binormal, world);

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

	float3 bumpValue = bumpConstant * tex2D(BumpMapSampler, input.TexCoord);

	float3 bumpNormal = input.Normal + (bumpValue.x * input.Tangent + bumpValue.y * input.Binormal);

	output.Normal.xyz = (normalize(bumpNormal).xyz / 2) + 0.5f;
	output.Normal.a = 1;

	// Negate and divide by distance to far clip (so that depth is in range [0,1])
	float fDepth = -input.Depth / farPlane;
	output.Depth = float4(fDepth, 1.0f, 1.0f, 1.0f);

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
