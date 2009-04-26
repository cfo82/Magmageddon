using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class TexturedRenderable : BasicRenderable
    {
        public TexturedRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model)
            : base(scale, rotation, position, model)
        { }

        protected override void SetDefaultMaterialParameters()
        {
            Alpha = 1.0f;
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.Zero;
            EmissiveColor = Vector3.Zero;
        }

        protected override void ApplyTechnique(Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques["Textured"];
        }

        protected override void ApplyCustomEffectParameters(Effect effect, Renderer renderer, GameTime gameTime)
        {
            base.ApplyCustomEffectParameters(effect, renderer, gameTime);

            effect.Parameters["DiffuseTexture"].SetValue(DiffuseTexture);
            effect.Parameters["SpecularTexture"].SetValue(SpecularTexture);
            effect.Parameters["NormalTexture"].SetValue(NormalTexture);
        }

        public Texture2D DiffuseTexture { get; set; }
        public Texture2D SpecularTexture { get; set; }
        public Texture2D NormalTexture { get; set; }
    }
}
