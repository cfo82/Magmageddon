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

namespace ProjectMagma.Simulation
{
    public class LavaRenderProperty : RendererUpdatableProperty
    {
        public LavaRenderProperty()
        {
        }

        public override void OnAttached(AbstractEntity entity)
        {
            base.OnAttached(entity);

            if (entity.HasVector3("scale"))
            {
                entity.GetVector3Attribute("scale").ValueChanged += ScaleChanged;
            }

            if (entity.HasQuaternion("rotation"))
            {
                entity.GetQuaternionAttribute("rotation").ValueChanged += RotationChanged;
            }

            if (entity.HasVector3("position"))
            {
                entity.GetVector3Attribute("position").ValueChanged += PositionChanged;
            }

            Game.Instance.Simulation.LevelLoaded += LevelLoaded;
            Game.Instance.Simulation.EntityManager.EntityAdded += EntityAdded;

            Game.Instance.Simulation.CurrentUpdateQueue.AddUpdate(new AddRenderableUpdate((Renderable)Updatable));
        }

        public override void OnDetached(AbstractEntity entity)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.AddUpdate(new RemoveRenderableUpdate((Renderable)Updatable));

            Game.Instance.Simulation.EntityManager.EntityAdded -= EntityAdded;
            Game.Instance.Simulation.LevelLoaded -= LevelLoaded;

            if (entity.HasVector3("position"))
            {
                entity.GetVector3Attribute("position").ValueChanged -= PositionChanged;
            }
            if (entity.HasQuaternion("rotation"))
            {
                entity.GetQuaternionAttribute("rotation").ValueChanged -= RotationChanged;
            }
            if (entity.HasVector3("scale"))
            {
                entity.GetVector3Attribute("scale").ValueChanged -= ScaleChanged;
            }

            base.OnDetached(entity);
        }

        protected override ProjectMagma.Renderer.Interface.RendererUpdatable CreateUpdatable(Entity entity)
        {
            int renderPriority = 1000;
            Vector3 scale = Vector3.One;
            Quaternion rotation = Quaternion.Identity;
            Vector3 position = Vector3.Zero;

            if (entity.HasInt("render_priority"))
            {
                renderPriority = entity.GetInt("render_priority");
            }
            if (entity.HasVector3("scale"))
            {
                scale = entity.GetVector3("scale");
            }
            if (entity.HasQuaternion("rotation"))
            {
                rotation = entity.GetQuaternion("rotation");
            }
            if (entity.HasVector3("position"))
            {
                position = entity.GetVector3("position");
            }

            // load the model
            string meshName = entity.GetString("mesh");
            MagmaModel magmaModel = Game.Instance.ContentManager.Load<MagmaModel>(meshName);
            Model model = magmaModel.XnaModel;

            // load textures
            string sparseStuccoTextureName = entity.GetString("sparsestucco_texture");
            string fireFractalTextureName = entity.GetString("firefractal_texture");
            string vectorCloudTextureName = entity.GetString("vectorcloud_texture");
            string graniteTextureName = entity.GetString("granite_texture");

            Texture2D sparseStuccoTexture = Game.Instance.ContentManager.Load<Texture2D>(sparseStuccoTextureName);
            Texture2D fireFractalTexture = Game.Instance.ContentManager.Load<Texture2D>(fireFractalTextureName);
            Texture2D vectorCloudTexture = Game.Instance.ContentManager.Load<Texture2D>(vectorCloudTextureName);
            Texture2D graniteTexture = Game.Instance.ContentManager.Load<Texture2D>(graniteTextureName);

            // collect pillars
            LavaRenderable.PillarInfo[] pillarData = new LavaRenderable.PillarInfo[Game.Instance.Simulation.PillarManager.Count];
            for (int i = 0; i < pillarData.Length; ++i)
            {
                Entity pillar = Game.Instance.Simulation.PillarManager[i];
                pillarData[i].Position = pillar.HasVector3("position") ? pillar.GetVector3("position") : Vector3.Zero;
                pillarData[i].Scale = pillar.HasVector3("scale") ? pillar.GetVector3("scale") : Vector3.One;
            }

            return new LavaRenderable(
                Game.Instance.Simulation.Time.At,
                renderPriority,
                scale, rotation, position, model, 
                sparseStuccoTexture, fireFractalTexture, vectorCloudTexture, graniteTexture,
                pillarData);
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
        }

        private void ScaleChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            ChangeVector3("Scale", newValue);
        }

        private void RotationChanged(
            QuaternionAttribute sender,
            Quaternion oldValue,
            Quaternion newValue
        )
        {
            ChangeQuaternion("Rotation", newValue);
        }

        private void PositionChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            ChangeVector3("Position", newValue);
        }

        private void EntityAdded(
            AbstractEntityManager<Entity> manager,
            Entity entity
        )
        {
            if (entity.HasString("kind") && entity.GetString("kind") == "pillar")
            {
                Debug.Assert(entity.HasVector3("position"));

                LavaRenderable.PillarInfo info = new LavaRenderable.PillarInfo();
                info.Position = entity.GetVector3("position");
                if (entity.HasVector3("scale"))
                {
                    info.Scale = entity.GetVector3("scale");
                }

                Game.Instance.Simulation.CurrentUpdateQueue.AddUpdate(new LavaRenderable.LavaPillarUpdate(Updatable, info));
            }
        }

        private void LevelLoaded(
            Simulation simulation
        )
        {
            Game.Instance.Simulation.CurrentUpdateQueue.AddUpdate(new LavaRenderable.RecomputeLavaTemperatureUpdate(Updatable));
        }
    }
}
