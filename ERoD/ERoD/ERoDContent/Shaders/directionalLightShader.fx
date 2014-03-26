float4x4 viewProjectionInv;
float4x4 lightViewProjection;

//direction of the light
float3 lightDirection;

float3 cameraPosition;

float power = 1;
float specularModifier = 3;

float2 halfPixel;

//color of the light 
float3 color;

int shadowMapSize;

texture normalMap;
sampler normalSampler = sampler_state
{
	Texture = <normalMap>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};

texture sgrMap;
sampler sgrSampler = sampler_state
{
	Texture = (sgrMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
};

texture depthMap;
sampler depthSampler = sampler_state
{
	Texture = <depthMap>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};

float shadowBias = 0.00000055f;
bool castShadow;
texture shadowMap;
sampler shadowSampler = sampler_state
{
	Texture = <shadowMap>;
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
	output.TexCoord = input.TexCoord - halfPixel;

    return output;
}

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
{
	float4 normalData = tex2D(normalSampler, input.TexCoord);
	float3 normal = 2.0f * normalData.xyz - 1.0f;

	float depth = 1 - tex2D(depthSampler, input.TexCoord).r;

	float4 screenPos;
	screenPos.x = input.TexCoord.x * 2.0f - 1.0f;
	screenPos.y = -(input.TexCoord.y * 2.0f - 1.0f);

	screenPos.z = depth;
	screenPos.w = 1.0f;

	float4 worldPos = mul(screenPos, viewProjectionInv);
	worldPos /= worldPos.w;

	float4 lightScreenPos = mul(worldPos, lightViewProjection);
	lightScreenPos /= lightScreenPos.w;

	//find sample position in shadow map
	float2 lightSamplePos;
	lightSamplePos.x = lightScreenPos.x / 2.0f + 0.5f;
	lightSamplePos.y = (-lightScreenPos.y / 2.0f + 0.5f);

	//determine shadowing criteria
	float realDistanceToLight = lightScreenPos.z;
	float distanceStoredInDepthMap = 1 - tex2D(shadowSampler, lightSamplePos).r;

	// add bias
	realDistanceToLight -= shadowBias;

	bool shadowCondition = distanceStoredInDepthMap <= realDistanceToLight;

	float shading = 0.5f;// CalcShadowTermSoftPCF(distanceStoredInDepthMap, lightSamplePos, 4);
	if (!castShadow || !shadowCondition)
	{
		shading = 1.0;
	} 

	//surface-to-light vector
	float3 lightVector = normalize(-lightDirection);

	//compute diffuse light
	float NdL = saturate(dot(normal, lightVector));
	float3 diffuseLight = (NdL * color.rgb) * power;

	float4 SGR = tex2D(sgrSampler, input.TexCoord);

	//reflection vector
	float3 r = normalize(2 * dot(lightVector, normal) * normal - lightVector);

	//view vector
	float3 v = normalize(cameraPosition - worldPos);

	float dotProduct = dot(r, v);
	
	float4 specular = SGR.r * float4(color, 1) * max(pow(dotProduct, 20), 0);

	diffuseLight += (specular * specularModifier * power);

	//output the two lights
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
