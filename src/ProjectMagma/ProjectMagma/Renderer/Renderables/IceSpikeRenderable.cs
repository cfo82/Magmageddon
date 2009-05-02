using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Renderer.ParticleSystem.Emitter;
using ProjectMagma.Renderer.ParticleSystem.Stateful.IceSpike;

namespace ProjectMagma.Renderer
{
    public class IceSpikeRenderable : Renderable
    {
        public IceSpikeRenderable(
            Vector3 position,
            Vector3 direction,
            bool dead
        )
        {
            this.position = position;
            this.direction = direction;
            this.dead = dead;

            lastFrameTime = currentFrameTime = 0.0;

            iceSpikeEmitter = null;
        }

        public override void LoadResources()
        {
            base.LoadResources();

            iceSpikeSystem = new IceSpike(Game.Instance.Renderer, Game.Instance.ContentManager, Game.Instance.GraphicsDevice);
            iceSpikeModel = Game.Instance.ContentManager.Load<Model>("Models/Sfx/IceSpike");
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
            // calculate the timestep to make
            lastFrameTime = currentFrameTime;
            double dtMs = (double)gameTime.ElapsedGameTime.Ticks / 10000d;
            double dt = dtMs / 1000.0;
            currentFrameTime = lastFrameTime + dt;

            if (!dead && iceSpikeEmitter == null)
            {
                iceSpikeEmitter = new PointEmitter(new Vector3(0, 25, 0), 2500.0f);
                iceSpikeSystem.AddEmitter(iceSpikeEmitter);
            }

            if (dead && iceSpikeEmitter != null)
            {
                iceSpikeSystem.RemoveEmitter(iceSpikeEmitter);
                iceSpikeEmitter = null;
            }

            if (iceSpikeEmitter != null)
            {
                iceSpikeEmitter.SetPoint(currentFrameTime, position);
                Debug.Assert(iceSpikeEmitter.Times[0] == lastFrameTime);
                Debug.Assert(iceSpikeEmitter.Times[1] == currentFrameTime);
            }
            iceSpikeSystem.Position = position;
            iceSpikeSystem.Direction = direction;
            iceSpikeSystem.Dead = dead;
            iceSpikeSystem.Update(lastFrameTime, currentFrameTime);
        }

        public override bool NeedsUpdate
        {
            get { return true; }
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

                DrawIceSpike(renderer, world, Game.Instance.View, Game.Instance.Projection);
            }
            iceSpikeSystem.Render(Game.Instance.View, Game.Instance.Projection);
        }

        private void DrawIceSpike(Renderer renderer, Matrix world, Matrix view, Matrix projection)
        {
            GraphicsDevice device = renderer.Device;

            CullMode saveCullMode = device.RenderState.CullMode;

            //device.RenderState.AlphaBlendEnable = true;
            //device.RenderState.AlphaSourceBlend = Blend.SourceAlpha;
            //device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            //device.RenderState.BlendFunction = BlendFunction.Add;
            //device.RenderState.AlphaTestEnable = false;
            //device.RenderState.DepthBufferEnable = false;
            //device.RenderState.CullMode = CullMode.None;

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

            if (id == "Position")
            {
                position = value;
            }
            else if (id == "Direction")
            {
                direction = value;
            }
        }

        public override Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public override RenderMode RenderMode
        {
            get { return RenderMode.RenderToSceneAlpha; }
        }

        private Vector3 position;
        private Vector3 direction;
        private bool dead;

        private float iceSpikeModelScale = 135;
        private PointEmitter iceSpikeEmitter;
        private IceSpike iceSpikeSystem;
        Model iceSpikeModel;
        private Effect iceSpikeEffect;
        private Texture2D iceSpikeTexture;

        private double lastFrameTime;
        private double currentFrameTime;
    }
}
