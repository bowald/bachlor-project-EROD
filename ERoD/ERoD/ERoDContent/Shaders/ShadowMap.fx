float4x4 World;
float4x4 LightView;
float4x4 LightProjection;


struct VertexShaderInput
{
    float4 Position : POSITION0;

};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Position2D : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, LightView);
    output.Position = mul(viewPosition, LightProjection);
	output.Position2D = output.Position;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{

	return input.Position2D.z / input.Position2D.w;
}

technique ShadowMap
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
