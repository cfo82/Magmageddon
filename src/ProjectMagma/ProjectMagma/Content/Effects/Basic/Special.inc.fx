//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
inline void PerturbIslandTexCoords(inout float2 texCoord, in float nY)
{
	if(abs(nY)<0.4)	
	{
		float4 clouds = tex2D(CloudsSampler, texCoord + RandomOffset);
		float2 perturbation = clouds.gb * 2 * WindStrength - WindStrength;
		texCoord.x += perturbation.x;
	}	
}


//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
inline void PerturbEnvGroundWavesAlpha(out float alpha, inout float4 renderChannelColor, in float4 position)
{	
	float2 offset = float2(RandomOffset.x, RandomOffset.y);
	
	float thresh1 = tex2D(CloudsSampler, position.xz*EnvGroundWavesFrequency + offset*10).x;
	float thresh2 = tex2D(CloudsSampler, position.xz*EnvGroundWavesFrequency - offset*10).x;	
	float thresh = (thresh1 + thresh2)/2;
	float y = position.y/EnvGroundWavesAmplitude;

	alpha = saturate((y - thresh)*EnvGroundWavesHardness);
	renderChannelColor = lerp(float4(0,1,0,1), renderChannelColor, alpha);
}


//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
void PerturbIslandGroundAlpha(inout float alpha, in float4 position)
{
	if(position.y<17)
	{
		alpha = saturate((position.y-17)*0.8);
	}
}
