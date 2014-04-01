float4x4 World;
float4x4 View;
float4x4 Projection;
float2 HalfPixel;

float3 lightPosition;
float3 cameraPosition;

float4x4 matVP;


sampler2D Scene: register(s0){
	AddressU = Clamp;
	AddressV = Clamp;
};

texture DepthBuffer;
sampler2D depthSampler = sampler_state
{
	Texture = <DepthBuffer>;
	MipFilter = NONE;
	MagFilter = POINT;
	MinFilter = POINT;
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
	float4 TexCoord : TEXCOORD0;

};

VertexShaderOutput VertexShaderConvertRGB(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.Position = input.Position;
	output.TexCoord.xy = input.TexCoord + HalfPixel;
	return output;
}

float4 PixelShaderConvertRGB(VertexShaderOutput input) : COLOR0
{
	// we use the depth buffer as a mask: the background values (closer to 1)
	// have more weight in the shaft texture. We could use some parameters
	// to better control the depth, like scale-bias the value at our taste

	// linear filtering
	//float3 color = tex2D(colorSampler, input.TexCoord).rgb;
	//	color += tex2D(colorSampler, input.TexCoord + float2(0, PixelSize.y)).rgb;
	//color += tex2D(colorSampler, input.TexCoord + float2(PixelSize.x, 0)).rgb;
	//color += tex2D(colorSampler, input.TexCoord + PixelSize).rgb;

	float3 color = 0.0f;
	float depth = 1 - tex2D(depthSampler, input.TexCoord).r;
	if (depth > 0.999999){
		color = tex2D(Scene, input.TexCoord).rgb;
	}
	return float4(color, 1);
}

technique ConvertDepth  //0
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderConvertRGB();
		PixelShader = compile ps_3_0 PixelShaderConvertRGB();
	}
}
