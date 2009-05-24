texture Clouds;
sampler2D CloudsSampler = sampler_state
{
	Texture = <Clouds>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};


uniform const texture DiffuseTexture;
uniform const sampler DiffuseSampler : register(s0) = sampler_state
{
	Texture = <DiffuseTexture>;
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;	
};


uniform const texture SpecularTexture;
uniform const sampler SpecularSampler = sampler_state
{
	Texture = <SpecularTexture>;
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;	
};

uniform const texture NormalTexture;
uniform const sampler NormalSampler = sampler_state
{
	Texture = <NormalTexture>;
	MipFilter = Linear;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;	
};

uniform const texture ShadowMap;
uniform const sampler ShadowSampler = sampler_state
{
	Texture = <ShadowMap>;
	MipFilter = Point;
	MinFilter = Point;
	MagFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;	
};

