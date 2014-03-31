#define g_numSamples 8

float4x4 Vp;
float4x4 ViewProjectionInverseMatrix;
float4x4 PreviousViewProjectionMatrix;

float Epsilon = 0.00033f;

texture Mask;
sampler maskSampler = sampler_state
{
	Texture = (Mask);
	AddressU = Clamp;
	AddressV = Clamp;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
};

float2 HalfPixel;
sampler Screen : register(s0);

texture DepthMap;
sampler depthSampler = sampler_state
{
	Texture = (DepthMap);
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = None;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TexCoord : TexCoord0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TexCoord0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = float4(input.Position.xyz, 1);
	output.TexCoord = input.TexCoord;

	return output;
}


float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{

	float mask = tex2D(maskSampler, texCoord).a;
	float4 color = tex2D(Screen, texCoord);

	if (mask == 0)
	{
		return color;
	}

	texCoord -= HalfPixel;

	float zOverW = 1 - tex2D(depthSampler, texCoord);
	// H is the viewport position at this pixel in the range -1 to 1.  
	float4 H = float4(texCoord.x * 2 - 1, (1 - texCoord.y) * 2 - 1,
		zOverW, 1);
	// Transform by the view-projection inverse.  
	float4 D = mul(H, ViewProjectionInverseMatrix);
	// Divide by w to get the world position.  
	float4 worldPos = D / D.w;

	// Current viewport position  
	float4 currentPos = H;
	// Use the world position, and transform by the previous view-  
	// projection matrix.  
	float4 previousPos = mul(worldPos, PreviousViewProjectionMatrix);
	// Convert to nonhomogeneous points [-1,1] by dividing by w.  
	previousPos /= previousPos.w;
	// Use this frame's position and last frame's to compute the pixel  
	// velocity.  
	float2 velocity = (currentPos - previousPos) / 2.f;
	// Decrease the "feel" of speed by faking lower speed	
	velocity /= 4.0f;

	// Get the initial color at this pixel.  
	texCoord += velocity;

	float4 blendedColor = color;
	for (int i = 1; i < g_numSamples; ++i, texCoord += velocity)
	{
		// Sample the color buffer along the velocity vector.  
		float4 currentColor = tex2D(Screen, texCoord);
		float depth = 1 - tex2D(depthSampler, texCoord).r;
		if (abs(zOverW - depth) > Epsilon)
		{
			blendedColor = lerp(blendedColor, color, 1.0f/(i+1));
		}
		else
		{
			blendedColor = lerp(blendedColor, currentColor, 1.0f / (i + 1));
		}
	}
	return blendedColor;
}

technique MotionBlur
{
	pass p0
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}