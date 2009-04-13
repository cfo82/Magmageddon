float4x4 World;
float4x4 View;
float4x4 Projection;

sampler TextureSampler : register(s0);

//texture2D Texture;
//
//sampler2D DiffuseTextureSampler = sampler_state
//{
    //Texture = <Texture>;
    //MinFilter = linear;
    //MagFilter = linear;
    //MipFilter = linear;
//};
//
// -------------------- Textures -------------------------
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

texture Stucco;
sampler2D StuccoSampler = sampler_state
{
	Texture = <Stucco>;
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

// -------------------- Transfer Types -------------------------
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 texCoord : TEXCOORD0;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 texCoord : TEXCOORD0;
    float3 lightDir : TEXCOORD1;
    float3 viewDir : TEXCOORD2;
    //float3 testNormal : TEXCOORD3;
    float3 testT : TEXCOORD4;
    //float3 testB : TEXCOORD5;
};

//float3 worldLightDir = float3(sqrt(2),sqrt(2),0);
float3 pointLightPos = float3(0,10,0);
//float3 worldLightDir = float3(0,-1,0);
float3 EyePosition; 
float flickerStrength;
float StuccoCompression;

// -------------------- Vertex Shader -------------------------
VertexShaderOutput ColorVS(VertexShaderInput input)
{
    VertexShaderOutput output;

	// transform position
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
   
    // helper matrices for transforming light and view position
    float3x3 worldToTangentSpace;
    worldToTangentSpace[0] = mul(input.tangent, World);
    worldToTangentSpace[1] = mul(cross(input.tangent, input.normal), World);
    worldToTangentSpace[2] = mul(input.normal, World);
    
    
    // transform light and view positions
    output.lightDir = mul(worldToTangentSpace, pointLightPos - worldPosition);
    //output.lightDir = normalize(mul(worldToTangentSpace, float3(0,1,0)));
    output.viewDir = mul(worldToTangentSpace, EyePosition - worldPosition);

    //output.lightDir = normalize(pointLightPos - worldPosition);
    //output.viewDir = normalize(EyePosition - worldPosition);
//
    //
    // forward texture coordinates
    output.texCoord = input.texCoord;
    //output.testNormal = normalize(mul(worldToTangentSpace, input.normal));
    //output.testNormal = input.normal;
    
    output.testT = input.tangent;
    //output.testB = cross(input.tangent, input.normal);

    return output;
}

bool boolParam1;
bool boolParam2;
bool boolParam3;
bool boolParam4;
bool boolParam5;
bool boolParam6;

float2 FireFractalOffset;
float3 getfirefractal(float2 texCoord)
{
	if(boolParam3)
	{
		float2 FFO90 = float2(FireFractalOffset.y, FireFractalOffset.x);
		return 2.5*(
			tex2D(FireFractalSampler, texCoord + FireFractalOffset).rgb +
			tex2D(FireFractalSampler, texCoord - FireFractalOffset).rgb
		)/2;
	} else {
		return 2.5*tex2D(FireFractalSampler, texCoord);
	}
}


float getstucco(float2 texCoord, float compression)
{
	texCoord = texCoord * 2;

	float stucco;
	if(boolParam3)
	{
		float4 stucco_rgba1 = tex2D(StuccoSparseSampler, texCoord + FireFractalOffset*.2);
		
		float2x2 rot45 = (0.707, -0.707, 0.707, 0.707);
		//float2 coords2 = texCoord - FireFractalOffset*.2;
		float2 FFO90 = float2(FireFractalOffset.y, -FireFractalOffset.x);
		//float4 stucco_rgba2 = tex2D(StuccoSparseSampler, float2(coords2.y,-coords2.x));
		//float4 stucco_rgba2 = tex2D(StuccoSparseSampler, texCoord + FFO90*.2);		
		float4 stucco_rgba2 = tex2D(StuccoSparseSampler, texCoord + mul(rot45,FireFractalOffset)*.2);		
		//stucco = ((1-stucco_rgba1.r)*(1-stucco_rgba2.r));
		stucco = 1-(stucco_rgba1.r)*(stucco_rgba2.r);
	} else
	{
		float4 stucco_rgba = tex2D(StuccoSampler, texCoord);
		stucco = 1-stucco_rgba.r;
	}
	
	//stucco = compression*stucco+(compression)/2;
	//stucco = 0.5*stucco+0.25;
	stucco = 0.9*stucco+0.05;
	return stucco;
}

