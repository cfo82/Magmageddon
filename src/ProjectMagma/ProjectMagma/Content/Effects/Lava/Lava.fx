#include "Textures.inc"
#include "Params.inc"
#include "Structs.inc"

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

//------------------------------------------------------------
//                COMPUTE HEIGHT MAP (STUCCO)
//------------------------------------------------------------
float getstucco(float2 texCoord, float compression)
{
	float stucco;
	float4 stucco_rgba1 = tex2D(StuccoSparseSampler, texCoord*1.60 + StuccoRandomOffset1*0.15);
	float4 stucco_rgba2 = tex2D(StuccoSparseSampler, texCoord*2.00 - StuccoRandomOffset2*0.20);
	float4 stucco_rgba3 = tex2D(StuccoSparseSampler, texCoord*2.75 - StuccoRandomOffset3*0.30);	
	stucco = 1-(stucco_rgba1.r)*(stucco_rgba2.r)*(stucco_rgba3.r);	
	stucco = (1-compression)*stucco+(compression)/2;
	return stucco;
}


//------------------------------------------------------------
//                 EVALUATE LIGHTING MODEL
//------------------------------------------------------------
float4 ComputeIlluminationCombo( float2 texCoord, float3 vLightTS, float fOcclusionShadow )
{
    float stucco = getstucco(texCoord, StuccoCompression);    

	// -- compute diffuse
   float4 granite = tex2D(GraniteSampler, texCoord + GraniteRandomOffset);
   float4 granite_for_diffuse = 0.3*granite+0.7*float4(1.0,0.6,0.4,1.0);    
   float4 diffuse = stucco*granite_for_diffuse;
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
   
   // Compute diffuse color component:
   float3 vLightTSAdj = float3( vLightTS.x, -vLightTS.y, vLightTS.z );
   float4 cDiffuse = saturate( dot( vNormalTS, vLightTSAdj )) * float4(0.4,0.4,0.4,1);
   
   // Composite the final color:
   
   float4 cFinalColor = cDiffuse * cBaseColor + incandescence;
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
        
    // Transform and output input position 
    Out.position = mul( inPositionOS, g_mWorldViewProjection );
       
    // Propagate texture coordinate through:
    Out.texCoord = inTexCoord;

    // Transform the normal, tangent and binormal vectors from object space to homogeneous projection space:
    float3 vNormalWS   = mul( vInNormalOS,   (float3x3) g_mWorld );
    float3 vTangentWS  = mul( vInTangentOS,  (float3x3) g_mWorld );
    float3 vBinormalWS = mul( vInBinormalOS, (float3x3) g_mWorld );
    
    // Propagate the world space vertex normal through:   
    Out.vNormalWS = vNormalWS;
    
    vNormalWS   = normalize( vNormalWS );
    vTangentWS  = normalize( vTangentWS );
    vBinormalWS = normalize( vBinormalWS );
    
    // Compute position in world space:
    float4 vPositionWS = mul( inPositionOS, g_mWorld );
                 
    // Compute denormalized light vector in world space:
    float3 vLightWS = g_LightDir;
       
    // Normalize the light and view vectors and transform it to the tangent space:
    float3x3 mWorldToTangent = float3x3( vTangentWS, vBinormalWS, vNormalWS );
       
    // Propagate the view and the light vectors (in tangent space):
    Out.vLightTS = mul( vLightWS, mWorldToTangent );
	
	// compute where the current plane is
	//Out.planeFraction = (inPositionOS.y - minPlaneY) / (maxPlaneY - minPlaneY);
	Out.pos = inPositionOS;
   return Out;
}


//------------------------------------------------------------
//                      PIXEL SHADER
//------------------------------------------------------------
float4 MultiPlanePS(PS_INPUT i) : COLOR0
{
	// compute perturbed texture coordinates
    float4 clouds = tex2D(CloudsSampler, i.texCoord + RandomOffset[0]);
	float2 perturbation = clouds.gb * 2 * flickerStrength - flickerStrength;
	float2 perturbedTexCoords = i.texCoord + perturbation;
	
	// evaluate phong model to get RGB value
	float3 vLightTS  = normalize( i.vLightTS );
	float4 cResultColor = ComputeIlluminationCombo(perturbedTexCoords, vLightTS, 1);
    
    // compute alpha channel to get A value
	float alphaStucco = getstucco(perturbedTexCoords, 0);
	if(invert) alphaStucco = 1 - alphaStucco;
	
	// compute composite RGBA color
	float3 cResultColor_rgb = cResultColor.rgb;
	float planeFraction = (i.pos.y - minPlaneY) / (maxPlaneY - minPlaneY);
	return float4(cResultColor_rgb*0.7, alphaStucco >= planeFraction ? 1 : 0);
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
        //AlphaBlendEnable = false;
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
