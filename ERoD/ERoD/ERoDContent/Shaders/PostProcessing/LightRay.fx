
#include "PPVertexShader.fx"

#define NUM_SAMPLES 128

float3 lightPosition;
float3 cameraPosition;

float4x4 matVP;

float2 halfPixel;

float Density = .5f;
float Decay = .95f;
float Weight = 1.0f;
float Exposure = .15f;
float Intensity = 1.0f;

sampler2D Scene: register(s0){
	AddressU = Clamp;
	AddressV = Clamp;
};

float4 lightRayPS(float2 texCoord : TEXCOORD0) : COLOR0
{
	// Find light pixel position
	float4 ScreenPosition = mul(lightPosition - cameraPosition, matVP);

	ScreenPosition.xyz /= ScreenPosition.w;
	ScreenPosition.x = ScreenPosition.x / 2.0f + 0.5f;
	ScreenPosition.y = (-ScreenPosition.y / 2.0f + 0.5f);

	float intensity = 1 - sqrt(ScreenPosition.x*ScreenPosition.x + ScreenPosition.y*ScreenPosition.y)*0.15f;
	intensity = min(1, intensity);
	intensity = max(0, intensity);

	//if (ScreenPosition.z < 0 || ScreenPosition.z > 1)
	//	float Intensity = 0;
	//else
	//{
	//	// make the intensity function more narrow (intensity^3)
	//	float Intensity = Intensity*(intensity*intensity*intensity);
	//}

	float2 TexCoord = texCoord - halfPixel;
	// Calculate vector from pixel to light source in screen space
	float2 DeltaTexCoord = (TexCoord - ScreenPosition.xy);
	// Divide by number of samples and scale by control factor.  
	DeltaTexCoord *= (1.0f / NUM_SAMPLES * Density);

	DeltaTexCoord = DeltaTexCoord * clamp(ScreenPosition.w * ScreenPosition.z, 0, .5f);

	float3 col = tex2D(Scene, TexCoord);
	// Set up illumination decay factor.  
	float IlluminationDecay = 1.0;
	float3 Sample;

	for (int i = 0; i < NUM_SAMPLES; ++i)
	{
		// Step sample location along ray.  
		TexCoord -= DeltaTexCoord;
		// Retrieve sample at new location.  
		Sample = tex2D(Scene, TexCoord);
		// Apply sample attenuation scale/decay factors. 
		Sample *= IlluminationDecay * Weight;
		// Accumulate combined color.  
		col += Sample;
		// Update exponential decay factor.
		IlluminationDecay *= Decay;
	}

	return float4(col * Exposure, 1);
	/*if (ScreenPosition.w > 0)
		return float4(col * Exposure, 1) * (ScreenPosition.w * .0025);
	else
		return 0;
*/
}

technique LightRayFX
{
	pass p0
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 lightRayPS();

		AlphaBlendEnable = true;
		BlendOp = Add;
		SrcBlend = One;
		DestBlend = One;
	}
}