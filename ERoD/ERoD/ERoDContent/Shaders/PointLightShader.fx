float4x4 World;
float4x4 View;
float4x4 Projection;

float3 Color;

float3 CameraPosition;

float4x4 InvertViewProjection;

float3 lightPosition;

float lightRadius;

float lightIntensity = 1.0f;

float2 halfPixel;

texture colorMap;
sampler colorSampler = sampler_state
{
	Texture = <colorMap>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
};

texture normalMap;
sampler normalSampler = sampler_state
{
	Texture = (normalMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};

texture depthMap;
sampler depthSampler = sampler_state
{
	Texture = (depthMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 ScreenPosition : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.ScreenPosition = output.Position;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	input.ScreenPosition.xy /= input.ScreenPosition.w;

	float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
	texCoord -= halfPixel;

	float depthVal = tex2D(depthSampler, texCoord).r;

	float4 normalData = tex2D(normalSampler, texCoord);
	float3 normal = 2.0f * normalData.xyz - 1.0f;

	float4 position;
	position.xy = input.ScreenPosition.xy;
	position.z = depthVal;
	position.w = 1.0f;

	position = mul(position, InvertViewProjection);
	position /= position.w;

	float3 lightVector = lightPosition - position;

	float attenuation = saturate(1.0f - length(lightVector) / lightRadius);

	lightVector = normalize(lightVector);

	float Ndl = saturate(dot(normal, lightVector));
	float3 diffuseLight = Ndl * Color.rgb;

	float3 r = normalize(reflect(-lightVector, normal));
	float3 v = normalize(CameraPosition - input.ScreenPosition);
	
	return (attenuation * lightIntensity * float4(diffuseLight.rgb, 1));// +specular * att *ligtI;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
