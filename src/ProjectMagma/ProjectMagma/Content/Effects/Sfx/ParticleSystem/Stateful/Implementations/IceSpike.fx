#include "../Params.inc"
#include "../Structs.inc"
#include "../Samplers.inc"
#include "../DefaultCreateParticles.inc"
#include "../DefaultUpdateParticles.inc"
#include "../DefaultRenderParticles.inc"



//-----------------------------------------------------------------------------------------------------------
#define MAX_ICESPIKE_COUNT 30




//-----------------------------------------------------------------------------------------------------------
float IceSpikeParticleLifetime = 1.2;
float IceSpikeRotationTime = 0.5;
float IceSpikeDamping = 0.8;
//float3 IceSpikePosition;
//float3 IceSpikeDirection;
//float IceSpikeGravityStart = 0.1;
float3 IceSpikePositionArray[MAX_ICESPIKE_COUNT];
float3 IceSpikeDirectionArray[MAX_ICESPIKE_COUNT];
float IceSpikeGravityStartArray[MAX_ICESPIKE_COUNT];
float IceSpikeRotationSpeed = 3000;




//-----------------------------------------------------------------------------------------------------------
CreateParticlesPixelShaderOutput CreateIceSpikePixelShader(
	CreateParticlesVertexShaderOutput input
)
{
	CreateParticlesPixelShaderOutput output;
	output.PositionTimeToDeath = float4(input.ParticlePosition, IceSpikeParticleLifetime);
	output.Velocity = float4(input.ParticleVelocity, input.EmitterIndex);
	return output;
}




//-----------------------------------------------------------------------------------------------------------
technique CreateParticles
{
    pass MainPass
    {
        VertexShader = compile vs_3_0 CreateParticlesVertexShader();
        PixelShader = compile ps_3_0 CreateIceSpikePixelShader();
    }
}




//-----------------------------------------------------------------------------------------------------------
UpdateParticlesPixelShaderOutput UpdateIceSpikePixelShader(
	float2 ParticleCoordinate : TEXCOORD0
)
{
	UpdateParticlesPixelShaderOutput output;
	
	float4 position_sample = tex2D(PositionSampler, ParticleCoordinate);
	float4 velocity_sample = tex2D(VelocitySampler, ParticleCoordinate);
	//float4 random_sample = tex2D(RandomSampler, float2(ParticleCoordinate.x*31, ParticleCoordinate.y*57));
	
	float current_time_to_death = position_sample.w;
	float age = IceSpikeParticleLifetime-current_time_to_death;
	float normalized_age = age/IceSpikeParticleLifetime;
	float normalized_time_to_death = 1-normalized_age; 
	float3 current_position = position_sample.xyz;
	float3 current_velocity = velocity_sample.xyz;
	
	int emitter_index = floor(velocity_sample.w);
	float3 ice_spike_position = IceSpikePositionArray[emitter_index];
	float3 ice_spike_direction = IceSpikeDirectionArray[emitter_index];
	float ice_spike_gravity_start = IceSpikeGravityStartArray[emitter_index];

	// calculate in world-space...
	float3 projected_position = ice_spike_position + dot(-ice_spike_direction, current_position - ice_spike_position) * (-ice_spike_direction);
	float3 to_position = current_position - projected_position;
	float3 normalized_to_position = normalize(to_position);
	float3 normal = cross(normalized_to_position, -ice_spike_direction);
	// calculate the force to apply in object space!
	float rotation_speed = /*random_sample.a*/IceSpikeRotationSpeed;
	float centralize_force = IceSpikeRotationSpeed/2 + /*random_sample.g*/IceSpikeRotationSpeed*2.5;
	float3 rotation_force = (normal * rotation_speed - normalized_to_position * centralize_force);
	// apply rotation force only for n seconds... and make it weaker towards the end...
	float rotation_age = min(age,IceSpikeRotationTime);
	float normalized_rotation_age = rotation_age/IceSpikeRotationTime;
	float normalized_rotation_age_4 = pow(normalized_rotation_age,4);
	rotation_force = rotation_force * (1-normalized_rotation_age_4);
	
	// calculate gravity... blend in slowly at the end of the rotation force
	float gravity_age = max(0,age-ice_spike_gravity_start);
	float normalized_gravity_age = gravity_age/(IceSpikeParticleLifetime-ice_spike_gravity_start);
	float3 gravity_force = float3(0,-9.81,0) * 1000 * pow(normalized_gravity_age,2);
	
	float3 force = rotation_force + gravity_force;
	
	float3 next_velocity = velocity_sample.xyz*IceSpikeDamping + Dt*force;
	float3 next_position = current_position + Dt*next_velocity;
	float next_time_to_death = current_time_to_death + Dt*-1;
	
	output.PositionTimeToDeath = float4(next_position, next_time_to_death);
	output.Velocity = float4(next_velocity, emitter_index);

    return output;
}




//-----------------------------------------------------------------------------------------------------------
technique UpdateParticles
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 UpdateIceSpikePixelShader();
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
		float4 random_sampler_value = tex2Dlod(RandomSampler, float4(input.ParticleCoordinate.x*31, input.ParticleCoordinate.y*57, 0, 0));
		float3 position = position_sampler_value.xyz;

		float4 world_position = float4(position,1);
		float4 view_position = mul(world_position, View);
	    
		float normalizedAge = 1.0 - position_sampler_value.w/IceSpikeParticleLifetime;
	    
		output.PositionCopy = output.Position = mul(view_position, Projection);
		output.Size = 3 + 5*random_sampler_value.b;
		output.Color = float4(1,1,1,1.0-normalizedAge);
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
	float4 value = tex2D(SpriteSampler, particleCoordinate);
	value *= value.a;
    return input.Color.a*value;
}




//-----------------------------------------------------------------------------------------------------------
technique RenderParticles
{
    pass Pass1
    {
        AlphaBlendEnable = true;
        //SrcBlend = SrcAlpha;
        //DestBlend = InvSrcAlpha;
        SrcBlend = One;
        DestBlend = One;
        BlendOp = Add;
        AlphaTestEnable = false;
        AlphaFunc = Greater;
        AlphaRef = 0.25;
        CullMode = None;
        
        ZEnable = true;
        ZWriteEnable = false;        
    
        VertexShader = compile vs_3_0 RenderIceSpikeVertexShader();
        PixelShader = compile ps_3_0 RenderIceSpikePixelShader();
    }
}
