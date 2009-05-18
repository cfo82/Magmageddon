//-----------------------------------------------------------------------------------------------------------
CreateParticlesVertexShaderOutput CreateParticlesVertexShader(
	CreateParticlesVertexShaderInput input
)
{
    CreateParticlesVertexShaderOutput output;

	output.ParticleCoordinate = float4(input.ParticleCoordinate, 0, 1);
	output.ParticlePosition = input.ParticlePosition;
	output.ParticleVelocity = input.ParticleVelocity;
	output.Size = 1;

    return output;
}




//-----------------------------------------------------------------------------------------------------------
CreateParticlesPixelShaderOutput CreateParticlesPixelShader(
	CreateParticlesVertexShaderOutput input
)
{
	CreateParticlesPixelShaderOutput output;
	output.PositionTimeToDeath = float4(input.ParticlePosition, 7.0f);
	output.Velocity = float4(input.ParticleVelocity, 0);
	return output;
}
