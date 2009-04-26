#define RANDOM_OFFSET_COUNT 5
float2 RandomOffset[RANDOM_OFFSET_COUNT];

#define ColorRandomOffset (RandomOffset[0]*0.2)
#define StuccoRandomOffset1 (RandomOffset[1])
#define StuccoRandomOffset2 (RandomOffset[2])
#define StuccoRandomOffset3 (RandomOffset[3])
#define GraniteRandomOffset (RandomOffset[4]*0.2)

uniform const float		FogEnabled;
uniform const float		FogStart;
uniform const float		FogEnd;
uniform const float3	FogColor;
uniform const float3	EyePosition;



float planeFraction;
float flickerStrength;
float StuccoCompression;
float4 RenderChannelColor;

float minPlaneY, maxPlaneY;

bool invert;

//float2 FireFractalOffset;


float3 g_LightDir;                  // Light's direction in world space
//float    g_fHeightMapScale;         // Describes the useful range of values for the height field

// Matrices:
float4x4 World;                  // World matrix for object
float4x4 WorldViewProjection;    // World * View * Projection matrix


float4x4 View;                   // View matrix, not really used here
float4x4 Projection; // not really used here