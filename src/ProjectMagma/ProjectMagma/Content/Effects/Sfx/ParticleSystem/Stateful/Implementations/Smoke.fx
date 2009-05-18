#include "../Params.inc"
#include "../Structs.inc"
#include "../Samplers.inc"
#include "../DefaultCreateParticles.inc"
#include "../DefaultUpdateParticles.inc"
#include "../DefaultRenderParticles.inc"



//-----------------------------------------------------------------------------------------------------------
technique CreateParticles
{
    pass MainPass
    {
        VertexShader = compile vs_3_0 CreateParticlesVertexShader();
        PixelShader = compile ps_3_0 CreateParticlesPixelShader();
    }
}




//-----------------------------------------------------------------------------------------------------------
float3 Gravity;




//-----------------------------------------------------------------------------------------------------------
UpdateParticlesPixelShaderOutput UpdateSmokePixelShader(
	float2 ParticleCoordinate : TEXCOORD0
)
{
	UpdateParticlesPixelShaderOutput output;
	
	float4 position_sample = tex2D(PositionSampler, ParticleCoordinate);
	float4 velocity_sample = tex2D(VelocitySampler, ParticleCoordinate);
	
	float3 current_position = position_sample.xyz;
	float3 current_velocity = velocity_sample.xyz;

	float3 velocity = current_position + Dt*Gravity;
	float3 position = current_velocity + Dt*velocity;
	float time_to_death = position_sample.w + Dt*-1;
	
	output.PositionTimeToDeath = float4(position, time_to_death);
	output.Velocity = float4(velocity, 0);

    return output;
}




//-----------------------------------------------------------------------------------------------------------
technique UpdateParticles
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 UpdateSmokePixelShader();
    }
}




//-----------------------------------------------------------------------------------------------------------
technique RenderParticles
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 RenderParticlesVertexShader();
        PixelShader = compile ps_3_0 RenderParticlesPixelShader();
    }
}
