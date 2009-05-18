using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Renderer;

namespace ProjectMagma.Simulation
{
    public class RobotRenderProperty : TexturedRenderProperty
    {
        protected override TexturedRenderable CreateTexturedRenderable(
            Entity entity, Vector3 scale, Quaternion rotation, Vector3 position, Model model,
            Texture2D diffuseTexture, Texture2D specularTexture, Texture2D normalTexture
        )
        {
            Debug.Assert(entity.HasVector3("color1"));
            Vector3 color1 = entity.GetVector3("color1");

            Debug.Assert(entity.HasVector3("color2"));
            Vector3 color2 = entity.GetVector3("color2");

            return new RobotRenderable(
                Game.Instance.Simulation.Time.At,
                scale, rotation, position, model,
                diffuseTexture, specularTexture, normalTexture,
                color1, color2);
        }

        public override void OnAttached(Entity entity)
        {
            base.OnAttached(entity);

            if (entity.HasInt("health"))
            {
                entity.GetIntAttribute("health").ValueChanged += HealthChanged;
            }
            if (entity.HasInt("frozen"))
            {
                entity.GetIntAttribute("frozen").ValueChanged += FrozenChanged;
            }
        }

        public override void OnDetached(Entity entity)
        {
            base.OnDetached(entity);

            if (entity.HasInt("health"))
            {
                entity.GetIntAttribute("health").ValueChanged -= HealthChanged;
            }
            if (entity.HasInt("frozen"))
            {
                entity.GetIntAttribute("frozen").ValueChanged -= FrozenChanged;
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
            IntAttribute sender,
            int oldValue,
            int newValue
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
                //ChangeBool("Frozen", false);
            }
        }

        private string nextPermanentState;
    }
}
