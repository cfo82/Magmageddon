sampler PositionSampler = sampler_state
{
    Texture = (PositionTexture);
    
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    
    AddressU = Clamp;
    AddressV = Clamp;
};
sampler VelocitySampler = sampler_state
{
    Texture = (VelocityTexture);
    
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    
    AddressU = Clamp;
    AddressV = Clamp;
};
sampler2D RandomSampler = sampler_state
{
	Texture = (RandomTexture);
	
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	
	AddressU = Wrap;
	AddressV = Wrap;
};
sampler2D RandomSamplerForVertexShader = sampler_state
{
	Texture = (RandomTextureForVertexShader);
	
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	
	AddressU = Wrap;
	AddressV = Wrap;
};
sampler SpriteSampler = sampler_state
{
    Texture = (SpriteTexture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};
