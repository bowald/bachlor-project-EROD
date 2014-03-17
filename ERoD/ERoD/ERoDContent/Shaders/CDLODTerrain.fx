float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldViewProjection;

float4x4 LightViewProjection;

float3 EyePosition;

Texture2D HeightMap;
sampler2D HeightSampler = sampler_state
{
	Texture = (HeightMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	Filter = POINT;
};

Texture2D NormalMap;
sampler2D NormalSampler = sampler_state
{
	Texture = (NormalMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	Filter = LINEAR;
};

Texture2D Texture;
sampler2D TextureSampler = sampler_state
{
	Texture = (Texture);
	AddressU = CLAMP;
	AddressV = CLAMP;
	Filter = LINEAR;
};


#define RANGE_START (input.rangeStartEndLevel.x)
#define RANGE_END (input.rangeStartEndLevel.y)
#define MORPH_FACTOR(d) (((d-RANGE_START)/(RANGE_END - RANGE_START)))

#define HALF2 ((float2)0.5)
#define GET_HEIGHT(pos)  (tex2Dlod(HeightSampler, float4(pos.xz + HALF2, 0, 0)).r)

#define WHITE ((float3)1)
#define RED (float3(1,0,0))
#define GREEN (float3(0,1,0))
#define BLUE (float3(0,0,1))
#define YELLOW (float3(1,1,0))

float3 levelColors[] = { BLUE, GREEN, RED, BLUE, GREEN, RED, BLUE, GREEN, RED };

struct VertexShaderInput
{
	float4 position : POSITION0;
	float4 morphTarget : NORMAL0;
	float4x4 instanceMatrix : FOG;
	float3 rangeStartEndLevel : NORMAL1;
	float2 texCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 position : POSITION0;
	float2 texCoord : TEXCOORD0;
	float3 worldPos : NORMAL0;
	int treeLevel : NORMAL1;
	float Depth : TEXCOORD1;
};

struct PixelShaderOutput
{
	float4 Color : COLOR0;
	float4 Normal : COLOR1;
	float4 Depth : COLOR2;
	float4 SGR : COLOR3;
};

struct VertexShaderOutputShadow
{
	float4 Position : POSITION0;
	float Depth : TEXCOORD0;
};

VertexShaderOutputShadow VertexShaderFunctionShadow(VertexShaderInput input)
{
	VertexShaderOutputShadow output;

	float4 position = mul(input.position, transpose(input.instanceMatrix));
	position.y = GET_HEIGHT(position);

	input.morphTarget = mul(input.morphTarget, transpose(input.instanceMatrix));
	input.morphTarget.y = GET_HEIGHT(input.morphTarget);

	float cameraDistance = distance(EyePosition, position);
	float morphFactor = MORPH_FACTOR(cameraDistance);
	morphFactor = saturate((morphFactor - 0.25) / 0.25);

	position = lerp(position, input.morphTarget, morphFactor);

	float4 worldPos = mul(position, World);
	output.Position = mul(worldPos, LightViewProjection);

	output.Depth.x = 1 - (output.Position.z / output.Position.w);

	return output;
}

float4 PixelShaderFunctionShadow(VertexShaderOutputShadow input) : COLOR0
{
	return float4(input.Depth.x, 0, 0, 1);
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.treeLevel = (int)input.rangeStartEndLevel.z;

	output.position = mul(input.position, transpose(input.instanceMatrix));
	output.position.y = GET_HEIGHT(output.position);

	input.morphTarget = mul(input.morphTarget, transpose(input.instanceMatrix));
	input.morphTarget.y = GET_HEIGHT(input.morphTarget);

	float cameraDistance = distance(EyePosition, output.position);
	float morphFactor = MORPH_FACTOR(cameraDistance);
	morphFactor = saturate((morphFactor - 0.25) / 0.25);

	output.position = lerp(output.position, input.morphTarget, morphFactor);

	output.texCoord = output.position.xz + HALF2;

	output.worldPos = mul(output.position, World);

	output.position = mul(output.position, WorldViewProjection);

	output.Depth = 1 - (output.position.z / output.position.w);

	return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) 
{
	PixelShaderOutput output;

	/// perform some very basic lighting
	float4 normal = tex2D(NormalSampler, input.texCoord);
	float y = normal.z;
	float x = normal.x;
	float z = normal.y;

	output.Normal = float4(x, y, z, 1);
	
	/// grab the color used for this level of the source quadtree
	//output.Color = float4(levelColors[input.treeLevel], 1);
	output.Color = tex2D(TextureSampler, input.texCoord);

	output.Depth = float4(input.Depth.x, 0, 0, 1);
	output.SGR = 0;

	return output;
}

Technique Deferred
{
	pass pass0
	{
		CullMode = CCW;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}

Technique Shadow
{
	pass pass0
	{
		VertexShader = compile vs_3_0 VertexShaderFunctionShadow();
		PixelShader = compile ps_3_0 PixelShaderFunctionShadow();
	}
}