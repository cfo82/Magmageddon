
//-----------------------------------------------------------------------------------------------------------
texture IceSpikeTexture;
float4x4 World;
float4x4 InverseTransposeWorld;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;




//-----------------------------------------------------------------------------------------------------------
sampler IceSpikeSampler = sampler_state
{
    Texture = (IceSpikeTexture);
    
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    
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
	output.MainAlpha = abs(dot(normalize(world_normal), dir_to_cam));
	output.TextureCoordinates = input.TextureCoordinates;

	return output;
}




//-----------------------------------------------------------------------------------------------------------
float4 IceSpikePixelShader(IceSpikePixelShaderInput input) : COLOR
{
    float4 color = tex2D(IceSpikeSampler, input.TextureCoordinates);
    color.a *= input.MainAlpha;
    return color;
}




//-----------------------------------------------------------------------------------------------------------
Technique IceSpikeEffect
{
	Pass
	{
		VertexShader = compile vs_3_0 IceSpikeVertexShader();
		PixelShader	= compile ps_3_0 IceSpikePixelShader();
	}
}
