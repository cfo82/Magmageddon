//-----------------------------------------------------------------------------
// Structure definitions
//-----------------------------------------------------------------------------

struct ColorPair
{
	float3 Diffuse;
	float3 Specular;
};

//-----------------------------------------------------------------------------
// Vertex shader inputs
//-----------------------------------------------------------------------------



// maybe use vertex color later on to transfer data?
// float4	Color		: COLOR;


//-----------------------------------------------------------------------------
// Vertex shader outputs
//-----------------------------------------------------------------------------

struct PixelLightingVSOutput
{
	float4	PositionPS	: POSITION;		// Position in projection space
	float4	PositionWS	: TEXCOORD0;
	float3	NormalWS	: TEXCOORD1;
};

struct PixelLightingVSOutputTx
{
	float4	PositionPS	: POSITION;		// Position in projection space
	float2	TexCoord	: TEXCOORD0;
	float4	PositionWS	: TEXCOORD1;
	float3	NormalWS	: TEXCOORD2;
};


//-----------------------------------------------------------------------------
// Pixel shader inputs
//-----------------------------------------------------------------------------

struct PixelLightingPSInput
{
	float4	PositionWS	: TEXCOORD0;
	float3	NormalWS	: TEXCOORD1;
};

struct PixelLightingPSInputTx
{
	float2	TexCoord	: TEXCOORD0;
	float4	PositionWS	: TEXCOORD1;
	float3	NormalWS	: TEXCOORD2;
};

struct PSOutput
{
	float4 Color              : COLOR0;
	float4 RenderChannelColor : COLOR1;
};
