using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class ArrowRenderable : BasicRenderable
    {
        public ArrowRenderable(
            double timestamp,
            Vector3 scale, Quaternion rotation, Vector3 position, Model model)
            //Vector3 color1, Vector3 color2)
        :   base(timestamp, scale, rotation, position, model)
        {
            //Color1 = color1;
            //Color2 = color2;
        }

        public override void LoadResources(Renderer renderer)
        {
            base.LoadResources(renderer);
        }

        protected override void ApplyTechnique(Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques["UnicoloredAlpha"];
        }

        protected override void SetDefaultMaterialParameters()
        {
            Alpha = 0.75f;
            //DiffuseColor = Color1 * 1.5f;
            //SpecularColor = Color2 * 2.0f;
            //EmissiveColor = Vector3.One * 0.3f;
            SpecularPower = 1f;
        }

        protected override void ApplyCustomEffectParameters(Effect effect, Renderer renderer)
        {
            base.ApplyCustomEffectParameters(effect, renderer);

           // effect.Parameters["LavaLightStrength"].SetValue(1.0f);
        }

        public override RenderMode RenderMode
        {
            get
            {
                return RenderMode.RenderToSceneAlpha;
            }
        }
        //public Vector3 Color1 { get; set; }
        //public Vector3 Color2 { get; set; }
    }
}
