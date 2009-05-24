#include "..\Global.inc.fx"
#include "..\Basic\Params.inc.fx"
#include "..\Basic\Structs.inc.fx"
#include "..\Basic\Textures.inc.fx"
#include "..\Basic\Lighting.inc.fx"
#include "..\Basic\Skinning.inc.fx"
#include "..\Basic\Special.inc.fx"
#include "..\Basic\Vertex.inc.fx"
#include "..\Basic\Pixel.inc.fx"

//-----------------------------------------------------------------------------
// Shader and technique definitions
//-----------------------------------------------------------------------------


Technique Unicolored
{
	Pass
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmSq();
		PixelShader	 = compile ps_3_0 PSBasicPixelLighting();
		AlphaBlendEnable = false;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		ZEnable = true;
	}
}

Technique Textured
{
	Pass 
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSq();
		PixelShader	 = compile ps_3_0 PSBasicPixelLightingTx();
		ZEnable = true;
		AlphaBlendEnable = false;
		AlphaTestEnable = false;						
	}
}

Technique TexturedNoCullNoDepth
{
	Pass 
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSq();
		PixelShader	 = compile ps_3_0 PSBasicPixelLightingTx();
		CullMode = None;
		ZEnable = false;
		AlphaBlendEnable = false;
		AlphaTestEnable = false;
	}
}

Technique Island
{
	Pass 
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSq();
		PixelShader	 = compile ps_3_0 PSIsland();
		ZEnable = true;		
		AlphaBlendEnable = false;
		AlphaTestEnable = false;
	}
}

Technique Environment
{
	Pass
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSq();
		PixelShader	 = compile ps_3_0 PSEnvironment();
		ZEnable = true;
		AlphaBlendEnable = false;
		AlphaTestEnable = true;		
		AlphaFunc = Greater;
        AlphaRef = 0.5;
	}
}

Technique AnimatedPlayer
{
	Pass
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSqSk();
		PixelShader = compile ps_3_0 PSBasicPixelLightingTxTo();
		ZEnable = true;		
		AlphaBlendEnable = false;
		AlphaTestEnable = false;		
	}
}

Technique DoublyColoredAnimatedPlayer
{
	Pass
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSqSk();
		PixelShader = compile ps_3_0 PSBasicPixelLightingTxToDb();
		ZEnable = true;		
		AlphaBlendEnable = false;
		AlphaTestEnable = false;				
	}
}