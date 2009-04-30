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
    public class BasicRenderProperty : ModelRenderProperty
    {
        public override void CreateRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model)
        {
            Renderable = new BasicRenderable(scale, rotation, position, model);
        }

        public override void SetRenderableParameters(Entity entity)
        {
            if (entity.HasFloat("lava_light_strength"))
            {
                Renderable.LavaLightStrength = entity.GetFloat("lava_light_strength");
            }
            if (entity.HasFloat("sky_light_strength"))
            {
                Renderable.SkyLightStrength = entity.GetFloat("sky_light_strength");
            }
            if (entity.HasFloat("spot_light_strength"))
            {
                Renderable.SpotLightStrength = entity.GetFloat("spot_light_strength");
            }
            if (entity.HasVector3("diffuse_color"))
            {
                (Renderable as BasicRenderable).DiffuseColor = entity.GetVector3("diffuse_color");
            }
            if (entity.HasVector3("emissive_color"))
            {
                (Renderable as BasicRenderable).EmissiveColor = entity.GetVector3("emissive_color");
            }
            if (entity.HasVector3("specular_color"))
            {
                (Renderable as BasicRenderable).SpecularColor = entity.GetVector3("specular_color");
            }
            if (entity.HasFloat("specular_power"))
            {
                (Renderable as BasicRenderable).SpecularPower = entity.GetFloat("specular_power");
            }
            if (entity.HasFloat("alpha"))
            {
                (Renderable as BasicRenderable).Alpha = entity.GetFloat("alpha");
            }
            if (entity.HasVector2("persistent_squash"))
            {
                (Renderable as BasicRenderable).SquashParams = entity.GetVector2("persistent_squash");
                (Renderable as BasicRenderable).PersistentSquash = true;
            }
        }

        public void Squash()
        {
            (Renderable as BasicRenderable).Squash();
        }

        public Vector2 SquashParams {
            get { return (Renderable as BasicRenderable).SquashParams; }
            set { (Renderable as BasicRenderable).SquashParams = value; }
        }

        public bool PersistentSquash {
            get { return (Renderable as BasicRenderable).PersistentSquash;  }
            set { (Renderable as BasicRenderable).PersistentSquash = value; }
        }
    }
}
