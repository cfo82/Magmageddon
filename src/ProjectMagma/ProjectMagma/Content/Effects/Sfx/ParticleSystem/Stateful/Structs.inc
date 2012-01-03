//-----------------------------------------------------------------------------------------------------------
struct CreateParticlesVertexShaderInput
{
	float3 ParticlePosition : POSITION0;
	float3 ParticleVelocity : NORMAL0;
    float2 ParticleCoordinate : TEXCOORD0;
	int2 TargetTextureCoordinate : POSITION1;
    float  EmitterIndex : TEXCOORD1;
};
struct CreateParticlesVertexShaderOutput
{
    float4 ParticleCoordinate : POSITION0;
    float3 ParticlePosition : TEXCOORD0;
    float3 ParticleVelocity : TEXCOORD1;
    float EmitterIndex : TEXCOORD2;
	int2 TargetTextureCoordinate : TEXCOORD3;
};
struct CreateParticlesPixelShaderOutput
{
	float4 PositionTimeToDeath : COLOR0;
	float4 Velocity : COLOR1;
};




//-----------------------------------------------------------------------------------------------------------
struct RenderParticlesVertexShaderInput
{
	int2 Corner : POSITION0;
	float2 ParticleCoordinate : TEXCOORD0;
};
struct RenderParticlesVertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
	float2 TextureCoordinate : TEXCOORD0;
    float Size : PSIZE0;
};
/*struct RenderParticlesVertexShaderOutput
{
    float4 Position : POSITION0;
    float4 PositionCopy : COLOR1;
    float4 Color : COLOR0;
	float2 TextureCoordinate : TEXCOORD0;
    float Size : PSIZE0;
};*/




//-----------------------------------------------------------------------------------------------------------
struct UpdateParticlesPixelShaderOutput
{
	float4 PositionTimeToDeath : COLOR0;
	float4 Velocity : COLOR1;
};
