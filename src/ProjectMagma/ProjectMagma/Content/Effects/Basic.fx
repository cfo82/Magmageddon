// modified by dpk
texture Clouds;
sampler2D CloudsSampler = sampler_state
{
	Texture = <Clouds>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};
float4 WindStrength;
float2 RandomOffset;
float4 RenderChannelColor;

//-----------------------------------------------------------------------------
// BasicEffect.fx
//
// This is a simple shader that supports 1 ambient and 3 directional lights.
// All lighting computations happen in world space.
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Texture sampler
//-----------------------------------------------------------------------------

uniform const texture BasicTexture;

uniform const sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (BasicTexture);
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
};


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

uniform const float4x4	World		: register(vs, c20);	// 20 - 23
uniform const float4x4	View		: register(vs, c24);	// 24 - 27
uniform const float4x4	Projection	: register(vs, c28);	// 28 - 31


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

struct VSInputNm
{
	float4	Position	: POSITION;
	float3	Normal		: NORMAL;
};


struct VSInputNmTx
{
	float4	Position	: POSITION;
	float2	TexCoord	: TEXCOORD0;
	float3	Normal		: NORMAL;
};

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
	float4	Diffuse		: COLOR0;		// diffuse.rgb and alpha
};

struct PixelLightingVSOutputTx
{
	float4	PositionPS	: POSITION;		// Position in projection space
	float2	TexCoord	: TEXCOORD0;
	float4	PositionWS	: TEXCOORD1;
	float3	NormalWS	: TEXCOORD2;
	float4	Diffuse		: COLOR0;		// diffuse.rgb and alpha
};


//-----------------------------------------------------------------------------
// Pixel shader inputs
//-----------------------------------------------------------------------------

struct PixelLightingPSInput
{
	float4	PositionWS	: TEXCOORD0;
	float3	NormalWS	: TEXCOORD1;
	float4	Diffuse		: COLOR0;		// diffuse.rgb and alpha
};

struct PixelLightingPSInputTx
{
	float2	TexCoord	: TEXCOORD0;
	float4	PositionWS	: TEXCOORD1;
	float3	NormalWS	: TEXCOORD2;
	float4	Diffuse		: COLOR0;		// diffuse.rgb and alpha
};

struct PSOutput
{
	float4 Color              : COLOR0;
	float4 RenderChannelColor : COLOR1;
};

//-----------------------------------------------------------------------------
// Compute per-pixel lighting.
// When compiling for pixel shader 2.0, the lit intrinsic uses more slots
// than doing this directly ourselves, so we don't use the intrinsic.
// E: Eye-Vector
// N: Unit vector normal in world space
//-----------------------------------------------------------------------------
ColorPair ComputePerPixelLights(float3 E, float3 N)
{
	ColorPair result;
	
	result.Diffuse = AmbientLightColor;
	result.Specular = 0;
	
	// Light0
	float3 L = -DirLight0Direction;
	float3 H = normalize(E + L);
	float dt = max(0,dot(L,N));
    result.Diffuse += DirLight0DiffuseColor * dt;
    if (dt != 0)
		result.Specular += DirLight0SpecularColor * pow(max(0,dot(H,N)), SpecularPower);

	// Light1
	L = -DirLight1Direction;
	H = normalize(E + L);
	dt = max(0,dot(L,N));
    result.Diffuse += DirLight1DiffuseColor * dt;
    if (dt != 0)
	    result.Specular += DirLight1SpecularColor * pow(max(0,dot(H,N)), SpecularPower);
    
	// Light2
	L = -DirLight2Direction;
	H = normalize(E + L);
	dt = max(0,dot(L,N));
    result.Diffuse += DirLight2DiffuseColor * dt;
    if (dt != 0)
	    result.Specular += DirLight2SpecularColor * pow(max(0,dot(H,N)), SpecularPower);
    
    result.Diffuse *= DiffuseColor;
    result.Diffuse += EmissiveColor;
    result.Specular *= SpecularColor;
		
	return result;
}


//-----------------------------------------------------------------------------
// Compute fog factor
//-----------------------------------------------------------------------------
float ComputeFogFactor(float d)
{
    return clamp((d - FogStart) / (FogEnd - FogStart), 0, 1) * FogEnabled;
}




//-----------------------------------------------------------------------------
// Vertex shaders
//-----------------------------------------------------------------------------

