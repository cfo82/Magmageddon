using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

            effect = Game.Instance.Content.Load<Effect>("Effects/LavaEffect");

            fireFractalOffset = new Vector2(0.5f, 0.5f);
            fireFractalRand = new Random(1234);
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
                effect.CurrentTechnique = effect.Techniques["TextureColor"];

                // transformation parameters
                effect.Parameters["World"].SetValue(world);
                effect.Parameters["View"].SetValue(Game.Instance.View);
                effect.Parameters["Projection"].SetValue(Game.Instance.Projection);
                effect.Parameters["EyePosition"].SetValue(Game.Instance.EyePosition);

                // texture parameters
                effect.Parameters["StuccoSparse"].SetValue(sparseStuccoTexture);
                effect.Parameters["FireFractal"].SetValue(fireFractalTexture);
                effect.Parameters["Granite"].SetValue(graniteTexture);
                effect.Parameters["Clouds"].SetValue(vectorCloudTexture);
                //glow.Parameters["FireFractalNormal"].SetValue(fireFractalNormal);
                //glow.Parameters["GraniteNormal"].SetValue(graniteNormal);

                // boolean parameters
                effect.Parameters["boolParam1"].SetValue(false);
                effect.Parameters["boolParam2"].SetValue(true);
                effect.Parameters["boolParam3"].SetValue(true);
                effect.Parameters["boolParam4"].SetValue(false);
                effect.Parameters["boolParam5"].SetValue(false);
                effect.Parameters["boolParam6"].SetValue(false);

                // other stuff
                effect.Parameters["flickerStrength"].SetValue(0.01f);
                effect.Parameters["StuccoCompression"].SetValue(0.65f);

                // update random offset to animate textures
                dd_fireFractalOffset = new Vector2(
                    (float)fireFractalRand.NextDouble() - 0.35f,
                    (float)fireFractalRand.NextDouble() - 0.35f
                );

                d_fireFractalOffset += dd_fireFractalOffset;
                d_fireFractalOffset.Normalize();

                fireFractalOffset += d_fireFractalOffset * 0.003f;

                effect.Parameters["FireFractalOffset"].SetValue(fireFractalOffset);

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

        Vector2 fireFractalOffset;
        Vector2 d_fireFractalOffset;
        Vector2 dd_fireFractalOffset;

        Random fireFractalRand;
    }
}
