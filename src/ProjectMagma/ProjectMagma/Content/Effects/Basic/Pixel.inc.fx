//-----------------------------------------------------------------------------
// The following will be executed for every pixel shader, they are required
// for most of the invoked lighting functions.
//-----------------------------------------------------------------------------
#define PS_START                                           \
	PSOutput Output;                                       \
	Output.RenderChannelColor = RenderChannelColor;        \
	Output.DepthColor = float4(pin.PositionWS.y/DepthClipY, ComputeFogFactor(length(EyePosition - pin.PositionWS.xyz)), 0.0, 1.0); \
	Output.RealDepth = pin.PositionPSP.z/pin.PositionPSP.w;                                \
	ColorPair lightResult;                                 \
	float3 normal;                                         
	//Output.DepthColor = float4(pin.PositionWS.y/DepthClipY, pin.PositionWS.w, 0.0, 1.0);
	//Output.DepthColor = float4(pin.PositionWS.y/DepthClipY, pin.PositionWS.w, 0.0, 1.0);


//pin.PositionPSP.z / pin.PositionPSP.w;                              

float ShadowLightPositionY=10000;
float ShadowMapFarClip=10500;
float ShadowMapNearClip=9500;

void BlendWithShadow(inout float4 color, in float4 positionWS, in float4 positionLS)
{
	// compute position where to sample
	float2 projectedTexCoords;	
	projectedTexCoords[0] = (positionLS.x / positionLS.w / 2.0f) + 0.5f;
	projectedTexCoords[1] = (-positionLS.y / positionLS.w / 2.0f) + 0.5f;
	
	// compute depth
	float4 depth = tex2D(ShadowSampler, projectedTexCoords);
	
	// compute distance to light source, assuming directional light in (0,-1,0) direction
	float len = (ShadowLightPositionY - positionWS.y/positionWS.w - ShadowMapNearClip) / (ShadowMapFarClip - ShadowMapNearClip);
	
	float depthBias = 0.007f;
	
	//float3 worldToLight = (ShadowLightPositionY - positionWS.y/positionWS.w);
	//float incidentAngle = dot(Input.Normal, normalize(worldToLight);
	
	if(len - depthBias > depth.r)
	{
		color = lerp(color, float4(0,0,0,1), 0.5);
	}
}


//-----------------------------------------------------------------------------
// Basic Unicolor Pixel Shader: used for various stuff
//-----------------------------------------------------------------------------
PSOutput PSBasicPixelLighting(PixelLightingPSInput pin) : COLOR
{
	PS_START
	ComputeLighting(lightResult, normal, pin.PositionWS, pin.NormalWS);	
	ComputeDiffColorUni(Output.Color, lightResult, pin.PositionWS.w);
	BlendWithShadow(Output.Color, pin.PositionWS, pin.PositionLS);
	return Output;
}


//-----------------------------------------------------------------------------
// Basic Textured Pixel Shader: used for various stuff
//-----------------------------------------------------------------------------
PSOutput PSBasicPixelLightingTx(PixelLightingPSInputTx pin) : COLOR
{
	PS_START
	float2 texCoord = pin.TexCoord;
	ComputeLighting(lightResult, normal, pin.PositionWS, pin.NormalWS);	
	ComputeDiffSpecColorTx(Output.Color, texCoord, lightResult, pin.PositionWS.w);
	BlendWithShadow(Output.Color, pin.PositionWS, pin.PositionLS);
	return Output;
}


//-----------------------------------------------------------------------------
// Island Pixel Shader: used for islands
//-----------------------------------------------------------------------------
PSOutput PSIsland(PixelLightingPSInputTx pin) : COLOR
{
	PS_START
	float2 texCoord = pin.TexCoord;
	ComputeLighting(lightResult, normal, pin.PositionWS, pin.NormalWS);	
	PerturbIslandTexCoords(texCoord, normal.y);
	ComputeDiffSpecColorTx(Output.Color, texCoord, lightResult, pin.PositionWS.w);
	PerturbIslandGroundAlpha(Output.Color.a, pin.PositionWS);
	BlendWithShadow(Output.Color, pin.PositionWS, pin.PositionLS);	
	return Output;
}


//-----------------------------------------------------------------------------
// Environment Pixel Shader: used for pillars and cave
//-----------------------------------------------------------------------------
PSOutput PSEnvironment(PixelLightingPSInputTx pin) : COLOR
{
	PS_START
	float2 texCoord = pin.TexCoord;
	ComputeLighting(lightResult, normal, pin.PositionWS, pin.NormalWS);	
	ComputeDiffSpecColorTx(Output.Color, texCoord, lightResult, pin.PositionWS.w);
	PerturbEnvGroundWavesAlpha(Output.Color.a, Output.RenderChannelColor, pin.PositionWS);
	//Output.Depth = float4(pin.PositionPSP.z / pin.PositionPSP.w*10000-9992,0,0,1);
	BlendWithShadow(Output.Color, pin.PositionWS, pin.PositionLS);
	return Output;
}

//-----------------------------------------------------------------------------
// Toned Textured Pixel Shader: used for colorizing players
//-----------------------------------------------------------------------------
PSOutput PSBasicPixelLightingTxTo(PixelLightingPSInputTx pin) : COLOR
{
	PS_START
	float2 texCoord = pin.TexCoord;
	ComputeLighting(lightResult, normal, pin.PositionWS, pin.NormalWS);	
	ComputeDiffSpecColorTxTo(Output.Color, texCoord, lightResult, pin.PositionWS.w, ToneColor);
	BlendWithShadow(Output.Color, pin.PositionWS, pin.PositionLS);
	return Output;
}


//-----------------------------------------------------------------------------
// Doubly Toned Textured Pixel Shader: used for colorizing players where the
// skin color should be colored as well, e.g. when being frozen.
//-----------------------------------------------------------------------------
PSOutput PSBasicPixelLightingTxToDb(PixelLightingPSInputTx pin) : COLOR
{
	PS_START
	float2 texCoord = pin.TexCoord;
	ComputeLighting(lightResult, normal, pin.PositionWS, pin.NormalWS);	
	ComputeDiffSpecColorTxToDb(Output.Color, texCoord, lightResult, pin.PositionWS.w, ToneColor, InvToneColor);
	BlendWithShadow(Output.Color, pin.PositionWS, pin.PositionLS);
	return Output;
}
