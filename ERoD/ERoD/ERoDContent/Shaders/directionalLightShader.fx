float4x4 viewProjectionInv;
float4x4 lightViewProjection;

//direction of the light
float3 lightDirection;

float3 cameraPosition;

float power = 1;

float2 halfPixel;

//color of the light 
float3 color;

texture normalMap;
sampler normalSampler = sampler_state
{
	Texture = <normalMap>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
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

float shadowBias = 0.0000033f;
bool castShadow;
texture shadowMap;
sampler shadowSampler = sampler_state
{
	Texture = <shadowMap>;
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

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 normalData = tex2D(normalSampler, input.TexCoord);
	float3 normal = 2.0f * normalData.xyz - 1.0f;

	float depth = 1 - tex2D(depthSampler, input.TexCoord).r;

	float4 screenPos;
	screenPos.x = input.TexCoord.x * 2.0f - 1.0f;
	screenPos.y = -(input.TexCoord.y * 2.0f - 1.0f);

	screenPos.z = depth;
	screenPos.w = 1.0f;

	float4 worldPos = mul(screenPos, viewProjectionInv);
	worldPos /= worldPos.w;

	float4 lightScreenPos = mul(worldPos, lightViewProjection);
	lightScreenPos /= lightScreenPos.w;

	//find sample position in shadow map
	float2 lightSamplePos;
	lightSamplePos.x = lightScreenPos.x / 2.0f + 0.5f;
	lightSamplePos.y = (-lightScreenPos.y / 2.0f + 0.5f);

	//determine shadowing criteria
	float realDistanceToLight = lightScreenPos.z;
	float distanceStoredInDepthMap = 1 - tex2D(shadowSampler, lightSamplePos).r;

	// add bias
	realDistanceToLight -= shadowBias;

	bool shadowCondition = distanceStoredInDepthMap <= realDistanceToLight;

	float shading = 0.5;
	if (!castShadow || !shadowCondition)
	{
		shading = 1;
	}

	//surface-to-light vector
	float3 lightVector = normalize(-lightDirection);

	//compute diffuse light
	float NdL = saturate(dot(normal, lightVector));
	float3 diffuseLight = (NdL * color.rgb) * power;


	////reflection vector
	//float3 r = normalize(reflect(-lightVector, normal));

	////view vector
	//float3 v = normalize(cameraPosition - screenPos);

	//float3 Half = normalize(r + v);
	//float specular = pow(saturate(dot(normalData, Half)), 25);

	//diffuseLight += (specular * power);

	//output the two lights
	return float4(diffuseLight.rgb, 1) * shading;

}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
