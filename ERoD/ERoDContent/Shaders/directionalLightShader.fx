float4x4 ViewInv;
float4x4 LightView;
float4x4 LightProj;

float4 CascadeDistances;

static const int NUM_SPLITS = 4;

float4x4 ViewMatrices[NUM_SPLITS];
float4x4 ProjectionMatrices[NUM_SPLITS];

//direction of the light
float3 LightDirection;

float3 CameraPosition;

float Power = 1;
float SpecularModifier = 3;

float2 HalfPixel;

float2	ClipPlanes[NUM_SPLITS];

// Vector size of the shadow map in use.
float2 ShadowMapSize;


// Length of the x and y sides of the far plane in view space from depth.
float2 SidesLengthVS;

// Distance to the far plane.
float FarPlane;

float StaticBias = 2.0f;

// color of the light 
float3 LightColor;

// true if the light casts shadows.
bool CastShadow;

texture ColorMap;
sampler ColorSampler = sampler_state
{
	Texture = <ColorMap>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
};

texture NormalMap;
sampler NormalSampler = sampler_state
{
	Texture = <NormalMap>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};

texture SGRMap;
sampler SGRSampler = sampler_state
{
	Texture = (SGRMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
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

texture ShadowMap;
sampler ShadowSampler = sampler_state
{
	Texture = <ShadowMap>;
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

float CalcShadowBias(float4 weights)
{

	float BIAS[NUM_SPLITS];

	for (int i = 1; i <= NUM_SPLITS; i++)
	{
		BIAS[i-1] = StaticBias * ((i / NUM_SPLITS) * (i / NUM_SPLITS));
	}

	float4 biases;
	biases.x = 0.0007f;
	biases.y = 0.0020f;
	biases.z = 0.0054f;
	biases.w = 0.0085f;

	float finalBias = biases.x * weights.x + biases.y * weights.y + biases.z * weights.z + biases.w * weights.w;

	return finalBias;
}

float2 CalcShadowTexCoord(float4 weights, float4 positionWS)
{
	float4x4 lightView = ViewMatrices[0] * weights.x + ViewMatrices[1] * weights.y + ViewMatrices[2] * weights.z + ViewMatrices[3] * weights.w;
	float4x4 lightProj = ProjectionMatrices[0] * weights.x + ProjectionMatrices[1] * weights.y + ProjectionMatrices[2] * weights.z + ProjectionMatrices[3] * weights.w;

	//remember that we need to find the correct cascade into our cascade atlas
	//float offset = weights.y*0.25f + weights.z*0.50f + weights.w * 0.75f;

	float offsetx = weights.y * 0.5f + weights.w * 0.5f;
	float offsety = weights.z * 0.5f + weights.w * 0.5f;

	// Get position in light viewspace and light clip-space.
	float4 positionLightVS = mul(positionWS, lightView);
	float4 positionLightCS = mul(positionLightVS, lightProj);

	// Get the coordinates for sampling the shadowmap.
	float2 shadowTexCoord = 0.5f * positionLightCS.xy / positionLightCS.w + float2(0.5f, 0.5f);
	shadowTexCoord.x = shadowTexCoord.x * 0.5f + offsetx;
	shadowTexCoord.y = (1 - shadowTexCoord.y) * 0.5f + offsety;

	return shadowTexCoord;

}

float CalcLightDepth(float4 weights, float4 positionWS)
{
	float4x4 lightView = ViewMatrices[0] * weights.x + ViewMatrices[1] * weights.y + ViewMatrices[2] * weights.z + ViewMatrices[3] * weights.w;
	float4 positionLightVS = mul(positionWS, lightView);

	// Get the distance from the camera in light viewspace depth
	float lightDepth = -positionLightVS.z / FarPlane;

	return lightDepth;
}

// Calculates the shadow term using PCF soft-shadowing
// sqrtSamples = number of samples, higher number -> softer shadows [1..7]
float CalcShadowTermSoftPCF(float lightDepth, float2 shadowTexCoord, float shadowBias, int sqrtSamples)
{
	float shadowTerm = 0.0f;

	float radius = (sqrtSamples - 1.0f) / 2;
	float weightAccum = 0.0f;

	for (float y = -radius; y <= radius; y++)
	{
		for (float x = -radius; x <= radius; x++)
		{
			float2 offset = 0;
			offset = float2(x, y);
			offset /= ShadowMapSize;
			float2 samplePoint = shadowTexCoord + offset;
			float depth = 1 - tex2D(ShadowSampler, samplePoint).x;
			float sampleVal = (lightDepth <= depth + shadowBias);

			// Edge tap smoothing
			float xWeight = 1;
			float yWeight = 1;

			if (x == -radius)
			{
				xWeight = 1 - frac(shadowTexCoord.x * ShadowMapSize.x);
			}
			else if (x == radius)
			{
				xWeight = frac(shadowTexCoord.x * ShadowMapSize.x);
			}

			if (y == -radius)
			{
				yWeight = 1 - frac(shadowTexCoord.y * ShadowMapSize.y);
			}
			else if (y == radius)
			{
				yWeight = frac(shadowTexCoord.y * ShadowMapSize.y);
			}

			shadowTerm += sampleVal * xWeight * yWeight;
			weightAccum = xWeight * yWeight;
		}
	}

	shadowTerm /= (sqrtSamples * sqrtSamples);
	shadowTerm *= 1.55f;

	return shadowTerm;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Reconstruct position
	float depth = 1 - tex2D(DepthSampler, input.TexCoord).r;

	float2 screenPos = input.TexCoord * 2.0f - 1.0f;

	//if depth value == 1, we can assume its a background value, so skip it
	clip(-depth + 0.9999f);

	depth *= FarPlane;

	// Camera View Space
	float4 positionCVS = float4(float3(SidesLengthVS * screenPos * depth, -depth), 1);
	// World Space
	float4 positionWS = mul(positionCVS, ViewInv);

	// Figure out which split this pixel belongs to, based on view-space depth.
	float4 weights = (positionCVS.z < CascadeDistances);
	weights.xyz -= weights.yzw;

	// Light calculation
	// Get normals
	float4 normalData = tex2D(NormalSampler, input.TexCoord);
	float3 normal = 2.0f * normalData.xyz - 1.0f;

	// surface-to-light vector
	float3 lightVector = normalize(-LightDirection);

	// compute diffuse light
	float NdL = saturate(dot(normal, lightVector));

	// Specular, Glow, Reflection map.
	float4 SGR = tex2D(SGRSampler, input.TexCoord);

	// Emissive
	float4 emissive = SGR.g * tex2D(ColorSampler, input.TexCoord);

	if (NdL - 0.00001f < 0)
	{
		return emissive;
	}

	float shading = 1.0f;

	if (CastShadow)
	{
		float2 shadowTexCoord = CalcShadowTexCoord(weights, positionWS);

		float shadowBias = CalcShadowBias(weights);

		float lightDepth = CalcLightDepth(weights, positionWS);

		float lerpBand = ((weights.x * CascadeDistances.y) + (weights.y * CascadeDistances.z) + (weights.z * CascadeDistances.w));
		
		// Get soft shadows
		shading = CalcShadowTermSoftPCF(lightDepth, shadowTexCoord, shadowBias, 7);

		if (lerpBand * 0.8 > positionCVS.z && lerpBand < positionCVS.z)
		{
			weights.w = weights.z;
			weights.z = weights.y;
			weights.y = weights.x;
			weights.x = 0;
			float shading2 = CalcShadowTermSoftPCF(CalcLightDepth(weights, positionWS), CalcShadowTexCoord(weights, positionWS), CalcShadowBias(weights), 7);
			float shadowDistribution = ((positionCVS.z - 0.8 * lerpBand) / (0.2 * lerpBand));
			shading = lerp(shading, shading2, shadowDistribution);
		}
	}

	
	float3 diffuseLight = (NdL * LightColor.rgb) * Power;


	// reflection vector
	float3 r = normalize(2 * dot(lightVector, normal) * normal - lightVector);

	// view vector
	float3 v = normalize(CameraPosition - positionWS);//Get worldpos from viewpos

	// Calculate specular using phong shading
	float4 specular = SGR.r * float4(LightColor, 1) * max(pow(dot(r, v), 20), 0);

	// Add specular to the Diffuse Light
	diffuseLight += (specular * SpecularModifier * Power);

	return float4(diffuseLight.rgb, 1) * shading + emissive;
	
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
