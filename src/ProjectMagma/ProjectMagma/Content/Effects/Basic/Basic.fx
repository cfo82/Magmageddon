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
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
	}	
}

Technique Textured
{
	Pass 
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSq();
		PixelShader	 = compile ps_3_0 PSBasicPixelLightingTx();
	}
}

Technique Island
{
	Pass 
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSq();
		PixelShader	 = compile ps_3_0 PSIsland();
	}
}

Technique Environment
{
	Pass 
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSq();
		PixelShader	 = compile ps_3_0 PSEnvironment();
	}
}

Technique AnimatedPlayer
{
	Pass
	{
		VertexShader = compile vs_3_0 VSBasicPixelLightingNmTxSqSk();
		PixelShader = compile ps_3_0 PSBasicPixelLightingTxTo();
	}
}