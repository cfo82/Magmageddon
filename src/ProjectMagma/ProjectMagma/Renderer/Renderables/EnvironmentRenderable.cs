using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Integration;

namespace ProjectMagma.Renderer
{
    public class EnvironmentRenderable : TexturedRenderable
    {
        public EnvironmentRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model)
            : base(scale, rotation, position, model)
        {
            randomOffset = new DoublyIntegratedVector2
            (
                Vector2.Zero, Vector2.Zero, 0.0f, 0.0f, -1.0f, 1.0f
            );
            RenderChannel = RenderChannelType.One;
        }

        //protected override void ApplyEffectsToModel()
        //{
        //    Effect effect = Game.Instance.Content.Load<Effect>("Effects/Environment/Island");
        //    SetDefaultMaterialParameters();
        //    SetModelEffect(Model, effect);
        //}
       
        protected override void ApplyCustomEffectParameters(Effect effect, Renderer renderer, GameTime gameTime)
        {
            base.ApplyCustomEffectParameters(effect, renderer, gameTime);

            randomOffset.RandomlyIntegrate(gameTime, 0.0008f, 0.0f);
            effect.Parameters["RandomOffset"].SetValue(randomOffset.Value);
        }

        //public float WindStrength { get; set; }
        
        private DoublyIntegratedVector2 randomOffset;
    }
}
