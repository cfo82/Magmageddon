﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer.ParticleSystem.Stateful.Implementations
{
    public class IceSpike : StatefulParticleSystem
    {
        public IceSpike(
            Renderer renderer,
            WrappedContentManager wrappedContent,
            GraphicsDevice device
        )
        :   base(renderer, wrappedContent, device)
        {
        }

        protected override void LoadResources(Renderer renderer, WrappedContentManager wrappedContent, GraphicsDevice device)
        {
            base.LoadResources(renderer, wrappedContent, device);

            trailSprite = Game.Instance.ContentManager.Load<Texture2D>("Textures/Sfx/IceSpikeTrail");
        }

        public override void UnloadResources()
        {
            // no need to release the trailSprite since its managed by the resource manager!
            base.UnloadResources();
        }

        protected override Effect LoadCreateEffect(WrappedContentManager wrappedContent)
        {
            return wrappedContent.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/Implementations/IceSpike");
        }

        protected override Effect LoadUpdateEffect(WrappedContentManager wrappedContent)
        {
            return wrappedContent.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/Implementations/IceSpike");
        }

        protected override Effect LoadRenderEffect(WrappedContentManager wrappedContent)
        {
            return wrappedContent.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/Implementations/IceSpike");
        }

        protected override void SetUpdateParameters(EffectParameterCollection parameters)
        {
            base.SetUpdateParameters(parameters);

            parameters["IceSpikePosition"].SetValue(position);
            parameters["IceSpikeDirection"].SetValue(direction);
            if (dead)
            {
                parameters["IceSpikeGravityStart"].SetValue(0.0f);
            }
        }

        protected override void SetRenderingParameters(EffectParameterCollection parameters)
        {
            base.SetRenderingParameters(parameters);

            parameters["RenderParticlesSpriteTexture"].SetValue(trailSprite);
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; direction.Normalize(); }
        }

        public bool Dead
        {
            get { return dead; }
            set { dead = true; }
        }

        private Vector3 position;
        private Vector3 direction;
        private bool dead;
        private Texture2D trailSprite;
    }
}