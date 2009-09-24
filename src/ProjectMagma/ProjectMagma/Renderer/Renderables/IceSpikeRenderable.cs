using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Model;
using ProjectMagma.Renderer.ParticleSystem.Emitter;
using ProjectMagma.Renderer.ParticleSystem.Stateful.Implementations;

namespace ProjectMagma.Renderer
{
    public class IceSpikeRenderable : ParticleSystemRenderable
    {
        public IceSpikeRenderable(
            double timestamp,
            int renderPriority,
            Vector3 position,
            Vector3 direction,
            bool dead
        )
        :   base(timestamp, renderPriority, position)
        {
            this.direction = direction;
            this.dead = dead;

            iceSpikeEmitter = null;
        }

        public override void LoadResources(Renderer renderer)
        {
            base.LoadResources(renderer);

            iceSpikeModel = Game.Instance.ContentManager.Load<MagmaModel>("Models/Sfx/IceSpike").XnaModel;
            iceSpikeTexture = Game.Instance.ContentManager.Load<Texture2D>("Textures/Sfx/IceSpikeHead");

            iceSpikeEffect = Game.Instance.ContentManager.Load<Effect>("Effects/Sfx/IceSpike").Clone(Game.Instance.GraphicsDevice);

            this.transforms = new Matrix[iceSpikeModel.Bones.Count];
        }

        public override void UnloadResources(Renderer renderer)
        {
            iceSpikeEffect.Dispose();

            base.UnloadResources(renderer);
        }

        public override void Update(Renderer renderer)
        {
            base.Update(renderer);

            if (!dead && iceSpikeEmitter == null)
            {
                iceSpikeEmitter = new PointEmitter(renderer.Time.PausableLast / 1000d, Position, 2500.0f);
                renderer.IceSpikeSystem.AddEmitter(iceSpikeEmitter);
            }

            if (dead && iceSpikeEmitter != null)
            {
                renderer.IceSpikeSystem.RemoveEmitter(iceSpikeEmitter);
                iceSpikeEmitter = null;
            }

            if (iceSpikeEmitter != null)
            {
                iceSpikeEmitter.SetPoint(renderer.Time.PausableAt / 1000d, Position);
                Debug.Assert(iceSpikeEmitter.Times[0] == renderer.Time.PausableLast / 1000d);
                Debug.Assert(iceSpikeEmitter.Times[1] == renderer.Time.PausableAt / 1000d);

                renderer.IceSpikeSystem.SetPosition(iceSpikeEmitter.EmitterIndex, Position);
                renderer.IceSpikeSystem.SetDirection(iceSpikeEmitter.EmitterIndex, direction);
                renderer.IceSpikeSystem.SetDead(iceSpikeEmitter.EmitterIndex, dead);
            }
        }

        public override void Draw(Renderer renderer)
        {
            if (!dead)
            {
                Vector3 up = Vector3.Up;
                Vector3 direction = renderer.IceSpikeSystem.GetDirection(iceSpikeEmitter.EmitterIndex);
                Vector3 right = Vector3.Cross(up, direction);
                up = Vector3.Cross(direction, right);

                Matrix scale = Matrix.CreateScale(iceSpikeModelScale);
                Matrix position = Matrix.CreateWorld(renderer.IceSpikeSystem.GetPosition(iceSpikeEmitter.EmitterIndex), right, up);
                Matrix world = Matrix.Multiply(scale, position);

                //DrawIceSpike(renderer, world, renderer.Camera.View, renderer.Camera.Projection);
            }
        }

        private void DrawIceSpike(Renderer renderer, Matrix world, Matrix view, Matrix projection)
        {
            GraphicsDevice device = renderer.Device;

            CullMode saveCullMode = device.RenderState.CullMode;

            float aspectRatio = device.Viewport.Width / device.Viewport.Height;
            iceSpikeModel.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in iceSpikeModel.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = iceSpikeEffect;

                    Matrix localWorld = transforms[mesh.ParentBone.Index] * world;
                    Matrix localInvTransposeWorld = Matrix.Transpose(Matrix.Invert(world));

                    iceSpikeEffect.Parameters["IceSpikeTexture"].SetValue(iceSpikeTexture);
                    iceSpikeEffect.Parameters["World"].SetValue(localWorld);
                    iceSpikeEffect.Parameters["InverseTransposeWorld"].SetValue(localInvTransposeWorld);
                    iceSpikeEffect.Parameters["View"].SetValue(view);
                    iceSpikeEffect.Parameters["Projection"].SetValue(projection);
                    iceSpikeEffect.Parameters["CameraPosition"].SetValue(renderer.Camera.Position);
                }

                mesh.Draw();
            }

            device.RenderState.CullMode = saveCullMode;
        }

        public override void UpdateBool(string id, double timestamp, bool value)
        {
            base.UpdateBool(id, timestamp, value);

            if (id == "Dead")
            {
                this.dead = value;
            }
        }

        public override void UpdateVector3(string id, double timestamp, Vector3 value)
        {
            base.UpdateVector3(id, timestamp, value);

            if (id == "Direction")
            {
                direction = value;
            }
        }

        private Vector3 direction;
        private bool dead;

        private float iceSpikeModelScale = 60;
        private PointEmitter iceSpikeEmitter;
        Model iceSpikeModel;
        private Effect iceSpikeEffect;
        private Texture2D iceSpikeTexture;
        private Matrix[] transforms;
    }
}
