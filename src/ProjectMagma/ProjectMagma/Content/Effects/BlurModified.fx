// Pixel shader applies a one dimensional gaussian blur filter.
// This is used twice by the bloom postprocess, first to
// blur horizontally, and then again to blur vertically.

//sampler TextureSampler : register(s0);

#define SAMPLE_COUNT 7

float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];

texture RenderChannelColor;
sampler2D RenderChannelSampler = sampler_state
{
	Texture = <RenderChannelColor>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};


texture geom;
sampler2D TextureSampler = sampler_state
{
	Texture = <geom>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};


struct PSOutput
{
	float4 Color              : COLOR0;
	float4 RenderChannelColor : COLOR1;
};


PSOutput PixelShader(float2 texCoord : TEXCOORD0) : COLOR0
{
	PSOutput outp;
    outp.Color = 0;
    outp.RenderChannelColor = 0;
    
    // Combine a number of weighted image filter taps.
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
		//if(tex2D(RenderChannelSampler, texCoord).r>0.5)
		//{
		//c += tex2D(TextureSampler, texCoord + SampleOffsets[i]*1.5) * SampleWeights[i] * tex2D(RenderChannelSampler, texCoord+ SampleOffsets[i]*1.5).r;
		//c += tex2D(TextureSampler, texCoord + SampleOffsets[i]*0.3) * SampleWeights[i] * tex2D(RenderChannelSampler, texCoord+ SampleOffsets[i]*0.3).g;
		////} else {
		outp.Color += tex2D(TextureSampler, texCoord + SampleOffsets[i]) * SampleWeights[i];
		outp.RenderChannelColor += tex2D(RenderChannelSampler, texCoord + SampleOffsets[i]) * SampleWeights[i];
		//}
    }	
	    
    //return tex2D(RenderChannelSampler, texCoord);
    //if(tex2D(RenderChannelSampler, texCoord).g>0.5)
	return outp;
	//else
		//return tex2D(TextureSampler, texCoord);
}


technique GaussianBlur
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShader();
    }
}
