//#include "Sm3SpriteBatch.fx.inc"

// Pixel shader applies a one dimensional gaussian blur filter.
// This is used twice by the bloom postprocess, first to
// blur horizontally, and then again to blur vertically.

sampler TextureSampler : register(s0);

#define SAMPLE_COUNT 15

float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];

float2 ViewportSize;
 
void SpriteVertexShader(inout float4 color    : COLOR0,
 
                       inout float2 texCoord : TEXCOORD0,
 
                       inout float4 position : POSITION0)
 
{
 
   // Half pixel offset for correct texel centering.
 
   position.xy -= 0.5;
 
   // Viewport adjustment.
 
   position.xy = position.xy / ViewportSize;
 
   position.xy *= float2(2, -2);
 
   position.xy -= float2(1, -1);
 
}

float4 PixelShaderMain(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 c = 0;
    
    // Combine a number of weighted image filter taps.
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
        c += tex2D(TextureSampler, texCoord + SampleOffsets[i]) * SampleWeights[i];
    }
    
    return c;
}


technique GaussianBlur
{
    pass Pass1
    {
		VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 PixelShaderMain();
    }
}
