//-----------------------------------------------------------------------------------------------------------
struct CreateParticlesVertexShaderInput
{
	float3 ParticlePosition : POSITION0;
	float3 ParticleVelocity : NORMAL0;
    float2 ParticleCoordinate : TEXCOORD0;
};
struct CreateParticlesVertexShaderOutput
{
    float4 ParticleCoordinate : POSITION0;
    float3 ParticlePosition : TEXCOORD0;
    float3 ParticleVelocity : TEXCOORD1;
    float Size : PSIZE0;
};
struct CreateParticlesPixelShaderOutput
{
	float4 PositionTimeToDeath : COLOR0;
	float4 Velocity : COLOR1;
};




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

CreateParticlesPixelShaderOutput CreateParticlesPixelShader(
	CreateParticlesVertexShaderOutput input
)
{
	CreateParticlesPixelShaderOutput output;
	output.PositionTimeToDeath = float4(input.ParticlePosition, 7.0f);
	output.Velocity = float4(input.ParticleVelocity, 0);
	return output;
}
