struct PSOutput
{
	float4 Color              : COLOR0;
	float4 RenderChannelColor : COLOR1;
	float4 RealDepth          : COLOR2;
};

const float DepthClipY = 600.0f;

uniform const float		DepthMapNearClip=0.0f;
uniform const float		DepthMapFarClip=3000.0f;

uniform const float		FogEnabled;	//	: register(c0);
uniform const float3	FogColor;	//	: register(c3);
uniform const float3	EyePosition;//		: register(c4);		// in world space


float4x4 LightViewProjection;