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


struct SkinVsInput
{
	float4 position : POSITION;
	float3 normal : NORMAL0;
	float2 texcoord : TEXCOORD0;

	// These are the indices (4 of them) that index the bones that affect
	// this vertex.  The indices refer to the MatrixPalette.
	half4 indices : BLENDINDICES0;

	// These are the weights (4 of them) that determine how much each bone
	// affects this vertex.
	float4 weights : BLENDWEIGHT0;
};


struct PositionAndNormal
{
    float4 position;
    float4 normal;
};
    
    
//-----------------------------------------------------------------------------
// Vertex shader outputs
//-----------------------------------------------------------------------------

struct PixelLightingVSOutput
{
	float4	PositionPS 	: POSITION;		// Position in projection space
	float4	PositionWS	: TEXCOORD0;
	float3	NormalWS	: TEXCOORD1;   
	float4  PositionPSP : TEXCOORD3;    // same as PositionPS but can only be used by pixel shader if declared this way
};

struct PixelLightingVSOutputTx
{
	float4	PositionPS	: POSITION;		// Position in projection space
	float2	TexCoord	: TEXCOORD0;
	float4	PositionWS	: TEXCOORD1;
	float3	NormalWS	: TEXCOORD2;
	float4  PositionPSP : TEXCOORD3;    // same as PositionPS but can only be used by pixel shader if declared this way
};


//-----------------------------------------------------------------------------
// Pixel shader inputs
//-----------------------------------------------------------------------------

struct PixelLightingPSInput
{
	float4	PositionWS	: TEXCOORD0;
	float3	NormalWS	: TEXCOORD1;
	//float   Depth       : DEPTH;
	float4  PositionPSP  : TEXCOORD3;
};

struct PixelLightingPSInputTx
{
	float2	TexCoord	: TEXCOORD0;
	float4	PositionWS	: TEXCOORD1;
	float3	NormalWS	: TEXCOORD2;
	//float   Depth       : DEPTH;
	float4  PositionPSP  : TEXCOORD3;
};
