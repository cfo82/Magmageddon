//-----------------------------------------------------------------------------
// Vertex shaders
// - Nm: Normal is supplied (to allow for lighting computations)
// - Tx: TexCoords are supplied (to allow for any texturing)
// - Fm: Frame is supplied (to allow normal mapping)
// - Sq: Squash is supported
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
//
//-----------------------------------------------------------------------------
PixelLightingVSOutput VSBasicPixelLightingNmSq
(
	float4	Position	: POSITION,
	float3	Normal		: NORMAL
)
{
	PixelLightingVSOutput vout;
	
	float4 squashed_ws = mul(squashMatrix(), Position);
	float4 pos_ws = mul(squashed_ws, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	
	vout.PositionPS		= pos_ps;
	vout.PositionWS.xyz	= pos_ws.xyz;
	vout.PositionWS.w	= ComputeFogFactor(length(EyePosition - pos_ws));
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
	
	float4 squashed_ws = mul(squashMatrix(), Position);
	float4 pos_ws = mul(squashed_ws, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	
	vout.PositionPS		= pos_ps;
	vout.PositionWS.xyz	= pos_ws.xyz;
	vout.PositionWS.w	= ComputeFogFactor(length(EyePosition - pos_ws));
	vout.NormalWS		= normalize(mul(Normal, World));
	vout.TexCoord		= TexCoord;

	return vout;
}
