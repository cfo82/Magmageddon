//-----------------------------------------------------------------------------------------------------------
float4x4 View;
float4x4 Projection;
float2 StartSize;
float2 EndSize;
float4 MinColor = float4(1,1,1,1);
float4 MaxColor = float4(1,1,1,1);
float ViewportHeight;
float ViewportWidth;
float Time;
float Dt;




//-----------------------------------------------------------------------------------------------------------
texture PositionTexture;
texture VelocityTexture;
texture SpriteTexture;
texture RandomTexture;
texture DepthTexture;
