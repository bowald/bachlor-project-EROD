float3 LightPosition;
float4x4 ViewProjection;
float4x4 InverseViewProjection;
float2 HalfPixel;

sampler2D Scene: register(s0){
    AddressU = Mirror;
    AddressV = Mirror;
};

texture DepthBuffer;
sampler2D DepthSampler = sampler_state
{
    Texture = <DepthBuffer>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
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
	float depth = 1 - (tex2D(DepthSampler, input.TexCoord).r);
	float4 color = tex2D(Scene, input.TexCoord);
	float4 position;

	position.x = input.TexCoord.x * 2.0 - 1.0;
	position.y = -(input.TexCoord.y * 2.0 - 1.0);
	position.z = depth;
	position.w = 1.0;

	float4 worldPosition = mul(position, InverseViewProjection);
	worldPosition /= worldPosition.w;

	float4 screenPosition = mul(LightPosition, ViewProjection);
	screenPosition.xyz /= screenPosition.w;
	screenPosition.x = screenPosition.x / 2.0 + 0.5;
	screenPosition.y = (-screenPosition.y / 2.0 + 0.5);

    if(depth < screenPosition.z - .00025)
        color = 0;
    return color;
}

technique SceneMask
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
