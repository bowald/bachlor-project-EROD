//------- Constants --------
float4x4 View;
float4x4 Projection;
float4x4 World;
float farPlane;

//------- Texture Samplers --------

Texture Texture;
sampler TextureSampler = sampler_state
{
	texture = <Texture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoords : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float4 Normal : NORMAL0;
	float2 TexCoords : TEXCOORD0;
	float Depth : TEXCOORD1;
};

struct PixelShaderOutput
{
	float4 Color : COLOR0;
	float4 Normal : COLOR1;
	float4 Depth : COLOR2;
	float4 SGR : COLOR3;
};


//------- Technique: Textured --------

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4x4 ViewProjection = mul(View, Projection);
	float4x4 WorldViewProjection = mul(World, ViewProjection);

	output.Position = mul(input.Position, WorldViewProjection);
	
	output.TexCoords = input.TexCoords;

	output.Normal = mul(input.Normal, World);

	float4x4 WorldView = mul(World, View);
	float4 PositionVS = mul(input.Position, WorldView);
	output.Position = mul(PositionVS, Projection);
	output.Depth = PositionVS.z;

	//output.Depth = 1 - (output.Position.z / output.Position.w);

	return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output = (PixelShaderOutput)0;

	output.Color = tex2D(TextureSampler, input.TexCoords);
	
	output.Normal.xyz = (normalize(input.Normal).xyz / 2) + 0.5f;
	output.Normal.a = 1;

	// Negate and divide by distance to far clip (so that depth is in range [0,1])
	float fDepth = -input.Depth / farPlane;
	output.Depth = float4(fDepth, 1.0f, 1.0f, 1.0f);
	
	//output.Depth = float4(input.Depth.x, 0, 0, 1);

	output.SGR.r = 1;//tex2D(SpecularSampler, input.TexCoord);
	output.SGR.g = 0;//tex2D(GlowSampler, input.TexCoord);
	output.SGR.b = 0;// tex2D(ReflectionMap, input.TexCoord);
	output.SGR.w = 0;

	return output;
}

technique Textured
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
