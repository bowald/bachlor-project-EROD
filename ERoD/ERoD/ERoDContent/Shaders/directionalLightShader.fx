float4x4 ViewInv;
float4x4 LightView;
float4x4 LightProj;

//direction of the light
float3 LightDirection;

float3 CameraPosition;

float Power = 1;
float SpecularModifier = 3;

float2 HalfPixel;

// Vector size of the shadow map in use.
float2 ShadowMapSize;

<<<<<<< HEAD
int shadowMapSize;

texture normalMap;
sampler normalSampler = sampler_state
=======
// Length of the x and y sides of the far plane in view space from depth.
float2 SidesLengthVS;

// Distance to the far plane.
float FarPlane;

// Bias term for shadows.
float BIAS = 0.0015f;

// color of the light 
float3 Color;

// true if the light casts shadows.
bool CastShadow;

texture NormalMap;
sampler NormalSampler = sampler_state
>>>>>>> Develop
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

<<<<<<< HEAD
// Calculates the shadow term using PCF with edge tap smoothing
float CalcShadowTermSoftPCF(float fLightDepth, float2 vTexCoord, int iSqrtSamples)
{
	float fShadowTerm = 0.0f;

	float fRadius = (iSqrtSamples - 1.0f) / 2;
	for (float y = -fRadius; y <= fRadius; y++)
	{
		for (float x = -fRadius; x <= fRadius; x++)
		{
			float2 vOffset = 0;
			vOffset = float2(x/shadowMapSize, y/shadowMapSize);
			float2 vSamplePoint = vTexCoord + vOffset;
			float fDepth = tex2D(shadowSampler, vSamplePoint).r;
			float fSample = (fLightDepth <= fDepth + shadowBias);

			// Edge tap smoothing
			float xWeight = 1;
			float yWeight = 1;

			if (x == -fRadius)
			{
				xWeight = 1 - frac(vTexCoord.x * shadowMapSize);
			}
			else if (x == fRadius)
			{
				xWeight = frac(vTexCoord.x * shadowMapSize);
			}

			if (y == -fRadius)
			{
				yWeight = 1 - frac(vTexCoord.y * shadowMapSize);
			}
			else if (y == fRadius)
			{
				yWeight = frac(vTexCoord.y * shadowMapSize);
			}

			fShadowTerm += fSample * xWeight * yWeight;
		}
	}

	fShadowTerm /= ((iSqrtSamples - 1) * (iSqrtSamples - 1));

	return fShadowTerm;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
=======
float CalcShadowTermPCF(float lightDepth, float2 shadowTexCoord)
>>>>>>> Develop
{
	float shadowTerm = 0.0f;

	// transform to texel space
	float2 shadowMapCoord = ShadowMapSize * shadowTexCoord;

	// Determine the lerp amounts           
	float2 lerps = frac(shadowMapCoord);

	// Read in the 4 samples, doing a depth check for each
	float samples[4];
	samples[0] = (1-tex2D(ShadowSampler, shadowTexCoord).x + BIAS < lightDepth) ? 0.0f : 1.0f;
	samples[1] = (1 - tex2D(ShadowSampler, shadowTexCoord + float2(1.0 / ShadowMapSize.x, 0)).x + BIAS < lightDepth) ? 0.0f : 1.0f;
	samples[2] = (1 - tex2D(ShadowSampler, shadowTexCoord + float2(0, 1.0 / ShadowMapSize.y)).x + BIAS < lightDepth) ? 0.0f : 1.0f;
	samples[3] = (1 - tex2D(ShadowSampler, shadowTexCoord + float2(1.0 / ShadowMapSize.x, 1.0 / ShadowMapSize.y)).x + BIAS < lightDepth) ? 0.0f : 1.0f;

	// lerp between the shadow values to calculate our light amount
	shadowTerm = lerp(lerp(samples[0], samples[1], lerps.x), lerp(samples[2], samples[3], lerps.x), lerps.y);

	return shadowTerm;
}

// Calculates the shadow term using PCF soft-shadowing
// sqrtSamples = number of samples, higher number -> better quality [1..7]
float CalcShadowTermSoftPCF(float lightDepth, float2 shadowTexCoord, int sqrtSamples)
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
			float sampleVal = (lightDepth <= depth + BIAS);

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
	float depth = 1-tex2D(DepthSampler, input.TexCoord).r;

	float2 screenPos = input.TexCoord * 2.0f - 1.0f;

	depth *= FarPlane;

	// Camera View Space
	float4 positionCVS = float4(float3(SidesLengthVS * screenPos * depth, -depth), 1);
	// World Space
	float4 positionWS = mul(positionCVS, ViewInv);

	float shading = 1.0f;

<<<<<<< HEAD
	float shading = 0.5f;// CalcShadowTermSoftPCF(distanceStoredInDepthMap, lightSamplePos, 4);
	if (!castShadow || !shadowCondition)
	{
		shading = 1.0;
	} 
=======
	if (CastShadow)
	{
		// Get position in light viewspace and light clip-space.
		float4 positionLightVS = mul(positionWS, LightView);
		float4 positionLightCS = mul(positionLightVS, LightProj);

		// Get the distance from the camera in light viewspace depth
		float lightDepth = -positionLightVS.z / FarPlane;

		// Get the coordinates for sampling the shadowmap.
		float2 shadowTexCoord = 0.5f * positionLightCS.xy / positionLightCS.w + float2(0.5f, 0.5f);
		shadowTexCoord.y = 1 - shadowTexCoord.y;

		// Get soft shadows
		shading = CalcShadowTermSoftPCF(lightDepth, shadowTexCoord, 7);
	}
>>>>>>> Develop

	// Light calculation
	// Get normals
	float4 normalData = tex2D(NormalSampler, input.TexCoord);
	float3 normal = 2.0f * normalData.xyz - 1.0f;

	// surface-to-light vector
	float3 lightVector = normalize(-LightDirection);

	// compute diffuse light
	float NdL = saturate(dot(normal, lightVector));
	float3 diffuseLight = (NdL * Color.rgb) * Power;

	// Specular, Glow, Reflection map.
	float4 SGR = tex2D(SGRSampler, input.TexCoord);

	// reflection vector
	float3 r = normalize(2 * dot(lightVector, normal) * normal - lightVector);

	// view vector
	float3 v = normalize(CameraPosition - positionWS);//Get worldpos from viewpos

	// Calculate specular using phong shading
	float4 specular = SGR.r * float4(Color, 1) * max(pow(dot(r, v), 20), 0);

	// Add specular to the Diffuse Light
	diffuseLight += (specular * SpecularModifier * Power);

	return float4(diffuseLight.rgb, 1) * shading;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
