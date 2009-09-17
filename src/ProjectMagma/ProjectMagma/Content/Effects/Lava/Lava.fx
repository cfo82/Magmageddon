#include "..\Global.inc.fx"
#include "Textures.inc"
#include "Params.inc"
#include "Structs.inc"
#include "..\Basic\Shadow.inc.fx"


//------------------------------------------------------------
//                COMPUTE COLOR MAP (FIRE FRACTAL)
//------------------------------------------------------------
float4 getfirefractal(float2 texCoord)
{
	return 2.5*(
		tex2D(FireFractalSampler, texCoord + ColorRandomOffset) +
		tex2D(FireFractalSampler, texCoord - ColorRandomOffset)
	)/2;
}

float gettemperature(float2 texCoord)
{
	float2 mirroredTexCoord = float2(1-texCoord.x, texCoord.y);
	//return pow(tex2D(TemperatureSampler, mirroredTexCoord).x,5); // mirror hack
	return tex2D(TemperatureSampler, mirroredTexCoord).x;
}

float ComputeFogFactor(float d)
{
    return clamp((d - DepthMapNearClip) / (DepthMapFarClip - DepthMapNearClip), 0, 1);
}

//------------------------------------------------------------
//                COMPUTE HEIGHT MAP (STUCCO)
//------------------------------------------------------------
float stuccoFrequency = 0.7;

float getstucco(float2 texCoord, float compression)
{
	float stucco;
	float4 stucco_rgba1 = tex2D(StuccoSparseSampler, texCoord*1.60*stuccoFrequency);
	float4 stucco_rgba2 = tex2D(StuccoSparseSampler, texCoord*2.00*stuccoFrequency);
	float4 stucco_rgba3 = tex2D(StuccoSparseSampler, texCoord*2.75*stuccoFrequency);	
	stucco = 1-(stucco_rgba1.r)*(stucco_rgba2.r)*(stucco_rgba3.r);	
	stucco = (1-compression)*stucco+(compression)/2;
	return stucco;
}

//const float temperatureImpact = 0.95;

//------------------------------------------------------------
//                 EVALUATE LIGHTING MODEL
//------------------------------------------------------------
float4 ComputeIlluminationCombo( float2 texCoord, float3 vLightTS, float fOcclusionShadow )
{
    float stucco = getstucco(texCoord, StuccoCompression);    

	// -- compute diffuse
   float4 granite = tex2D(GraniteSampler, texCoord*25 + GraniteRandomOffset);
   float4 granite_for_diffuse = 0.7*granite+0.3*float4(1.0,0.6,0.4,1.0);    
   float4 diffuse = (1-stucco)*granite_for_diffuse;
   float4 cBaseColor = diffuse;
   
   // --- compute incandescence
   float4 firefractal = getfirefractal(texCoord);
   float4 granite_for_incandescence = .6*granite+.4*float4(1.0,0.6,0.4,1.0); 
	float4 incandescence = stucco*firefractal + (1-stucco)*0.5*granite_for_incandescence;

   // Sample the normal from the normal map for the given texture sample:
   //// compute animated normal
   	float3 n1 = 2*tex2D(GraniteNormalSampler, texCoord + GraniteRandomOffset)-1.0;
	float3 n2 = 2*tex2D(FireFractalNormalSampler, texCoord - ColorRandomOffset)-1.0;
		float stucco_unc = getstucco(texCoord, 0);
		if(invert) stucco_unc = 1-stucco_unc;
		float3 vNormalTS = normalize(((1-stucco_unc)* n2 + stucco_unc) * n1);
		
	//vNormalTS = float3(0,0,1);
   
   // Compute diffuse color component:
   float3 vLightTSAdj = float3( vLightTS.x, -vLightTS.y, vLightTS.z );
   float4 cDiffuse = saturate( dot( vNormalTS, vLightTSAdj )) * float4(0.4,0.4,0.4,1);
   cDiffuse = float4(1,1,1,1);
   
   // Composite the final color:
   
   //float4 cFinalColor = cDiffuse * cBaseColor + incandescence;
   float4 cFinalColor = incandescence;
   
   cFinalColor *= (1 - TemperatureBrightnessInfluence) + (gettemperature(texCoord) * TemperatureBrightnessInfluence);
   return cFinalColor;  
}   


