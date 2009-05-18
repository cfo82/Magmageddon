
//-----------------------------------------------------------------------------------------------------------
UpdateParticlesPixelShaderOutput UpdateParticlesPixelShader(
	float2 ParticleCoordinate : TEXCOORD0
)
{
	UpdateParticlesPixelShaderOutput output;
	
	float4 position_sample = tex2D(PositionSampler, ParticleCoordinate);
	float4 velocity_sample = tex2D(VelocitySampler, ParticleCoordinate);
	
	float3 current_position = position_sample.xyz;
	float3 current_velocity = velocity_sample.xyz;

	float3 velocity = current_position;
	float3 position = current_velocity + Dt*velocity;
	float time_to_death = position_sample.w + Dt*-1;
	
	output.PositionTimeToDeath = float4(position, time_to_death);
	output.Velocity = float4(velocity, 0);

    return output;
}
