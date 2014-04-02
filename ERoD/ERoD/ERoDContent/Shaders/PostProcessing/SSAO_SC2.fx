#define SampleCount 10
float3 SSAOSamplePoints[SampleCount];

//VertexShader globals
float2 HalfPixel;

float4x4 CamWorld;
float4x4 ViewProjection;

//SSAO globals
float OcclusionRadius;
float FullOcclusionThreshold;
float NoOcclusionThreshold;
float OcclusionPower;

//2D position to WorldSpace position globals
float FarPlane;
float2 SidesLengthVS;


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

float OcclusionFunction(float distance, float FullOcclusionThreshold, float NoOcclusionThreshold, float OcclusionPower)
{
	float occlusionEpsilon = 0.002f;
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

float TestOcclusion(float centerDepth, float3 worldPos, float3 SamplePointDelta, float OcclusionRadius, float FullOcclusionThreshold, float NoOcclusionThreshold, float OcclusionPower )
{
	float4 samplePoint = float4(worldPos + OcclusionRadius * SamplePointDelta, 1);
	samplePoint = mul(samplePoint, ViewProjection);

    float2 samplePointUV;
    samplePointUV = samplePoint.xy / samplePoint.w;
	samplePointUV.y = -samplePointUV.y;
	samplePointUV = 0.5 * samplePointUV + 0.5;

    float sampleDepth = 1 - tex2D( DepthSampler, samplePointUV ).r;
	float distance = centerDepth - sampleDepth;
    return OcclusionFunction(distance, FullOcclusionThreshold, NoOcclusionThreshold, OcclusionPower );
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
	float3 WorldPos = mul(float4(ViewPos, 1), CamWorld).xyz;

	half accumulatedBlock = 0.0f;
	for (int i = 0; i < SampleCount; i++)
	{
		float3 samplePointDelta = SSAOSamplePoints[i];
		//float block = TestOcclusion(ViewPos, samplePointDelta, OcclusionRadius, FullOcclusionThreshold, NoOcclusionThreshold, OcclusionPower);
		float block = TestOcclusion((-ViewPos.z / FarPlane), WorldPos, samplePointDelta, 5, 0.52, 15.0, 1.0);
		accumulatedBlock += block;
	}
	accumulatedBlock /= SampleCount;

	float color = 1.0f - accumulatedBlock;
	return float4(color, color, color, 1);
}

technique SSAO
{
    pass p0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PostProcessSSAO();
    }
}