//------------------------------------------------------------
//                      VERTEX SHADER
//------------------------------------------------------------
VS_OUTPUT MultiPlaneVS(float4 inPositionOS  : POSITION, 
                         float2 inTexCoord    : TEXCOORD0,
                         float3 vInNormalOS   : NORMAL,
                         float3 vInBinormalOS : BINORMAL,
                         float3 vInTangentOS  : TANGENT )
{
    VS_OUTPUT Out;
    
    float4x4 localWorld = mul(Local, World);
    
    // Transform and output input position 
    float4 transformed = mul( inPositionOS, mul(Local, WorldViewProjection) );
    Out.position = transformed;
    Out.positionPSP = transformed;
    
    //Out.position.y += sin(inPositionOS.x/7)*40;
       
    // Propagate texture coordinate through:
    Out.texCoord = inTexCoord;

    // Transform the normal, tangent and binormal vectors from object space to homogeneous projection space:
    float3 vNormalWS   = mul( vInNormalOS,   (float3x3) localWorld );
    float3 vTangentWS  = mul( vInTangentOS,  (float3x3) localWorld );
    float3 vBinormalWS = mul( vInBinormalOS, (float3x3) localWorld );
    
    // Propagate the world space vertex normal through:   
    Out.vNormalWS = vNormalWS;
    
    vNormalWS   = normalize( vNormalWS );
    vTangentWS  = normalize( vTangentWS );
    vBinormalWS = normalize( vBinormalWS );
    
    // Compute position in world space:
    float4 vPositionWS = mul( inPositionOS, localWorld );
                 
    // Compute denormalized light vector in world space:
    float3 vLightWS = g_LightDir;
       
    // Normalize the light and view vectors and transform it to the tangent space:
    float3x3 mWorldToTangent = float3x3( vTangentWS, vBinormalWS, vNormalWS );
       
    // Propagate the view and the light vectors (in tangent space):
    Out.vLightTS = mul( vLightWS, mWorldToTangent );
	
	// compute where the current plane is
	//Out.planeFraction = (inPositionOS.y - minPlaneY) / (maxPlaneY - minPlaneY);
	Out.pos = inPositionOS;
	
	// hack: w is in world space, xyz in object space
	float3 OutPosition = Out.position;
	Out.pos.w = ComputeFogFactor(length(EyePosition - vPositionWS));
	
	float4 pos_ls = mul(vPositionWS, LightViewProjection);
	Out.PositionLS = pos_ls;
	
   return Out;
}


//------------------------------------------------------------
//                      PIXEL SHADER
//------------------------------------------------------------
//float RandomOffsetX;

