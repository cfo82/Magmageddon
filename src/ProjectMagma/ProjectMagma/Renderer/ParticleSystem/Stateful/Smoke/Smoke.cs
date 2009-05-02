using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer.ParticleSystem.Stateful.Smoke
{
    public class Smoke : StatefulParticleSystem
    {
        public Smoke(
            Renderer renderer,
            WrappedContentManager wrappedContent,
            GraphicsDevice device
        )
            : base(renderer, wrappedContent, device)
        {
        }
        protected override void LoadResources(Renderer renderer, WrappedContentManager wrappedContent, GraphicsDevice device)
        {
            base.LoadResources(renderer, wrappedContent, device);
            smokeTexture = Game.Instance.ContentManager.Load<Texture2D>("smoke");
        }

        public override void UnloadResources()
        {
            // no need to release the smokeTexture since it is managed by the content manager
            base.UnloadResources();
        }

        protected override Effect LoadCreateEffect(WrappedContentManager wrappedContent)
        {
            return wrappedContent.Load<Effect>("Effects/ParticleSystem/Stateful/Smoke/Smoke");
        }

        protected override Effect LoadUpdateEffect(WrappedContentManager wrappedContent)
        {
            return wrappedContent.Load<Effect>("Effects/ParticleSystem/Stateful/Smoke/Smoke");
        }

        protected override Effect LoadRenderEffect(WrappedContentManager wrappedContent)
        {
            return wrappedContent.Load<Effect>("Effects/ParticleSystem/Stateful/Smoke/Smoke");
        }

        protected override void SetUpdateParameters(EffectParameterCollection parameters)
        {
            base.SetUpdateParameters(parameters);

            windAngle += 0.25f - 0.5f * (float)random.NextDouble();

            parameters["Gravity"].SetValue(new Vector3(windSpeed * (float)Math.Cos(windAngle), +5, windSpeed * (float)Math.Sin(windAngle)));
        }

        protected override void SetRenderingParameters(
            EffectParameterCollection parameters
        )
        {
            base.SetRenderingParameters(parameters);

            parameters["RenderParticlesSpriteTexture"].SetValue(smokeTexture);
        }

        private Texture2D smokeTexture;
        private float windSpeed = -20.0f;
        private float windAngle = 0.0f;
        private Random random = new Random();
    }
}
