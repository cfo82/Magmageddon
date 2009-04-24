texture blah : register(s0);

texture GeometryRender;
sampler2D GeometryRenderSampler = sampler_state
{
	Texture = <GeometryRender>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture BlurGeometryRender;
sampler2D BlurGeometryRenderSampler = sampler_state
{
	Texture = <BlurGeometryRender>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture RenderChannelColor;
sampler2D RenderChannelColorSampler = sampler_state
{
	Texture = <RenderChannelColor>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};
  

float BloomIntensity[3] = {2.0, 2.0, 2.0};
float BaseIntensity[3] = {1.0, 1.0, 1.0};

float BloomSaturation[3] = {1.0, 0.0, -1.0};
float BaseSaturation[3] = {1.0, 0.0, -1.0};


// Helper for modifying the saturation of a color.
float4 AdjustSaturation(float4 color, float saturation)
{
    // The constants 0.3, 0.59, and 0.11 are chosen because the
    // human eye is more sensitive to green light, and less to blue.
    float grey = dot(color, float3(0.3, 0.59, 0.11));

    return lerp(grey, color, saturation);
}


float4 ChannelPixelShader(float2 texCoord : TEXCOORD0, int channel)
{
    // Look up the bloom and original base image colors.
    float4 bloom = tex2D(BlurGeometryRenderSampler, texCoord);
    float4 base = tex2D(GeometryRenderSampler, texCoord);
    
    // Adjust color saturation and intensity.
    bloom = AdjustSaturation(bloom, BloomSaturation[channel]) * BloomIntensity[channel];
    base = AdjustSaturation(base, BaseSaturation[channel]) * BaseIntensity[channel];
    
    // Darken down the base image in areas where there is a lot of bloom,
    // to prevent things looking excessively burned-out.
    base *= (1 - saturate(bloom));
    
    // Combine the two images.
    return base + bloom;
}


float4 PixelShader(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 channel1 = ChannelPixelShader(texCoord, 0);
	float4 channel2 = ChannelPixelShader(texCoord, 1);
	float4 channel3 = ChannelPixelShader(texCoord, 2);
	
	float4 channel_map = tex2D(RenderChannelColorSampler, texCoord);
    
    // Combine the two images.
    return channel1*channel_map.r + channel2*channel_map.g + channel3*channel_map.b;
}


technique BloomCombine
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShader();
    }
}
