//-----------------------------------------------------------------------------
// The following will be executed for every pixel shader, they are required
// for most of the invoked lighting functions.
//-----------------------------------------------------------------------------
/*#define PS_START                                           \
	PSOutput Output;                                       \
	Output.RenderChannelColor = RenderChannelColor;        \
	Output.DepthColor = float4(pin.PositionWS.y/DepthClipY, ComputeFogFactor(length(EyePosition - pin.PositionWS.xyz)), 0.0, 1.0); \
	Output.RealDepth = pin.PositionPSP.z/pin.PositionPSP.w;                                \
	ColorPair lightResult;                                 \
	float3 normal;                                         
	//Output.DepthColor = float4(pin.PositionWS.y/DepthClipY, pin.PositionWS.w, 0.0, 1.0);
	//Output.DepthColor = float4(pin.PositionWS.y/DepthClipY, pin.PositionWS.w, 0.0, 1.0);*/

#define PS_START                                           \
	PSOutput Output;                                       \
	Output.RenderChannelColor = RenderChannelColor;        \
	Output.DepthColor = float4(pin.PositionWS.y/DepthClipY, 0, 0.0, 1.0); \
	Output.RealDepth = pin.PositionPSP.z/pin.PositionPSP.w;                                \
	ColorPair lightResult;                                 \
	float3 normal;                                         



//-----------------------------------------------------------------------------
// Basic Unicolor Pixel Shader: used for various stuff
//-----------------------------------------------------------------------------
PSOutput PSBasicPixelLighting(PixelLightingPSInput pin) : COLOR
{
	PS_START
	ComputeLighting(lightResult, normal, pin.PositionWS, pin.NormalWS);	
	ComputeDiffColorUni(Output.Color, lightResult, pin.PositionWS.w);
	BlendWithShadow(Output.Color, pin.PositionWS, pin.PositionLS, pin.NormalWS);
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
	BlendWithShadow(Output.Color, pin.PositionWS, pin.PositionLS, pin.NormalWS);
	return Output;
}

//-----------------------------------------------------------------------------
// Basic Textured Pixel Shader ignoring Island Depth: for player arrows
//-----------------------------------------------------------------------------
PSOutput PSBasicPixelLightingTxIgnoreIslandDepth(PixelLightingPSInputTx pin) : COLOR
{
	PSOutput basic = PSBasicPixelLightingTx(pin);
	//float2 depthTexCoord = pin.PositionPSP.xy/float2(1280*2,720*2)+float2(0.5,0.5);
	float2 depthTexCoord = pin.PositionPSP.xy/pin.PositionPSP.z/2+float2(0.5,0.5);
	depthTexCoord.y = 1-depthTexCoord.y;
	//if(tex2D(DepthMap, 
	float depth = pin.PositionPSP.z/pin.PositionPSP.w;
	if(depth > tex2D(DepthSampler, depthTexCoord).r && tex2D(ToolSampler, depthTexCoord).z<1) basic.Color.a = 0;
	//basic.Color = float4(pin.PositionPSP.xyz,1);
	//basic.Color = tex2D(DepthSampler, depthTexCoord);

	return basic;
}

//-----------------------------------------------------------------------------
// Island Pixel Shader: used for islands
//-----------------------------------------------------------------------------
PSOutput PSIsland(PixelLightingPSInputTx pin) : COLOR
{
	PS_START
	float2 texCoord = pin.TexCoord;
	ComputeLighting(lightResult, normal, pin.PositionWS, pin.NormalWS);	
//	PerturbIslandTexCoords(texCoord, normal.y);
	ComputeDiffSpecColorTx(Output.Color, texCoord, lightResult, pin.PositionWS.w);
	PerturbIslandGroundAlpha(Output.Color.a, pin.PositionWS);
	BlendWithShadow(Output.Color, pin.PositionWS, pin.PositionLS, pin.NormalWS);	
	//Output.DepthColor.z = 1;
	Output.RenderChannelColor.a = Output.Color.a;
	return Output;
}
float4 PSIslandAlphaColor(PixelLightingPSInputTx pin) : COLOR0
{
	return PSIsland(pin).Color;
}
float4 PSIslandAlphaRenderChannel(PixelLightingPSInputTx pin) : COLOR0
{
	return PSIsland(pin).RenderChannelColor;
}
float4 PSIslandAlphaDepth(PixelLightingPSInputTx pin) : COLOR0
{
	return PSIsland(pin).RealDepth;
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
	BlendWithShadow(Output.Color, pin.PositionWS, pin.PositionLS, pin.NormalWS);
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
	BlendWithShadow(Output.Color, pin.PositionWS, pin.PositionLS, pin.NormalWS);
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
	BlendWithShadow(Output.Color, pin.PositionWS, pin.PositionLS, pin.NormalWS);
	return Output;
}
