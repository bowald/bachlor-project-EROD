// Intensity
float LastSceneIntensity;		//= 1.3;
float OriginalIntensity;		//= 1.0;

// Saturation
float LastSceneSaturation;		//= 1.0;
float OriginalSaturation;	    //= 1.0;

//Backbuffer
sampler2D Scene: register(s0){
	AddressU = Mirror;
	AddressV = Mirror;
};

texture OriginalScene;
sampler2D originalScene = sampler_state
{
	Texture = <OriginalScene>;
	AddressU = CLAMP;
	AddressV = CLAMP;
};


float4 AdjustSaturation(float4 color, float saturation)
{
	float grey = dot(color, float3(0.3, 0.59, 0.11));
	return lerp(grey, color, saturation);
}

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 lastSceneColor = tex2D(Scene, texCoord);
	float4 originalColor = tex2D(originalScene, texCoord);

	lastSceneColor = AdjustSaturation(lastSceneColor, LastSceneSaturation) * LastSceneIntensity;
	originalColor = AdjustSaturation(originalColor, OriginalSaturation) * OriginalIntensity;

	originalColor *= (1 - saturate(lastSceneColor));

	return originalColor + lastSceneColor;
}

technique Blend
{
    pass P0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
