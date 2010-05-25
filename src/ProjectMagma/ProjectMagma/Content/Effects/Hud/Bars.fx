sampler i_hate_microsoft_dont_remove_this_it_wont_work : register(s0);

texture BackgroundTexture;
sampler2D BackgroundSampler = sampler_state
{
	Texture = <BackgroundTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture ComponentTexture;
sampler2D ComponentSampler = sampler_state
{
	Texture = <ComponentTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};


const float3x3 mirrorXY = float3x3(
	-1,  0,  1,
	 0, -1,  1,
	 0,  0,  1
);

float3x3 PlayerMirror;
float2 Size;
float HealthValue;
float EnergyValue;

float3 PlayerColor1;
float3 PlayerColor2;
const float3 EnergyColor1 = float3(0.87, 0.91, 0.97);
const float3 EnergyColor2 = float3(0.84, 0.84, 0.86);

const float BevelStrength = 0.7;

float HealthBlink;
float EnergyBlink;

float4 BarColor(float2 texCoord, float3 beginColor, float3 endColor, float value, float blink)
{
	// load four-channel texture containing individual components in each channel
	float4 components = tex2D(ComponentSampler, texCoord);
	
	// compute background color
	float4 backgroundRgba = tex2D(BackgroundSampler, texCoord);
	float3 background = backgroundRgba.rgb;	
	
	// compute content mask
	float contentMask = components.b;
	if(texCoord.x > 0.2)
	{
		float2 valueOffset = float2((1-value) * 0.76, 0);
		contentMask *= tex2D(ComponentSampler, texCoord + valueOffset).b;
	}
	
	// compute content color
	float3 color = lerp(beginColor, endColor, texCoord.x);
	float highlight = components.r * BevelStrength;
	float shadow = components.g * BevelStrength;
	float3 contentColor = color + highlight - shadow;
	contentColor = lerp(contentColor, float3(1, 1, 1), blink);
	
	// compute final color
	float notch = (components.a - 0.4) * 0.75;
	float3 sum = lerp(background, contentColor, contentMask) + notch;	
	return float4(sum, backgroundRgba.a);
}


float4 PixelShader(float2 texCoord : TEXCOORD0) : COLOR0
{
	// different players want their huds in different mirroring configurations
	float2 healthBarCoord = mul(PlayerMirror, float3(texCoord,1)).xy;
	float2 energyBarCoord = mul(PlayerMirror, mul(mirrorXY, float3(texCoord,1))).xy;
	
	// normalize everything to [0,1]x[0,1] inside one bar
	float2 normalizeMultiplier = float2(Size.x/224.0f, Size.y/38.0f);
	
	// compute bar colors and return them
	float4 healthBar = BarColor(healthBarCoord*normalizeMultiplier, PlayerColor1, PlayerColor2, HealthValue, HealthBlink);
	float4 energyBar = BarColor(energyBarCoord*normalizeMultiplier, EnergyColor1, EnergyColor2, EnergyValue, EnergyBlink);
    return healthBar + energyBar;
}


technique BloomCombine
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShader();	
    }
}
