using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Renderer
{
    public class LavaRenderable : BasicRenderable
    {
        public LavaRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model,
            Texture2D sparseStuccoTexture,
            Texture2D fireFractalTexture,
            Texture2D vectorCloudTexture,
            Texture2D graniteTexture)
            : base(scale, rotation, position, model)
        {
            UseLights = false;
            UseMaterialParameters = false;
            UseSquash = false;

            RenderChannel = RenderChannelType.Two;

            this.sparseStuccoTexture = sparseStuccoTexture;
            this.fireFractalTexture = fireFractalTexture;
            this.vectorCloudTexture = vectorCloudTexture;
            this.graniteTexture = graniteTexture;
            SetDefaultMaterialParameters();

            Effect effect = Game.Instance.ContentManager.Load<Effect>("Effects/Lava/Lava");
            InitializeRandomOffsets(effect);
        }

        public override void LoadResources()
        {
            base.LoadResources();

            this.temperatureTexture = Game.Instance.ContentManager.Load<Texture2D>("Textures/lava/temperature");
        }

        protected override void ApplyEffectsToModel()
        {
            Effect effect = Game.Instance.ContentManager.Load<Effect>("Effects/Lava/Lava");
            ReplaceBasicEffect(Model, effect);
        }

        protected override void ApplyTechnique(Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques["MultiPlaneLava"];
        }

        private void InitializeRandomOffsets(Effect effect)
        {
            //randomOffsetParameter = ;
            randomOffsetCount = effect.Parameters["RandomOffset"].Elements.Count;
            randomOffset = new Vector2[randomOffsetCount];
            d_randomOffset = new Vector2[randomOffsetCount];
            dd_randomOffset = new Vector2[randomOffsetCount];

            for (int i = 0; i < randomOffsetCount; i++)
            {
                randomOffset[i] = new Vector2(0.5f, 0.5f);
                d_randomOffset[i] = new Vector2(0.5f, 0.5f);
            }
            offsetRand = new Random(1234);

            Console.WriteLine("initializing rand");
        }

        private void UpdateRandomOffsets(Effect effect)
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

            effect.Parameters["RandomOffset"].SetValue(randomOffset);
            //effect.Parameters["RandomOffsetX"].SetValue((float) offsetRand.NextDouble());

            Console.WriteLine("off: "+randomOffset[1].ToString());
        }

        private Texture2D temperatureTexture;        

        protected override void ApplyCustomEffectParameters(Effect effect, Renderer renderer, GameTime gameTime)
        {
            base.ApplyCustomEffectParameters(effect, renderer, gameTime);

            effect.Parameters["WorldViewProjection"].SetValue(
                effect.Parameters["World"].GetValueMatrix() *
                effect.Parameters["View"].GetValueMatrix() *
                effect.Parameters["Projection"].GetValueMatrix()
            );

            effect.Parameters["StuccoSparse"].SetValue(sparseStuccoTexture);
            effect.Parameters["FireFractal"].SetValue(fireFractalTexture);
            effect.Parameters["Granite"].SetValue(graniteTexture);
            effect.Parameters["Clouds"].SetValue(vectorCloudTexture);
            effect.Parameters["Temperature"].SetValue(temperatureTexture);

            effect.Parameters["g_LightDir"].SetValue(Vector3.Normalize(new Vector3(0, 1, 0)));
            effect.Parameters["invert"].SetValue(true);
            effect.Parameters["flickerStrength"].SetValue(0.01f);
            effect.Parameters["StuccoCompression"].SetValue(0.5f);

            effect.Parameters["minPlaneY"].SetValue(-45.0f);
            effect.Parameters["maxPlaneY"].SetValue(45.0f);

            effect.Parameters["FogEnabled"].SetValue(0.0f);
            effect.Parameters["FogStart"].SetValue(1000.0f);
            effect.Parameters["FogEnd"].SetValue(2000.0f);
            effect.Parameters["FogColor"].SetValue(Vector3.One);
            effect.Parameters["EyePosition"].SetValue(renderer.Camera.Position);

            UpdateRandomOffsets(effect);
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

        //    // shadows should be floating a little above the receiving surface
        //    Matrix world_offset = world;
        //    world_offset *= Matrix.CreateTranslation(new Vector3(0, 3, 0));

        //    foreach (ModelMesh mesh in model.Meshes)
        //    {
        //        effect.CurrentTechnique = effect.Techniques["MultiPlaneLava"];

        //        // transformation parameters
        //        effect.Parameters["g_mWorld"].SetValue(world);
        //        effect.Parameters["g_mView"].SetValue(Game.Instance.View);
        //        effect.Parameters["g_mWorldViewProjection"].SetValue(world * Game.Instance.View * Game.Instance.Projection);

        //        // texture parameters
        //        effect.Parameters["StuccoSparse"].SetValue(sparseStuccoTexture);
        //        effect.Parameters["FireFractal"].SetValue(fireFractalTexture);
        //        effect.Parameters["Granite"].SetValue(graniteTexture);
        //        effect.Parameters["Clouds"].SetValue(vectorCloudTexture);

        //        // other stuff
        //        effect.Parameters["g_LightDir"].SetValue(Vector3.Normalize(new Vector3(0, 1, 0)));
        //        effect.Parameters["invert"].SetValue(true);
        //        effect.Parameters["flickerStrength"].SetValue(0.01f);
        //        effect.Parameters["StuccoCompression"].SetValue(0.65f);
                
        //        //effect.Parameters["minPlaneY"].SetValue(boundingBox.Min.Y);
        //        //effect.Parameters["maxPlaneY"].SetValue(boundingBox.Max.Y);

        //        effect.Parameters["minPlaneY"].SetValue(-45.0f);
        //        effect.Parameters["maxPlaneY"].SetValue(0.0f);

        //        UpdateRandomOffsets();

        //        // draw the lava plane
        //        foreach (ModelMeshPart meshPart in mesh.MeshParts)
        //        {
        //            meshPart.Effect = effect;
        //        }
        //        mesh.Draw();

        //        // draw the shadow
        //        Effect shadowEffect = renderer.ShadowEffect;

        //        shadowEffect.CurrentTechnique = shadowEffect.Techniques["Scene"];
        //        shadowEffect.Parameters["ShadowMap"].SetValue(renderer.LightResolve);
        //        shadowEffect.Parameters["WorldCameraViewProjection"].SetValue(
        //            world_offset * Game.Instance.View * Game.Instance.Projection);
        //        shadowEffect.Parameters["World"].SetValue(world_offset);

        //        shadowEffect.Parameters["WorldLightViewProjection"].SetValue(
        //            world_offset * renderer.LightView * renderer.LightProjection);
        //        foreach (ModelMeshPart meshPart in mesh.MeshParts)
        //        {
        //            meshPart.Effect = shadowEffect;
        //        }
        //        mesh.Draw();
        //    }
        //}

        //public RenderMode RenderMode 
        //{
        //    get { return RenderMode.RenderToScene; }
        //}

        //public Vector3 Scale
        //{
        //    get { return scale; }
        //    set { scale = value; }
        //}

        //public Quaternion Rotation
        //{
        //    get { return rotation; }
        //    set { rotation = value; }
        //}

        //public Vector3 Position
        //{
        //    get { return position; }
        //    set { position = value; }
        //}

        //private Vector3 scale;
        //private Quaternion rotation;
        //private Vector3 position;
        //private Model model;

        private Texture2D sparseStuccoTexture;
        private Texture2D fireFractalTexture;
        private Texture2D vectorCloudTexture;
        private Texture2D graniteTexture;

        //private Effect effect;

        Vector2[] randomOffset, d_randomOffset, dd_randomOffset;
        int randomOffsetCount;
//        EffectParameter randomOffsetParameter;

        //AlignedBox3 boundingBox;

        Random offsetRand;
    }
}
