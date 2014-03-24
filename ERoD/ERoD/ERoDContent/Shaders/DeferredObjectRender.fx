float4x4 World;
float4x4 View;
float4x4 Projection;

// Color of the object.
float3 Color = 1;

// Distance to the far plane.
float FarPlane;

// Strength of normalmap.
float BumpConstant = 1.0f;

// Diffuse texture enabled.
bool TextureEnabled;


texture DiffuseTexture;
sampler DiffuseSampler = sampler_state
{
	texture = <DiffuseTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

texture BumpMap;
sampler BumpMapSampler = sampler_state
{
	Texture = <BumpMap>;
	AddressU = Wrap;
	AddressV = Wrap;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

texture SpecularMap;
sampler SpecularSampler = sampler_state
{
	Texture = <SpecularMap>;
	AddressU = Wrap;
	AddressV = Wrap;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

//texture GlowMap;
//sampler GlowSampler = sampler_state
//{
//	Texture = <GlowMap>;
//	AddressU = Wrap;
//	AddressV = Wrap;
//	MipFilter = LINEAR;
//	MinFilter = LINEAR;
//	MagFilter = LINEAR;
//};

//texture ReflectionMap;
//sampler ReflectionMap = sampler_state
//{
//	Texture = <ReflectionMap>;
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

    float4 worldPos = mul(input.Position, World);
	float4 viewPos = mul(worldPos, View);
	output.Position = mul(viewPos, Projection); 
		
	output.TexCoord = input.TexCoord;

	output.Depth = viewPos.z;

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

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output = (PixelShaderOutput)0;

	if (TextureEnabled)
	{
		output.Color = tex2D(DiffuseSampler, input.TexCoord) * float4(Color, 1);
	}
	else
	{
		output.Color = float4(Color, 1);
	}

	float3 bumpValue = BumpConstant * tex2D(BumpMapSampler, input.TexCoord);

	float3 bumpNormal = input.Normal + (bumpValue.x * input.Tangent + bumpValue.y * input.Binormal);

	output.Normal.xyz = (normalize(bumpNormal).xyz / 2) + 0.5f;
	output.Normal.a = 1;

	float depth = 1 - (-input.Depth / FarPlane);
	output.Depth = float4(depth, 0, 0, 1);

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
