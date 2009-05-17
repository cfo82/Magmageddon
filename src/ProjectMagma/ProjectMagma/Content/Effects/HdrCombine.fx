sampler i_hate_microsoft_dont_remove_this_it_wont_work : register(s0);

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

texture DepthTexture;
sampler2D DepthTextureSampler = sampler_state
{
	Texture = <DepthTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};
  
float BloomSensitivity[3];

float BloomIntensity[3];
float BaseIntensity[3];

float BloomSaturation[3];
float BaseSaturation[3];

float In1[3];
float Out1[3];
float In2[3];
float Out2[3];


// Helper for modifying the saturation of a color.
float4 AdjustSaturation(float4 color, float saturation)
{
    // The constants 0.3, 0.59, and 0.11 are chosen because the
    // human eye is more sensitive to green light, and less to blue.
    float grey = dot(color, float3(0.3, 0.59, 0.11));

	//return color;

    return lerp(grey, color, saturation);
}


float4 ToneMap(float4 color, int channel)
{
    float amplitude = length(color);
    float in1 = In1[channel]; float out1 = Out1[channel];
    float in2 = In2[channel]; float out2 = Out2[channel];

    // 2nd order lagrangian polynomial, could be simplified to monic 
    // basis but then the coefficients are harder to determine.    
    float weight =
		(out1*amplitude*(amplitude-in2))/(in1*(in1-in2)) +
		(out2*amplitude*(amplitude-in1))/(in2*(in2-in1));	

	return weight*normalize(color);	
}

		//(out2*ll*(ll-in3))/(in2*(in2-in3)) +
		//(out3*ll*(ll-in2))/(in3*(in3-in2));	


float4 ChannelPixelShader(float2 texCoord : TEXCOORD0, int channel)
{
    // Look up the bloom and original base image colors.
    float4 bloom = tex2D(BlurGeometryRenderSampler, texCoord);
    float4 base = tex2D(GeometryRenderSampler, texCoord);
    
    // Weight Bloom component according to sensitivity
    bloom = saturate(bloom/(1.0f-BloomSensitivity[channel]) - 2*BloomSensitivity[channel]);

    // Adjust color saturation and intensity.
    //bloom = AdjustSaturation(bloom, BloomSaturation[channel]) * BloomIntensity[channel];
    //base = AdjustSaturation(base, BaseSaturation[channel]) * BaseIntensity[channel];

    bloom *= BloomIntensity[channel];
    base *= BaseIntensity[channel];
    
    // Darken down the base image in areas where there is a lot of bloom,
    // to prevent things looking excessively burned-out.
    //base *= (1 - saturate(bloom));
    
    // Combine the two images.
    float4 tonemapped = ToneMap(base + bloom, channel);    
    return tonemapped;
}

float4 GradientMap(float4 input, float2 texCoord)
{
	float yRaw = tex2D(DepthTextureSampler, texCoord).x;
	
	float y = saturate(yRaw * 2.0 - 0.8);
	
	float newRed = input.r;
	float newGreen = saturate(input.g / 0.6) + 0.03;
	float newBlue = saturate(input.b / 0.15) + 0.1;
	float newAlpha = input.a;
	
	float4 newColor = float4(newRed, newGreen, newBlue, newAlpha);
	newColor = saturate(newColor / 0.5);
	
	//input = float4(0,0,0,1);
	//newColor = float4(1,1,1,1);
	
	return lerp(input, newColor, y);
}

float4 PixelShader(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 channel1 = ChannelPixelShader(texCoord, 0);
	float4 channel2 = ChannelPixelShader(texCoord, 1);
	float4 channel3 = ChannelPixelShader(texCoord, 2);
	
	float4 channel_map = tex2D(RenderChannelColorSampler, texCoord);
    
    float4 combined = channel1*channel_map.r + channel2*channel_map.g + channel3*channel_map.b;
    return GradientMap(combined, texCoord);
    //return tex2D(DepthTextureSampler, texCoord);

    // Combine the two images.

}


technique BloomCombine
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShader();
    }
}
