using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Integration;

namespace ProjectMagma.Renderer
{
    public class IslandRenderable : ModelRenderable
    {
        public IslandRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model)
            : base(scale, rotation, position, model)
        {
            randomOffset = new DoublyIntegratedVector2(Vector2.Zero, Vector2.Zero, 0.0f, 0.0f, -1.0f, 1.0f);

            effect.Parameters["DiffuseColor"].SetValue(Vector3.One * 1.0f);
            effect.Parameters["EmissiveColor"].SetValue(new Vector3(0.0f, 0.0f, 0.0f));
            effect.Parameters["SpecularColor"].SetValue(Vector3.One * 0.0f);
            effect.Parameters["SpecularPower"].SetValue(1.0f);
            effect.Parameters["Alpha"].SetValue(1.0f);
        }

        protected override void ApplyEffectsToModel()
        {
            effect = Game.Instance.Content.Load<Effect>("Effects/Environment/Island");
            SetModelEffect(Model, effect);
        }

        protected override void DrawMesh(Renderer renderer, GameTime gameTime, ModelMesh mesh)
        {
            // set matrices
            effect.Parameters["World"].SetValue(BoneTransformMatrix(mesh) * World);
            effect.Parameters["View"].SetValue(Game.Instance.View);
            effect.Parameters["Projection"].SetValue(Game.Instance.Projection);

                // set lights
                SetLights(effect, renderer.LightManager);

                // set inherited parameters
                SetBasicEffectParameters(effect, gameTime, renderer);
            mesh.Draw();
        }

        private DoublyIntegratedVector2 randomOffset;

        public void SetTexture(
            Texture2D islandTexture
        )
        {
            this.islandTexture = islandTexture;
        }

        protected void SetBasicEffectParameters(Effect effect, GameTime gameTime, Renderer renderer)
        {
            effect.Parameters["EyePosition"].SetValue(Game.Instance.EyePosition);
            effect.Parameters["ShaderIndex"].SetValue(10);
            effect.Parameters["BasicTexture"].SetValue(islandTexture);
            effect.Parameters["Clouds"].SetValue(renderer.VectorCloudTexture);
            effect.Parameters["WindStrength"].SetValue(WindStrength);
            randomOffset.RandomlyIntegrate(gameTime, 0.2f, 0.0f);
            effect.Parameters["RandomOffset"].SetValue(randomOffset.Value);

        }

        //public void Draw(
        //    Renderer renderer,
        //    GameTime gameTime
        //)
        //{
        //    Matrix world = Matrix.Identity;
        //    world *= Matrix.CreateScale(scale);
        //    world *= Matrix.CreateFromQuaternion(rotation);
        //    world *= Matrix.CreateTranslation(position);

        //    foreach (ModelMesh mesh in model.Meshes)
        //    {
        //        effect.CurrentTechnique = effect.Techniques["BasicEffect"];

        //        // first render pass - render mesh with its actual shader
        //        foreach (ModelMeshPart meshPart in mesh.MeshParts)
        //        {
        //            meshPart.Effect = effect;
        //        }
        //        effect.Parameters["World"].SetValue(world);
        //        effect.Parameters["View"].SetValue(Game.Instance.View);
        //        effect.Parameters["Projection"].SetValue(Game.Instance.Projection);

        //        UpdateLights(renderer.LightManager);

        //        mesh.Draw();

        //        // second render pass - draw shadows upon current mesh
        //        DrawShadow(ref renderer, mesh, world);
        //    }
        //}

        public RenderMode RenderMode 
        {
            get { return RenderMode.RenderToScene; }
        }


        public float WindStrength { get; set; }

        private Effect effect;
        
        private Texture2D islandTexture;
    }
}
