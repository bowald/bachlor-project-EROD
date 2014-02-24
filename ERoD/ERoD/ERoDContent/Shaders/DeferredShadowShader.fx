// Global Variables
float4x4 World;
float4x4 vp : ViewProjection;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Extras : POSITION1;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 ScreenPosition : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPos = mul(input.Position, World);
	output.Position = mul(worldPos, vp);
	output.ScreenPosition = output.Position;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 output = input.ScreenPosition.z / input.ScreenPosition.w;

	output.a = 1;

	return output;
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