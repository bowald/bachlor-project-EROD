float4x4 World;
float4x4 View;
float4x4 Projection;

// TODO: add effect parameters here.
float near;
float far;


struct VertexShaderInput
{
    float4 Position : POSITION0;

};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
    float2 Depth : TEXCOORD0;

};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Depth = output.Position.zw;
    // TODO: add your vertex shader code here.

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // TODO: add your pixel shader code here.
	float d = (input.Depth.x - near) / (far - near);
	return float4(d, d, d, 1);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
