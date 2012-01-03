#include "../Params.inc"
#include "../Structs.inc"
#include "../Samplers.inc"
#include "../DefaultCreateParticles.inc"
#include "../DefaultUpdateParticles.inc"
#include "../DefaultRenderParticles.inc"



//-----------------------------------------------------------------------------------------------------------
float ExplosionParticleLifetime = 2.6;
float ExplosionVelocityDamping = 0.8;
float ExplosionSize = 40;
float ExplosionRgbMultiplier = 0.33;
float ExplosionDotMultiplier = 0.025;





//-----------------------------------------------------------------------------------------------------------
CreateParticlesPixelShaderOutput CreateExplosionPixelShader(
	CreateParticlesVertexShaderOutput input,
	int2 inScreenPos : VPOS
)
{
	// clip if we're not on the correct screen pos!
	int2 targetDiff = input.TargetTextureCoordinate - inScreenPos;
	clip(-abs(targetDiff));

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
	
	float4 position_sample = tex2D(PositionSampler, ParticleCoordinate);
	float4 velocity_sample = tex2D(VelocitySampler, ParticleCoordinate);
	
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
	output.TextureCoordinate = input.Corner / 2.0f + 0.5f;

	float4 position_sampler_value = tex2Dlod(PositionSampler, float4(input.ParticleCoordinate , 0, 0));

	if (position_sampler_value.w > 0)
	{

		float3 position = position_sampler_value.xyz;

		float4 world_position = float4(position,1);
		float4 view_position = mul(world_position, View);
	    
	    float time_to_life = position_sampler_value.w;
	    float normalized_time_to_death = time_to_life/ExplosionParticleLifetime;
		float normalized_age = 1.0 - time_to_life/ExplosionParticleLifetime;
	    
		output.Size = ExplosionSize*pow(normalized_time_to_death,4);
		output.Position = mul(view_position, Projection);
		output.Position.xy += input.Corner * output.Size;
		output.Color = float4(1,1,1,1.0-normalized_age);
	}
	else
	{
		output.Position = float4(-10,-10,0,-2);
		output.Size = 0;
		output.Color = float4(0,0,0,0);
	}
	
    return output;
}




//-----------------------------------------------------------------------------------------------------------
float4 RenderExplosionPixelShader(
	RenderParticlesVertexShaderOutput input
) : COLOR0
{
    float4 color = input.Color*tex2D(SpriteSampler, input.TextureCoordinate/4);
	clip(color.a - 0.5f);
	color *= 4;
    color.rgb *= dot(float3(ExplosionDotMultiplier, ExplosionDotMultiplier, ExplosionDotMultiplier), color.rgb);
    color.rgb *= input.Color.a;
    color.rgb *= ExplosionRgbMultiplier;
    return color;
}




//-----------------------------------------------------------------------------------------------------------
technique RenderParticles
{
    pass Pass1
    {
        AlphaBlendEnable = false;
        //AlphaTestEnable = true;
        //AlphaFunc = Greater;
        //AlphaRef = 0.5;
        
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
