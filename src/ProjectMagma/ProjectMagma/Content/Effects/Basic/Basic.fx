#include "..\Global.inc.fx"
#include "..\Basic\Params.inc.fx"
#include "..\Basic\Structs.inc.fx"
#include "..\Basic\Textures.inc.fx"
#include "..\Basic\Lighting.inc.fx"
#include "..\Basic\Shadow.inc.fx"
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
		ZWriteEnable = true;
	}
}

Technique UnicoloredAlpha
{
	Pass
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmSq();
		PixelShader	 = compile ps_3_0 PSBasicPixelLighting();
		AlphaBlendEnable = true;
		BlendOp = Add;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		ZEnable = true;
		ZWriteEnable = true;
	}
}

Technique Textured
{
	Pass 
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSq();
		PixelShader	 = compile ps_3_0 PSBasicPixelLightingTx();
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		//AlphaTestEnable = false;						
	}
}

Technique TexturedAlphaNoCullNoDepth
{
	Pass 
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSq();
		PixelShader	 = compile ps_3_0 PSBasicPixelLightingTxIgnoreIslandDepth();
		CullMode = None;
		ZEnable = false;
		AlphaBlendEnable = true;
		BlendOp = Add;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		//AlphaTestEnable = false;
	}
}

Technique Island
{
	Pass 
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSq();
		PixelShader	 = compile ps_3_0 PSIsland();
		ZEnable = true;		
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		//AlphaTestEnable = false;
	}
}

Technique IslandAlphaColor
{
	Pass 
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSq();
		PixelShader	 = compile ps_3_0 PSIslandAlphaColor();
		ZEnable = true;		
		ZWriteEnable = false;
		//AlphaTestEnable = false;

		AlphaBlendEnable = true;
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;
		
	}
}
Technique IslandAlphaRenderChannel
{
	Pass 
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSq();
		PixelShader	 = compile ps_3_0 PSIslandAlphaRenderChannel();
		ZEnable = true;		
		ZWriteEnable = false;
		//AlphaTestEnable = false;

		AlphaBlendEnable = true;
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;
	}
}
Technique IslandAlphaDepth
{
	Pass 
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSq();
		PixelShader	 = compile ps_3_0 PSIslandAlphaDepth();
		ZEnable = true;		
		ZWriteEnable = false;
		//AlphaTestEnable = false;

		AlphaBlendEnable = true;
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;
	}
}

Technique Environment
{
	Pass
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSq();
		PixelShader	 = compile ps_3_0 PSEnvironment();
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		//AlphaTestEnable = true;		
		//AlphaFunc = Greater;
        //AlphaRef = 0.1;
	}
}

Technique AnimatedPlayer
{
	Pass
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSqSk();
		PixelShader = compile ps_3_0 PSBasicPixelLightingTxTo();
		ZEnable = true;		
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		//AlphaTestEnable = false;		
	}
}


Technique DoublyColoredAnimatedPlayer
{
	Pass
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSqSk();
		PixelShader = compile ps_3_0 PSBasicPixelLightingTxToDb();
		ZEnable = true;		
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		//AlphaTestEnable = false;				
	}
}