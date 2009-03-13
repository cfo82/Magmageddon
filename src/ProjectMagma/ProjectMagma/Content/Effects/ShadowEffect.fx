//Our shader's global variables. We have 2 different output structures, 
//one for the DepthMap technique, and one for the Scene technique, 
//this is because they both require different values (HLSL requires that all
//values of an output structure be set).

struct VS_INPUT
{
    float4 Position         : POSITION;
    float2 TexCoords        : TEXCOORD0;
};

struct DEPTH_VS_OUTPUT
{
    float4 Position         : POSITION;
    float4 WorldPosition    : TEXCOORD1;
};

struct SCENE_VS_OUTPUT
{
    float4 Position         : POSITION;
    float2 TexCoords        : TEXCOORD0;
    float4 WorldPosition    : TEXCOORD1;
    
    // ShadowProjection is similar to Position, except its the position 
    // from the light's view, not the camera. We need both to be able to project
    // our shadow map.
    float4 ShadowProjection : TEXCOORD2; 
	float4 Color            : COLOR0;                                         
                                         
};

// Our 2 WVP matrices, one for the camera, and one for the light
float4x4 WorldCameraViewProjection;
float4x4 WorldLightViewProjection;

float4x4 World;

// The position of our light
float3 LightPosition;

// The far clip distance of our light's view matrix. 
//We use this value to normalise our distances 
//(so that they fit into the 0.0 - 1.0 colour range in HLSL)
float NearClip = 9500;
float FarClip = 10500;

Texture ShadowMap;

sampler shadowSampler = sampler_state
{
    Texture = <ShadowMap>;
    
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    
    AddressU = CLAMP;
    AddressV = CLAMP;
};


//Depth Map Pass
//==============
//This part of the shader is used to render the scene from the view of our spot light.
//It's just like a normal shader, except instead of colouring the models with a texture,
//it gives the pixels a colour based on their distance from the light.


DEPTH_VS_OUTPUT Depth_VS (VS_INPUT Input)
{
    DEPTH_VS_OUTPUT Output;
    
    // Our standard WVP multiply, using the light's 
    //View and Projection matrices
    Output.Position = mul(Input.Position, WorldLightViewProjection);

    // We also keep a copy of our position that is only 
    //multiplied by the world matrix. This is
    // because we do not want the camera's angle/position to 
    //affect our distance calculations
    Output.WorldPosition = mul(Input.Position, World);
    Output.WorldPosition /= Output.WorldPosition.w;
    
    return Output;
}

float4 Depth_PS (DEPTH_VS_OUTPUT Input) : COLOR0
{
    // Get the distance from this pixel to the light, 
    //we divide by the far clip of the light so that it 
    //will fit into the 0.0-1.0 range of pixel shader colours
    
    // general case, spotlight:
    // float dist = length(LightPosition - Input.WorldPosition);
    
    // special case for us: orthographic from above:
    float dist = LightPosition.y - Input.WorldPosition.y;
    float depth = (dist - NearClip) / (FarClip - NearClip);
    
    return float4(depth,depth,depth,1);
}

technique DepthMap
{
    pass P0
    {
        VertexShader = compile vs_2_0 Depth_VS();
        PixelShader  = compile ps_2_0 Depth_PS();
    }
}


//Final Scene Render Pass
//=======================


SCENE_VS_OUTPUT Scene_VS (VS_INPUT Input,     float4 InColor			: COLOR0)
{
    SCENE_VS_OUTPUT Output;
    
    Output.Position = mul(Input.Position, WorldCameraViewProjection);

    Output.WorldPosition = mul(Input.Position, World);
    Output.WorldPosition /= Output.WorldPosition.w;
    
    Output.TexCoords = Input.TexCoords;
    Output.Color = InColor;
    
    // This time we need the position from the light's view as well,
    // to project the depth map
    Output.ShadowProjection = mul(Input.Position, 
                                  WorldLightViewProjection);

    return Output;
}

float4 Scene_PS (SCENE_VS_OUTPUT Input) : COLOR0
{
    float2 projectedTexCoords;
    
    // Calculate the texture projection texcoords, we divide by 2 
    //and add 0.5 because we need to convert the coordinates 
    //from a -1.0 - 1.0 range into the 0.0 - 1.0 range
    projectedTexCoords[0] = (Input.ShadowProjection.x / 
                             Input.ShadowProjection.w / 2.0f) + 0.5f;
    projectedTexCoords[1] = (-Input.ShadowProjection.y / 
                              Input.ShadowProjection.w / 2.0f) + 0.5f;
    
    // Get our depth colour from the projected depth map
    float4 depth = tex2D(shadowSampler, projectedTexCoords);

    // Calculate our distance to the light again
    // general case, spotlight:
    // float len = (length(LightPosition - Input.WorldPosition) - NearClip) / (FarClip - NearClip);
    
    // special case for us: orthographic from above:
    float len = (LightPosition.y - Input.WorldPosition.y - NearClip) / (FarClip - NearClip);


    
    // This isn't necessary, it just gives the scene a little more definition
    //float4 color = 1.0 - len;
    float4 color = len;
        
    // The depth bias is a very small value that we subtract 
    //from our length to avoid any calculation issues 
    //(which will result in image artifacts)
    //float depthBias = (1.0f / 200.0f);
    //float depthBias = -0.95f;
    float depthBias = 0.007f;
    //float depthBias = -0.005f;
    
    // Test if the distance from the pixel to the light is bigger 
    //than the depth we have in the texture
    if(len - depthBias > depth.r)
    {
        // yes? then lets darken the pixel to black
        // (In normal use, you'd use a lighting equation here to 
        //determine exactly how much to darken the pixel)
        color = float4(0,0,0,.3);
    } else {
		color = float4(1,1,1,0);
    }
    
    return color;
}

technique Scene
{
    pass P0
    {
        VertexShader = compile vs_2_0 Scene_VS();
        PixelShader  = compile ps_2_0 Scene_PS();
    }
}
