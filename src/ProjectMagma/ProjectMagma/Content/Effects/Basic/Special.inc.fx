//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
inline void PerturbIslandTexCoords(inout float2 texCoord, in float nY)
{
	if(abs(nY)<0.4)	
	{
		float4 clouds = tex2D(CloudsSampler, texCoord + RandomOffset);
		float2 perturbation = clouds.gb * 2 * WindStrength - WindStrength;
		texCoord.y += perturbation.y;
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


//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
inline float Brightness(in float3 color)
{
	return dot(color, float3(0.3, 0.59, 0.11));
}

inline float3 Toned(in float3 input, in float3 tone)
{
	float brightness = Brightness(input);
	float weight = saturate(abs(brightness - 0.5) * 2);
	return lerp(tone, float3(brightness,brightness,brightness), weight);
}


inline void ComputeDiffuseTxTo(out float4 diffuse, in float2 texCoord, in ColorPair lightResult, in float3 tone)
{
	float4 texDiffuseColorRGBA = tex2D(DiffuseSampler, texCoord);
	float3 texDiffuseColor = texDiffuseColorRGBA.rgb;	
	float3 tonedDiffuseColor = Toned(texDiffuseColor, tone);
	float3 finalColor = lerp(texDiffuseColor, tonedDiffuseColor, texDiffuseColorRGBA.a);
	float4 uniDiffuseColor = float4(lightResult.Diffuse * DiffuseColor, Alpha);
	diffuse = float4(finalColor,1.0) * uniDiffuseColor;
}


inline void ComputeDiffSpecColorTxTo(
	out float4 color, in float2 texCoord, in ColorPair lightResult, in float fogFactor, in float3 tone
)
{
	float4 diffuse, specular;
	ComputeDiffuseTxTo(diffuse, texCoord, lightResult, tone);
	ComputeSpecularTx(specular, texCoord, lightResult);	
	color = diffuse + float4(specular.rgb, 0);
	color.rgb = lerp(color.rgb, FogColor, fogFactor* FogEnabled);
	color.rgb = lerp(color.rgb, BlinkingColor, BlinkingState);
}


inline void ComputeDiffuseTxToDb(
	out float4 diffuse, in float2 texCoord, in ColorPair lightResult,
	in float3 tone, in float3 invTone
)
{
	float4 texDiffuseColorRGBA = tex2D(DiffuseSampler, texCoord);
	float3 texDiffuseColor = texDiffuseColorRGBA.rgb;	
	float3 tonedDiffuseColor = Toned(texDiffuseColor, tone);
	float3 invTonedDiffuseColor = Toned(texDiffuseColor, invTone);
	float3 finalColor = lerp(invTonedDiffuseColor, tonedDiffuseColor, texDiffuseColorRGBA.a);
	float4 uniDiffuseColor = float4(lightResult.Diffuse * DiffuseColor, Alpha);
	diffuse = float4(finalColor,1.0) * uniDiffuseColor;
}


inline void ComputeDiffSpecColorTxToDb(
	out float4 color, in float2 texCoord, in ColorPair lightResult, in float fogFactor,
	in float3 tone, in float3 invTone
)
{
	float4 diffuse, specular;
	ComputeDiffuseTxToDb(diffuse, texCoord, lightResult, tone, invTone);
	ComputeSpecularTx(specular, texCoord, lightResult);	
	color = diffuse + float4(specular.rgb, 0);
	color.rgb = lerp(color.rgb, FogColor, fogFactor* FogEnabled);
	color.rgb = lerp(color.rgb, BlinkingColor, BlinkingState);
}
