//http://www.gamedev.net/page/resources/_/reference/programming/140/lighting-and-shading/a-simple-and-practical-approach-to-ssao-r2753

float2 halfPixel;
float4x4 View;
float4x4 viewProjectionInv;
float2 random_size;
float g_sample_rad = .1f;
float g_intensity = 1;
float g_scale = .1;
float g_bias = .1;
float2 screenSize;

texture normalMap;
sampler normalSampler = sampler_state
{
	Texture = (normalMap);
};

texture random;
sampler g_random = sampler_state
{
	Texture = (random);
	AddressU = MIRROR;
	AddressV = MIRROR;
};

texture depthMap;
sampler depthSampler = sampler_state
{
	Texture = <depthMap>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
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

struct PS_INPUT
{
	float2 uv : TEXCOORD0;
};

struct PS_OUTPUT
{
	float4 color : COLOR0;
};

float3 getPosition(in float2 uv)
{
	float depth = 1 - tex2D(depthSampler, uv).r;

	float4 screenPos;
	screenPos.x = uv.x * 2.0f - 1.0f;
	screenPos.y = -(uv.y * 2.0f - 1.0f);

	screenPos.z = depth;
	screenPos.w = 1.0f;

	float4 worldPos = mul(screenPos, viewProjectionInv);
	worldPos /= worldPos.w;
	return worldPos.xyz;


}


float3 getNormal(in float2 uv)
{
	return normalize(tex2D(normalSampler, uv).xyz * 2.0f - 1.0f);
}

float2 getRandom(in float2 uv)
{
	return normalize(tex2D(g_random, screenSize * uv / random_size).xy * 2.0f - 1.0f);
}

float doAmbientOcclusion(in float2 tcoord, in float2 uv, in float3 p, in float3 cnorm)
{
	float3 diff = getPosition(tcoord + uv) - p;
		const float3 v = normalize(diff);
	const float d = length(diff)*g_scale;
	return max(0.0, dot(cnorm, v) + g_bias)*(1.0 / (1.0 + d))*g_intensity;
}

PS_OUTPUT main(PS_INPUT i)
{
	PS_OUTPUT o = (PS_OUTPUT)0;

	o.color.rgb = 1.0f;
	const float2 vec[4] = { float2(1, 0), float2(-1, 0),
		float2(0, 1), float2(0, -1) };

	i.uv -= halfPixel;

	float3 p = getPosition(i.uv);
		float3 n = getNormal(i.uv);
		float2 rand = getRandom(i.uv);

		n = mul(n, View);

	float ao = 0.0f;
	float rad = g_sample_rad / p.z;

	//**SSAO Calculation**//
	int iterations = 3;

	for (int j = 0; j < iterations; ++j)
	{
		float2 coord1 = reflect(vec[j], rand)*rad;
			float2 coord2 = float2(coord1.x*0.707 - coord1.y*0.707,
			coord1.x*0.707 + coord1.y*0.707);

		ao += doAmbientOcclusion(i.uv, coord1*0.25, p, n);
		ao += doAmbientOcclusion(i.uv, coord2*0.5, p, n);
		ao += doAmbientOcclusion(i.uv, coord1*0.75, p, n);
		ao += doAmbientOcclusion(i.uv, coord2, p, n);
	}

	ao /= (float)iterations * 4.0;

	//**END**//

	//o.color = float4(p,1);
	//o.color = float4(n,1);
	o.color = 1 * ao;
	//Do stuff here with your occlusion value “ao”: modulate ambient lighting, write it to a buffer for later //use, etc.

	//o.color = float4(rand,1,1);

	return o;
}

technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 main();
	}
}