//-----------------------------------------------------------------------------
// The following will be executed for every pixel shader, they are required
// for most of the invoked lighting functions.
//-----------------------------------------------------------------------------
#define PS_START                                           \
	PSOutput Output;                                       \
	Output.RenderChannelColor = RenderChannelColor;        \
	Output.DepthColor = float4(pin.PositionWS.y/DepthClipY, pin.PositionWS.w, 0.0, 1.0); \
	ColorPair lightResult;                                 \
	float3 normal;                                         
	//Output.DepthColor = float4(pin.PositionWS.y/DepthClipY, pin.PositionWS.w, 0.0, 1.0); \


//-----------------------------------------------------------------------------
// Basic Unicolor Pixel Shader: used for various stuff
//-----------------------------------------------------------------------------
PSOutput PSBasicPixelLighting(PixelLightingPSInput pin) : COLOR
{
	PS_START
	ComputeLighting(lightResult, normal, pin.PositionWS, pin.NormalWS);	
	ComputeDiffColorUni(Output.Color, lightResult, pin.PositionWS.w);
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
	return Output;
}