// -------------------- Pixel Shaders -------------------------
float4 ColorPS(VertexShaderOutput input) : COLOR0
{
	// normalize things
	input.lightDir = normalize(input.lightDir);
	input.viewDir = normalize(input.viewDir);

    float3 clouds = tex2D(CloudsSampler, input.texCoord + FireFractalOffset);

    //float3 clouds =
		//(tex2D(CloudsSampler, input.texCoord + FireFractalOffset)
		//+tex2D(CloudsSampler, input.texCoord - FireFractalOffset))/2;
//
    float4 nondisplacedstucco_rgba = tex2D(StuccoSampler, input.texCoord);
    float nondisplacedstucco = 1-nondisplacedstucco_rgba.r;

	nondisplacedstucco = .5*nondisplacedstucco+0.25;

	float2 displacedTexCoord = input.texCoord + nondisplacedstucco*float2(
		clouds.g * 2*flickerStrength - flickerStrength,
		clouds.b * 2*flickerStrength - flickerStrength
	);
	
	float stucco = getstucco(displacedTexCoord, 1);
	
	// compute animated stucco if desired

	// increase contrast on stucco
	if(boolParam5)
	{	
		float contrast = 3;
		stucco = atan(contrast*(stucco-.5)*6.28)/3.14+.5;
		//stucco = pow(-2*stucco*stucco*stucco+3*stucco*stucco,20);
		
	}
	//--stucco = stucco > 0.5 ? 1 : 0;
    
    
    
    float uncompressed_stucco = stucco;
    //stucco = 0.5*stucco+0.25/2;
    stucco = uncompressed_stucco;
    
    //if(boolParam1)
    //{
		//float3 stucco_grad = tex2Dgrad(StuccoSampler, displacedTexCoord, 0.01, 0.01);
		//displacedTexCoord += float2(0,stucco_grad.g*0.05);
	    //
		//stucco_rgba = tex2D(StuccoSampler, displacedTexCoord);
		//stucco = 1-stucco_rgba.r;
	//}    
   
   float3 firefractal = getfirefractal(displacedTexCoord);
	
    float3 granite = tex2D(GraniteSampler, displacedTexCoord).rgb;
    

    float3 granite_for_incandescence = .6*granite+.4*float3(1.0,0.6,0.4);    
    float3 granite_for_diffuse = .3*granite+.7*float3(1.0,0.6,0.4);    

    float3 incandescence = stucco*firefractal + (1-stucco)*0.5*granite_for_incandescence;
    float3 diffuse = stucco*float3(1.0,0.4,0.0) + (1-stucco)*granite_for_diffuse;
    
    
    
    
    //float3 diffuse = float3(0.0,0.0,0.0);	
    //float3 normal = input.testNormal;
    //float3 normal = float3(0,0,1);
    //float3 normal = 2*normalize(tex2D(NormalMapSampler, displacedTexCoord)) - 1.0;
    //float3 normal = abs(normalize(tex2D(NormalMapSampler, displacedTexCoord)));

	// compute animated normal map if desired
	float3 normal;    
   	if(boolParam3)
	{
		//float n1 = tex2D(NormalMapSampler, displacedTexCoord + FireFractalOffset*.2);
		//float n2 = tex2D(NormalMapSampler, displacedTexCoord - FireFractalOffset*.2);
		//normal = 2 * (n1+n2)/2 - 1.0;
		float3 n1 = 2*tex2D(GraniteNormalSampler, displacedTexCoord + FireFractalOffset*.2)-1.0;
		float3 n2 = 2*tex2D(FireFractalNormalSampler, displacedTexCoord - FireFractalOffset*.2)-1.0;
		//normal = n2;
		normal = uncompressed_stucco * n2 + (1-uncompressed_stucco) * n1;
		//normal = stucco * n2 + (1-stucco) * n1;
	} else
	{
		normal = 2 * tex2D(NormalMapSampler, displacedTexCoord) - 1.0;
	}


    //float3 normal = 2 * tex2D(NormalMapSampler, displacedTexCoord) - 1.0;
 	
 	        
	// diffuse
    //float diffuse_eval = saturate(dot(N, LightDir)); 
    float diffuse_eval = saturate(dot(normal, input.lightDir)); 
    
    // reflection
    float3 reflection = normalize(2 * diffuse_eval * normal - input.lightDir);  // R
 	
 	// specular
    // bruker normalmappen en alpha kan denne brukes til å redusere hvor mye den gitte pikelen skal reflektere
    float specularity = .8;
    float specular = pow(saturate(dot(reflection, input.viewDir)), specularity);

	float3 diffuse_out =
			0.5 * diffuse_eval * diffuse // diffuse
			+ .04 * specular;	 // specular
	
	
//	if(boolParam3)
//		diffuse_out *= 0.7;
    //
	//float3 diffuse_out = saturate(float3(max(0,reflection.x*input.viewDir.x),max(0,reflection.y*input.viewDir.y),max(0,reflection.z*input.viewDir.z)));
	//float3 diffuse_out = saturate(dot(reflection, float3(1,1,1)));
    //float3 diffuse_out = specular;6
    //float3 diffuse_out = normalize(abs(input.testT));
    
    
    
    //float4 stucco_rgba = tex2D(StuccoSampler, displacedTexCoord);
    //stucco = 1-stucco_rgba.r;
    //
    //stucco = .5*stucco+0.25;
    //
    //incandescence = stucco;
    //
    //float3 diffuse_out = float3(0.0, 0.0, 0.0);
    
    float3 color = diffuse_out + incandescence;
    //float3 color = incandescence;
	//float3 color = diffuse_out;
    //float3 color = normal.x * normal.z * .5;
    //float3 color = length(normal)*.5;
    //float3 color = granite;
    //return float4(saturate(color),stucco*(0.5+clouds.r*0.5));
    //return float4(saturate(color*.7),stucco);
    return float4(color*.7,stucco);
    //return float4(1,0,0,0);
}

// -------------------- Techniques -------------------------
technique TextureColor
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 ColorVS();
        PixelShader = compile ps_3_0 ColorPS();
    }
}
