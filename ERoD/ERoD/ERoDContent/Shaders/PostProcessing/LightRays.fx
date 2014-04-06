#define NumberOfSamples 128
float3 LightPosition;
float4x4 ViewProjection;
float2 HalfPixel;

float Density;
float Decay;
float Weight;
float Exposure;

sampler2D Scene: register(s0){
	AddressU = Mirror;
	AddressV = Mirror;
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
	VertexShaderOutput output;

	output.Position = float4(input.Position.xyz, 1);
	output.TexCoord = input.TexCoord - HalfPixel;

	return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{

	//Get lightPositions screen space position
    float4 screenPosition = mul(LightPosition, ViewProjection);
	screenPosition.xyz /= screenPosition.w;
	screenPosition.x = screenPosition.x / 2.0 + 0.5;
	screenPosition.y = (-screenPosition.y / 2.0 + 0.5);

	//Compute a ray
	float2 deltaTexCoord = (input.TexCoord - screenPosition.xy);
	deltaTexCoord *= (1.0 / NumberOfSamples * Density);
	deltaTexCoord = deltaTexCoord * clamp(screenPosition.w * screenPosition.z,0, 0.5);
	float3 color = tex2D(Scene, input.TexCoord);
	float illuminationDecay = 1.0;
    float3 sample;
	float2 TexCoord = input.TexCoord;
	for( int i = 0; i < NumberOfSamples; ++i )
    {
        TexCoord -= deltaTexCoord;
        sample = tex2D(Scene, TexCoord);
        sample *= illuminationDecay * Weight;
        color += sample;
        illuminationDecay *= Decay;           
    }
	return float4(color * Exposure, 1);
}

technique LightRays
{
    pass P0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
