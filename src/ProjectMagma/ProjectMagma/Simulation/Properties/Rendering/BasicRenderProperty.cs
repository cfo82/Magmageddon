using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Framework.Attributes;
using ProjectMagma.Renderer;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Simulation
{
    public class BasicRenderProperty : ModelRenderProperty
    {
        protected override ModelRenderable CreateRenderable(Entity entity, Vector3 scale, Quaternion rotation, Vector3 position, Model model)
        {
            return new BasicRenderable(Game.Instance.Simulation.Time.At, scale, rotation, position, model);
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
            if (entity.HasFloat("lava_light_strength"))
            {
                ChangeFloat("LavaLightStrength", entity.GetFloat("lava_light_strength"));
            }
            if (entity.HasFloat("sky_light_strength"))
            {
                ChangeFloat("SkyLightStrength", entity.GetFloat("sky_light_strength"));
            }
            if (entity.HasFloat("spot_light_strength"))
            {
                ChangeFloat("SpotLightStrength", entity.GetFloat("spot_light_strength"));
            }
            if (entity.HasVector3("diffuse_color"))
            {
                ChangeVector3("DiffuseColor", entity.GetVector3("diffuse_color"));
            }
            if (entity.HasVector3("emissive_color"))
            {
                ChangeVector3("EmissiveColor", entity.GetVector3("emissive_color"));
            }
            if (entity.HasVector3("specular_color"))
            {
                ChangeVector3("SpecularColor", entity.GetVector3("specular_color"));
            }
            if (entity.HasFloat("specular_power"))
            {
                ChangeFloat("SpecularPower", entity.GetFloat("specular_power"));
            }
            if (entity.HasFloat("alpha"))
            {
                ChangeFloat("Alpha", entity.GetFloat("alpha"));
            }
            if (entity.HasVector2("persistent_squash"))
            {
                ChangeVector2("SquashParams", entity.GetVector2("persistent_squash"));
                ChangeBool("PersistentSquash", true);
            }
        }

        public void Squash()
        {
            ChangeBool("Squash", true);
        }

        public void Blink()
        {
            ChangeBool("Blink", true);
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
