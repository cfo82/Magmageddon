using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class DefaultRenderable : Renderable
    {
        public DefaultRenderable(
            Vector3 scale,
            Quaternion rotation,
            Vector3 position,
            Model model
        )
        {
            this.scale = scale;
            this.rotation = rotation;
            this.position = position;
            this.model = model;
        }

        public void Draw(
            Renderer renderer,
            GameTime gameTime
        )
        {
            Matrix world = Matrix.Identity;
            world *= Matrix.CreateScale(scale);
            world *= Matrix.CreateFromQuaternion(rotation);
            world *= Matrix.CreateTranslation(position);

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            // shadows should be floating a little above the receiving surface
            Matrix world_offset = world;
            world_offset *= Matrix.CreateTranslation(new Vector3(0, 3, 0));

            foreach (ModelMesh mesh in model.Meshes)
            {
                Effect effect = renderer.ShadowEffect;

                renderer.Device.RenderState.DepthBufferEnable = true;
                foreach (BasicEffect effectx in mesh.Effects)
                {
                    effectx.EnableDefaultLighting();
                    effectx.View = Game.Instance.View;
                    effectx.Projection = Game.Instance.Projection;
                    effectx.World = transforms[mesh.ParentBone.Index] * world;
                }
                mesh.Draw();
                renderer.Device.RenderState.DepthBufferEnable = true;

                effect.CurrentTechnique = effect.Techniques["Scene"];
                effect.Parameters["ShadowMap"].SetValue(renderer.LightResolve);
                effect.Parameters["WorldCameraViewProjection"].SetValue(
                    transforms[mesh.ParentBone.Index] * world_offset * Game.Instance.View * Game.Instance.Projection);
                effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * world_offset);

                effect.Parameters["WorldLightViewProjection"].SetValue(
                    transforms[mesh.ParentBone.Index] * world_offset * renderer.LightView * renderer.LightProjection);


                Effect backup = mesh.MeshParts[0].Effect;
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effect;
                }
                renderer.Device.RenderState.AlphaBlendEnable = false;
                renderer.Device.RenderState.SourceBlend = Blend.SourceAlpha;
                renderer.Device.RenderState.DestinationBlend = Blend.DestinationColor;

                mesh.Draw();

                renderer.Device.RenderState.AlphaBlendEnable = false;

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = backup;
                }
            }
        }

        public RenderMode RenderMode 
        {
            get { return RenderMode.RenderToScene; }
        }

        public Vector3 Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public Quaternion Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        private Vector3 scale;
        private Quaternion rotation;
        private Vector3 position;
        private Model model;
    }
}
