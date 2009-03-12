using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Framework
{
    public class RenderProperty : Property
    {
        public RenderProperty()
        {
            model = null;
        }

        public void OnAttached(Entity entity)
        {
            entity.Draw += new DrawHandler(OnDraw);

            // load the model
            string meshName = entity.GetString("mesh");
            model = Game.Instance.Content.Load<Model>(meshName);
        }

        public void OnDetached(Entity entity)
        {
            entity.Draw -= new DrawHandler(OnDraw);
        }

        private void OnDraw(Entity entity, GameTime gameTime)
        {
            Debug.Assert(entity.HasAttribute("mesh"));
            Debug.Assert(entity.HasAttribute("position"));

            Vector3 position = entity.GetVector3("position");
            Vector3 scale = new Vector3(1, 1, 1);
            if (entity.HasAttribute("scale"))
            {
                scale = entity.GetVector3("scale");
            }

            Matrix world = Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    effect.View = Game.Instance.View;
                    effect.Projection = Game.Instance.Projection;
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                }

                mesh.Draw();
            }
        }

        private Model model;
    }
}
