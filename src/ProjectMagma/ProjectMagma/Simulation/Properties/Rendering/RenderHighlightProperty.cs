using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Simulation
{
    public class RenderHighlightProperty : Property
    {
        public RenderHighlightProperty()
        {
            model = null;
        }

        public void OnAttached(Entity entity)
        {
            this.entity = entity;
            entity.Draw += OnDraw;
            enabled = false;

            // load the model
            string meshName = entity.GetString("mesh");
            model = Game.Instance.Content.Load<Model>(meshName);

            // attach listener for management form
#if !XBOX
            Game.Instance.ManagementForm.EntitySelectionChanged += OnEntitySelectionChanged;
#endif
        }

        public void OnDetached(Entity entity)
        {
#if !XBOX
            Game.Instance.ManagementForm.EntitySelectionChanged -= OnEntitySelectionChanged;
#endif
            this.model = null;
            entity.Draw -= OnDraw;
            this.entity = null;
            enabled = true;
        }

        private void OnDraw(Entity entity, GameTime gameTime, RenderMode renderMode)
        {
            if (renderMode == RenderMode.RenderToSceneAlpha &&
                enabled)
            {
                Debug.Assert(entity.HasAttribute("mesh"));
                Debug.Assert(entity.HasAttribute("position"));

                Matrix world = Matrix.Identity;

                float scaleModificator = 1.2f;

                // scaling
                Vector3 scale = Vector3.Zero;
                if (entity.HasVector3("scale"))
                {
                    scale = entity.GetVector3("scale");
                    scale.X *= scaleModificator;
                    scale.Y *= scaleModificator;
                    scale.Z *= scaleModificator;
                    world *= Matrix.CreateScale(scale);
                }
                else
                {
                    scale = new Vector3(scaleModificator, scaleModificator, scaleModificator);
                    world *= Matrix.CreateScale(scale);
                }

                // y rotation (if we need other rotations, these are yet to be added)
                if (entity.HasQuaternion("rotation"))
                {
                    Quaternion rotation = entity.GetQuaternion("rotation");
                    world *= Matrix.CreateFromQuaternion(rotation);
                }

                // translation
                Vector3 position = entity.GetVector3("position");
                // small hack
                /*if (Math.Abs(((BoundingBox)model.Tag).Max.Y) < 0.0001f)
                {
                    position += new Vector3(0.0f, (((2.0f * scale.Y) / 2.0f) - ((2.0f * scale.Y / scaleModificator) / 2.0f))/scale.Y, 0.0f);
                }
                else
                {
                    position -= new Vector3(0.0f, (((2.0f * scale.Y) / 2.0f) - ((2.0f * scale.Y / scaleModificator) / 2.0f))/scale.Y, 0.0f);
                }*/
                world *= Matrix.CreateTranslation(position);

                Matrix[] transforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(transforms);

                foreach (ModelMesh mesh in model.Meshes)
                {
                    Game.Instance.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                    Vector3[] diffuseColors = new Vector3[mesh.Effects.Count];
                    int i = 0;
                    foreach (BasicEffect effectx in mesh.Effects)
                    {
                        diffuseColors[i] = effectx.DiffuseColor;
                        effectx.DiffuseColor = new Vector3(1.0f, 1.0f, 0.0f);
                        effectx.EnableDefaultLighting();
                        effectx.View = Game.Instance.View;
                        effectx.Projection = Game.Instance.Projection;
                        effectx.World = transforms[mesh.ParentBone.Index] * world;
                        ++i;
                    }

                    //Game.Instance.GraphicsDevice.RenderState.DepthBufferEnable = false;
                    Game.Instance.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    Game.Instance.GraphicsDevice.RenderState.StencilFunction = CompareFunction.Always;
                    Game.Instance.GraphicsDevice.RenderState.BlendFactor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    Game.Instance.GraphicsDevice.RenderState.SourceBlend = Blend.BlendFactor;
                    Game.Instance.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
                    Game.Instance.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseBlendFactor;

                    mesh.Draw();

                    Game.Instance.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    //Game.Instance.GraphicsDevice.RenderState.DepthBufferEnable = true;

                    i = 0;
                    foreach (BasicEffect effectx in mesh.Effects)
                    {
                        effectx.DiffuseColor = diffuseColors[i];
                        ++i;
                    }
                }
            }
        }

#if !XBOX
        private void OnEntitySelectionChanged(ManagementForm managementForm, Entity oldSelection, Entity newSelection)
        {
            enabled = this.entity == newSelection;
        }
#endif

        private Entity entity;
        private Model model;
        private bool enabled;
    }
}
