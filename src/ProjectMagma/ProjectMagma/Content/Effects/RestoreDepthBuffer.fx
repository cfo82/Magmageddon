
//#include "Sm3SpriteBatch.inc"

// dominik käser's comment... in fact the SpriteBatch class which we
// use to render fullscreen quads usually takes one texture as an input
// and renders to a given rendertarget. the input texture is set as
// s0. Therefore if we don't explicitly specify a sampler at s0
// we may end up with one of the other textures set as s0. Which one
// gets s0 is up to the optimizer (we may influence it as well
// by just assining s0 to the appropriate sampler below.
sampler i_hate_microsoft_dont_remove_this_it_wont_work : register(s0);

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

texture DepthBuffer;
sampler2D DepthSampler = sampler_state
{
	Texture = <DepthBuffer>;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct PSOutput
{
	float4 color : COLOR0;
	float depth  : DEPTH;
};

PSOutput ps_main(float2 texCoord : TEXCOORD0)
{
	PSOutput ps_out;
	ps_out.color = float4(0,0,0,0);
	ps_out.depth = tex2D(DepthSampler, texCoord).x;
	return ps_out;
}

technique RestoreDepth
{
    pass Pass1
    {
		VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 ps_main();
		ZEnable = true;		
		ZWriteEnable = true;
    }
}