float SquashAmount = 0.0f;
PixelLightingVSOutput VSBasicPixelLightingNm(VSInputNm vin)
{
	PixelLightingVSOutput vout;
	
	float4x4 squashMatrix = float4x4
	(
		lerp(1.0f, sqrt(2.0f), SquashAmount), 0.0f, 0.0f, 0.0f,
		0.0f, lerp(1.0f, 0.5f, SquashAmount), 0.0f, -SquashAmount*0.6f,
		0.0f, 0.0f, lerp(1.0f, sqrt(2.0f), SquashAmount), 0.0f,
		0.0f, 0.0f, 0.0f, 1.0f
	);
	
	float4 squashed_ws = mul(squashMatrix, vin.Position);
	float4 pos_ws = mul(squashed_ws, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	
	vout.PositionPS		= pos_ps;
	vout.PositionWS.xyz	= pos_ws.xyz;
	vout.PositionWS.w	= ComputeFogFactor(length(EyePosition - pos_ws));
	vout.NormalWS		= normalize(mul(vin.Normal, World));
	vout.Diffuse		= float4(1, 1, 1, Alpha);
	
	return vout;
}




PixelLightingVSOutputTx VSBasicPixelLightingNmTx(VSInputNmTx vin)
{
	PixelLightingVSOutputTx vout;
	
	float4x4 squashMatrix = float4x4
	(
		lerp(1.0f, sqrt(2.0f), SquashAmount), 0.0f, 0.0f, 0.0f,
		0.0f, lerp(1.0f, 0.5f, SquashAmount), 0.0f, -SquashAmount*1.0f,
		0.0f, 0.0f, lerp(1.0f, sqrt(2.0f), SquashAmount), 0.0f,
		0.0f, 0.0f, 0.0f, 1.0f
	);
	
	float4 squashed_ws = mul(squashMatrix, vin.Position);
	float4 pos_ws = mul(squashed_ws, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	
	vout.PositionPS		= pos_ps;
	vout.PositionWS.xyz	= pos_ws.xyz;
	vout.PositionWS.w	= ComputeFogFactor(length(EyePosition - pos_ws));
	vout.NormalWS		= normalize(mul(vin.Normal, World));
	vout.Diffuse		= float4(1, 1, 1, Alpha);
	vout.TexCoord		= vin.TexCoord;

	return vout;
}



//-----------------------------------------------------------------------------
// Pixel shaders
//-----------------------------------------------------------------------------

PSOutput PSBasicPixelLighting(PixelLightingPSInput pin) : COLOR
{
	float3 posToEye = EyePosition - pin.PositionWS.xyz;
	
	float3 N = normalize(pin.NormalWS);
	float3 E = normalize(posToEye);
	
	ColorPair lightResult = ComputePerPixelLights(E, N);

	float4 diffuse = float4(lightResult.Diffuse * pin.Diffuse.rgb, pin.Diffuse.a);
	float4 color = diffuse + float4(lightResult.Specular, 0);
	color.rgb = lerp(color.rgb, FogColor, pin.PositionWS.w);

//return float4(1,1,0,1);
	PSOutput Output;
	Output.Color = color;
	Output.RenderChannelColor = RenderChannelColor;
	Output.RenderChannelColor.a = saturate(pin.PositionWS.y/2000);

	return Output;
}


PSOutput PSBasicPixelLightingTx(PixelLightingPSInputTx pin) : COLOR
{
	float3 posToEye = EyePosition - pin.PositionWS.xyz;
	
	float3 N = normalize(pin.NormalWS);
	float3 E = normalize(posToEye);
	
	ColorPair lightResult = ComputePerPixelLights(E, N);
	
	float2 texCoord = pin.TexCoord;

	if(abs(N.y)<0.4)	
	{
		float4 clouds = tex2D(CloudsSampler, texCoord + RandomOffset);
		//float4 clouds = tex2D(CloudsSampler, texCoord);
		float2 perturbation = clouds.gb * 2 * WindStrength - WindStrength;
		texCoord.x += perturbation.x;
	}	
	
	float4 diffuse = tex2D(TextureSampler, texCoord) * float4(lightResult.Diffuse * pin.Diffuse.rgb, pin.Diffuse.a);
	float4 color = diffuse + float4(lightResult.Specular, 0);
	color.rgb = lerp(color.rgb, FogColor, pin.PositionWS.w);

//	if(abs(N.y)<0.4)
		//return float4(0,0,0,1);
//	else
	PSOutput Output;
	Output.Color = color;
	Output.RenderChannelColor = RenderChannelColor;
	Output.RenderChannelColor.a = saturate(pin.PositionWS.y/2000);
	//Output.RenderChannelColor.a = 0.3f;
	return Output;
}


//-----------------------------------------------------------------------------
// Shader and technique definitions
//-----------------------------------------------------------------------------


Technique Unicolored
{
	Pass
	{
		VertexShader = compile vs_1_1 VSBasicPixelLightingNm();
		PixelShader	 = compile ps_3_0 PSBasicPixelLighting();
	}
}

Technique Textured
{
	Pass 
	{
		VertexShader = compile vs_1_1 VSBasicPixelLightingNmTx();
		PixelShader	 = compile ps_3_0 PSBasicPixelLightingTx();
	}
}
