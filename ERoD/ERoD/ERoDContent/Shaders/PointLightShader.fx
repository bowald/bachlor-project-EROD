float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 ViewInv;

float3 CameraPosition;

float3 Color;

float3 LightPosition;

float LightRadius;

float LightIntensity = 1.0f;

float2 HalfPixel;

// Length of the x and y sides of the far plane in view space from depth.
float2 SidesLengthVS;

// Distance to the far plane.
float FarPlane;

bool DebugPosition = false;

texture ColorMap;
sampler ColorSampler = sampler_state
{
	Texture = <ColorMap>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
};

texture NormalMap;
sampler NormalSampler = sampler_state
{
	Texture = (NormalMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};

texture DepthMap;
sampler DepthSampler = sampler_state
{
	Texture = (DepthMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;
};

texture SGRMap;
sampler SGRSampler = sampler_state
{
	Texture = (SGRMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
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
	texCoord -= HalfPixel;

	float depthVal = 1 - tex2D(DepthSampler, texCoord).r;

	float4 normalData = tex2D(NormalSampler, texCoord);
	float3 normal = 2.0f * normalData.xyz - 1.0f;

	depthVal *= FarPlane;

	float4 positionCVS = float4(float3(SidesLengthVS * (2 * texCoord - 1) * depthVal, -depthVal), 1);
	float4 positionWS = mul(positionCVS, ViewInv);

	float3 lightVector = LightPosition - positionWS;

	float attenuation = saturate(1.0f - pow(length(lightVector) / LightRadius, 5));

	lightVector = normalize(lightVector);

	float NdotL = saturate(dot(normal, lightVector));
	float3 diffuseLight = NdotL * Color.rgb;

	if (DebugPosition && attenuation < 0.001)
	{
		return float4(1, attenuation, 0, 1);
	}

	// Specular
	float3 r = normalize(reflect(-lightVector, normal));
	float3 v = normalize(CameraPosition - positionWS);

	float specularMask = tex2D(SGRSampler, texCoord).r;

	float3 h = normalize(r + v);
	float4 specular = float4(pow(saturate(dot(normalData, h)), 25) * specularMask * Color.rgb, 1);

	return (attenuation * LightIntensity * (float4(diffuseLight.rgb, 1) + specular));
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
