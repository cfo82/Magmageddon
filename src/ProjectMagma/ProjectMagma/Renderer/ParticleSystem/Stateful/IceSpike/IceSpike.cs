using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace ProjectMagma.Renderer.ParticleSystem.Stateful.IceSpike
{
    public class IceSpike : StatefulParticleSystem
    {
        public IceSpike(
            ContentManager content,
            GraphicsDevice device
        )
        :   base(content, device)
        {
            trailSprite = content.Load<Texture2D>("Textures/Sfx/IceSpikeTrail");
        }

        protected override Effect LoadCreateEffect(ContentManager content)
        {
            return content.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/IceSpike/IceSpike");
        }

        protected override Effect LoadUpdateEffect(ContentManager content)
        {
            return content.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/IceSpike/IceSpike");
        }

        protected override Effect LoadRenderEffect(ContentManager content)
        {
            return content.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/IceSpike/IceSpike");
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
