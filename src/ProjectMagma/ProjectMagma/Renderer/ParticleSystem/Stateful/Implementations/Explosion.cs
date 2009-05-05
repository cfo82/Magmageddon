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
    public class Explosion : StatefulParticleSystem
    {
        public enum ExplosionType
        {
            Fire,
            Ice
        }

        public Explosion(
            Renderer renderer,
            WrappedContentManager wrappedContent,
            GraphicsDevice device,
            ExplosionType type
        )
        :   base(renderer, wrappedContent, device)
        {
            this.type = type;
            switch (type)
            {
                case ExplosionType.Fire: explosionSprite = wrappedContent.Load<Texture2D>("Textures/Sfx/FireExplode"); break;
                case ExplosionType.Ice: explosionSprite = wrappedContent.Load<Texture2D>("Textures/Sfx/IceExplode"); break;
                default: throw new ArgumentException("invalid type parameter");
            }
        }

        private Effect LoadEffect(WrappedContentManager wrappedContent)
        {
            switch (type)
            {
                case ExplosionType.Fire: return wrappedContent.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/Implementations/FireExplosion");
                case ExplosionType.Ice: return wrappedContent.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/Implementations/IceExplosion");
                default: throw new ArgumentException("invalid type parameter");
            }
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
            base.SetUpdateParameters(parameters);
        }

        protected override void SetRenderingParameters(EffectParameterCollection parameters)
        {
            base.SetRenderingParameters(parameters);

            parameters["RenderParticlesSpriteTexture"].SetValue(explosionSprite);
        }

        private Texture2D explosionSprite;
        private ExplosionType type;
    }
}
