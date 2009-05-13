using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace ProjectMagma.Renderer.ParticleSystem.Stateful.Implementations
{
    public class Snow : StatefulParticleSystem
    {
        public Snow(
            Renderer renderer,
            WrappedContentManager wrappedContent,
            GraphicsDevice device
        )
        :   base(renderer, wrappedContent, device)
        {
            snowSprite = wrappedContent.Load<Texture2D>("Textures/Sfx/Snow.1");
        }

        private Effect LoadEffect(WrappedContentManager wrappedContent)
        {
            return wrappedContent.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/Implementations/Snow");
        }

        protected override Effect LoadCreateEffect(WrappedContentManager wrappedContent)
        {
            return LoadEffect(wrappedContent);
        }

        protected override Effect LoadUpdateEffect(WrappedContentManager wrappedContent)
        {
            return LoadEffect(wrappedContent);
        }

        protected override Effect LoadRenderEffect(WrappedContentManager wrappedContent)
        {
            return LoadEffect(wrappedContent);
        }

        protected override void SetUpdateParameters(EffectParameterCollection parameters)
        {
            windAngle += 0.5f * ((float)random.NextDouble() - 0.5f);
            parameters["WindForce"].SetValue(new Vector3(windForce * (float)Math.Cos(windAngle), 0, windForce * (float)Math.Sin(windAngle)));

            base.SetUpdateParameters(parameters);
        }

        protected override void SetRenderingParameters(EffectParameterCollection parameters)
        {
            base.SetRenderingParameters(parameters);

            parameters["RenderParticlesSpriteTexture"].SetValue(snowSprite);
        }

        private Texture2D snowSprite;
        private float windForce = 20.0f;
        private float windAngle = 0.0f;
        private static Random random = new Random();
    }
}
