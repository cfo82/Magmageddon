// Camera parameters.
float4x4 View;
float4x4 Projection;

// Parameters describing the billboard itself.
float3 BillboardPosition;
float BillboardWidth;
float BillboardHeight;
float4 BillboardColor;

texture BillboardTexture;

//-----------------------------------------------------------------------------------------------------------
sampler BillboardSampler = sampler_state
{
    Texture = (BillboardTexture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VS_INPUT
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};


VS_OUTPUT VertexShader(VS_INPUT input)
{
    float width = BillboardWidth;
    float height = BillboardHeight;
    float3 normal = float3(0,1,0);

    // Work out what direction we are viewing the billboard from.
    float3 viewDirection = View._m02_m12_m22;

    float3 rightVector = normalize(cross(viewDirection, normal));

    // Calculate the position of this billboard vertex.
    float3 position = BillboardPosition;

    // Offset to the left or right.
    position -= rightVector * (input.TexCoord.x - 0.5) * width;
    
    // Offset upward if we are one of the top two vertices.
    position += normal * (1 - input.TexCoord.y) * height;

    // Apply the camera transform.
    float4 viewPosition = mul(float4(position, 1), View);

    VS_OUTPUT output;
    output.Position = mul(viewPosition, Projection);
    output.TexCoord = input.TexCoord;
    return output;
}

float4 PixelShader(float2 texCoord : TEXCOORD0) : COLOR0
{
    return tex2D(BillboardSampler, texCoord) * BillboardColor;
}

technique Billboards
{
    pass Render
    {
        VertexShader = compile vs_1_1 VertexShader();
        PixelShader = compile ps_1_1 PixelShader();

        AlphaBlendEnable = true;
        BlendOp = Add;
        SrcBlend = One;
        DestBlend = One;

        AlphaTestEnable = false;
        ZEnable = false;
        ZWriteEnable = false;
        CullMode = None;
    }
}
