//-----------------------------------------------------------------------------------------------------------
float4x4 View;
float4x4 Projection;
float2 StartSize;
float2 EndSize;
float4 MinColor = float4(1,1,1,1);
float4 MaxColor = float4(1,1,1,1);
float ViewportHeight;

texture RenderParticlesPositionTexture;
texture RenderParticlesSpriteTexture;




//-----------------------------------------------------------------------------------------------------------
sampler RenderParticlesPositionSampler = sampler_state
{
    Texture = (RenderParticlesPositionTexture);
    
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    
    AddressU = Clamp;
    AddressV = Clamp;
};
sampler RenderParticlesSpriteSampler = sampler_state
{
    Texture = (RenderParticlesSpriteTexture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};




//-----------------------------------------------------------------------------------------------------------
struct RenderParticlesVertexShaderInput
{
	float2 ParticleCoordinate : TEXCOORD0;
};

struct RenderParticlesVertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float Size : PSIZE0;
};




//-----------------------------------------------------------------------------------------------------------
float ComputeParticleSize(float4 projectedPosition,
                          float randomValue, float normalizedAge)
{
    // Apply a random factor to make each particle a slightly different size.
    float startSize = lerp(StartSize.x, StartSize.y, randomValue);
    float endSize = lerp(EndSize.x, EndSize.y, randomValue);
    
    // Compute the actual size based on the age of the particle.
    float size = lerp(startSize, endSize, normalizedAge);
    
    // Project the size into screen coordinates.
    return size * Projection._m11 / projectedPosition.w * ViewportHeight / 2;
}




//-----------------------------------------------------------------------------------------------------------
float4 ComputeParticleColor(float randomValue, float normalizedAge)
{
    // Apply a random factor to make each particle a slightly different color.
    float4 color = lerp(MinColor, MaxColor, randomValue);
    
    // Fade the alpha based on the age of the particle. This curve is hard coded
    // to make the particle fade in fairly quickly, then fade out more slowly:
    // plot x*(1-x)*(1-x) for x=0:1 in a graphing program if you want to see what
    // this looks like. The 6.7 scaling factor normalizes the curve so the alpha
    // will reach all the way up to fully solid.
    
    color.a *= normalizedAge * (1-normalizedAge) * (1-normalizedAge) * 7.0;
   
    return color;
}




//-----------------------------------------------------------------------------------------------------------
RenderParticlesVertexShaderOutput RenderParticlesVertexShader(
	RenderParticlesVertexShaderInput input
)
{
    RenderParticlesVertexShaderOutput output;

	float4 position_sampler_value = tex2Dlod(RenderParticlesPositionSampler, float4(input.ParticleCoordinate , 0, 0));

	if (position_sampler_value.w > 0)
	{

		float3 position = position_sampler_value.xyz;

		float4 world_position = float4(position,1);
		float4 view_position = mul(world_position, View);
	    
		float normalizedAge = 1.0 - position_sampler_value.w/7.0f;
	    
		output.Position = mul(view_position, Projection);
		output.Size = ComputeParticleSize(output.Position, 0.5, normalizedAge);
		output.Color = ComputeParticleColor(0.25, normalizedAge);
	}
	else
	{
		output.Position = float4(-10,-10,0,0);
		output.Size = 0;
		output.Color = float4(0,0,0,0);
	}
	
    return output;
}




//-----------------------------------------------------------------------------------------------------------
float4 RenderParticlesPixelShader(
	RenderParticlesVertexShaderOutput input,
#ifdef XBOX
	float2 particleCoordinate : SPRITETEXCOORD
#else
	float2 particleCoordinate : TEXCOORD0
#endif

) : COLOR0
{
    return input.Color*tex2D(RenderParticlesSpriteSampler, particleCoordinate);
}
