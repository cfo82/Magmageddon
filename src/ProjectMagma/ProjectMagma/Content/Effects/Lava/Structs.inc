//--------------------------------------------------------------------------------------
// 1. VERTEX SHADER OUTPUT STRUCTURE
//--------------------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 position          : POSITION;
    float2 texCoord          : TEXCOORD0;
    float4 positionPSP       : TEXCOORD2;   // see comments of basic.fx
    float4 pos : TEXCOORD3;
    float3 vLightTS          : TEXCOORD1;   // light vector in tangent space, denormalized
    float3 vNormalWS         : TEXCOORD4;   // Normal vector in world space    
};  

//--------------------------------------------------------------------------------------
// PARALLAX 4: Pixel shader output structure
//--------------------------------------------------------------------------------------
struct PS_OUTPUT
{
    float4 RGBColor : COLOR0;  // Pixel color    
};

struct PS_INPUT
{
   float2 texCoord          : TEXCOORD0;
   float3 vLightTS          : TEXCOORD1;   // light vector in tangent space, denormalized
   float4 positionPSP       : TEXCOORD2;
   float4 pos               : TEXCOORD3;
   float3 vNormalWS         : TEXCOORD4;   // Normal vector in world space
  // float depth              : DEPTH;
};

/*struct PSOutput
{
	float4 Color              : COLOR0;
	float4 RenderChannelColor : COLOR1;
	float4 DepthColor         : COLOR2;
};
*/