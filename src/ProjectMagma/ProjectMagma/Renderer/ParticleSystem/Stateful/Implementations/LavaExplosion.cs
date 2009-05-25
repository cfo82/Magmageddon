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
    public class LavaExplosion : StatefulParticleSystem
    {
        public LavaExplosion(
            Renderer renderer,
            WrappedContentManager wrappedContent,
            GraphicsDevice device,
            float explosionSize,
            float explosionRgbMultiplier,
            float explosionDotMultiplier
        )
        :   base(renderer, wrappedContent, device)
        {
            this.explosionSize = explosionSize;
            this.explosionRgbMultiplier = explosionRgbMultiplier;
            this.explosionDotMultiplier = explosionDotMultiplier;
        }

        private Effect LoadEffect(WrappedContentManager wrappedContent)
        {
            return wrappedContent.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/Implementations/LavaExplosion");
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

        protected override Texture2D LoadSprite(WrappedContentManager wrappedContent)
        {
            return wrappedContent.Load<Texture2D>("Textures/Sfx/LavaExplosion");
        }

        protected override void SetUpdateParameters(EffectParameterCollection parameters)
        {
            parameters["ExplosionSize"].SetValue(explosionSize);
            parameters["ExplosionRgbMultiplier"].SetValue(explosionRgbMultiplier);
            parameters["ExplosionDotMultiplier"].SetValue(explosionDotMultiplier);

            base.SetUpdateParameters(parameters);
        }

        protected override void SetRenderingParameters(EffectParameterCollection parameters)
        {
            parameters["ExplosionSize"].SetValue(explosionSize);
            parameters["ExplosionRgbMultiplier"].SetValue(explosionRgbMultiplier);
            parameters["ExplosionDotMultiplier"].SetValue(explosionDotMultiplier);

            base.SetRenderingParameters(parameters);
        }

        private float explosionSize;
        private float explosionRgbMultiplier;
        private float explosionDotMultiplier;
    }
}
