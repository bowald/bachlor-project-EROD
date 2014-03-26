// Global Variables
float4x4 World;
float4x4 LightView;
float4x4 LightProjection;

float FarPlane;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Extras : POSITION1;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float Depth : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPos = mul(input.Position, World);
	float4 viewPos = mul(worldPos, LightView);
	output.Position = mul(viewPos, LightProjection);
	output.Depth.x = viewPos.z;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float depth = 1 - (-input.Depth / FarPlane);
	return float4(depth, 0, 0, 1);
}

technique ShadowMap
{
	pass Pass1
	{
		CULLMODE = NONE;
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}