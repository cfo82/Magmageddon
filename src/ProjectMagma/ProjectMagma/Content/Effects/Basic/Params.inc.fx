float SquashAmount = 0.0f;
float4 WindStrength;
float2 RandomOffset;
float4 RenderChannelColor;
float DirLight1BottomAmpMaxY = 0;// = 300;
float DirLight1BottomAmpStrength = 1;//= 3;

float EnvGroundWavesAmplitude;
float EnvGroundWavesFrequency;
float EnvGroundWavesHardness;


//-----------------------------------------------------------------------------
// Fog settings
//-----------------------------------------------------------------------------

uniform const float		FogEnabled		: register(c0);
uniform const float		FogStart		: register(c1);
uniform const float		FogEnd			: register(c2);
uniform const float3	FogColor		: register(c3);

uniform const float3	EyePosition		: register(c4);		// in world space


//-----------------------------------------------------------------------------
// Material settings
//-----------------------------------------------------------------------------

uniform const float3	DiffuseColor	: register(c5) = 1;
uniform const float		Alpha			: register(c6) = 1;
uniform const float3	EmissiveColor	: register(c7) = 0;
uniform const float3	SpecularColor	: register(c8) = 1;
uniform const float		SpecularPower	: register(c9) = 16;


//-----------------------------------------------------------------------------
// Lights
// All directions and positions are in world space and must be unit vectors
//-----------------------------------------------------------------------------

uniform const float3	AmbientLightColor		: register(c10);

uniform const float3	DirLight0Direction		: register(c11);
uniform const float3	DirLight0DiffuseColor	: register(c12);
uniform const float3	DirLight0SpecularColor	: register(c13);

uniform const float3	DirLight1Direction		: register(c14);
uniform const float3	DirLight1DiffuseColor	: register(c15);
uniform const float3	DirLight1SpecularColor	: register(c16);

uniform const float3	DirLight2Direction		: register(c17);
uniform const float3	DirLight2DiffuseColor	: register(c18);
uniform const float3	DirLight2SpecularColor	: register(c19);


//-----------------------------------------------------------------------------
// Matrices
//-----------------------------------------------------------------------------

uniform const float4x4	Local;
uniform const float4x4	World		: register(vs, c20);	// 20 - 23
uniform const float4x4	View		: register(vs, c24);	// 24 - 27
uniform const float4x4	Projection	: register(vs, c28);	// 28 - 31
