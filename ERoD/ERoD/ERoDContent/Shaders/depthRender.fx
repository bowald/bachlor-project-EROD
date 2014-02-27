uniform extern texture sceneMap;
sampler screen = sampler_state
{
	texture = <sceneMap>;
	MinFilter = Point;
	MagFilter = Point;
};

struct PS_INPUT
{
	float2 TexCoord	: TEXCOORD0;
};

float4 Invert(PS_INPUT Input) : COLOR0
{
	float4 color = 0;
	color.r = tex2D(screen, Input.TexCoord).r * 300;
	color.a = 1;

	return color;
}

technique RenderDepth
{
	pass P0
	{
		PixelShader = compile ps_2_0 Invert();
	}
}