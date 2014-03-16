float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldViewProjection;

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
};

struct PixelShaderOutput
{
	float4 Color : COLOR0;
	float4 Normal : COLOR1;
	float4 Depth : COLOR2;
	float4 SGR : COLOR3;
};

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
	morphFactor = saturate((morphFactor - 0.25) / 0.5);

	output.position = lerp(output.position, input.morphTarget, morphFactor);

	output.texCoord = output.position.xz + HALF2;

	output.worldPos = mul(output.position, World);

	output.position = mul(output.position, WorldViewProjection);

	return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) 
{
	PixelShaderOutput output;

	/// perform some very basic lighting
	output.Normal = tex2D(NormalSampler, input.texCoord);

	/// grab the color used for this level of the source quadtree
	output.Color = tex2D(TextureSampler, input.texCoord);

	output.Depth = float4(1, 1, 1, 1);// 0;
	output.SGR = 0;

	return output;
}

Technique Deferred
{
	pass pass0
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}

Technique Shadow // TODO
{
	pass pass0
	{
		//TODO change
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}