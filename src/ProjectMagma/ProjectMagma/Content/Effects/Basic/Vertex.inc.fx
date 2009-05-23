//-----------------------------------------------------------------------------
// Vertex shaders
// - Nm: Normal is supplied (to allow for lighting computations)
// - Tx: TexCoords are supplied (to allow for any texturing)
// - Fm: Frame is supplied (to allow normal mapping)
// - Sq: Squash is supported
// - Sk: Skinning is used (for bone animated models)
//-----------------------------------------------------------------------------


float4x4 squashMatrix()
{
	return float4x4
	(
		lerp(1.0f, sqrt(2.0f), SquashAmount), 0.0f, 0.0f, 0.0f,
		0.0f, lerp(1.0f, 0.5f, SquashAmount), 0.0f, -SquashAmount*0.6f,
		0.0f, 0.0f, lerp(1.0f, sqrt(2.0f), SquashAmount), 0.0f,
		0.0f, 0.0f, 0.0f, 1.0f
	);
}


//-----------------------------------------------------------------------------
// Compute fog factor
//-----------------------------------------------------------------------------
float ComputeFogFactor(float d)
{
    return clamp((d - DepthMapNearClip) / (DepthMapFarClip - DepthMapNearClip), 0, 1);
}


    
//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
PixelLightingVSOutput VSBasicPixelLightingNmSq
(
	float4	Position	: POSITION,
	float3	Normal		: NORMAL
)
{
	PixelLightingVSOutput vout;
	
	float4 pos_loc = mul(Position, Local);
	float4 squashed_ws = mul(squashMatrix(), pos_loc);
	float4 pos_ws = mul(squashed_ws, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	float4 pos_ls = mul(pos_ws, LightViewProjection);
	
	vout.PositionPS		= pos_ps;
	vout.PositionPSP	= pos_ps;
	vout.PositionWS.xyz	= pos_ws.xyz;
	vout.PositionWS.w	= ComputeFogFactor(length(EyePosition - pos_ws));
	vout.PositionLS		= pos_ls;
	vout.NormalWS		= normalize(mul(Normal, World));	
	
	return vout;
}


//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
PixelLightingVSOutputTx VSBasicPixelLightingNmTxSq
(
	float4	Position	: POSITION,
	float2	TexCoord	: TEXCOORD0,
	float3	Normal		: NORMAL
)
{
	PixelLightingVSOutputTx vout;
	
	float4 pos_loc = mul(Position, Local);
	float4 squashed_ws = mul(squashMatrix(), pos_loc);
	float4 pos_ws = mul(squashed_ws, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	float4 pos_ls = mul(pos_ws, LightViewProjection);
	
	vout.PositionPS		= pos_ps;
	vout.PositionPSP	= pos_ps;
	vout.PositionWS.xyz	= pos_ws.xyz;
	vout.PositionWS.w	= ComputeFogFactor(length(EyePosition - pos_ws));
	vout.NormalWS		= normalize(mul(Normal, World));
	vout.PositionLS		= pos_ls;
	vout.TexCoord		= TexCoord;

	return vout;
}


//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
PixelLightingVSOutputTx VSBasicPixelLightingNmTxSqSk
(
	SkinVsInput input
)
{
	PixelLightingVSOutputTx vout;

	// Calculate the skinned position
	PositionAndNormal skinned_input = Skin4(input);
    
    float4 pos_ws = mul(skinned_input.position, World);
    float4 pos_vs = mul(pos_ws, View);
    float4 pos_ps = mul(pos_vs, Projection);
    float4 pos_ls = mul(pos_ws, LightViewProjection);
    
    vout.PositionPS		= pos_ps;
    vout.PositionPSP	= pos_ps;
    vout.PositionWS.xyz = pos_ws.xyz;
	vout.PositionWS.w	= ComputeFogFactor(length(EyePosition - pos_ws));
	vout.NormalWS		= normalize(mul(skinned_input.normal, World));
	vout.PositionLS		= pos_ls;
	vout.TexCoord		= input.texcoord;
    //
    //// This is the final position of the vertex, and where it will be drawn on the screen
    ////float4x4 WorldViewProjection = mul(World,mul(View,Projection));
    ////vout.PositionPS = mul(input.position, WorldViewProjection);
    //// This is not used by is included to demonstrate how to get the normal in world space
    ////float4 transformedNormal = mul(skin.normal, WorldViewProjection);
    //vout.TexCoord = input.texcoord;
    //
    //vout.NormalWS = normalize(mul(skin.normal, World));   //float3(0,1,0);
    //vout.PositionWS = mul(skin.position, World);
    //
    return vout;
}