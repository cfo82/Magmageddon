using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Model;
using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
using ProjectMagma.Renderer;
using ProjectMagma.Renderer.Interface;

// todo: this should inherit from basicrenderproperty
namespace ProjectMagma.Simulation
{
    public class AlphaIslandRenderProperty : IslandRenderProperty
    {
        protected override TexturedRenderable CreateTexturedRenderable(
            Entity entity, int renderPriority, Vector3 scale, Quaternion rotation, Vector3 position, Model model,
            Texture2D diffuseTexture, Texture2D specularTexture, Texture2D normalTexture)
        {
            return new AlphaIslandRenderable(Game.Instance.Simulation.Time.At, renderPriority, scale, rotation, position, model, diffuseTexture, specularTexture, normalTexture);
        }
    }
}
