
// dominik käser's comment... in fact the SpriteBatch class which we
// use to render fullscreen quads usually takes one texture as an input
// and renders to a given rendertarget. the input texture is set as
// s0. Therefore if we don't explicitly specify a sampler at s0
// we may end up with one of the other textures set as s0. Which one
// gets s0 is up to the optimizer (we may influence it as well
// by just assining s0 to the appropriate sampler below.
sampler i_hate_microsoft_dont_remove_this_it_wont_work : register(s0);

texture OpaqueDepthBuffer;
sampler2D OpaqueDepthSampler = sampler_state
{
	Texture = <OpaqueDepthBuffer>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture TransparentDepthBuffer;
sampler2D TransparentDepthSampler = sampler_state
{
	Texture = <TransparentDepthBuffer>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float4 ps_main(float2 texCoord : TEXCOORD0) : COLOR0
{
	float opaqueDepth = tex2D(OpaqueDepthSampler, texCoord).x;
	float transparentDepth = tex2D(TransparentDepthSampler, texCoord).x;
	return float4(opaqueDepth, transparentDepth, 0, 0);
}

technique Combine
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 ps_main();
    }
}
