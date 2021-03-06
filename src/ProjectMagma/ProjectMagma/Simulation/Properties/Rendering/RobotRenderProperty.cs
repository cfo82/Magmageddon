﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
using ProjectMagma.Renderer;

namespace ProjectMagma.Simulation
{
    public class RobotRenderProperty : TexturedRenderProperty
    {
        protected override TexturedRenderable CreateTexturedRenderable(
            Entity entity, int renderPriority, Vector3 scale, Quaternion rotation, Vector3 position, Model model,
            Texture2D diffuseTexture, Texture2D specularTexture, Texture2D normalTexture
        )
        {
            Debug.Assert(entity.HasVector3(CommonNames.Color1));
            Vector3 color1 = entity.GetVector3(CommonNames.Color1);

            Debug.Assert(entity.HasVector3(CommonNames.Color2));
            Vector3 color2 = entity.GetVector3(CommonNames.Color2);

            return new RobotRenderable(
                Game.Instance.Simulation.Time.At, renderPriority,
                scale, rotation, position, model,
                diffuseTexture, specularTexture, normalTexture,
                color1, color2);
        }

        public override void OnAttached(AbstractEntity entity)
        {
            base.OnAttached(entity);

            if (entity.HasFloat(CommonNames.Health))
            {
                entity.GetFloatAttribute(CommonNames.Health).ValueChanged += HealthChanged;
            }
            if (entity.HasInt(CommonNames.Frozen))
            {
                entity.GetIntAttribute(CommonNames.Frozen).ValueChanged += FrozenChanged;
            }
        }

        public override void OnDetached(AbstractEntity entity)
        {
            base.OnDetached(entity);

            if (entity.HasFloat(CommonNames.Health))
            {
                entity.GetFloatAttribute(CommonNames.Health).ValueChanged -= HealthChanged;
            }
            if (entity.HasInt(CommonNames.Frozen))
            {
                entity.GetIntAttribute(CommonNames.Frozen).ValueChanged -= FrozenChanged;
            }
        }

        public string NextOnceState
        {
            set
            {
                ChangeString("NextOnceState", value);
            }
        }

        public string NextPermanentState
        {
            get
            {
                return nextPermanentState;
            }
            set
            {
                if(nextPermanentState!=value)
                {
                    ChangeString("NextPermanentState", value);
                    nextPermanentState = value;
                }
            }
        }

        private void HealthChanged(
            FloatAttribute sender,
            float oldValue,
            float newValue
        )
        {
            if(oldValue > newValue && newValue < 100) // hack, this should be maxhealth but where do i get it?
            {
                ChangeBool("Blink", true);
            }
        }

        private void FrozenChanged(
           IntAttribute sender,
           int oldValue,
           int newValue
       )
        {
            if (newValue > 0)
            {
                ChangeBool("Frozen", true);
            }
            else
            {
                ChangeBool("Frozen", false);
            }
        }

        private string nextPermanentState;
    }
}
