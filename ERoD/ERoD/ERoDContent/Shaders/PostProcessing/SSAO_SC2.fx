#define SampleCount 11

uniform extern float4 SSAOSamplePoints[SampleCount];

//VertexShader globals
float2 HalfPixel;

//SSAO globals
float OcclusionRadious;
float FullOcclusionThreshold;
float NoOcclusionThreshold;
float OcclusionPower;
float2 CameraSize;

//2D position to WorldSpace position globals
float FarPlane;
float2 SidesLengthVS;
float4x4 ViewInverse;

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

float3 get_Position(float2 screenUV)
{
	float depth = 1 - tex2D(DepthSampler, screenUV).r;

	float2 screenPos = screenUV * 2.0f - 1.0f;
	depth *= FarPlane;

	// Camera View Space
	float4 positionCVS = float4(float3(SidesLengthVS * screenPos * depth, -depth), 1.0f);
	
	// World Space
	return mul(positionCVS, ViewInverse);
}

float OcclusionFunction(float distance, float FullOcclusionThreshold, float NoOcclusionThreshold, float OcclusionPower)
{
	float occlusionEpsilon = 0.1f;
	if (distance > occlusionEpsilon)
	{
		float noOcclusionRange = NoOcclusionThreshold - FullOcclusionThreshold;
		if(distance < FullOcclusionThreshold)
		{
			return 1.0f;
		}
		else 
		{
			return max(1.0f - pow( (distance - FullOcclusionThreshold) / noOcclusionRange, OcclusionPower), 0.0f);
		}
	}
	else
	{
		return 0.0f;
	}
}

float TestOcclusion( float3 viewPos, float3 SamplePointDelta, float OcclusionRadius, float FullOcclusionThreshold, float NoOcclusionThreshold, float OcclusionPower )
{
    float3 samplePoint = viewPos + OcclusionRadius * SamplePointDelta;
    float2 samplePointUV;
    samplePointUV = samplePoint.xy / samplePoint.z;
    samplePointUV = samplePointUV / CameraSize / 0.5f;
    samplePointUV = samplePointUV + float2( 1.0f, -1.0f );
    samplePointUV = samplePointUV * float2( 0.5f, -0.5f );
    float sampleDepth = tex2D( DepthSampler, samplePointUV ).r;
    float distance = samplePoint.z - sampleDepth;
    return OcclusionFunction( distance, FullOcclusionThreshold,NoOcclusionThreshold, OcclusionPower );
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

struct PS_INPUT
{
	float2 uv : TEXCOORD0;
	//float2 vPos : VPOS; Used in gems 8
};

struct PS_OUTPUT
{
	float4 color : COLOR0;
};

PS_OUTPUT PostProcessSSAO( PS_INPUT input )
{
	PS_OUTPUT o = (PS_OUTPUT)0;
	o.color.rgb = 1.0f;

	float2 screenUV;
	float ViewPos = get_Position(input.uv); //VertexPos och screenpos => ViewPos i worldspace

	half accumulatedBlock = 0.0f; //Amount of occlusion
	for (int i = 0; i < SampleCount; i++)
	{
		float3 samplePointDelta = SSAOSamplePoints[i];
		float block = TestOcclusion(ViewPos, samplePointDelta, OcclusionRadious, FullOcclusionThreshold, NoOcclusionThreshold, OcclusionPower);
		accumulatedBlock += block;
	}
	accumulatedBlock /= SampleCount;

	o.color = 1 * (1.0f - accumulatedBlock);
	return o;
}

technique SSAO
{
    pass p0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PostProcessSSAO();
    }
}
