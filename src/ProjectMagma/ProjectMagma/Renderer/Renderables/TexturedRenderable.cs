using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class TexturedRenderable : BasicRenderable
    {
        public TexturedRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model)
            : base(scale, rotation, position, model)
        {
            SpotLightStrength = 0.3f; // hack for pillars
        }

        protected virtual void SetBasicEffectParameters(BasicEffect basicEffect)
        {
            basicEffect.DiffuseColor = Vector3.One;
            basicEffect.SpecularColor = Vector3.Zero;
            basicEffect.EmissiveColor = Vector3.Zero;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = texture;
        }

        public void SetTexture(Texture2D texture)
        {
            this.texture = texture;
        }

        private Texture2D texture;
    }
}
