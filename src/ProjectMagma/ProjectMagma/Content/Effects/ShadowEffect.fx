//Our shader's global variables. We have 2 different output structures, 
//one for the DepthMap technique, and one for the Scene technique, 
//this is because they both require different values (HLSL requires that all
//values of an output structure be set).

struct VS_INPUT
{
    float4 Position         : POSITION;
    float2 TexCoords        : TEXCOORD0;
	float4 Normal			: NORMAL;
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
    float4 ShadowProjection : TEXCOORD2; 
    float4 Normal			: TEXCOORD3;
    // ShadowProjection is similar to Position, except its the position 
    // from the light's view, not the camera. We need both to be able to project
    // our shadow map.
	//float4 Color            : COLOR0;                                         
                                         
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

float SquashAmount;
float4x4 squashMatrix()
{
	return float4x4
	(
		lerp(1.0f, sqrt(2.0f), SquashAmount), 0.0f, 0.0f, 0.0f,
		0.0f, lerp(1.0f, 0.5f, SquashAmount), 0.0f, 0.0f,
		0.0f, 0.0f, lerp(1.0f, sqrt(2.0f), SquashAmount), 0.0f,
		0.0f, -SquashAmount*0.6f, 0.0f, 1.0f
	);
}
float4x4 Local = 1;
float4x4 LightViewProjection;

DEPTH_VS_OUTPUT Depth_VS (VS_INPUT Input)
{
    DEPTH_VS_OUTPUT Output;
    
    // Our standard WVP multiply, using the light's 
    //View and Projection matrices
    //Output.Position = mul(Input.Position, WorldLightViewProjection);


    float4 pos_ls = mul(Input.Position, Local);
    float4 pos_ss = mul(pos_ls, squashMatrix());
    float4 pos_ws = mul(pos_ss, World);
    float4 pos_ps = mul(pos_ws, LightViewProjection);

	Output.Position = pos_ps/pos_ps.w;

    // We also keep a copy of our position that is only 
    //multiplied by the world matrix. This is
    // because we do not want the camera's angle/position to 
    //affect our distance calculations
    Output.WorldPosition = pos_ws/pos_ws.w;
    //Output.WorldPosition /= Output.WorldPosition.w;
    
    return Output;
}

void Depth_PS (in DEPTH_VS_OUTPUT Input, out float4 outcolor : COLOR0)
{
    // Get the distance from this pixel to the light, 
    //we divide by the far clip of the light so that it 
    //will fit into the 0.0-1.0 range of pixel shader colours
    
    // general case, spotlight:
    // float dist = length(LightPosition - Input.WorldPosition);
    
    // special case for us: orthographic from above:
    //float dist = LightPosition.y - Input.WorldPosition.y;
    //float depth = (dist - NearClip) / (FarClip - NearClip);
    
    //return float4(depth,depth,depth,1);
    outcolor = Input.WorldPosition.y;
    //outcolor = 0.2;
}

technique DepthMap
{
    pass P0
    {
        VertexShader = compile vs_2_0 Depth_VS();
        PixelShader  = compile ps_2_0 Depth_PS();
        AlphaBlendEnable = false;
    }
}


//Final Scene Render Pass
//=======================


SCENE_VS_OUTPUT Scene_VS (VS_INPUT Input)
{
    SCENE_VS_OUTPUT Output;
   
    
    Output.Position = mul(Input.Position, WorldCameraViewProjection);

    Output.WorldPosition = mul(Input.Position, World);
    Output.WorldPosition /= Output.WorldPosition.w;
    
    Output.TexCoords = Input.TexCoords;
    Output.Normal = Input.Normal;
   // Output.Color = InColor;
    
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
    float3 blah2= LightPosition-Input.WorldPosition;
    float blah=dot(Input.Normal,normalize(blah2));
    if(len - depthBias > depth.r && abs(blah)>0.4)
    {
        // yes? then lets darken the pixel to black
        // (In normal use, you'd use a lighting equation here to 
        //determine exactly how much to darken the pixel)
        float alpha=0.4;
        
        // 9500 is 1
        // 10500 is 0
        // we want 9950
        
        //if(depth.r>0.4)
		//	alpha=0;
			
		alpha=saturate((0.485-depth.r)*30)*0.3;
		if(len>0.498) alpha=0; // evil hack. only topmost lava layer should be affected.
		color = float4(.15,.05,0,alpha);
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
        
        //AlphaBlendEnable = true;
        //SrcBlend = SrcAlpha;
        //DestBlend = InvSrcAlpha;
        
        //AlphaTestEnable = true;
		//AlphaFunc = Greater;
        //AlphaRef = 0.5;
    }
}
