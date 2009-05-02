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

texture HighlightTexture;
sampler2D HighlightSampler = sampler_state
{
	Texture = <HighlightTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float2 TotalSize;
float HealthValue;
float EnergyValue = 0.3;

float3 HealthColor1 = float3(0.91, 0.08, 0.64);
float3 HealthColor2 = float3(0.77, 0.08, 0.86);


const float BevelStrength = 0.7;
float4 PixelShader(float2 texCoord : TEXCOORD0) : COLOR0
{
	float2 healthBarCoord = float2(
		texCoord.x * TotalSize.x/224.0f,
		texCoord.y * TotalSize.y/38.0f
	);
	
	float2 energyBarCoord = float2(
		(1-texCoord.x) * TotalSize.x/224.0f,
		(1-texCoord.y) * TotalSize.y/38.0f
	);		

	float4 healthBg = tex2D(BackgroundSampler, healthBarCoord);
	float4 energyBg = tex2D(BackgroundSampler, energyBarCoord);
	float4 healthHl = tex2D(HighlightSampler, healthBarCoord);
	float4 energyHl = tex2D(HighlightSampler, energyBarCoord);
	
	float3 healthCl = lerp(HealthColor1, HealthColor2, healthBarCoord.x);
	float hl = healthHl.r*BevelStrength;
	float sh = healthHl.g*BevelStrength;
	float mask1 = healthHl.b;
	float notch = healthHl.a;
	float notchMask = abs(notch-0.5)*2;
	float notchAdd = saturate(notch-0.5);
	float notchSub = -saturate(0.5-notch);
	
	//float2 healthMaskCoord = float2(
		//(1-texCoord.x) * TotalSize.x/224.0f,
		//(1-texCoord.y) * TotalSize.y/38.0f
	//);		
	//
	float mask2 = mask1 * tex2D(HighlightSampler, healthBarCoord + float2(HealthValue*0.75,0)).b;
	float mask;
	if(healthBarCoord.x<0.2)
		mask = mask1;
	else
		mask = mask2;
	
	//float4 healthSum = healthBg;// + float4(float3(1,1,1) * healthHl.a, 0);
	float3 healthSum = healthBg.rgb * (1-mask)     +       (healthCl + hl - sh)*(mask-notchMask)    +    (notchAdd-notchSub)*notchMask;
	//float4 energySum = energyBg + energyHl.rgb * energyHl.a;
	float4 energySum = 0;
	//float4 highlight = tex2D(HighlightSampler, barCoord);
	return float4(notchMask, notchMask, notchMask,1);
    return float4(healthSum,healthBg.a) + energySum;// + float4(0.5,0,0,0.5);
    //return float4(1,0,0,1);
}


technique BloomCombine
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShader();
    }
}
