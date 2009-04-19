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
        }
    }
}
