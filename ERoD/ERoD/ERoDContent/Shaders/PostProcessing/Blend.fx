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

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TexCoord0;
};
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TexCoord : TexCoord0;
};
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = float4(input.Position.xyz, 1);
	output.TexCoord = input.TexCoord;

	return output;
}
float4 BlendPS(float2 texCoord : TEXCOORD0) : COLOR0
{
	texCoord -= halfPixel;
	float4 col = tex2D(orgScene, texCoord) * tex2D(Scene, texCoord);

		return col;
}

float4 AditivePS(float2 texCoord : TEXCOORD0) : COLOR0
{
	texCoord -= halfPixel;
	float4 col = tex2D(orgScene, texCoord) + tex2D(Scene, texCoord);
		return col;
}

technique Blend
{
	pass p0
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 BlendPS();
	}
}

technique Aditive
{
	pass p0
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 AditivePS();
	}
}