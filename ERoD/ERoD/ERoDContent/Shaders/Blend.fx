float2 halfPixel;

sampler2D Scene: register(s0){
	AddressU = Mirror;
	AddressV = Mirror;
};

texture OrgScene;
sampler2D orgScene = sampler_state
{
	Texture = <OrgScene>;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

struct VertexShaderInput
{
	float3 Position : POSITION0;
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

	output.Position = float4(input.Position, 1);
	output.TexCoord = input.TexCoord - halfPixel;

	return output;
}

float4 BlendPS(float2 texCoord : TEXCOORD0) : COLOR0
{
	texCoord -= halfPixel;
	float4 col = tex2D(orgScene, texCoord) * tex2D(Scene, texCoord);

		return col;
}
technique Blend
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 BlendPS();
	}
}