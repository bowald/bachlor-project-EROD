#define SampleCount 10
uniform extern float3 SSAOSamplePoints[SampleCount];

//VertexShader globals
float2 HalfPixel;

//SSAO globals
float OcclusionRadious;
float FullOcclusionThreshold;
float NoOcclusionThreshold;
float OcclusionPower;
float2 NoiseScale;

//2D position to WorldSpace position globals
float FarPlane;
float2 SidesLengthVS;

texture NormalMap;
sampler NormalSampler = sampler_state
{
	Texture = (NormalMap);
};

texture NoiseMap;
sampler NoiseSampler = sampler_state
{
	Texture = (NoiseMap);
	AddressU = MIRROR;
	AddressV = MIRROR;
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
	
	// Camera View Space range = [0,1]
	return float3(SidesLengthVS * screenPos * depth, -depth); //Camera range: [0.44,0.74] * [-1,1] [0, Farplane] = [-Farplane, Farplane] och [-Farplane, 0]
	
}

float3 get_Normal(float2 texCoord)
{
	return normalize(tex2D(NormalSampler, texCoord).xyz * 2.0f - 1.0f);
}

float3x3 get_tbn(float2 texCoord, float3 normal){
	//NoiseScale scales vTexcoord to tile the noise texture.
	float3 rvec = tex2D(NoiseSampler, texCoord * NoiseScale).xyz * 2.0 - 1.0;
	float3 tangent = normalize(rvec - normal * dot(rvec, normal));
	float3 bitangent = cross(normal, tangent);
	float3x3 tbn = {bitangent, tangent, normal};
	return tbn;
}

float OcclusionFunction(float distance, float FullOcclusionThreshold, float NoOcclusionThreshold, float OcclusionPower)
{
	float occlusionEpsilon = 0.00005f;
	if (distance > occlusionEpsilon)
	{
		//float noOcclusionRange = NoOcclusionThreshold - FullOcclusionThreshold;
		//if(distance < FullOcclusionThreshold)
		//{
			return 1.0f;
		//}
		//else 
		//{
		//	return max(1.0f - pow( (distance - FullOcclusionThreshold) / noOcclusionRange, OcclusionPower), 0.0f);
		//}
	}
	else
	{
		return 0.0f;
	}
}

float TestOcclusion( float3 viewPos, float3 normal, float3 SamplePointDelta, float3x3 rotateKernel, float OcclusionRadius, float FullOcclusionThreshold, float NoOcclusionThreshold, float OcclusionPower )
{
	float3 RotatedSamplePoint;
	//Rotate Samplepoint to normal of origin
	//float3 RotatedSamplePoint = mul(SamplePointDelta, rotateKernel);
    float3 samplePoint = viewPos + OcclusionRadius * SamplePointDelta;	//Samplepoint range [0, Farplane+ Radius]
	float2 samplePointUV;
	samplePointUV = samplePoint.xy / (-samplePoint.z);
    samplePointUV = samplePointUV / SidesLengthVS;
	samplePointUV = samplePointUV + float2( 1.0f, 1.0f );
    samplePointUV = samplePointUV * float2( 0.5f, 0.5f );

	float sampleDepth = 1 - tex2D( DepthSampler, samplePointUV ).r;
    float distance = (-samplePoint.z / FarPlane) - sampleDepth; //samplePoint.z = -depth [-Farplane, 0] => [0,1]
    return OcclusionFunction( distance, FullOcclusionThreshold, NoOcclusionThreshold, OcclusionPower );
	//return distance * 5000;
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
	float3x3 tangentBitangentNormal = get_tbn(input.TexCoord, ViewNormal);

	half accumulatedBlock = 0.0f; //Amount of occlusion
	for (int i = 0; i < SampleCount; i++)
	{
		float3 samplePointDelta = SSAOSamplePoints[i];
		//float block = TestOcclusion(ViewPos, samplePointDelta, OcclusionRadious, FullOcclusionThreshold, NoOcclusionThreshold, OcclusionPower);
		float block = TestOcclusion(ViewPos, ViewNormal, samplePointDelta, tangentBitangentNormal, 3, 0.3, 5.0, 1.0);
		accumulatedBlock += block;
	}
	accumulatedBlock /= SampleCount;
	
	float color = 1.0f - accumulatedBlock;
	return float4(color,color, color, 1);
}

technique SSAO
{
    pass p0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PostProcessSSAO();
    }
}
