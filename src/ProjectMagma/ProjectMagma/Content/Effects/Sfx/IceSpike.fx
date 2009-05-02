
//-----------------------------------------------------------------------------------------------------------
texture IceSpikeTexture;
float4x4 World;
float4x4 InverseTransposeWorld;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;

struct PSOutput
{
	float4 Color              : COLOR0;
	float4 RenderChannelColor : COLOR1;
};



//-----------------------------------------------------------------------------------------------------------
sampler IceSpikeSampler = sampler_state
{
    Texture = (IceSpikeTexture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};




//-----------------------------------------------------------------------------------------------------------
struct IceSpikeVertexShaderInput
{
	float4 Position : POSITION;
	float3 Normal : NORMAL;
	float2 TextureCoordinates : TEXCOORD0;
};

struct IceSpikeVertexShaderOutput
{
	float4 ScreenPosition : POSITION;
	float2 TextureCoordinates : TEXCOORD0;
	float MainAlpha : TEXCOORD1;
};

struct IceSpikePixelShaderInput
{
	float2 TextureCoordinates : TEXCOORD0;
	float MainAlpha : TEXCOORD1;
};





//-----------------------------------------------------------------------------------------------------------
IceSpikeVertexShaderOutput IceSpikeVertexShader(
	IceSpikeVertexShaderInput input
)
{
	IceSpikeVertexShaderOutput output;
	
	float4 world_position = mul(input.Position, World);
	float4 view_position = mul(world_position, View);
	float4 screen_position = mul(view_position, Projection);
	
	float3 world_normal = mul(input.Normal, InverseTransposeWorld);
	float3 dir_to_cam = CameraPosition - world_position;
	
	output.ScreenPosition = screen_position;
	output.MainAlpha = abs(dot(normalize(world_normal), normalize(dir_to_cam)));
	output.TextureCoordinates = input.TextureCoordinates;

	return output;
}




//-----------------------------------------------------------------------------------------------------------
PSOutput IceSpikePixelShader(IceSpikePixelShaderInput input)
{
	PSOutput result;
    float4 color = tex2D(IceSpikeSampler, input.TextureCoordinates);
    color.a = color.a;//*clamp(input.MainAlpha, 0, 1);
    
    result.Color = color;
    result.RenderChannelColor = float4(0,0,1,0);
    return result;
}




//-----------------------------------------------------------------------------------------------------------
Technique IceSpikeEffect
{
	Pass
	{
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
        BlendOp = Add;
        AlphaTestEnable = true;
        AlphaFunc = Greater;
        AlphaRef = 0.5;
        ZEnable = false;
        ZWriteEnable = false;
        CullMode = None;

		VertexShader = compile vs_3_0 IceSpikeVertexShader();
		PixelShader	= compile ps_3_0 IceSpikePixelShader();
	}
}
