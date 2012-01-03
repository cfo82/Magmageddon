#include "../Params.inc"
#include "../Structs.inc"
#include "../Samplers.inc"
#include "../DefaultCreateParticles.inc"
#include "../DefaultUpdateParticles.inc"
#include "../DefaultRenderParticles.inc"



//-----------------------------------------------------------------------------------------------------------
float SnowParticleLifetime = 25;
float SnowVelocityDamping = 0.2;
float SnowParticleMass = 0.01;
float SnowMeltingStart = 600;
float SnowMeltingEnd = 300;
float SnowMaxAlpha = 0.6;
float2 ViewPortScale;
float3 Gravity = float3(0,-9.81,0);
float3 WindForce;
float SnowBaseSize = 5;
float SnowRandomSizeModification = 8;




//-----------------------------------------------------------------------------------------------------------
struct RenderSnowParticlesVertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float4 RandomValues : COLOR1;
	float2 TextureCoordinate : TEXCOORD0;
    float Size : PSIZE0;
};




//-----------------------------------------------------------------------------------------------------------
CreateParticlesPixelShaderOutput CreateSnowPixelShader(
	CreateParticlesVertexShaderOutput input,
	int2 inScreenPos : VPOS
)
{
	// clip if we're not on the correct screen pos!
	int2 targetDiff = input.TargetTextureCoordinate - inScreenPos;
	clip(-abs(targetDiff));

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
	
	float4 position_sample = tex2D(PositionSampler, ParticleCoordinate);
	float4 velocity_sample = tex2D(VelocitySampler, ParticleCoordinate);

	float current_time_to_death = position_sample.w;
	float age = SnowParticleLifetime-current_time_to_death;
	float normalized_age = age/SnowParticleLifetime;
	float normalized_time_to_death = 1-normalized_age; 
	float3 current_position = position_sample.xyz;
	float3 current_velocity = velocity_sample.xyz;
	
	float3 force = Gravity + WindForce;
	
	float3 next_velocity = current_velocity - Dt*current_velocity*SnowVelocityDamping + Dt*force;
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
RenderSnowParticlesVertexShaderOutput RenderSnowVertexShader(
	RenderParticlesVertexShaderInput input
)
{
    RenderSnowParticlesVertexShaderOutput output;
	output.TextureCoordinate = input.Corner / 2.0f + 0.5f;

	float4 position_sampler_value = tex2Dlod(PositionSampler, float4(input.ParticleCoordinate , 0, 0));

	if (position_sampler_value.w > 0)
	{
		float3 position = position_sampler_value.xyz;
		//float4 random_sampler_value = tex2Dlod(RandomSampler, float4(input.ParticleCoordinate.x*31, input.ParticleCoordinate.y*57, 0, 0));
		float random_sampler_value = float4(0.323, 0.584, 0.9823, 0.239);

		float4 world_position = float4(position,1);
		float4 view_position = mul(world_position, View);
	    
		float normalizedAge = 1.0 - position_sampler_value.w/SnowParticleLifetime;
	    
		output.Position = mul(view_position, Projection);
		output.Size = SnowBaseSize + random_sampler_value.x*SnowRandomSizeModification;
		output.Position.xy += input.Corner*output.Size;
		
		// calculate alpha lerp between given SnowMeltingStart/SnowMeltingEnd based on height
		float amount = (SnowMeltingStart - world_position.y) / (SnowMeltingStart - SnowMeltingEnd);
		// clamp to range between [0,1]
		amount = min(1, amount);
		amount = max(0, amount);
		float alpha = lerp(SnowMaxAlpha, 0, amount);
		
		//float calculatedAlpha = min(SnowMaxAlpha, 2*(1-normalizedAge) * lerp(1.4, 0.6, min(1,normalizedAge)));
		//float alpha = normalizedAge<0.5?1:calculatedAlpha;
		
		output.Color = float4(1,1,1,alpha);
		output.RandomValues = random_sampler_value;
	}
	else
	{
		output.Position = float4(-10,-10,0,0);
		output.Size = 0;
		output.Color = float4(0,0,0,0);
		output.RandomValues = float4(0,0,0,0);
	}
	
    return output;
}




//-----------------------------------------------------------------------------------------------------------
float4 RenderSnowPixelShader(
	RenderSnowParticlesVertexShaderOutput input
) : COLOR0
{
	//int spriteNumber = ceil(input.RandomValues.y*4) - 1;
	//int horizontalIndex = spriteNumber/2;
	//int verticalIndex = spriteNumber%2;

	float spriteNumber = ceil(input.RandomValues.y*4) - 1;
	float horizontalIndex = floor(spriteNumber*0.5);
	float verticalIndex = spriteNumber - horizontalIndex*2;

	float2 modifiedTextureCoordinates = float2(input.TextureCoordinate.x/2 + horizontalIndex*0.5, input.TextureCoordinate.y/2 + verticalIndex*0.5);
    return input.Color*tex2D(SpriteSampler, modifiedTextureCoordinates);
}




//-----------------------------------------------------------------------------------------------------------
technique RenderParticles
{
    pass Pass1
    {
        AlphaBlendEnable = true;
        //SrcBlend = SrcAlpha;
        //DestBlend = InvSrcAlpha;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
        BlendOp = Add;
        //AlphaTestEnable = false;
        //AlphaFunc = Greater;
        //AlphaRef = 0.25;
        ZEnable = false;
        ZWriteEnable = false;
        CullMode = None;
        
        ZEnable = true;
        ZWriteEnable = false;        
    
        VertexShader = compile vs_3_0 RenderSnowVertexShader();
        PixelShader = compile ps_3_0 RenderSnowPixelShader();
    }
}