PSOutput MultiPlanePS(PS_INPUT i) : COLOR0
{
	PSOutput outp;

//	outp.RenderChannelColor = float4(1,0,0,1);
//	outp.Color = float4(RandomOffset[0].x, RandomOffset[1].y, RandomOffset[2].x, 1);
//	outp.Color = float4(RandomOffsetX, 0,0, 1);
//	outp.Color = float4(0.5, 0,0, 1);
//	return outp;

	// compute perturbed texture coordinates
	int index = (i.texCoord + RandomOffset[0]) * 200;
	float4 clouds = random_array[index%200]; 
    //float4 clouds = tex2D(CloudsSampler, i.texCoord + RandomOffset[0]);
	float2 perturbation = clouds.gb * 2 * flickerStrength - flickerStrength;
	float2 perturbedTexCoords = i.texCoord + perturbation;
	
	// evaluate phong model to get RGB value
	float3 vLightTS  = normalize( i.vLightTS );
	float4 cResultColor = ComputeIlluminationCombo(perturbedTexCoords, vLightTS, 1);
    
    // compute alpha channel to get A value
	float alphaStucco = getstucco(perturbedTexCoords, 0);
	if(invert) alphaStucco = 1 - alphaStucco;
	
	// compute composite RGBA color
	float3 cResultColor_rgb = lerp(cResultColor.rgb,FogColor, i.pos.w* FogEnabled);
	//float planeFraction = (i.pos.y - minPlaneY) / (maxPlaneY - minPlaneY);
	//float minPlaneYb = -45.0;
	//float maxPlaneYb = 45.0;
	//float minPlaneYb = minPlaneY;
	//float maxPlaneYb = maxPlaneY;
		float yy = i.pos.y;
		outp.Color.r=yy/2;
	float minPlaneYb = 45;
	float maxPlaneYb = 0;

	
	////float planeFraction = ((-10.0f) - minPlaneYb) / (maxPlaneYb - minPlaneYb);
	float planeFraction = (i.pos.y - minPlaneYb) / (maxPlaneYb - minPlaneYb);
//	float planeFraction = ((0.0-10.0) - (0.0-45.0)) / (45.0 - (0.0-45.0));
	//float planeFraction = (i.pos.y - (0.0-45.0)) / (45.0 - (0.0-45.0));
	//float planeFraction = ((-10.0) - (-45.0)) / (45.0- (-45.0));
	//float planeFraction = ((-10) - (-45)) / (45- (-45));
	//float planeFraction = 0.35;
	//
	outp.Color = float4(cResultColor_rgb*0.7, alphaStucco >= planeFraction ? 1 : 0);
//	outp.Color = gettemperature(i.texCoord);
//	outp.Color = float4(planeFraction,0,0,1);
	outp.RenderChannelColor = RenderChannelColor;
	//outp.Depth = i.positionPSP.z / i.positionPSP.w;
	outp.RealDepth = i.positionPSP.z / i.positionPSP.w;
	//outp.DepthColor = float4(i.pos.y/DepthClipY,0,0,1);
	
	// original
	//outp.DepthColor = float4(i.pos.y/DepthClipY,i.pos.w,0,1);
	
	// cheat:
	outp.DepthColor = float4(25/DepthClipY,i.pos.w,0,1);
	
	BlendWithShadow(outp.Color, i.pos, i.PositionLS, float3(0,1,0));
	
	return outp;	
	
	if(planeFraction<=0.0)
		outp.Color = float4(1,0,0,1);
	else if(planeFraction<=0.1)
		outp.Color = float4(0,1,0,1);
	else if(planeFraction<=0.2)
		outp.Color = float4(0,0,1,1);
	else if(planeFraction<=0.3)
		outp.Color = float4(1,1,0,1);
	else if(planeFraction<=0.4)
		outp.Color = float4(1,0,1,1);
	else if(planeFraction<=0.5)
		outp.Color = float4(0,1,1,1);
	else if(planeFraction<=0.6)
		outp.Color = float4(1,1,1,1);
	else if(planeFraction<=0.7)
		outp.Color = float4(0.5,0,0,1);
	else if(planeFraction<=0.8)
		outp.Color = float4(0,0.5,0,1);
	else if(planeFraction<=0.9)
		outp.Color = float4(0,0,0.5,1);
	else if(planeFraction<=1.0)
		outp.Color = float4(0.5,0.5,0.5,1);
	//outp.Color = float4(1,0,0,1);
	//outp.Color = tex2D(TemperatureSampler, i.texCoord);
	//outp.RenderChannelColor = float4(0,0,1,1);
	outp.RenderChannelColor = RenderChannelColor;
	return outp;
	//return float4(cResultColor_rgb*0.7, alphaStucco >= planeFraction ? 1 : 0);
	//return float4(alphaStucco,0,0,1);
	//return float4(-i.pos.y*0.025,0,0,1);
	//return float4(planeFraction,0,0,1);
}


// -------------------- Techniques -------------------------
technique MultiPlaneLava
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 MultiPlaneVS();
        PixelShader = compile ps_3_0 MultiPlanePS();
        AlphaBlendEnable = false;
        AlphaTestEnable = true;
        AlphaFunc = Greater;
        AlphaRef = 0.5;
        
        //AlphaTestEnable = false;
        //AlphaBlendEnable = true;
        //SrcBlend = SrcAlpha;
        //DestBlend = InvSrcAlpha;
        
        
        ZEnable = true;
        ZWriteEnable = true;        
    }
}
