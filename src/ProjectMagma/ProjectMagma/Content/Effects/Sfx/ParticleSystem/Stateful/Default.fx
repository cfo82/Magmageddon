#include "./CreateParticles.fx"
#include "./UpdateParticles.fx"
#include "./RenderParticles.fx"



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
