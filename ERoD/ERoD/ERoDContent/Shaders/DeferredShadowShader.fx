// Global Variables
float4x4 world;
float4x4 vp : ViewProjection;

float4x4	g_matWorld;
float4x4	g_matWorldIT;
float4x4	g_matViewProj;

float4x4	g_matInvView;
float		g_fFarClip;

float3		cameraPosition;
float4x4	cameraTransform;

static const int NUM_SPLITS = 3;

float4x4	g_matLightViewProj[NUM_SPLITS];
float2		g_vClipPlanes[NUM_SPLITS];
float2		g_vShadowMapSize;
float2		g_vOcclusionTextureSize;

float2		halfPixel;
float2		ShadowMapPixelSize;
float2		ShadowMapSize;

float3		FrustumCorners[4];

//direction of the light
float3		lightDirection;

bool		g_bShowSplitColors = false;

//Cascade shadow maps parameters
float4x4	MatLightViewProj[NUM_SPLITS];
float2		ClipPlanes[NUM_SPLITS];
float3		CascadeDistances;

float shadowBias = 0.00065f;

texture DepthTexture;
sampler2D DepthTextureSampler = sampler_state
{
	Texture = <DepthTexture>;
	MinFilter = point;
	MagFilter = point;
	MipFilter = none;
};

texture ShadowMap;
sampler2D ShadowMapSampler = sampler_state
{
	Texture = <ShadowMap>;
	MinFilter = point;
	MagFilter = point;
	MipFilter = none;
};

float3 GetFrustumRay(in float2 texCoord)
{
	float index = texCoord.x + (texCoord.y * 2);
	return FrustumCorners[index];
}

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Extras : POSITION1;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 Depth : TEXCOORD0;
};

// Vertex shader for outputting light-space depth to the shadow map
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	// Figure out the position of the vertex in view space and clip space
	float4x4 matWorldViewProj = mul(world, vp);
	output.Position = mul(input.Position, matWorldViewProj);
	output.Depth = output.Position.zw;

	return output;
}

// Pixel shader for outputting light-space depth to the shadow map
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Negate and divide by distance to far clip (so that depth is in range [0,1])
	float fDepth = input.Depth.x / input.Depth.y;

	return float4(fDepth, 1, 1, 1);
}

//VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
//{
//	VertexShaderOutput output;
//
//	float4 worldPos = mul(input.Position, World);
//	output.Position = mul(worldPos, vp);
//	output.Depth.x = output.Position.z / output.Position.w;
//
//	return output;
//}
//
//float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
//{
//	return float4(input.Depth.x, 0, 0, 1);
//}

struct VertexShaderInputSO
{
	float3 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutputSO
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 FrustumRay : TEXCOORD1;
};

