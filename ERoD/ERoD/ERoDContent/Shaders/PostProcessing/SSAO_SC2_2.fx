#define SampleCount 10
float3 SSAOSamplePoints[SampleCount];

float4x4 CamWorld;
float4x4 ViewProjection;

//VertexShader globals
float2 HalfPixel;

//2D position to WorldSpace position globals
float FarPlane;
float2 SidesLengthVS;

texture NormalMap;
sampler NormalSampler = sampler_state
{
	Texture = (NormalMap);
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


float3 get_Position(float2 texCoord)
{
	float depth = 1 - tex2D(DepthSampler, texCoord).r;
	
	float2 screenPos = texCoord * 2.0f - 1.0f;
	depth *= FarPlane;

	return float3(SidesLengthVS * screenPos * depth, -depth);
}


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
	output.TexCoord = input.TexCoord - HalfPixel;

	return output;
}

float4 PostProcessSSAO( VertexShaderOutput input ) : COLOR0
{
	float3 ViewPos = get_Position(input.TexCoord);
	float4 WorldPos = mul(float4(ViewPos, 1), CamWorld);

	float4 randomPos = float4(WorldPos.xyz + float3(2, 2, 2), 1);
	float4 randomPosCS = mul(randomPos, ViewProjection);
	float2 randomUV = randomPosCS.xy / randomPosCS.w;
	randomUV.y = -randomUV.y;
	randomUV = 0.5 * randomUV + 0.5;


	float centerDepth = (-ViewPos.z / FarPlane);
	float compDepth = 1 - tex2D(DepthSampler, randomUV).r;
	float diff = centerDepth - compDepth;

	float3 color = float3(diff*50,0,0);
	//float3 color = float3(randomUV.x, 0, 0);

	return float4(color, 1);
}

technique SSAO
{
    pass p0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PostProcessSSAO();
    }
}
