using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class TexturedRenderable : BasicRenderable
    {
        public TexturedRenderable(
            double timestamp,
            int renderPriority,
            Vector3 scale, Quaternion rotation, Vector3 position, Model model,
            Texture2D diffuseTexture, Texture2D specularTexture, Texture2D normalTexture)
        :   base(timestamp, renderPriority, scale, rotation, position, model)
        {
            if (diffuseTexture != null)
            {
                DiffuseTexture = diffuseTexture;
            }
            if (specularTexture != null)
            {
                SpecularTexture = specularTexture;
            }
            if (normalTexture != null)
            {
                NormalTexture = normalTexture;
            }
        }

        public override void LoadResources(Renderer renderer)
        {
            base.LoadResources(renderer);
        }

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

        protected override void ApplyCustomEffectParameters(Effect effect, Renderer renderer)
        {
            base.ApplyCustomEffectParameters(effect, renderer);

            effect.Parameters["DiffuseTexture"].SetValue(DiffuseTexture);
            effect.Parameters["SpecularTexture"].SetValue(SpecularTexture);
            effect.Parameters["NormalTexture"].SetValue(NormalTexture);
        }

        public Texture2D DiffuseTexture { get; set; }
        public Texture2D SpecularTexture { get; set; }
        public Texture2D NormalTexture { get; set; }
    }
}
