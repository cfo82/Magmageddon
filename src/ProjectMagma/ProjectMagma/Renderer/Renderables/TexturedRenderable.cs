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

            effect.Parameters["BasicTexture"].SetValue(Texture);
        }

        public void SetTexture(Texture2D texture)
        {
            this.Texture = texture;
        }

        protected Texture2D Texture { get; set; }
    }
}
