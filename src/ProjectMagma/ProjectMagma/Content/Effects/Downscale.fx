sampler i_hate_microsoft_dont_remove_this_it_wont_work : register(s0);

float2 HalfPixelSize;

texture HDRColorBuffer;
sampler2D HDRColorSampler = sampler_state
{
	Texture = <HDRColorBuffer>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture RenderChannelBuffer;
sampler2D RenderChannelSampler = sampler_state
{
	Texture = <RenderChannelBuffer>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct PSOutput
{
	float4 DownscaledHDRColor      : COLOR0;
	float4 DownscaledRenderChannel : COLOR1;
};

PSOutput downscale_ps(float2 texCoord : TEXCOORD0)
{
	PSOutput outp;
    outp.DownscaledHDRColor = tex2D(HDRColorSampler, texCoord - HalfPixelSize);
    outp.DownscaledRenderChannel = tex2D(RenderChannelSampler, texCoord - HalfPixelSize);
	return outp;
}

technique Downscale
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 downscale_ps();
    }
}
