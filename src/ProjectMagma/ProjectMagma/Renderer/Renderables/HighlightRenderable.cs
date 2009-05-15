using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class HighlightRenderable : Renderable
    {
        public HighlightRenderable(
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

        public override void Draw(
            Renderer renderer
        )
        {
            Matrix world = Matrix.Identity;

            float scaleModificator = 1.2f;

            // scaling
            Vector3 scale = this.scale;
            scale.X *= scaleModificator;
            scale.Y *= scaleModificator;
            scale.Z *= scaleModificator;
            world *= Matrix.CreateScale(scale);
            world *= Matrix.CreateFromQuaternion(rotation);
            world *= Matrix.CreateTranslation(position);

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                renderer.Device.RenderState.DepthBufferEnable = true;
                Vector3[] diffuseColors = new Vector3[mesh.Effects.Count];
                int i = 0;
                foreach (BasicEffect effectx in mesh.Effects)
                {
                    diffuseColors[i] = effectx.DiffuseColor;
                    effectx.DiffuseColor = new Vector3(1.0f, 1.0f, 0.0f);
                    effectx.EnableDefaultLighting();
                    effectx.View = renderer.Camera.View;
                    effectx.Projection = renderer.Camera.Projection;
                    effectx.World = transforms[mesh.ParentBone.Index] * world;
                    ++i;
                }

                //renderer.Device.RenderState.DepthBufferEnable = false;
                renderer.Device.RenderState.AlphaBlendEnable = true;
                renderer.Device.RenderState.StencilFunction = CompareFunction.Always;
                renderer.Device.RenderState.BlendFactor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                renderer.Device.RenderState.SourceBlend = Blend.BlendFactor;
                renderer.Device.RenderState.BlendFunction = BlendFunction.Add;
                renderer.Device.RenderState.DestinationBlend = Blend.InverseBlendFactor;

                mesh.Draw();

                renderer.Device.RenderState.AlphaBlendEnable = false;

                i = 0;
                foreach (BasicEffect effectx in mesh.Effects)
                {
                    effectx.DiffuseColor = diffuseColors[i];
                    ++i;
                }
            }
        }

        public override RenderMode RenderMode 
        {
            get { return RenderMode.RenderToSceneAlpha; }
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

        public override Vector3 Position
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
