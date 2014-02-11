float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.2f;

float3 DiffuseLightDirection = float3(1, 0.5f, 0);
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseIntensity = 1;

float Shininess = 200;
float4 SpecularColor = float4(1, 1, 1, 1);
float SpecularIntensity = 1.0;
float3 ViewVector = float3(1, 0, 0);

texture ModelTexture;
sampler2D textureSampler = sampler_state {
	Texture = (ModelTexture);
	MagFilter = Linear;
	MinFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

// Bumbconstant, how much the normalmap effects normals, 0 = nothing. >3 = too much.
float BumpConstant = 1;
texture NormalMap;
sampler2D bumpSampler = sampler_state {
	Texture = (NormalMap);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

// Structures that represent the input and output of the vertex shader.
// Get input from the scene and models.
struct VertexShaderInput
{
    float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float3 Tangent : TANGENT0;
	float3 Binormal : BINORMAL0;
	float2 TexCoord : TEXCOORD0;
};

// Output to the pixel shader (fragment shader in opengl)
struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float3 Tangent : TEXCOORD2;
	float3 Binormal : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	// Transform input position with the worldviewprojection matrix
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.Normal = normalize(mul(input.Normal, WorldInverseTranspose));
	output.Tangent = normalize(mul(input.Tangent, WorldInverseTranspose));
	output.Binormal = normalize(mul(input.Binormal, WorldInverseTranspose));

	output.TexCoord = input.TexCoord;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// calculate normal from the normalmap
	float3 bump = BumpConstant * (tex2D(bumpSampler, input.TexCoord) - (0.5f, 0.5f, 0.5f));
	float3 bumpNormal = input.Normal + (bump.x * input.Tangent + bump.y * input.Binormal);
	bumpNormal = normalize(bumpNormal);

	// diffuse light intensity from angle.
	float diffuseIntensity = dot(normalize(DiffuseLightDirection), bumpNormal);
	if (diffuseIntensity < 0)
		diffuseIntensity = 0;

	// light dir
	float3 light = normalize(DiffuseLightDirection);
	// bling phong or smth
	float3 r = normalize(2 * dot(light, bumpNormal) * bumpNormal - light);
	float3 v = normalize(mul(normalize(ViewVector), World));
	float dotProduct = dot(r, v);

	float4 specular = SpecularIntensity * SpecularColor * 
		max(pow(dotProduct, Shininess), 0) * diffuseIntensity;

	float4 textureColor = tex2D(textureSampler, input.TexCoord);
	textureColor.a = 1;

	return saturate(textureColor * diffuseIntensity + AmbientColor * AmbientIntensity + specular);
}

technique Normal
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
