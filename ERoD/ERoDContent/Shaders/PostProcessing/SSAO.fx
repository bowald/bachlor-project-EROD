//http://www.gamedev.net/page/resources/_/reference/programming/140/lighting-and-shading/a-simple-and-practical-approach-to-ssao-r2753

float2 HalfPixel;
float4x4 View;

//float4x4 ViewProjectionInv;
float4x4 ViewInverse;
float2 Random_size;

float Radius;
float Intensity;
float Scale;
float Bias;

float2 ScreenSize;

// Length of the x and y sides of the far plane in view space from depth.
float2 SidesLengthVS;

// Distance to the far plane.
float FarPlane;


texture NormalMap;
sampler normalSampler = sampler_state
{
	Texture = (NormalMap);
};

texture Random;
sampler randomSampler = sampler_state
{
	Texture = (Random);
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

float3 getPosition(in float2 uv)
{
	// Reconstruct position
	float depth = 1 - tex2D(DepthSampler, uv).r;

	float2 screenPos = uv * 2.0f - 1.0f;

	depth *= FarPlane;

	// Camera View Space
	return float4(float3(SidesLengthVS * screenPos * depth, -depth), 1);
}


float3 getNormal(in float2 uv)
{
	return normalize(tex2D(normalSampler, uv).xyz * 2.0f - 1.0f);
}

float2 getRandom(in float2 uv)
{
	return normalize(tex2D(randomSampler, ScreenSize * uv / Random_size).xy * 2.0f - 1.0f);
}

float doAmbientOcclusion(in float2 tcoord, in float2 uv, in float3 position, in float3 cnorm)
{
	float3 diff = getPosition(tcoord + uv) - position;
	const float3 v = normalize(diff);
	const float d = length(diff)* Scale;
	return max(0.0, dot(cnorm, v) + Bias)*(1.0 / (1.0 + d))* Intensity;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	const float2 vec[4] = { float2(1, 0), float2(-1, 0),
		float2(0, 1), float2(0, -1) };

	input.TexCoord -= HalfPixel;

	float3 position = getPosition(input.TexCoord);
	float3 normal = getNormal(input.TexCoord);
	float2 rand = getRandom(input.TexCoord);

	normal = mul(normal, View);

	float ao = 0.0f;
	float rad = Radius / position.z;

	//**SSAO Calculation**//
	int iterations = 3;

	for (int j = 0; j < iterations; ++j)
	{
		float2 coord1 = reflect(vec[j], rand)*rad;
		float2 coord2 = float2(coord1.x*0.707 - coord1.y*0.707,
		coord1.x*0.707 + coord1.y*0.707);

		ao += doAmbientOcclusion(input.TexCoord, coord1*0.25, position, normal);
		ao += doAmbientOcclusion(input.TexCoord, coord2*0.5, position, normal);
		ao += doAmbientOcclusion(input.TexCoord, coord1*0.75, position, normal);
		ao += doAmbientOcclusion(input.TexCoord, coord2, position, normal);
	}

	ao /= (float)iterations * 4.0;

	return float4(ao, ao, ao, 1);
}

technique SSAO
{
	pass p0
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}