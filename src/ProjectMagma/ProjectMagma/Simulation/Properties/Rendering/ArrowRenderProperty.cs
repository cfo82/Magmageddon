using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Renderer;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Simulation
{
    public class ArrowRenderProperty : BasicRenderProperty
    {
        protected override ModelRenderable CreateRenderable(Entity entity, Vector3 scale, Quaternion rotation, Vector3 position, Model model)
        {
            return new BasicRenderable(scale, rotation, position, model);
        }

        protected override void SetUpdatableParameters(Entity entity)
        {            
            if (entity.HasFloat("player_color"))
            {
                ChangeFloat("DiffuseColor", entity.GetFloat("alpha"));
            }
            //if (entity.HasVector2("persistent_squash"))
            //{
            //    ChangeVector2("SquashParams", entity.GetVector2("persistent_squash"));
            //    ChangeBool("PersistentSquash", true);
            //}
        }

        public void Squash()
        {
            ChangeBool("Squash", true);
        }

        public Vector2 SquashParams 
        {
            set
            {
                ChangeVector2("SquashParams", value);
            }
        }

        public bool PersistentSquash
        {
            set
            {
                ChangeBool("PersistentSquash", value);
            }
        }
    }
}
