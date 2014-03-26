
#include "PPVertexShader.fx"

float3 lightPosition;
float3 cameraPosition;

float4x4 matVP;

float2 halfPixel;

float SunSize = 1500;

texture flare;
sampler Flare = sampler_state
{
	Texture = (flare);
	AddressU = CLAMP;
	AddressV = CLAMP;
};

float4 LightSourceMaskPS(float2 texCoord : TEXCOORD0) : COLOR0
{
	texCoord -= halfPixel;

	// Get the scene
	float4 col = 0;

		// Find the suns position in the world and map it to the screen space.
		float4 ScreenPosition = mul(lightPosition - cameraPosition, matVP);
		float scale = ScreenPosition.z;
	ScreenPosition.xyz /= ScreenPosition.w;
	ScreenPosition.x = ScreenPosition.x / 2.0f + 0.5f;
	ScreenPosition.y = (-ScreenPosition.y / 2.0f + 0.5f);




	// Are we lokoing in the direction of the sun?
	if (ScreenPosition.w > 0)
	{
		float2 coord;

		float size = SunSize / scale;

		float2 center = ScreenPosition.xy;

			coord = .5 - (texCoord - center) / size * .5;


		col += (pow(tex2D(Flare, coord), 2) * 1) * 2;
	}

	return col;
}

technique LightSourceMask
{
	pass p0
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 LightSourceMaskPS();
	}
}