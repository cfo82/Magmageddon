using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Integration;

namespace ProjectMagma.Renderer
{
    public class IslandRenderable : Renderable
    {
        public IslandRenderable (
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

            effect = Game.Instance.Content.Load<Effect>("Effects/Environment/Island");

            lavaBrightness = new DoublyIntegratedFloat(1.0f, 0.0f, 0.5f, 1.5f, -1.0f, 1.0f);
            lightTime = new DoublyIntegratedFloat(0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 1.0f);
        }

        private DoublyIntegratedFloat lightTime;
        private DoublyIntegratedFloat lavaBrightness;

        public void SetTexture(
            Texture2D islandTexture
        )
        {
            this.islandTexture = islandTexture;
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

            foreach (ModelMesh mesh in model.Meshes)
            {
                effect.CurrentTechnique = effect.Techniques["BasicEffect"];

                // first render pass - render mesh with its actual shader
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effect;
                }
                effect.Parameters["World"].SetValue(world);
                effect.Parameters["View"].SetValue(Game.Instance.View);
                effect.Parameters["Projection"].SetValue(Game.Instance.Projection);
                effect.Parameters["EyePosition"].SetValue(Game.Instance.EyePosition);
                effect.Parameters["ShaderIndex"].SetValue(10);

                effect.Parameters["DiffuseColor"].SetValue(Vector3.One * 1.0f);
                effect.Parameters["EmissiveColor"].SetValue(new Vector3(0.0f, 0.0f, 0.0f));
                effect.Parameters["SpecularColor"].SetValue(Vector3.One * 0.0f);
                effect.Parameters["SpecularPower"].SetValue(1.0f);
                effect.Parameters["Alpha"].SetValue(1.0f);

                //effect.Parameters["DirLight0DiffuseColor"].SetValue(new Vector3(185, 203, 251)/255.0f*1.5f);
                effect.Parameters["DirLight0DiffuseColor"].SetValue(new Vector3(112, 213, 255)/255.0f*1f);
                effect.Parameters["DirLight0Direction"].SetValue(new Vector3(0,-1,0));
                effect.Parameters["DirLight0SpecularColor"].SetValue(Vector3.One * 1.0f);

                //lavaBrightness = (float)random.NextDouble() + 0.5f;
                //float dd_lavaBrightness = (float) random.NextDouble() * 0.00002f - 0.00001f;
                //d_lavaBrightness += dd_lavaBrightness * gameTime.ElapsedGameTime.Milliseconds;
                //lavaBrightness += d_lavaBrightness * gameTime.ElapsedGameTime.Milliseconds;
                //lavaBrightness = 1.0f;
                lavaBrightness.RandomlyIntegrate(gameTime, 30.0f, 0.0f);
                effect.Parameters["DirLight1DiffuseColor"].SetValue(new Vector3(1.0f, 0.5f, 0.1f)*lavaBrightness.Value);
                effect.Parameters["DirLight1Direction"].SetValue(new Vector3(0, 1, -1));
                effect.Parameters["DirLight1SpecularColor"].SetValue(Vector3.One * 1.0f);

                effect.Parameters["DirLight2DiffuseColor"].SetValue(new Vector3(1.0f, 1.0f, 1.0f)*0.9f);
                //Vector3 dir = new Vector3(7, -3, 20);

                ////lightTimeX.RandomlyIntegrate(gameTime, 1000.0f, 0.0f);
                lightTime.RandomlyIntegrate(gameTime, 10.0f, 0.2f);
                Vector3 dir = new Vector3((float) Math.Cos(lightTime.Value),-0.2f,(float) Math.Sin(lightTime.Value));
                effect.Parameters["DirLight2Direction"].SetValue(Vector3.Normalize(dir));
                effect.Parameters["DirLight2SpecularColor"].SetValue(Vector3.One * 1.0f);

                effect.Parameters["BasicTexture"].SetValue(islandTexture);

                mesh.Draw();

                // second render pass - draw shadows upon current mesh
                DrawShadow(ref renderer, mesh, world);
            }
        }

        private void DrawShadow(ref Renderer renderer, ModelMesh mesh, Matrix world)
        {
            Effect shadowEffect = renderer.ShadowEffect;

            // shadows should be floating a little above the receiving surface
            Matrix world_offset = world * Matrix.CreateTranslation(new Vector3(0, 3, 0));

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

        private Effect effect;
        
        private Texture2D islandTexture;

        private Random random;
    }
}
