// Pixel shader extracts the brighter areas of an image.
// This is the first step in applying a bloom postprocess.

uniform extern float BloomThreshold;
//uniform extern texture SceneTex;

float2 halfPixel;
sampler TextureSampler : register(s0);
/* = sampler_state
{
Texture = <SceneTex>;
MinFilter = LINEAR;
MagFilter = LINEAR;
MipFilter = LINEAR;
};*/


float4 BrightPassPS(float2 texCoord : TEXCOORD0) : COLOR0
{
	texCoord -= halfPixel;
	// Look up the original image color.
	float4 c = tex2D(TextureSampler, texCoord);

		// Adjust it to keep only values brighter than the specified threshold.
		return saturate((c - BloomThreshold) / (1 - BloomThreshold));
}


technique BloomExtract
{
	pass P0
	{
		PixelShader = compile ps_2_0 BrightPassPS();

		//ZWriteEnable = false;
	}
}