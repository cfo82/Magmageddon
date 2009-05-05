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

            return new RobotRenderable(scale, rotation, position, model,
                diffuseTexture, specularTexture, normalTexture,
                color1, color2);
        }

        public string NextOneTimeState
        {
            set
            {
                ChangeString("NextOneTimeState", value);
            }
        }

        public string NextPermanentState
        {
            set
            {
                ChangeString("NextPermanentState", value);
            }
        }
    }
}
