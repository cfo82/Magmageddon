using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Integration;

namespace ProjectMagma.Renderer
{
    public class AlphaIslandRenderable : IslandRenderable
    {
        public AlphaIslandRenderable(
            double timestamp, int renderPriority, Vector3 scale, Quaternion rotation, Vector3 position, Model model,
            Texture2D diffuseTexture, Texture2D specularTexture, Texture2D normalTexture)
            : base(timestamp, renderPriority, scale, rotation, position, model, diffuseTexture, specularTexture, normalTexture)
        {
        }

        protected override void ApplyEffectsToModel()
        {
            Effect effect = Game.Instance.ContentManager.Load<Effect>("Effects/Environment/Island");
            ReplaceBasicEffect(Model, effect);
        }

        protected override void ApplyTechnique(Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques[CurrentPass];
        }

        public override RenderMode RenderMode
        {
            get
            {
                return RenderMode.RenderToSceneAlphaTEST;
            }
        }

        public string CurrentPass { set; get; }
    }
}
