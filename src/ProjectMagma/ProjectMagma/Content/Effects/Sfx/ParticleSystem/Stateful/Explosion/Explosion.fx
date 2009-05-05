#include "../CreateParticles.fx"
#include "../UpdateParticles.fx"
#include "../RenderParticles.fx"



//-----------------------------------------------------------------------------------------------------------
float ExplosionParticleLifetime = 2.6;
float ExplosionVelocityDamping = 0.5;




//-----------------------------------------------------------------------------------------------------------
CreateParticlesPixelShaderOutput CreateExplosionPixelShader(
	CreateParticlesVertexShaderOutput input
)
{
	CreateParticlesPixelShaderOutput output;
	output.PositionTimeToDeath = float4(input.ParticlePosition, ExplosionParticleLifetime);
	output.Velocity = float4(input.ParticleVelocity, 0);
	return output;
}




//-----------------------------------------------------------------------------------------------------------
technique CreateParticles
{
    pass MainPass
    {
        VertexShader = compile vs_3_0 CreateParticlesVertexShader();
        PixelShader = compile ps_3_0 CreateExplosionPixelShader();
    }
}




//-----------------------------------------------------------------------------------------------------------
UpdateParticlesPixelShaderOutput UpdateExplosionPixelShader(
	float2 ParticleCoordinate : TEXCOORD0
)
{
	UpdateParticlesPixelShaderOutput output;
	
	float4 position_sample = tex2D(UpdateParticlesPositionSampler, ParticleCoordinate);
	float4 velocity_sample = tex2D(UpdateParticlesVelocitySampler, ParticleCoordinate);
	
	float3 current_position = position_sample.xyz;
	float3 current_velocity = velocity_sample.xyz;
	float current_time_to_death = position_sample.w;

	float3 next_velocity = current_velocity - current_velocity * Dt * ExplosionVelocityDamping;
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
        PixelShader = compile ps_2_0 UpdateExplosionPixelShader();
    }
}




//-----------------------------------------------------------------------------------------------------------
RenderParticlesVertexShaderOutput RenderExplosionVertexShader(
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
	    
	    float time_to_life = position_sampler_value.w;
		float normalizedAge = 1.0 - time_to_life/ExplosionParticleLifetime;
	    
		output.Position = mul(view_position, Projection);
		output.Size = 40;
		output.Color = float4(1,1,1,1.0-normalizedAge);
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
float4 RenderExplosionPixelShader(
	RenderParticlesVertexShaderOutput input,
#ifdef XBOX
	float2 particleCoordinate : SPRITETEXCOORD
#else
	float2 particleCoordinate : TEXCOORD0
#endif
) : COLOR0
{
    float4 color = input.Color*tex2D(RenderParticlesSpriteSampler, particleCoordinate/4);
    color.rgb *= dot(float3(0.025, 0.025, 0.025), color.rgb) * input.Color.a;
    return color;
}




//-----------------------------------------------------------------------------------------------------------
technique RenderParticles
{
    pass Pass1
    {
        AlphaBlendEnable = false;
        AlphaTestEnable = true;
        AlphaFunc = Greater;
        AlphaRef = 0.5;
        
        //AlphaTestEnable = false;
        AlphaBlendEnable = true;
        SrcBlend = One;
        DestBlend = One;
        BlendOp = Add;
        
        
        ZEnable = true;
        ZWriteEnable = false;        
    
    
        VertexShader = compile vs_3_0 RenderExplosionVertexShader();
        PixelShader = compile ps_3_0 RenderExplosionPixelShader();
    }
}
