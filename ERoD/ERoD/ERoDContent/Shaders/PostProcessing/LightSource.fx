float3 LightPosition;
float4x4 ViewProjection;
float SunSize;
float2 HalfPixel;

texture SunSource;
sampler SunSampler = sampler_state
{
	Texture = (SunSource);
	AddressU = Clamp;
	AddressV = Clamp;
}; 

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = float4(input.Position.xyz, 1);
	output.TexCoord = input.TexCoord - HalfPixel;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = 0;
	float4 ScreenPosition = mul(LightPosition, ViewProjection);
	float scale = ScreenPosition.z;
	ScreenPosition.xyz /= ScreenPosition.w;
	ScreenPosition.x = ScreenPosition.x/2.0 + 0.5;
	ScreenPosition.y = (-ScreenPosition.y/2.0 + 0.5);

	if(ScreenPosition.w > 0)
	{
		float2 coord;
		float size = SunSize / scale;
		float2 center = ScreenPosition.xy;
		coord = 0.5 - (input.TexCoord - center) / size * 0.5;
		color += (pow(tex2D(SunSampler,coord),2) * 1) * 2; 
	}
	return color;

}

technique LightSource
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
