#include "../Params.inc"
#include "../Structs.inc"
#include "../Samplers.inc"
#include "../DefaultCreateParticles.inc"
#include "../DefaultUpdateParticles.inc"
#include "../DefaultRenderParticles.inc"



//-----------------------------------------------------------------------------------------------------------
float FlamethrowerParticleLifetime = 0.7;
float FlamethrowerVelocityDamping = 0.5;
float3 Gravity = float3(0,-9.81,0);




//-----------------------------------------------------------------------------------------------------------
CreateParticlesPixelShaderOutput CreateFlamethrowerPixelShader(
	CreateParticlesVertexShaderOutput input
)
{
	CreateParticlesPixelShaderOutput output;
	output.PositionTimeToDeath = float4(input.ParticlePosition, FlamethrowerParticleLifetime);
	output.Velocity = float4(input.ParticleVelocity, 0);
	return output;
}




//-----------------------------------------------------------------------------------------------------------
technique CreateParticles
{
    pass MainPass
    {
        VertexShader = compile vs_3_0 CreateParticlesVertexShader();
        PixelShader = compile ps_3_0 CreateFlamethrowerPixelShader();
    }
}




//-----------------------------------------------------------------------------------------------------------
UpdateParticlesPixelShaderOutput UpdateFlamethrowerPixelShader(
	float2 ParticleCoordinate : TEXCOORD0
)
{
	UpdateParticlesPixelShaderOutput output;
	
	float4 position_sample = tex2D(PositionSampler, ParticleCoordinate);
	float4 velocity_sample = tex2D(VelocitySampler, ParticleCoordinate);
	
	float current_time_to_death = position_sample.w;
	float age = FlamethrowerParticleLifetime-current_time_to_death;
	float normalized_age = age/FlamethrowerParticleLifetime;
	float normalized_time_to_death = 1-normalized_age; 
	float3 current_position = position_sample.xyz;
	float3 current_velocity = velocity_sample.xyz;
	
	float3 force = -Gravity /*+ float3(0,1,0)*max(0, normalized_age-0.6)*40000*/;
	
	float3 next_velocity = current_velocity - Dt * current_velocity * FlamethrowerVelocityDamping + Dt * force;
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
        PixelShader = compile ps_2_0 UpdateFlamethrowerPixelShader();
    }
}




//-----------------------------------------------------------------------------------------------------------
RenderParticlesVertexShaderOutput RenderIceSpikeVertexShader(
	RenderParticlesVertexShaderInput input
)
{
    RenderParticlesVertexShaderOutput output;

	float4 position_sampler_value = tex2Dlod(PositionSampler, float4(input.ParticleCoordinate , 0, 0));

	if (position_sampler_value.w > 0)
	{

		float3 position = position_sampler_value.xyz;

		float4 world_position = float4(position,1);
		float4 view_position = mul(world_position, View);
	    
		float normalizedAge = 1.0 - position_sampler_value.w/FlamethrowerParticleLifetime;
	    
		output.PositionCopy = output.Position = mul(view_position, Projection);
		output.Size = lerp(6,50,normalizedAge) * (1-pow(normalizedAge,6));
		//output.Color = float4(1,1-normalizedAge,(1-normalizedAge)/4,1.0);
		output.Color = float4(1,1,1,1-pow(normalizedAge,6));
	}
	else
	{
		output.PositionCopy = output.Position = float4(-10,-10,0,0);
		output.Size = 0;
		output.Color = float4(0,0,0,0);
	}
	
    return output;
}




//-----------------------------------------------------------------------------------------------------------
float4 RenderIceSpikePixelShader(
	RenderParticlesVertexShaderOutput input,
#ifdef XBOX
	float2 particleCoordinate : SPRITETEXCOORD
#else
	float2 particleCoordinate : TEXCOORD0
#endif
) : COLOR0
{
    return tex2D(SpriteSampler, particleCoordinate/4)*0.025*input.Color.a;
}




//-----------------------------------------------------------------------------------------------------------
technique RenderParticles
{
    pass Pass1
    {
       
        //AlphaTestEnable = true;
        //AlphaFunc = Greater;
        //AlphaRef = 0.1;
        
        AlphaBlendEnable = true;
        SrcBlend = One;
        DestBlend = One;
        BlendOp = Add;
        
        
        ZEnable = true;
        ZWriteEnable = false;        

        VertexShader = compile vs_3_0 RenderIceSpikeVertexShader();
        PixelShader = compile ps_3_0 RenderIceSpikePixelShader();
    }
}
