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
            Vector3 position,
            Vector3 direction,
            bool dead
        )
        :   base(position)
        {
            this.direction = direction;
            this.dead = dead;

            iceSpikeEmitter = null;
        }

        public override void LoadResources()
        {
            base.LoadResources();

            iceSpikeSystem = new IceSpike(Game.Instance.Renderer, Game.Instance.ContentManager, Game.Instance.GraphicsDevice);
            iceSpikeModel = Game.Instance.ContentManager.Load<MagmaModel>("Models/Sfx/IceSpike").XnaModel;
            iceSpikeTexture = Game.Instance.ContentManager.Load<Texture2D>("Textures/Sfx/IceSpikeHead");

            iceSpikeEffect = Game.Instance.ContentManager.Load<Effect>("Effects/Sfx/IceSpike").Clone(Game.Instance.GraphicsDevice);
        }

        public override void UnloadResources()
        {
            iceSpikeSystem.UnloadResources();
            iceSpikeEffect.Dispose();

            base.UnloadResources();
        }

        public override void Update(Renderer renderer, GameTime gameTime)
        {
            base.Update(renderer, gameTime);

            if (!dead && iceSpikeEmitter == null)
            {
                iceSpikeEmitter = new PointEmitter(Position, 2500.0f);
                iceSpikeSystem.AddEmitter(iceSpikeEmitter);
            }

            if (dead && iceSpikeEmitter != null)
            {
                iceSpikeSystem.RemoveEmitter(iceSpikeEmitter);
                iceSpikeEmitter = null;
            }

            if (iceSpikeEmitter != null)
            {
                iceSpikeEmitter.SetPoint(CurrentFrameTime, Position);
                Debug.Assert(iceSpikeEmitter.Times[0] == LastFrameTime);
                Debug.Assert(iceSpikeEmitter.Times[1] == CurrentFrameTime);
            }

            iceSpikeSystem.Position = Position;
            iceSpikeSystem.Direction = direction;
            iceSpikeSystem.Dead = dead;
            iceSpikeSystem.Update(LastFrameTime, CurrentFrameTime);
        }

        public override void Draw(Renderer renderer, GameTime gameTime)
        {
            if (!dead)
            {
                Vector3 up = Vector3.Up;
                Vector3 direction = iceSpikeSystem.Direction;
                Vector3 right = Vector3.Cross(up, direction);
                up = Vector3.Cross(direction, right);

                Matrix scale = Matrix.CreateScale(iceSpikeModelScale);
                Matrix position = Matrix.CreateWorld(iceSpikeSystem.Position, right, up);
                Matrix world = Matrix.Multiply(scale, position);

                DrawIceSpike(renderer, world, renderer.Camera.View, renderer.Camera.Projection);
            }
            iceSpikeSystem.Render(LastFrameTime, CurrentFrameTime, renderer.Camera.View, renderer.Camera.Projection);
        }

        private void DrawIceSpike(Renderer renderer, Matrix world, Matrix view, Matrix projection)
        {
            GraphicsDevice device = renderer.Device;

            CullMode saveCullMode = device.RenderState.CullMode;

            Matrix[] transforms = new Matrix[iceSpikeModel.Bones.Count];
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
                    iceSpikeEffect.Parameters["CameraPosition"].SetValue(Game.Instance.CameraPosition);
                }

                mesh.Draw();
            }

            device.RenderState.CullMode = saveCullMode;
        }

        public override void UpdateBool(string id, bool value)
        {
            base.UpdateBool(id, value);

            if (id == "Dead")
            {
                this.dead = value;
            }
        }

        public override void UpdateVector3(string id, Vector3 value)
        {
            base.UpdateVector3(id, value);

            if (id == "Direction")
            {
                direction = value;
            }
        }

        private Vector3 direction;
        private bool dead;

        private float iceSpikeModelScale = 60;
        private PointEmitter iceSpikeEmitter;
        private IceSpike iceSpikeSystem;
        Model iceSpikeModel;
        private Effect iceSpikeEffect;
        private Texture2D iceSpikeTexture;
    }
}
