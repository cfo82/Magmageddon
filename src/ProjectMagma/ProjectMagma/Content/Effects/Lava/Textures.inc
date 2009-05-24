texture NormalMap;
sampler2D NormalMapSampler = sampler_state
{
	Texture = <NormalMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};

texture GraniteNormal;
sampler2D GraniteNormalSampler = sampler_state
{
	Texture = <GraniteNormal>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};

texture FireFractalNormal;
sampler2D FireFractalNormalSampler = sampler_state
{
	Texture = <FireFractalNormal>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};

texture StuccoSparse;
sampler2D StuccoSparseSampler = sampler_state
{
	Texture = <StuccoSparse>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};

texture FireFractal;
sampler2D FireFractalSampler = sampler_state
{
	Texture = <FireFractal>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};

texture Granite;
sampler2D GraniteSampler = sampler_state
{
	Texture = <Granite>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};

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

texture Temperature;
sampler2D TemperatureSampler = sampler_state
{
	Texture = <Temperature>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Mirror;
	AddressV = Mirror;
};

uniform const texture ShadowMap;
uniform const sampler ShadowSampler = sampler_state
{
	Texture = <ShadowMap>;
	MipFilter = Point;
	MinFilter = Point;
	MagFilter = Point;
	AddressU = Wrap;
	AddressV = Wrap;	
};
