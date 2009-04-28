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
            Vector3 direction
        )
        {
            iceSpikeEffect = Game.Instance.Content.Load<Effect>("Effects/Sfx/IceSpike").Clone(Game.Instance.GraphicsDevice);

            iceSpikeEmitter = new PointEmitter(new Vector3(0, 25, 0), 5000.0f);
            iceSpikeSystem = new IceSpike(Game.Instance.Content, Game.Instance.GraphicsDevice);
            iceSpikeSystem.AddEmitter(iceSpikeEmitter);
            iceSpikeModel = Game.Instance.Content.Load<Model>("Models/Sfx/IceSpike");
            iceSpikeTexture = Game.Instance.Content.Load<Texture2D>("Textures/Sfx/IceSpikeHead");
        }

        public override void Update(Renderer renderer, GameTime gameTime)
        {
            iceSpikeEmitter.Point = position;
            iceSpikeSystem.Position = position;
            iceSpikeSystem.Direction = direction;
            iceSpikeSystem.Update(gameTime);
        }

        public override bool NeedsUpdate
        {
            get { return true; }
        }

        public override void Draw(Renderer renderer, GameTime gameTime)
        {
            Vector3 up = Vector3.Up;
            Vector3 direction = iceSpikeSystem.Direction;

            Matrix scale = Matrix.CreateScale(iceSpikeModelScale);
            Matrix position = Matrix.CreateWorld(iceSpikeSystem.Position, Vector3.Cross(up, direction), Vector3.Up);
            Matrix world = Matrix.Multiply(scale, position);

            DrawIceSpike(renderer, world, Game.Instance.View, Game.Instance.Projection);
            iceSpikeSystem.Render(Game.Instance.View, Game.Instance.Projection);
        }

        private void DrawIceSpike(Renderer renderer, Matrix world, Matrix view, Matrix projection)
        {
            GraphicsDevice device = renderer.Device;

            CullMode saveCullMode = device.RenderState.CullMode;

            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.AlphaSourceBlend = Blend.SourceAlpha;
            device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            device.RenderState.BlendFunction = BlendFunction.Add;
            device.RenderState.AlphaTestEnable = false;
            device.RenderState.DepthBufferEnable = false;
            device.RenderState.CullMode = CullMode.None;

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

        public override Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        public override RenderMode RenderMode
        {
            get { return RenderMode.RenderToSceneAlpha; }
        }

        private Vector3 position;
        private Vector3 direction;

        private float iceSpikeModelScale = 15;
        private PointEmitter iceSpikeEmitter;
        private IceSpike iceSpikeSystem;
        Model iceSpikeModel;
        private Effect iceSpikeEffect;
        private Texture2D iceSpikeTexture;
    }
}
