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

texture ToolTexture;
sampler2D ToolTextureSampler = sampler_state
{
	Texture = <ToolTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture CloudTexture;
sampler2D CloudTextureSampler = sampler_state
{
	Texture = <CloudTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};
  
texture DepthTexture;
sampler2D DepthTextureSampler = sampler_state
{
	Texture = <DepthTexture>;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
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
float In1_Precomp[3];

float In2[3];
float Out2[3];
float In2_Precomp[3];

float2 RandomOffset;

// Helper for modifying the saturation of a color.
float4 AdjustSaturation(float4 color, float saturation)
{
    // The constants 0.3, 0.59, and 0.11 are chosen because the
    // human eye is more sensitive to green light, and less to blue.
    float grey = dot(color, float3(0.3, 0.59, 0.11));

    return lerp(grey, color, saturation);
}


float4 ToneMap(float4 color, int channel)
{
    float amplitude = length(color);
    float in1 = In1[channel]; float out1 = Out1[channel];
    float in2 = In2[channel]; float out2 = Out2[channel];

	float in1_precomp = In1_Precomp[channel]; // == out1 / (in1*(in1-in2))
	float in2_precomp = In2_Precomp[channel]; // == out2 / (in2*(in2-in1))

    // 2nd order lagrangian polynomial, could be simplified to monic 
    // basis but then the coefficients are harder to determine.    
    //float weight =
	//	(out1*amplitude*(amplitude-in2))/(in1*(in1-in2)) +
	//	(out2*amplitude*(amplitude-in1))/(in2*(in2-in1));	
    float weight =
		amplitude*(amplitude-in2)*in1_precomp +
		amplitude*(amplitude-in1)*in2_precomp;	

	return weight*normalize(color);	
}

float4 ChannelPixelShader(float2 texCoord : TEXCOORD0, int channel)
{
    // Look up the bloom and original base image colors.
    float4 bloom = tex2D(BlurGeometryRenderSampler, texCoord);
    float4 base = tex2D(GeometryRenderSampler, texCoord);
    
    // Weight Bloom component according to sensitivity
    bloom = saturate(bloom/(1.0f-BloomSensitivity[channel]) - 2*BloomSensitivity[channel]);

    // Adjust color saturation and intensity.
    bloom = AdjustSaturation(bloom, BloomSaturation[channel]) * BloomIntensity[channel];
    base = AdjustSaturation(base, BaseSaturation[channel]) * BaseIntensity[channel];

    //bloom *= BloomIntensity[channel];
    //base *= BaseIntensity[channel];
    
    // Darken down the base image in areas where there is a lot of bloom,
    // to prevent things looking excessively burned-out.
    //base *= (1 - saturate(bloom));
    
    // Combine the two images.
    float4 tonemapped = ToneMap(base + bloom, channel);    
    return tonemapped;
}

inline float GradientY(float2 texCoord)
{
	float yRaw = tex2D(ToolTextureSampler, texCoord).x;
	return saturate(yRaw * 2 - 1.1); // higher value: blue tone starts at higher altitude
}

float BlueTopOverlayStrength = 1;
inline float4 GradientYBlueMap(float4 input, float2 texCoord, float weight)
{
	float y = GradientY(texCoord);
	
	float newRed = input.r;
	float newGreen = saturate(input.g / 0.6) + 0.03;
	float newBlue = saturate(input.b / 0.15) + 0.1;
	float newAlpha = input.a;
	
	float4 newColor = float4(newRed, newGreen, newBlue, newAlpha);
	newColor = saturate(newColor * 2.5);
	
	return lerp(input, newColor, y*weight*BlueTopOverlayStrength);
}

//float4 orangeFogColor = float4(1,0.3,0,1);
float4 orangeFogColor = float4(1,1,1,1);
float4 blueFogColor = float4(0,0.7,1,1);

float FogZOff, FogZMul, FogYOff, FogYMul, FogGlobMul;
float3 FogColor;//=float4(0,0,1,1);


inline void ApplyFog(inout float4 img, in float2 texCoord, in float weight)
{
	// compute depth intensity
	float zRaw = tex2D(DepthTextureSampler, texCoord).r;
	float zRescaled = float4(1-(1-tex2D(DepthTextureSampler, texCoord).r-FogZOff)*FogZMul,0,0,1);
	float z = saturate(zRescaled);

	// compute vertical intensity
	float yRaw = tex2D(ToolTextureSampler, texCoord).x;
	float y = saturate((1-yRaw*2+FogYOff)*FogYMul);

	// compute total intensity
	float grad = GradientY(texCoord);
	float fogIntensity = saturate((z*y)*FogGlobMul);

	// compute final image
	img = lerp(img, float4(FogColor,1), saturate(fogIntensity*weight));
}

const float flickerStrength                                                                                                                                                                                                                                                                                                                    = 0.0025;
inline float2 PerturbTexCoord(in float2 texCoord)
{
	return texCoord; // TODO: find some better way for randomness than some cloud texture...

	float yRaw = tex2D(ToolTextureSampler, texCoord).x;
	float y = saturate((1-yRaw*2));
	
	float4 clouds = tex2D(CloudTextureSampler, texCoord + RandomOffset);
	float2 perturbation = clouds.gb * 2 * flickerStrength - flickerStrength;
	float2 perturbedTexCoord = texCoord + perturbation;
	
	return lerp(texCoord, perturbedTexCoord, y);	
}

struct PostPixelShaderOutput
{
	float4 color : COLOR0;
	float depth  : DEPTH;
};

PostPixelShaderOutput PostPixelShader(float2 texCoord : TEXCOORD0)
{
	PostPixelShaderOutput result;

	float2 perturbedTexCoord = PerturbTexCoord(texCoord);

	float4 channel1 = ChannelPixelShader(perturbedTexCoord, 0);
	float4 channel2 = ChannelPixelShader(perturbedTexCoord, 1);
	float4 channel3 = ChannelPixelShader(perturbedTexCoord, 2);
		
	float4 channel_map = tex2D(RenderChannelColorSampler, perturbedTexCoord);
    
    float4 combined = channel1*channel_map.r + channel2*channel_map.g + channel3*channel_map.b;
    
    float gradientWeight = saturate(channel_map.r * 0.3 + channel_map.g * 1 + channel_map.b * 1); // players should be less affected
    float fogWeight = saturate(channel_map.r * 0.75 + channel_map.g * 1 + channel_map.b * 1); // players should be less affected
    
    combined = GradientYBlueMap(combined, perturbedTexCoord, gradientWeight);
    ApplyFog(combined, perturbedTexCoord, fogWeight);
    
    result.depth = tex2D(DepthTextureSampler, texCoord).r;
	result.color = combined;
	
	return result;
}

technique BloomCombine
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PostPixelShader();
        ZEnable = true;
        ZWriteEnable = true;
    }
}
