float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

//float4 AmbientColor; // = float4(1, 1, 1, 1);
float AmbientIntensity = 0.2f;

float3 DiffuseLightDirection = float3(1, 0.5f, 0);
//float4 DiffuseColor; // = float4(1, 1, 1, 1);
float DiffuseIntensity = 1;

float Shininess = 23;
float4 SpecularColor = float4(1, 1, 1, 1);
float SpecularIntensity = 0.1;
float3 ViewVector;

float3 CameraPosition;

float4 AmbientLightColor; // = float3(.15, .15, .15);
float4 DiffuseColor; // = float3(.85, .85, .85);
float3 LightPosition; // = float3(0, 0, 0);
float4 LightColor; // = float3(1, 1, 1);
float LightAttenuation; // = 5000;
float LightFalloff; // = 2;

bool TextureEnabled = false;
texture ModelTexture;
sampler2D textureSampler = sampler_state {
	Texture = (ModelTexture);
	MagFilter = Linear;
	MinFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float3 ViewDirection : TEXCOORD2;
	float4 WorldPosition : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	output.WorldPosition = worldPosition;
	output.Normal = normalize(mul(input.Normal, WorldInverseTranspose));
	output.ViewDirection = worldPosition - CameraPosition;
	output.TexCoord = input.TexCoord;

	return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 diffuseColor = DiffuseColor;
	if (TextureEnabled)
		diffuseColor *= tex2D(textureSampler, input.TexCoord);
	float4 totalLight = float4(0, 0, 0, 1);

	totalLight += AmbientLightColor;
	float3 lightDir = normalize(LightPosition - input.WorldPosition);
	float diffuse = saturate(dot(normalize(input.Normal), lightDir));
	float d = distance(LightPosition, input.WorldPosition);
	float att = 1 - pow(clamp(d / LightAttenuation, 0, 1), LightFalloff);
	totalLight += diffuse * att * LightColor;

	float3 viewVector = normalize(input.ViewDirection);
	float3 normal = normalize(input.Normal);
	float3 r = normalize(2 * dot(lightDir, normal) * normal - lightDir);
	float3 v = normalize(mul(normalize(viewVector), World));
	float dotProduct = dot(r, v);

	float4 specular = SpecularIntensity * SpecularColor * max(pow(dotProduct, Shininess), 0) * att;

	return saturate(diffuseColor * totalLight + specular);
}

technique Textured
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}