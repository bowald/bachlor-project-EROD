float4x4 viewProjectionInv;
float4x4 lightViewProjection;

//direction of the light
float3 lightDirection;

float3 cameraPosition;
float4x4 CameraTransform;

float power = 1;
float specularModifier = 3;

float3 FrustumCorners[4];

float2 halfPixel;
float2 ShadowMapPixelSize;
float2 ShadowMapSize;

//color of the light 
float3 color;

//Cascade shadow maps parameters
static const int NUM_SPLITS = 3;
float4x4	MatLightViewProj[NUM_SPLITS];
float2		ClipPlanes[NUM_SPLITS];
float3		CascadeDistances;

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

float shadowBias = 0.00000065f;
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

/////////////////////////////////////////////////
// Helper to compute shadow attenuation with PCF
/////////////////////////////////////////////////
float ComputeShadow4Samples(float nl, float2 shadowTexCoord, float ourdepth)
{
	// Get the current depth stored in the shadow map
	float samples[4];
	samples[0] = tex2D(shadowSampler, shadowTexCoord).r < ourdepth ? 0 : 1;
	samples[1] = tex2D(shadowSampler, shadowTexCoord + float2(0, 2)*ShadowMapPixelSize).r < ourdepth ? 0 : 1;
	samples[2] = tex2D(shadowSampler, shadowTexCoord + float2(2, 0)*ShadowMapPixelSize).r < ourdepth ? 0 : 1;
	samples[3] = tex2D(shadowSampler, shadowTexCoord + float2(2, 2)*ShadowMapPixelSize).r < ourdepth ? 0 : 1;

	// Determine the lerp amounts           
	float2 lerps = frac(shadowTexCoord*ShadowMapSize);
	// lerp between the shadow values to calculate our light amount
	half shadow = lerp(lerp(samples[0], samples[1], lerps.y), lerp(samples[2], samples[3], lerps.y), lerps.x);

	return nl*shadow;
}

float3 GetFrustumRay(in float2 texCoord)
{
	float index = texCoord.x + (texCoord.y * 2);
	return FrustumCorners[index];
}

half3 DecodeNormal(half4 enc)
{
	float kScale = 1.7777;
	float3 nn = enc.xyz*float3(2 * kScale, 2 * kScale, 0) + float3(-kScale, -kScale, 1);
		float g = 2.0 / dot(nn.xyz, nn.xyz);
	float3 n;
	n.xy = g*nn.xy;
	n.z = g - 1;
	return n;
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
	float3 FrustumRay : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = float4(input.Position, 1);
	output.TexCoord = input.TexCoord - halfPixel;
	output.FrustumRay = GetFrustumRay(input.TexCoord);

	return output;
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

	float shading = 0.5;
	if (!castShadow || !shadowCondition)
	{
		shading = 1;
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

float4 DirectionalLightShadowPS(VertexShaderOutput input) : COLOR0
{
	// If we want the WorldPosition, we have to multiply by the world camera matrix
	float depthValue = tex2D(depthSampler, input.TexCoord).r;

	//if depth value == 1, we can assume its a background value, so skip it
	clip(-depthValue + 0.9999f);

	float3 pos = input.FrustumRay * depthValue;

	float4 screenPos;
	screenPos.x = input.TexCoord.x * 2.0f - 1.0f;
	screenPos.y = -(input.TexCoord.y * 2.0f - 1.0f);

	screenPos.z = depthValue;
	screenPos.w = 1.0f;

	float4 worldPos = mul(screenPos, viewProjectionInv);
	worldPos /= worldPos.w;

		// Convert normal back with the decoding function
		float4 normalMap = tex2D(normalSampler, input.TexCoord);
		float3 normal = DecodeNormal(normalMap);

		//surface-to-light vector
		float3 lightVector = normalize(-lightDirection);

		float nl = saturate(dot(normal, lightVector));

	float spec = 0;
	float4 finalColor = 0;

	clip(nl - 0.00001f);

	{
		// Figure out which split this pixel belongs to, based on view-space depth.
		float3 weights = (pos.z < CascadeDistances);
			weights.xy -= weights.yz;


		float4x4 lightViewProj = MatLightViewProj[0] * weights.x + MatLightViewProj[1] * weights.y + MatLightViewProj[2] * weights.z;

		//remember that we need to find the correct cascade into our cascade atlas
		float fOffset = weights.y*0.33333f + weights.z*0.666666f;


		// Find the position of this pixel in light space
		float4 lightScreenPos = mul(worldPos, lightViewProj);
		lightScreenPos /= lightScreenPos.w;

		// Find the position of this pixel in light space
		//float4 lightingPosition = mul(mul(float4(pos, 1), CameraTransform), lightViewProj);

		// Find the position in the shadow map for this pixel
		float2 shadowTexCoord = 0.5 * lightScreenPos.xy / lightScreenPos.w + float2(0.5, 0.5);

		shadowTexCoord.x = shadowTexCoord.x *0.3333333f + fOffset;
		shadowTexCoord.y = 1.0f - shadowTexCoord.y;
		shadowTexCoord += ShadowMapPixelSize;

		// Calculate the current pixel depth
		// The bias is used to prevent floating point errors that occur when
		// the pixel of the occluder is being drawn
		float ourdepth = (lightScreenPos.z / lightScreenPos.w) - shadowBias;

		//the pixel can be outside of shadow distance, so skip it in that case
		float shadowSkip = ClipPlanes[2].y > pos.z;
		nl = nl*shadowSkip + ComputeShadow4Samples(nl, shadowTexCoord, ourdepth)*(1 - shadowSkip);

	}

	
	float3 diffuseLight = (nl * color.rgb) * power;

	float4 SGR = tex2D(sgrSampler, input.TexCoord);

	//reflection vector
	float3 r = normalize(2 * dot(lightVector, normal) * normal - lightVector);

	//view vector
	float3 v = normalize(cameraPosition - worldPos);

	float dotProduct = dot(r, v);

	float4 specular = SGR.r * float4(color, 1) * max(pow(dotProduct, 20), 0);

	diffuseLight += (specular * specularModifier * power);

	//As our position is relative to camera position, we dont need to use (ViewPosition - pos) here
		/*float3 h = normalize(reflect(LightDir, normal));
		spec = nl*pow(saturate(dot(camDir, h)), normalMap.b * 100);
	finalColor = float4(LightColor * (nl), spec);*/

	//output light
	return float4(diffuseLight.rgb, 1);// * LightBufferScale;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 DirectionalLightShadowPS();
    }
}
