float4x4 World;
float4x4 wvp : WorldViewProjection;

float3 color = 1;

bool textureEnabled;
texture diffuseTexture;
sampler diffuseSampler = sampler_state
{
	texture = <diffuseTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	float Depth : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(input.Position, wvp);

	output.TexCoord = input.TexCoord;

	output.Depth = 1 - (output.Position.z / output.Position.w);

	output.Normal = mul(input.Normal, World);

    return output;
}

struct PixelShaderOutput
{
	float4 Color : COLOR0;
	float4 Normal : COLOR1;
	float4 Depth : COLOR2;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	PixelShaderOutput output = (PixelShaderOutput)0;

	if (textureEnabled)
	{
		output.Color = tex2D(diffuseSampler, input.TexCoord) * float4(color, 1);
	}
	else
	{
		output.Color = float4(color, 1);
	}

	output.Normal.xyz = (normalize(input.Normal).xyz / 2) + .5;
	output.Normal.a = 1;

	output.Depth = float4(input.Depth.x, 0, 0, 1);

	return output;
}

technique Deferred
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
