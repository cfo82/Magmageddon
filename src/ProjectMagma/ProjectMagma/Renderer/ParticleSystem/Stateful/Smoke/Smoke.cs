﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer.ParticleSystem.Stateful.Smoke
{
    public class Smoke : StatefulParticleSystem
    {
        public Smoke(
            ContentManager content,
            GraphicsDevice device
        )
        :   base(content, device)
        {
            smokeTexture = content.Load<Texture2D>("smoke");
        }

        protected override Effect LoadCreateEffect(ContentManager content)
        {
            return content.Load<Effect>("Effects/ParticleSystem/Stateful/Smoke/Smoke");
        }

        protected override Effect LoadUpdateEffect(ContentManager content)
        {
            return content.Load<Effect>("Effects/ParticleSystem/Stateful/Smoke/Smoke");
        }

        protected override Effect LoadRenderEffect(ContentManager content)
        {
            return content.Load<Effect>("Effects/ParticleSystem/Stateful/Smoke/Smoke");
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