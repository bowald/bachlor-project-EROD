
//Backbuffer
sampler TextureSampler : register(s0);
//Amount of lightning accepted
float Threshold;

float4 PixelShaderFunction(float2 texCoord: TEXCOORD0) : COLOR0
{
    float4 Color = tex2D(TextureSampler, texCoord);
	return saturate((Color - Threshold) / (1 - Threshold));
}

technique BloomMask
{
    pass P0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
