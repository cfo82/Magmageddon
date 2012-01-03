//-----------------------------------------------------------------------------------------------------------
CreateParticlesVertexShaderOutput CreateParticlesVertexShader(
	CreateParticlesVertexShaderInput input
)
{
    CreateParticlesVertexShaderOutput output;

	output.ParticleCoordinate = float4(input.ParticleCoordinate.x, -input.ParticleCoordinate.y, 0, 1);
	output.TargetTextureCoordinate = input.TargetTextureCoordinate;

	output.ParticlePosition = input.ParticlePosition;
	output.ParticleVelocity = input.ParticleVelocity;
	output.EmitterIndex = input.EmitterIndex;

    return output;
}




//-----------------------------------------------------------------------------------------------------------
CreateParticlesPixelShaderOutput CreateParticlesPixelShader(
	CreateParticlesVertexShaderOutput input,
	int2 inScreenPos : VPOS
)
{
	CreateParticlesPixelShaderOutput output;
	output.PositionTimeToDeath = float4(input.ParticlePosition, 7.0f);
	output.Velocity = float4(input.ParticleVelocity, 0);
	return output;
}
