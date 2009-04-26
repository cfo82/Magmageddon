//-----------------------------------------------------------------------------
// The following will be executed for every pixel shader.
//-----------------------------------------------------------------------------
#define PS_START                                           \
	PSOutput Output;                                       \
	Output.RenderChannelColor = RenderChannelColor;        \
	ColorPair lightResult;                                 \
	float3 normal;                                         


//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
inline void ComputeLighting(out ColorPair lightResult, out float3 N, in float4 position, in float3 normal)
{
	float3 posToEye = EyePosition - position.xyz;
	float3 E = normalize(posToEye);
	N = normalize(normal);
	lightResult = ComputePerPixelLights(E, N, position.y);
}

inline void ComputeDiffuse(out float4 diffuse, in float2 texCoord, in ColorPair lightResult)
{
	float4 texDiffuseColor = tex2D(DiffuseSampler, texCoord);
	float4 uniDiffuseColor = float4(lightResult.Diffuse * DiffuseColor, Alpha);
	diffuse = texDiffuseColor * uniDiffuseColor;
}

inline void ComputeSpecular(out float4 specular, in float2 texCoord, in ColorPair lightResult)
{
	float4 texSpecularColor = tex2D(SpecularSampler, texCoord);
	float4 uniSpecularColor = float4(lightResult.Specular * SpecularColor, Alpha);
	specular = texSpecularColor * uniSpecularColor;
}


inline void ComputeDiffSpecColor(out float4 color, in float2 texCoord, in ColorPair lightResult, in float fogFactor)
{
	float4 diffuse, specular;
	ComputeDiffuse(diffuse, texCoord, lightResult);
	ComputeSpecular(specular, texCoord, lightResult);	
	color = diffuse + float4(specular.rgb, 0);
	color.rgb = lerp(color.rgb, FogColor, fogFactor);
}


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
	//Output.Color.a = y < thresh ? 0 : 1;	
	
	// this is for pillars
	alpha = saturate((y - thresh)*EnvGroundWavesHardness);
	renderChannelColor = lerp(float4(0,1,0,1), renderChannelColor, alpha);
}


//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
void PerturbIslandGroundAlpha(out float alpha, in float4 position)
{
	if(position.y<17)
	{
		alpha = saturate((position.y-17)*0.8);
	}
}


//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
PSOutput PSBasicPixelLighting(PixelLightingPSInput pin) : COLOR
{
	PS_START
	ComputeLighting(lightResult, normal, pin.PositionWS, pin.NormalWS);	
	float4 diffuse = float4(lightResult.Diffuse * DiffuseColor, Alpha);
	float4 color = diffuse + float4(lightResult.Specular, 0);
	color.rgb = lerp(color.rgb, FogColor, pin.PositionWS.w);
	Output.Color = color;

	return Output;
}


PSOutput PSBasicPixelLightingTx(PixelLightingPSInputTx pin) : COLOR
{
	PS_START
	float2 texCoord = pin.TexCoord;
	ComputeLighting(lightResult, normal, pin.PositionWS, pin.NormalWS);	
	ComputeDiffSpecColor(Output.Color, texCoord, lightResult, pin.PositionWS.w);
	return Output;
}


PSOutput PSIsland(PixelLightingPSInputTx pin) : COLOR
{
	PS_START
	float2 texCoord = pin.TexCoord;
	ComputeLighting(lightResult, normal, pin.PositionWS, pin.NormalWS);	
	PerturbIslandTexCoords(texCoord, normal.y);
	ComputeDiffSpecColor(Output.Color, texCoord, lightResult, pin.PositionWS.w);
	PerturbIslandGroundAlpha(Output.Color.a, pin.PositionWS);
	return Output;
}


PSOutput PSEnvironment(PixelLightingPSInputTx pin) : COLOR
{
	PS_START
	float2 texCoord = pin.TexCoord;
	ComputeLighting(lightResult, normal, pin.PositionWS, pin.NormalWS);	
	ComputeDiffSpecColor(Output.Color, texCoord, lightResult, pin.PositionWS.w);
	PerturbEnvGroundWavesAlpha(Output.Color.a, Output.RenderChannelColor, pin.PositionWS);
	return Output;
}
