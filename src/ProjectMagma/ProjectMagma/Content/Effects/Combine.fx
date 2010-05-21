
// dominik käser's comment... in fact the SpriteBatch class which we
// use to render fullscreen quads usually takes one texture as an input
// and renders to a given rendertarget. the input texture is set as
// s0. Therefore if we don't explicitly specify a sampler at s0
// we may end up with one of the other textures set as s0. Which one
// gets s0 is up to the optimizer (we may influence it as well
// by just assining s0 to the appropriate sampler below.
sampler i_hate_microsoft_dont_remove_this_it_wont_work : register(s0);

texture OpaqueColorBuffer;
sampler2D OpaqueColorSampler = sampler_state
{
	Texture = <OpaqueColorBuffer>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture TransparentColorBuffer;
sampler2D TransparentColorSampler = sampler_state
{
	Texture = <TransparentColorBuffer>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float4 ps_main(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 opaqueColor = tex2D(OpaqueColorSampler, texCoord);
	float4 transparentColor = tex2D(TransparentColorSampler, texCoord);
	return float4(transparentColor.a * transparentColor.rgb + (1-transparentColor.a) * opaqueColor.rgb, 1);
}

technique Combine
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 ps_main();
    }
}
