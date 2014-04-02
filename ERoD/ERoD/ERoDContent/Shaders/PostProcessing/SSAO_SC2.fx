#define SampleCount 10
uniform extern float3 SSAOSamplePoints[SampleCount];

//VertexShader globals
float2 HalfPixel;

//SSAO globals
float OcclusionRadious;
float FullOcclusionThreshold;
float NoOcclusionThreshold;
float OcclusionPower;

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
	//Texcoord range: [0,1]
	float depth = 1 - tex2D(DepthSampler, texCoord).r; //Depth range: [0,1]
	
	float2 screenPos = texCoord * 2.0f - 1.0f;	//Screenpos range: [-1,1]
	depth *= FarPlane;	//Depth range: [0,7000] [0,Farplane]

	//S.
	// Camera View Space range = [0,1]
	return float3(SidesLengthVS * screenPos * depth, -depth); //Camera range: [0.44,0.74] * [-1,1] [0, Farplane] = [-Farplane, Farplane] och [-Farplane, 0]
	
}

float3 get_Normal(float2 texCoord)
{
	return normalize(tex2D(NormalSampler, texCoord).xyz * 2.0f - 1.0f);
}

//float3x3 get_tbn(float2 texCoord, float3 normal){
//}

float OcclusionFunction(float distance, float FullOcclusionThreshold, float NoOcclusionThreshold, float OcclusionPower)
{
	float occlusionEpsilon = 0.0001f;
	if (distance > occlusionEpsilon)
	{
		return 1.0f;
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
    float3 samplePoint = viewPos + OcclusionRadius * SamplePointDelta;	//Samplepoint range [0, Farplane+ Radius]
    float2 samplePointUV;
    samplePointUV = samplePoint.xy / samplePoint.z;			// [-1, 1]
    samplePointUV = samplePointUV / SidesLengthVS;			// [-1, 1]
    samplePointUV = samplePointUV + float2( -1.0f, -1.0f );	// [-2, 0]
    samplePointUV = samplePointUV * float2( -0.5f, -0.5f ); // [-1,0]
    float sampleDepth = 1 - tex2D( DepthSampler, samplePointUV ).r;	//[0,1]
    float distance = (-samplePoint.z / FarPlane) - sampleDepth; //samplePoint.z = -depth [-Farplane, 0] => [0,1]
    return OcclusionFunction( distance, FullOcclusionThreshold, NoOcclusionThreshold, OcclusionPower );
	//return (-samplePoint.z / FarPlane);
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
	float3 ViewPos = get_Position(input.TexCoord); //VertexPos och screenpos => ViewPos i worldspace
	float3 ViewNormal = get_Normal(input.TexCoord);
	//float3x3 tangentBitangentNormal = get_tbn(input.TexCoord, ViewNormal);

	half accumulatedBlock = 0.0f; //Amount of occlusion
	for (int i = 0; i < SampleCount; i++)
	{
		float3 samplePointDelta = SSAOSamplePoints[i];
		//float block = TestOcclusion(ViewPos, samplePointDelta, OcclusionRadious, FullOcclusionThreshold, NoOcclusionThreshold, OcclusionPower);
		float block = TestOcclusion(ViewPos, samplePointDelta, 2, 0.52, 15.0, 1.0);
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
