#include "../Params.inc"
#include "../Structs.inc"
#include "../Samplers.inc"
#include "../DefaultCreateParticles.inc"
#include "../DefaultUpdateParticles.inc"
#include "../DefaultRenderParticles.inc"



//-----------------------------------------------------------------------------------------------------------
float SnowParticleLifetime = 25;
float SnowVelocityDamping = 0.2;
float SnowParticleMass = 0.01;
float SnowMaxAlpha = 0.6;		
float3 Gravity = float3(0,-9.81,0);
float3 WindForce;




//-----------------------------------------------------------------------------------------------------------
struct RenderSnowParticlesVertexShaderOutput
{
    float4 Position : POSITION0;
    float4 PositionCopy : POSITION1;
    float4 Color : COLOR0;
    float4 RandomValues : COLOR1;
    float Size : PSIZE0;
};




//-----------------------------------------------------------------------------------------------------------
CreateParticlesPixelShaderOutput CreateSnowPixelShader(
	CreateParticlesVertexShaderOutput input
)
{
	CreateParticlesPixelShaderOutput output;
	output.PositionTimeToDeath = float4(input.ParticlePosition, SnowParticleLifetime);
	output.Velocity = float4(input.ParticleVelocity, 0);
	return output;
}




//-----------------------------------------------------------------------------------------------------------
technique CreateParticles
{
    pass MainPass
    {
        VertexShader = compile vs_3_0 CreateParticlesVertexShader();
        PixelShader = compile ps_3_0 CreateSnowPixelShader();
    }
}




//-----------------------------------------------------------------------------------------------------------
UpdateParticlesPixelShaderOutput UpdateSnowPixelShader(
	float2 ParticleCoordinate : TEXCOORD0
)
{
	UpdateParticlesPixelShaderOutput output;
	
	float4 position_sample = tex2D(PositionSampler, ParticleCoordinate);
	float4 velocity_sample = tex2D(VelocitySampler, ParticleCoordinate);

	float current_time_to_death = position_sample.w;
	float age = SnowParticleLifetime-current_time_to_death;
	float normalized_age = age/SnowParticleLifetime;
	float normalized_time_to_death = 1-normalized_age; 
	float3 current_position = position_sample.xyz;
	float3 current_velocity = velocity_sample.xyz;
	
	float3 force = Gravity + WindForce;
	
	float3 next_velocity = current_velocity - Dt*current_velocity*SnowVelocityDamping + Dt*force;
	float3 next_position = current_position + Dt*next_velocity;
	float next_time_to_death = current_time_to_death + Dt*-1;
	
	output.PositionTimeToDeath = float4(next_position, next_time_to_death);
	output.Velocity = float4(next_velocity, 0);

    return output;
}




//-----------------------------------------------------------------------------------------------------------
technique UpdateParticles
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 UpdateSnowPixelShader();
    }
}




//-----------------------------------------------------------------------------------------------------------
RenderSnowParticlesVertexShaderOutput RenderSnowVertexShader(
	RenderParticlesVertexShaderInput input
)
{
    RenderSnowParticlesVertexShaderOutput output;

	float4 position_sampler_value = tex2Dlod(PositionSampler, float4(input.ParticleCoordinate , 0, 0));

	if (position_sampler_value.w > 0)
	{
		float3 position = position_sampler_value.xyz;
		float4 random_sampler_value = tex2Dlod(RandomSampler, float4(input.ParticleCoordinate.x*31, input.ParticleCoordinate.y*57, 0, 0));

		float4 world_position = float4(position,1);
		float4 view_position = mul(world_position, View);
	    
		float normalizedAge = 1.0 - position_sampler_value.w/SnowParticleLifetime;
	    
		output.PositionCopy = output.Position = mul(view_position, Projection);
		output.Size = 5 + random_sampler_value.x*8;
		
		float calculatedAlpha = min(SnowMaxAlpha, 2*(1-normalizedAge) * lerp(1.4, 0.6, min(1,normalizedAge)));
		//float alpha = normalizedAge<0.5?1:calculatedAlpha;
		
		output.Color = float4(1,1,1,calculatedAlpha);
		output.RandomValues = random_sampler_value;
	}
	else
	{
		output.PositionCopy = output.Position = float4(-10,-10,0,0);
		output.Size = 0;
		output.Color = float4(0,0,0,0);
		output.RandomValues = float4(0,0,0,0);
	}
	
    return output;
}




//-----------------------------------------------------------------------------------------------------------
float4 RenderSnowPixelShader(
	RenderSnowParticlesVertexShaderOutput input,
#ifdef XBOX
	float2 particleCoordinate : SPRITETEXCOORD
#else
	float2 particleCoordinate : TEXCOORD0
#endif
) : COLOR0
{
	int spriteNumber = ceil(input.RandomValues.y*4) - 1;
	int horizontalIndex = spriteNumber/2;
	int verticalIndex = spriteNumber%2;

	float2 modifiedTextureCoordinates = float2(particleCoordinate.x/2 + horizontalIndex*0.5, particleCoordinate.y/2 + verticalIndex*0.5);
    return input.Color*tex2D(SpriteSampler, modifiedTextureCoordinates);
}




//-----------------------------------------------------------------------------------------------------------
technique RenderParticles
{
    pass Pass1
    {
        AlphaBlendEnable = true;
        //SrcBlend = SrcAlpha;
        //DestBlend = InvSrcAlpha;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
        BlendOp = Add;
        AlphaTestEnable = false;
        AlphaFunc = Greater;
        AlphaRef = 0.25;
        ZEnable = false;
        ZWriteEnable = false;
        CullMode = None;
        
        ZEnable = true;
        ZWriteEnable = false;        
    
        VertexShader = compile vs_3_0 RenderSnowVertexShader();
        PixelShader = compile ps_3_0 RenderSnowPixelShader();
    }
}
