//--------------------------------------------------------------------------------------
// 1. VERTEX SHADER OUTPUT STRUCTURE
//--------------------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 position          : POSITION;
    float2 texCoord          : TEXCOORD0;
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
   float4 pos : TEXCOORD3;
   float3 vNormalWS         : TEXCOORD4;   // Normal vector in world space
};
