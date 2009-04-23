using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Renderer
{
    public class LavaRenderable : Renderable
    {
        public LavaRenderable(
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

            effect = Game.Instance.Content.Load<Effect>("Effects/Lava/Lava");
            InitializeRandomOffsets();

            VolumeCollection collection = (VolumeCollection)model.Tag;
            boundingBox = (AlignedBox3)collection.GetVolume(VolumeType.AlignedBox3);
        }

        private void InitializeRandomOffsets()
        {
            randomOffsetParameter = effect.Parameters["RandomOffset"];
            randomOffsetCount = randomOffsetParameter.Elements.Count;
            randomOffset = new Vector2[randomOffsetCount];
            d_randomOffset = new Vector2[randomOffsetCount];
            dd_randomOffset = new Vector2[randomOffsetCount];

            for (int i = 0; i < randomOffsetCount; i++)
            {
                randomOffset[i] = new Vector2(0.5f, 0.5f);
                d_randomOffset[i] = new Vector2(0.5f, 0.5f);
            }
            offsetRand = new Random(1234);
        }

        private void UpdateRandomOffsets()
        {
            for (int i = 0; i < randomOffsetCount; ++i)
            {
                dd_randomOffset[i] = new Vector2(
                    (float)offsetRand.NextDouble() - 0.35f,
                    (float)offsetRand.NextDouble() - 0.35f
                );

                d_randomOffset[i] += dd_randomOffset[i];
                d_randomOffset[i].Normalize();

                randomOffset[i] += d_randomOffset[i] * 0.001f;

            }

            randomOffsetParameter.SetValue(randomOffset);
        }

        public void SetTextures(
            Texture2D sparseStuccoTexture,
            Texture2D fireFractalTexture,
            Texture2D vectorCloudTexture,
            Texture2D graniteTexture
        )
        {
            this.sparseStuccoTexture = sparseStuccoTexture;
            this.fireFractalTexture = fireFractalTexture;
            this.vectorCloudTexture = vectorCloudTexture;
            this.graniteTexture = graniteTexture;
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

            // shadows should be floating a little above the receiving surface
            Matrix world_offset = world;
            world_offset *= Matrix.CreateTranslation(new Vector3(0, 3, 0));

            foreach (ModelMesh mesh in model.Meshes)
            {
                effect.CurrentTechnique = effect.Techniques["MultiPlaneLava"];

                // transformation parameters
                effect.Parameters["g_mWorld"].SetValue(world);
                effect.Parameters["g_mView"].SetValue(Game.Instance.View);
                effect.Parameters["g_mWorldViewProjection"].SetValue(world * Game.Instance.View * Game.Instance.Projection);

                // texture parameters
                effect.Parameters["StuccoSparse"].SetValue(sparseStuccoTexture);
                effect.Parameters["FireFractal"].SetValue(fireFractalTexture);
                effect.Parameters["Granite"].SetValue(graniteTexture);
                effect.Parameters["Clouds"].SetValue(vectorCloudTexture);

                // other stuff
                effect.Parameters["g_LightDir"].SetValue(Vector3.Normalize(new Vector3(0, 1, 0)));
                effect.Parameters["invert"].SetValue(true);
                effect.Parameters["flickerStrength"].SetValue(0.01f);
                effect.Parameters["StuccoCompression"].SetValue(0.65f);
                
                //effect.Parameters["minPlaneY"].SetValue(boundingBox.Min.Y);
                //effect.Parameters["maxPlaneY"].SetValue(boundingBox.Max.Y);

                effect.Parameters["minPlaneY"].SetValue(-45.0f);
                effect.Parameters["maxPlaneY"].SetValue(0.0f);

                UpdateRandomOffsets();

                // draw the lava plane
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effect;
                }
                mesh.Draw();

                // draw the shadow
                Effect shadowEffect = renderer.ShadowEffect;

                shadowEffect.CurrentTechnique = shadowEffect.Techniques["Scene"];
                shadowEffect.Parameters["ShadowMap"].SetValue(renderer.LightResolve);
                shadowEffect.Parameters["WorldCameraViewProjection"].SetValue(
                    world_offset * Game.Instance.View * Game.Instance.Projection);
                shadowEffect.Parameters["World"].SetValue(world_offset);

                shadowEffect.Parameters["WorldLightViewProjection"].SetValue(
                    world_offset * renderer.LightView * renderer.LightProjection);
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = shadowEffect;
                }
                mesh.Draw();
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

        private Texture2D sparseStuccoTexture;
        private Texture2D fireFractalTexture;
        private Texture2D vectorCloudTexture;
        private Texture2D graniteTexture;

        private Effect effect;

        Vector2[] randomOffset, d_randomOffset, dd_randomOffset;
        int randomOffsetCount;
        EffectParameter randomOffsetParameter;

        AlignedBox3 boundingBox;

        Random offsetRand;
    }
}