VertexShaderOutputSO VertexShaderFunctionSO(VertexShaderInputSO input)
{
	VertexShaderOutputSO output;

	output.Position = float4(input.Position, 1);
	output.TexCoord = input.TexCoord - halfPixel;
	output.FrustumRay = GetFrustumRay(input.TexCoord);

	return output;
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
			vOffset = float2(x, y);
			vOffset /= g_vShadowMapSize;
			float2 vSamplePoint = vShadowTexCoord + vOffset;
				float fDepth = tex2D(ShadowMapSampler, vSamplePoint).x;
			float fSample = (fLightDepth <= fDepth + shadowBias);

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

// Pixel shader for computing the shadow occlusion factor
float4 ShadowTermPS(VertexShaderOutputSO input) : COLOR0
{

	// If we want the WorldPosition, we have to multiply by the world camera matrix
	float depthValue = tex2D(DepthTextureSampler, input.TexCoord).r;

	//if depth value == 1, we can assume its a background value, so skip it
	/*clip(-depthValue + 0.99999999999999f);*/

	float3 pos = input.FrustumRay * depthValue;

	//surface-to-light vector
	float3 lightVector = normalize(-lightDirection);

	float spec = 0;
	float4 finalColor = 0;
	float3 shadowTerm = 0;

	// Skip points with no light
	/*clip(nl - 0.00001f);*/

	{
		// Figure out which split this pixel belongs to, based on view-space depth.
		float3 weights = (pos.z < CascadeDistances);
		weights.xy -= weights.yz;


		float4x4 lightViewProj = MatLightViewProj[0] * weights.x + MatLightViewProj[1] * weights.y + MatLightViewProj[2] * weights.z;

		//remember that we need to find the correct cascade into our cascade atlas
		float fOffset = weights.y*0.33333f + weights.z*0.666666f;


		// Find the position of this pixel in light space
		float4 lightingPosition = mul(mul(float4(pos, 1), cameraTransform), lightViewProj);

		// Find the position in the shadow map for this pixel
		float2 shadowTexCoord = 0.5 * lightingPosition.xy / lightingPosition.w + float2(0.5, 0.5);

		shadowTexCoord.x = shadowTexCoord.x *0.3333333f + fOffset;
		shadowTexCoord.y = 1.0f - shadowTexCoord.y;
		shadowTexCoord += ShadowMapPixelSize;

		// Calculate the current pixel depth
		// The bias is used to prevent floating point errors that occur when
		// the pixel of the occluder is being drawn
		float ourDepth = (lightingPosition.z / lightingPosition.w) - shadowBias;


		//the pixel can be outside of shadow distance, so skip it in that case
		if (false)//ClipPlanes[2].y > pos.z)
		{
			shadowTerm = 0;
		}
		else
		{
			shadowTerm = CalcShadowTermSoftPCF(ourDepth, shadowTexCoord, 5/*iFilterSize*/);
		}
	}



	//// Reconstruct view-space position from the depth buffer
	//float fPixelDepth = tex2D(DepthTextureSampler, input.TexCoord).r;
	//float4 vPositionVS = float4(fPixelDepth * input.FrustumCornerVS, 1.0f);

	//// Figure out which split this pixel belongs to, based on view-space depth.
	//float4x4 matLightViewProj = g_matLightViewProj[0];
	//float fOffset = 0;

	//float3 vSplitColors[4];
	//vSplitColors[0] = float3(1, 0, 0);
	//vSplitColors[1] = float3(0, 1, 0);
	//vSplitColors[2] = float3(0, 0, 1);
	//vSplitColors[3] = float3(1, 1, 0);
	//float3 vColor = vSplitColors[0];
	//int iCurrentSplit = 0;

	//// Unrolling the loop allows for a performance boost on the 360
	//for (int i = 1; i < NUM_SPLITS; i++)
	//{
	//	if (vPositionVS.z <= g_vClipPlanes[i].x && vPositionVS.z > g_vClipPlanes[i].y)
	//	{
	//		matLightViewProj = g_matLightViewProj[i];
	//		fOffset = i / (float)NUM_SPLITS;
	//		vColor = vSplitColors[i];
	//		iCurrentSplit = i;
	//	}
	//}

	//// If we're not showing the split colors, set it back to 1 to remove the coloring
	//if (!g_bShowSplitColors)
	//	vColor = 1;

	//// Determine the depth of the pixel with respect to the light
	//float4x4 matViewToLightViewProj = mul(g_matInvView, matLightViewProj);
	//float4 vPositionLightCS = mul(vPositionVS, matViewToLightViewProj);

	//float fLightDepth = vPositionLightCS.z / vPositionLightCS.w;

	//// Transform from light space to shadow map texture space.
	//float2 vShadowTexCoord = 0.5 * vPositionLightCS.xy / vPositionLightCS.w + float2(0.5f, 0.5f);
	//vShadowTexCoord.x = vShadowTexCoord.x / NUM_SPLITS + fOffset;
	//vShadowTexCoord.y = 1.0f - vShadowTexCoord.y;

	//// Offset the coordinate by half a texel so we sample it correctly
	//vShadowTexCoord += (0.5f / g_vShadowMapSize);

	//// Get the shadow occlusion factor and output it
	//float fShadowTerm = CalcShadowTermSoftPCF(fLightDepth, vShadowTexCoord, 5/*iFilterSize*/);

	return float4(shadowTerm, 1);
}


technique GenerateShadowMap
{
	pass Pass1
	{
		CULLMODE = NONE;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}

technique GenerateShadowOcclusion
{
	pass Pass1
	{
		ZWriteEnable = false;
		ZEnable = false;
		AlphaBlendEnable = false;
		CullMode = NONE;

		VertexShader = compile vs_3_0 VertexShaderFunctionSO();
		PixelShader = compile ps_3_0 ShadowTermPS();
	}
}