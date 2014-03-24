float4x4 viewInv;
float4x4 lightView;
float4x4 lightProj;

//direction of the light
float3 lightDirection;

float3 cameraPosition;

float power = 1;
float specularModifier = 3;

float2 halfPixel;
float farPlane;
float2 TanAspect;

float BIAS = 0.001f;
//color of the light 
float3 color;

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
	float3 TexCoordAndCornerInfo : TEXCOORD0;
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
	output.TexCoord = input.TexCoordAndCornerInfo.xy - halfPixel;

	return output;
}

float CalcShadowTermPCF(float fLightDepth, float2 vShadowTexCoord)
{
	float fShadowTerm = 0.0f;

	// transform to texel space
	float2 vShadowMapCoord = float2(2048.0f, 2048.0) * vShadowTexCoord;

		// Determine the lerp amounts           
	float2 vLerps = frac(vShadowMapCoord);

	// Read in the 4 samples, doing a depth check for each
	float fSamples[4];
	fSamples[0] = (1-tex2D(shadowSampler, vShadowTexCoord).x + BIAS < fLightDepth) ? 0.0f : 1.0f;
	fSamples[1] = (1-tex2D(shadowSampler, vShadowTexCoord + float2(1.0 / 2048.0, 0)).x + BIAS < fLightDepth) ? 0.0f : 1.0f;
	fSamples[2] = (1-tex2D(shadowSampler, vShadowTexCoord + float2(0, 1.0 / 2048.0)).x + BIAS < fLightDepth) ? 0.0f : 1.0f;
	fSamples[3] = (1-tex2D(shadowSampler, vShadowTexCoord + float2(1.0 / 2048.0, 1.0 / 2048.0)).x + BIAS < fLightDepth) ? 0.0f : 1.0f;

	// lerp between the shadow values to calculate our light amount
	fShadowTerm = lerp(lerp(fSamples[0], fSamples[1], vLerps.x), lerp(fSamples[2], fSamples[3], vLerps.x), vLerps.y);

	return fShadowTerm;
}

// Calculates the shadow term using PCF soft-shadowing
float CalcShadowTermSoftPCF(float fLightDepth, float2 vShadowTexCoord, int iSqrtSamples)
{
	float fShadowTerm = 0.0f;

	float fRadius = (iSqrtSamples - 1.0f) / 2;
	float fWeightAccum = 0.0f;

	for (float y = -fRadius; y <= fRadius; y++)
	{
		for (float x = -fRadius; x <= fRadius; x++)
		{
			float2 vOffset = 0;
				float2 g_vShadowMapSize = float2(2048, 2048);
			vOffset = float2(x, y);
			vOffset /= g_vShadowMapSize;
			float2 vSamplePoint = vShadowTexCoord + vOffset;
			float fDepth = 1 - tex2D(shadowSampler, vSamplePoint).x;
			float fSample = (fLightDepth <= fDepth + BIAS);

			// Edge tap smoothing
			float xWeight = 1;
			float yWeight = 1;

			if (x == -fRadius)
				xWeight = 1 - frac(vShadowTexCoord.x * g_vShadowMapSize.x);
			else if (x == fRadius)
				xWeight = frac(vShadowTexCoord.x * g_vShadowMapSize.x);

			if (y == -fRadius)
				yWeight = 1 - frac(vShadowTexCoord.y * g_vShadowMapSize.y);
			else if (y == fRadius)
				yWeight = frac(vShadowTexCoord.y * g_vShadowMapSize.y);

			fShadowTerm += fSample * xWeight * yWeight;
			fWeightAccum = xWeight * yWeight;
		}
	}

	fShadowTerm /= (iSqrtSamples * iSqrtSamples);
	fShadowTerm *= 1.55f;

	return fShadowTerm;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 normalData = tex2D(normalSampler, input.TexCoord);
	float3 normal = 2.0f * normalData.xyz - 1.0f;

	float depth = 1-tex2D(depthSampler, input.TexCoord).r;

	float2 screenPos = input.TexCoord * 2.0f - 1.0f;

	depth *= farPlane;

	float4 positionCVS = float4(float3(TanAspect * screenPos * depth, -depth), 1);
	float4 positionWS = mul(positionCVS, viewInv);

	float4 positionLightVS = mul(positionWS, lightView);
	float4 positionLightCS = mul(positionLightVS, lightProj);

	//float4 pos = mul(positionVS, mul(viewInv, lightView));

	float lightDepth = -positionLightVS.z / farPlane;

	float2 shadowTexCoord = 0.5f * positionLightCS.xy / positionLightCS.w + float2(0.5f, 0.5f);
	shadowTexCoord.y = 1 - shadowTexCoord.y;
	//shadowTexCoord += (0.5f / float2(2048, 2048));

	//determine shadowing criteria
	//float realDistanceToLight = lightDepth;
	//float distanceStoredInDepthMap = 1 - tex2D(shadowSampler, shadowTexCoord).r;

	// add bias
	//realDistanceToLight -= shadowBias;

	//float shading = (tex2D(shadowSampler, shadowTexCoord).x < 0.1) ? 0.0f : 1.0f;
	float shading = CalcShadowTermSoftPCF(lightDepth, shadowTexCoord, 7);

	/*bool shadowCondition = distanceStoredInDepthMap <= realDistanceToLight;

	float shading = 0.5;
	if (!castShadow || !shadowCondition)
	{
		shading = 1;
	}*/

	//surface-to-light vector
	float3 lightVector = normalize(-lightDirection);

	//compute diffuse light
	float NdL = saturate(dot(normal, lightVector));
	float3 diffuseLight = (NdL * color.rgb) * power;

	float4 SGR = tex2D(sgrSampler, input.TexCoord);

	//reflection vector
	float3 r = normalize(2 * dot(lightVector, normal) * normal - lightVector);

	//view vector
	float3 v = normalize(cameraPosition - positionWS);//Get worldpos from viewpos

	float dotProduct = dot(r, v);
	
	float4 specular = SGR.r * float4(color, 1) * max(pow(dotProduct, 20), 0);

	diffuseLight += (specular * specularModifier * power);

	//output the two lights
	return float4(diffuseLight.rgb, 1) * shading;
	//return float4(1, 1, 1, 1) * shading;
	//return float4(tex2D(shadowSampler, shadowTexCoord).x, 0, 0, 1);
	//return float4(shadowTexCoord.x, 0, 0, 1);
	//return float4(-positionCVS.z/farPlane, 0, 0, 1);
	//return float4(0.5 * positionCVS.x / (TanAspect.x * depth) + 0.5, 0.5 * positionCVS.y / (TanAspect.y * depth) + 0.5, 1 - (-positionCVS.z / (farPlane)), 1);
	//return float4(0.5 * positionWS.x + 0.5, 0,0,1);//positionWS.y, 1 - (-positionWS.z / (farPlane)), 1);
	//return float4(1-lightDepth, 0, 0, 1);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
