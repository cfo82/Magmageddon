using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class TexturedRenderable : DefaultRenderable
    {
        public TexturedRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model, Texture2D texture)
            : base(scale, rotation, position, model)
        {
            this.texture = texture;
            SpotLightStrength = 0.3f;
        }

        protected virtual void SetBasicEffectParameters(BasicEffect basicEffect)
        {
            basicEffect.DiffuseColor = Vector3.One;
            basicEffect.SpecularColor = Vector3.Zero;
            basicEffect.EmissiveColor = Vector3.Zero;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = texture;
        }

        private Texture2D texture;
    }
}
