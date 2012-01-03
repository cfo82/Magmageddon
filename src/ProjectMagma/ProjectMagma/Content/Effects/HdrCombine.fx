//#include "Sm3SpriteBatch.inc"

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

texture BlurGeometryRender;
sampler2D BlurGeometryRenderSampler = sampler_state
{
	Texture = <BlurGeometryRender>;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture RenderChannelColor;
sampler2D RenderChannelColorSampler = sampler_state
{
	Texture = <RenderChannelColor>;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

//texture ToolTexture;
//sampler2D ToolTextureSampler = sampler_state
//{
//	Texture = <ToolTexture>;
//	MinFilter = Point;
//	MagFilter = Point;
//	MipFilter = Point;
//	AddressU = Clamp;
//	AddressV = Clamp;
//};

texture CloudTexture;
sampler2D CloudTextureSampler = sampler_state
{
	Texture = <CloudTexture>;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
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
float4x4 InverseView;
float4x4 InverseProjection;

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

float4 ChannelPixelShader(int channel, float4 bloom, float4 base)
{
    // Look up the bloom and original base image colors. (get them passed into the function)
    
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

inline float GradientY(float2 texCoord, in float yRaw)
{
	return saturate(yRaw * 2 - 1.1); // higher value: blue tone starts at higher altitude
}

float BlueTopOverlayStrength = 1;
inline float4 GradientYBlueMap(float4 input, float2 texCoord, float weight, float yRaw)
{
	float y = GradientY(texCoord, yRaw);
	
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


inline void ApplyFog(inout float4 img, in float2 texCoord, in float weight, in float alpha, in float yRaw)
{
	// zRescaled.g seems to be correct on a pc but may be wrong on the xbox. we need
	// to check as soon as the ToolTexture has been replaced.

	// compute depth intensity
	float2 zRaw = tex2D(DepthTextureSampler, texCoord).rg;
	float2 zRescaled = 1-(1-tex2D(DepthTextureSampler, texCoord).rg-FogZOff)*FogZMul;
	float z = saturate(zRescaled.r);//saturate((1-alpha)*zRescaled.r+alpha*zRescaled.g);

	// compute vertical intensity
	float y = saturate((1-yRaw*2+FogYOff)*FogYMul);

	// compute total intensity
	float grad = GradientY(texCoord, yRaw);
	float fogIntensity = saturate((z*y)*FogGlobMul);

	// compute final image
	img = lerp(img, float4(FogColor,1), saturate(fogIntensity*weight));
}

const float flickerStrength = 0.0025;
inline float2 PerturbTexCoord(in float2 texCoord)
{
	return texCoord; // TODO: find some better way for randomness than some cloud texture...

	//float yRaw = tex2D(ToolTextureSampler, texCoord).x;
	//float y = saturate((1-yRaw*2));
	
	//float4 clouds = tex2D(CloudTextureSampler, texCoord + RandomOffset);
	//float2 perturbation = clouds.gb * 2 * flickerStrength - flickerStrength;
	//float2 perturbedTexCoord = texCoord + perturbation;
	
	//return lerp(texCoord, perturbedTexCoord, y);	
}

struct PostPixelShaderOutput
{
	float4 color : COLOR0;
	float depth  : DEPTH;
};

float3 GetWorldSpacePosition(float2 texCoord, float2 vpos)
{
	float3 s = float3(texCoord.x*2-1,(1-texCoord.y)*2-1,tex2D(DepthTextureSampler, texCoord).r);
	float3 v = mul(s, InverseProjection);
	return mul(v,InverseView);

	/*float z = tex2D(DepthTextureSampler, texCoord).r;
	float x = texCoord.x*2-1;
	float y = (1-texCoord.y)*2-1;
	float4 vProjectedPos = float4(x,y,z,1);
	float4 vPositionVS = mul(vProjectedPos,InverseProjection);
	vPositionVS.xyz = vPositionVS.xyz/vPositionVS.w;
	float4 vPositionWS = mul(float4(vPositionVS.xyz,1),InverseView);
	return vPositionWS.xyz;

	float3 v0 = texCoord.x*FrustumCorners[0] + (1-texCoord.x)*FrustumCorners[1];
	float3 v1 = texCoord.x*FrustumCorners[2] + (1-texCoord.x)*FrustumCorners[3];
	float3 dir = texCoord.y*v0 + (1-texCoord.y)*v1;

	float depth = tex2D(DepthTextureSampler, texCoord).r;
	float2 planes = float2(1000/(1-1000), 1000*1/(1-1000));
	
	float3 pos_vs;
	pos_vs.z = -planes.y / (depth + planes.x);
	pos_vs.xy = 2*(vpos/float2(1279,719))-float2(1,1);
	pos_vs.xy = pos_vs.xy*float2(-pos_vs.z,-pos_vs.z) / ProjectionWH;
	return mul(pos_vs, InverseView);
	
	
	//float modifier = dir.z / abs(distance);
	//float3 viewSpacePosition = dir*modifier;*/
}

PostPixelShaderOutput PostPixelShader(
	float2 texCoord : TEXCOORD0,
	float2 vpos : VPOS
)
{
	PostPixelShaderOutput result;

	float2 perturbedTexCoord = PerturbTexCoord(texCoord);
	float3 worldPosition = GetWorldSpacePosition(texCoord, vpos);

    float4 bloom = tex2D(BlurGeometryRenderSampler, texCoord);
    float4 base = tex2D(GeometryRenderSampler, texCoord);

	float4 channel1 = ChannelPixelShader(0, float4(bloom.rgb, 1), float4(base.rgb, 1));
	float4 channel2 = ChannelPixelShader(1, float4(bloom.rgb, 1), float4(base.rgb, 1));
	float4 channel3 = ChannelPixelShader(2, float4(bloom.rgb, 1), float4(base.rgb, 1));
		
	float4 channel_map = tex2D(RenderChannelColorSampler, perturbedTexCoord);
    
    float4 combined = channel1*channel_map.r + channel2*channel_map.g + channel3*channel_map.b;
    
    float gradientWeight = saturate(channel_map.r * 0.3 + channel_map.g * 1 + channel_map.b * 1); // players should be less affected
    float fogWeight = saturate(channel_map.r * 0.75 + channel_map.g * 1 + channel_map.b * 1); // players should be less affected
    
    combined = GradientYBlueMap(combined, perturbedTexCoord, gradientWeight, worldPosition.y);
    ApplyFog(combined, perturbedTexCoord, fogWeight, base.a, worldPosition.y);
    
    result.depth = tex2D(DepthTextureSampler, texCoord).r;
	result.color = combined;
	
	return result;
}

technique BloomCombine
{
    pass Pass1
    {
		VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 PostPixelShader();
        ZEnable = true;
        ZWriteEnable = true;
    }
}
