using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Integration;

namespace ProjectMagma.Renderer
{
    public class IslandRenderable : TexturedRenderable
    {
        public IslandRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model, Texture2D texture)
            : base(scale, rotation, position, model, texture, null, null)
        {
            randomOffset = new DoublyIntegratedVector2
            (
                Vector2.Zero, Vector2.Zero, 0.0f, 0.0f, -1.0f, 1.0f
            );
            RenderChannel = RenderChannelType.Three;
            SetDefaultMaterialParameters();
        }

        protected override void ApplyEffectsToModel()
        {
            Effect effect = Game.Instance.ContentManager.Load<Effect>("Effects/Environment/Island");
            ReplaceBasicEffect(Model, effect);
        }

        protected override void ApplyTechnique(Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques["Island"];
        }
       
        protected override void ApplyCustomEffectParameters(Effect effect, Renderer renderer)
        {
            base.ApplyCustomEffectParameters(effect, renderer);

            randomOffset.RandomlyIntegrate(renderer.Time.DtMs, 0.04f, 0.0f);
            //effect.Parameters["Clouds"].SetValue(renderer.VectorCloudTexture);
            effect.Parameters["WindStrength"].SetValue(WindStrength);
            effect.Parameters["RandomOffset"].SetValue(randomOffset.Value);
        }

        public override void UpdateFloat(string id, float value)
        {
            base.UpdateFloat(id, value);

            if (id == "WindStrength")
            {
                WindStrength = value;
            }
        }

        public override void UpdateBool(string id, bool value)
        {
            base.UpdateBool(id, value);

            if (id == "Interactable")
            {
                Interactable = value;
            }
        }
        public float WindStrength { get; set; }
        public bool Interactable { get; set; }
        
        private DoublyIntegratedVector2 randomOffset;
    }
}
