#include "..\Global.inc.fx"
#include "Textures.inc"
#include "Params.inc"
#include "Structs.inc"
#include "..\Basic\Shadow.inc.fx"

float ComputeFogFactor(float d)
{
    return clamp((d - DepthMapNearClip) / (DepthMapFarClip - DepthMapNearClip), 0, 1);
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
	return tex2D(TemperatureSampler, mirroredTexCoord).x;
}


struct SharedTextures
{
	float4 stucco_granite;
};


//------------------------------------------------------------
//                 EVALUATE LIGHTING MODEL
//------------------------------------------------------------
float4 ComputeIlluminationCombo(
	float2 texCoord, 
	float3 vLightTS,
	float fOcclusionShadow,
	SharedTextures shared_tex
	)
{
	float stucco = (1-StuccoCompression)*(1-shared_tex.stucco_granite.b)+(StuccoCompression)/2;

	// -- compute diffuse
	float4 granite = float4(shared_tex.stucco_granite.rg,0,1);
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
    float stucco_unc = 1-shared_tex.stucco_granite.b;

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
//                      PIXEL SHADER
//------------------------------------------------------------
PSOutput MultiPlanePS(PS_INPUT i)
{
	PSOutput outp;
	float minPlaneYb = 45;
	float maxPlaneYb = 0;
	float planeFraction = (i.pos.y - minPlaneYb) / (maxPlaneYb - minPlaneYb);

	// compute perturbed texture coordinates
	int index = (i.texCoord + RandomOffset[0]) * RANDOM_ARRAY_SIZE;
	float4 clouds = random_array[index%RANDOM_ARRAY_SIZE]; 
    //float4 clouds = tex2D(CloudsSampler, i.texCoord + RandomOffset[0]);
	float2 perturbation = clouds.gb * 2 * flickerStrength - flickerStrength;
	float2 perturbedTexCoords = i.texCoord + perturbation;
	
	// sample shared textures (currently only one)
	SharedTextures shared_tex;
	shared_tex.stucco_granite = tex2D(StuccoSparseSampler, perturbedTexCoords);
	
    // compute alpha channel to get A value
	float alphaStucco = 1-shared_tex.stucco_granite.b;
	if(invert) alphaStucco = 1 - alphaStucco;

	// Using the clip instruction to do alpha testing. They removed the alpha test
	// state for the new xna4... But since there is a lot going on in ComputeIlluminationCombo
	// this might turn out to be a nice little optimization here!
	float alpha = alphaStucco >= planeFraction ? 1 : 0;
	clip(alpha-0.5);

	// evaluate phong model to get RGB value
	float3 vLightTS  = normalize( i.vLightTS );
	float4 cResultColor = ComputeIlluminationCombo(perturbedTexCoords, vLightTS, 1, shared_tex);
	
	// compute composite RGBA color
	float3 cResultColor_rgb = lerp(cResultColor.rgb,FogColor, i.pos.w* FogEnabled);
	float yy = i.pos.y;
	outp.Color.r=yy/2;

	outp.Color = float4(cResultColor_rgb*0.7, alphaStucco >= planeFraction ? 1 : 0);
	outp.RenderChannelColor = RenderChannelColor;
	outp.RealDepth = float4(i.positionPSP.z / i.positionPSP.w, 0, 0, 1);
	
	BlendWithShadow(outp.Color, i.pos, i.PositionLS, float3(0,1,0));

	return outp;	
}


// -------------------- Techniques -------------------------
technique MultiPlaneLava
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 MultiPlaneVS();
        PixelShader = compile ps_3_0 MultiPlanePS();

		// can't do alpha testing anymore. See the comment at the
		// clip instruction in the pixel shader!
		//AlphaTestEnable = true;
		//AlphaFunc = Greater;
		//AlphaRef = 0.5;

		AlphaBlendEnable = false;

        ZEnable = true;
        ZWriteEnable = true;
    }
}
