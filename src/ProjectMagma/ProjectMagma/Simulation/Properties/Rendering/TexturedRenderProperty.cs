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
    public class TexturedRenderProperty : BasicRenderProperty
    {
        public override void CreateRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model)
        {
            Renderable = new TexturedRenderable(scale, rotation, position, model);
        }

        public override void SetRenderableParameters(Entity entity)
        {
            base.SetRenderableParameters(entity);

            string textureName = entity.GetString("texture");
            Texture2D texture = Game.Instance.Content.Load<Texture2D>(textureName);            
            (Renderable as TexturedRenderable).SetTexture(texture);
        }
    }
}
