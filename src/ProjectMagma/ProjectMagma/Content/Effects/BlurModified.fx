#define SAMPLE_COUNT 7

sampler i_hate_microsoft_dont_remove_this_it_wont_work : register(s0);

float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];

texture RenderChannelColor;
sampler2D RenderChannelSampler = sampler_state
{
	Texture = <RenderChannelColor>;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture GeometryRender;
sampler2D GeometryRenderSampler = sampler_state
{
	Texture = <GeometryRender>;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};


struct PSOutput
{
	float4 Color              : COLOR0;
	float4 RenderChannelColor : COLOR1;
};


PSOutput PixelShaderMain(float2 texCoord : TEXCOORD0)
{
	PSOutput outp;
    outp.Color = 0;
    outp.RenderChannelColor = 0;
    
    // Combine a number of weighted image filter taps.
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
		outp.Color += tex2D(GeometryRenderSampler, texCoord + SampleOffsets[i]) * SampleWeights[i];
		outp.RenderChannelColor += tex2D(RenderChannelSampler, texCoord + SampleOffsets[i]) * SampleWeights[i];
    }	
	    
	return outp;
}


technique GaussianBlur
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderMain();
    }
}
