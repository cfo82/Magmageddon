float4x4 Local;


//----------------------

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


uniform const texture DiffuseTexture;
uniform const sampler DiffuseSampler = sampler_state
{
	Texture = <DiffuseTexture>;
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;	
};


uniform const texture SpecularTexture;
uniform const sampler SpecularSampler = sampler_state
{
	Texture = <SpecularTexture>;
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;	
};

uniform const texture NormalTexture;
uniform const sampler NormalSampler = sampler_state
{
	Texture = <NormalTexture>;
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;	
};
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

uniform const float		FogEnabled;		
uniform const float		FogStart;		
uniform const float		FogEnd	;		
uniform const float3	FogColor;		

uniform const float3	EyePosition;


//-----------------------------------------------------------------------------
// Material settings
//-----------------------------------------------------------------------------

uniform const float3	DiffuseColor	;
uniform const float		Alpha			;
uniform const float3	EmissiveColor	;
uniform const float3	SpecularColor	;
uniform const float		SpecularPower	;


//-----------------------------------------------------------------------------
// Lights
// All directions and positions are in world space and must be unit vectors
//-----------------------------------------------------------------------------

uniform const float3	AmbientLightColor		;

uniform const float3	DirLight0Direction		;
uniform const float3	DirLight0DiffuseColor	;
uniform const float3	DirLight0SpecularColor	;

uniform const float3	DirLight1Direction		;
uniform const float3	DirLight1DiffuseColor	;
uniform const float3	DirLight1SpecularColor	;

uniform const float3	DirLight2Direction		;
uniform const float3	DirLight2DiffuseColor	;
uniform const float3	DirLight2SpecularColor	;
///----------

    // These are required by the Animation library
    float4x4 MatrixPalette[56];
    float4x4 World;
 
    // These are not
    float4x4 View;
    float4x4 Projection;
    texture BasicTexture;
 
    // Stores sampler state info for the texture
    sampler TextureSampler = sampler_state
    {
       Texture = (BasicTexture);
    };
    
        // This is passed into our vertex shader from Xna
    struct VS_INPUT
    {
        // This is the position of the vertex in the model file
	float4 position : POSITION;
        // The vertex normal
	float3 normal : NORMAL0;
	// This is the texture coordinate for the vertex in the model file
	float2 texcoord : TEXCOORD0;
	// These are the indices (4 of them) that index the bones that affect
	// this vertex.  The indices refer to the MatrixPalette.
	half4 indices : BLENDINDICES0;
	// These are the weights (4 of them) that determine how much each bone
	// affects this vertex.
	float4 weights : BLENDWEIGHT0;
    };
    
        // This is passed out from our vertex shader once we have processed the input
    struct VS_OUTPUT
    {
        // The final position of the vertex in world space
	float4 position : POSITION;
	// The texture coordinate associated with the vertex
	float2 texcoord : TEXCOORD0;
    };
    
        // This is the output from our skinning method
    struct SKIN_OUTPUT
    {
        float4 position;
        float4 normal;
    };
    
        // This method takes in a vertex and applies the bone transforms to it.
    SKIN_OUTPUT Skin4( const VS_INPUT input)
    {
        SKIN_OUTPUT output = (SKIN_OUTPUT)0;
        // Since the weights need to add up to one, store 1.0 - (sum of the weights)
        float lastWeight = 1.0;
        float weight = 0;
        // Apply the transforms for the first 3 weights
        for (int i = 0; i < 3; ++i)
        {
            weight = input.weights[i];
            lastWeight -= weight;
            output.position     += mul( input.position, MatrixPalette[input.indices[i]]) * weight;
            output.normal       += mul( input.normal, MatrixPalette[input.indices[i]]) * weight;
        }
        // Apply the transform for the last weight
        output.position     += mul( input.position, MatrixPalette[input.indices[3]])*lastWeight;
        output.normal       += mul( input.normal, MatrixPalette[input.indices[3]])*lastWeight;
        return output;
    };

    void TransformVertex (in VS_INPUT input, out VS_OUTPUT output)
    {
        // Calculate the skinned position
        SKIN_OUTPUT skin = Skin4(input);
        // This is the final position of the vertex, and where it will be drawn on the screen
        float4x4 WorldViewProjection = mul(World,mul(View,Projection));
        output.position = mul(skin.position, WorldViewProjection);
        // This is not used by is included to demonstrate how to get the normal in world space
        float4 transformedNormal = mul(skin.normal, WorldViewProjection);
        output.texcoord = input.texcoord;
    }
    
        // This is passed into our pixel shader
    struct PS_INPUT
    {
        float2 texcoord : TEXCOORD0;
    };
    
        // This is the final color rendered on the screen
    struct PS_OUTPUT
    {
        float4 color : COLOR;
    };
    
    void TransformPixel (in PS_INPUT input, out PS_OUTPUT output)
    {
        output.color = tex2D(TextureSampler,input.texcoord);
    }
    
        technique TransformTechnique
    {
        pass P0
        {
            VertexShader = compile vs_3_0 TransformVertex();
            PixelShader  = compile ps_3_0 TransformPixel();
        }
    }