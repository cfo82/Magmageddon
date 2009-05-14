#include "../CreateParticles.fx"
#include "../UpdateParticles.fx"
#include "../RenderParticles.fx"



//-----------------------------------------------------------------------------------------------------------
float SnowParticleLifetime = 200;
float SnowVelocityDamping = 0.05;
float SnowParticleMass = 0.01;
float3 Gravity = float3(0,-9.81,0);
float3 WindForce;




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
	
	float4 position_sample = tex2D(UpdateParticlesPositionSampler, ParticleCoordinate);
	float4 velocity_sample = tex2D(UpdateParticlesVelocitySampler, ParticleCoordinate);

	float current_time_to_death = position_sample.w;
	float age = SnowParticleLifetime-current_time_to_death;
	float normalized_age = age/SnowParticleLifetime;
	float normalized_time_to_death = 1-normalized_age; 
	float3 current_position = position_sample.xyz;
	float3 current_velocity = velocity_sample.xyz;
	
	float3 force = Gravity + WindForce;
	
	float3 next_velocity = current_velocity - current_velocity*SnowVelocityDamping + Dt*force;
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
RenderParticlesVertexShaderOutput RenderSnowVertexShader(
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
	    
		float normalizedAge = 1.0 - position_sampler_value.w/SnowParticleLifetime;
	    
		output.Position = mul(view_position, Projection);
		output.Size = 12;
		output.Color = float4(1,1,1,1);
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
float4 RenderSnowPixelShader(
	RenderParticlesVertexShaderOutput input,
#ifdef XBOX
	float2 particleCoordinate : SPRITETEXCOORD
#else
	float2 particleCoordinate : TEXCOORD0
#endif
) : COLOR0
{
    return input.Color*tex2D(RenderParticlesSpriteSampler, particleCoordinate/2);
}




//-----------------------------------------------------------------------------------------------------------
technique RenderParticles
{
    pass Pass1
    {
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
        BlendOp = Add;
        AlphaTestEnable = true;
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
