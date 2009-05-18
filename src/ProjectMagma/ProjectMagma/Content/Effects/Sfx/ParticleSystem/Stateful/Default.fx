#include "./Params.inc"
#include "./Structs.inc"
#include "./Samplers.inc"
#include "./DefaultCreateParticles.inc"
#include "./DefaultUpdateParticles.inc"
#include "./DefaultRenderParticles.inc"



//-----------------------------------------------------------------------------------------------------------
technique CreateParticles
{
    pass MainPass
    {
        VertexShader = compile vs_3_0 CreateParticlesVertexShader();
        PixelShader = compile ps_3_0 CreateParticlesPixelShader();
    }
}




//-----------------------------------------------------------------------------------------------------------
technique UpdateParticles
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 UpdateParticlesPixelShader();
    }
}




//-----------------------------------------------------------------------------------------------------------
technique RenderParticles
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 RenderParticlesVertexShader();
        PixelShader = compile ps_3_0 RenderParticlesPixelShader();
    }
}
