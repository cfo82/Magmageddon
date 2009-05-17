using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Primitives;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Renderer
{
    public class LavaRenderable : BasicRenderable
    {
        public struct PillarInfo
        {
            public Vector3 Position;
            public Vector3 Scale;
        }

        #region specific update classes

        public class LavaPillarUpdate : TargetedRendererUpdate
        {
            public LavaPillarUpdate(
                RendererUpdatable updatable,
                PillarInfo pillarInfo
            )
            :   base(updatable)
            {
                Debug.Assert(updatable.GetType() == typeof(LavaRenderable));
                this.pillarInfo = pillarInfo;
            }

            public override void Apply()
            {
                ((LavaRenderable)updatable).AddPillarInfo(pillarInfo);
            }

            private PillarInfo pillarInfo;
        }

        public class RecomputeLavaTemperatureUpdate : TargetedRendererUpdate
        {
            public RecomputeLavaTemperatureUpdate(
                RendererUpdatable updatable
            )
            :   base(updatable)
            {
                Debug.Assert(updatable.GetType() == typeof(LavaRenderable));
            }

            public override void Apply()
            {
                ((LavaRenderable)updatable).RecomputeLavaTemperatureTexture();
            }
        }

        #endregion

        public LavaRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model,
            Texture2D sparseStuccoTexture,
            Texture2D fireFractalTexture,
            Texture2D vectorCloudTexture,
            Texture2D graniteTexture,
            PillarInfo[] pillarInfos
            )
            : base(scale, rotation, position, model)
        {
            UseLights = false;
            UseMaterialParameters = false;
            UseSquash = false;
            UseBlinking = false;

            RenderChannel = RenderChannelType.Two;

            this.sparseStuccoTexture = sparseStuccoTexture;
            this.fireFractalTexture = fireFractalTexture;
            this.vectorCloudTexture = vectorCloudTexture;
            this.graniteTexture = graniteTexture;
            this.pillarInfos = new List<PillarInfo>();
            this.pillarInfos.AddRange(pillarInfos);
            SetDefaultMaterialParameters();

            Effect effect = Game.Instance.ContentManager.Load<Effect>("Effects/Lava/Lava");
            InitializeRandomOffsets(effect);
        }

        public override void LoadResources(Renderer renderer)
        {
            base.LoadResources(renderer);

            Texture2D temperatureTemplate = Game.Instance.ContentManager.Load<Texture2D>("Textures/lava/temperaturebase");
            this.temperatureTexture = new Texture2D(renderer.Device, temperatureTemplate.Width, temperatureTemplate.Height);
        }

        public override void UnloadResources()
        {
            this.temperatureTexture.Dispose();

            base.UnloadResources();
        }

        public void AddPillarInfo(PillarInfo info)
        {
            this.pillarInfos.Add(info);
        }

        public void RecomputeLavaTemperatureTexture()
        {
            Texture2D temperatureTemplate = Game.Instance.ContentManager.Load<Texture2D>("Textures/lava/temperaturebase");
            Color[] f = new Color[temperatureTemplate.Width * temperatureTemplate.Height];
            temperatureTemplate.GetData<Color>(f);

            Vector3 scale = Scale * 2;

            for (int i = 0; i < temperatureTemplate.Width; i++)
            {
                for (int j = 0; j < temperatureTemplate.Height; j++)
                {
                    float x = Position.X + ((float)j) / temperatureTemplate.Width * scale.X - scale.X / 2;
                    float y = Position.Z + ((float)i) / temperatureTemplate.Height * scale.Z - scale.Z / 2;
                    Vector2 pixelPosition = new Vector2(x, y);

                    float v = 1;
                    foreach (PillarInfo pillarInfo in pillarInfos)
                    {
                        Vector2 pillarPosition = new Vector2(pillarInfo.Position.X, pillarInfo.Position.Z);
                        Vector2 pillarScale = new Vector2(pillarInfo.Scale.X, pillarInfo.Scale.Z);
                        float distancSqr = (pillarPosition - pixelPosition).LengthSquared();

                        // this assumes that pillars are always round. if they aren't, this code has to be adapted.
                        float pillarRadiusSq = pillarScale.LengthSquared();
                        float radiusSq = pillarRadiusSq * 0.12f;
                        if (distancSqr < radiusSq)
                        { v *= distancSqr / radiusSq; }
                    }
                    if (v < 1)
                    {
                        Vector3 oldColorVector = f[i * temperatureTemplate.Width + j].ToVector3();
                        Vector3 newColorVector = Vector3.One * v;
                        f[i * temperatureTemplate.Width + j] = new Color(oldColorVector * newColorVector);
                    }
                }
            }


            temperatureTexture.SetData<Color>(f);
            //temperatureTexture.Save("TEST.dds", ImageFileFormat.Dds);
            //Random r = new Random();
            //for (int i = 0; i < resolution; i++)
            //{
            //    for (int j = 0; j < resolution; j++)
            //    {
            //        float x = Position.X + ((float)j) / resolution * Scale.X;
            //        float y = Position.Y + ((float)i) / resolution * Scale.Y;
            //        //f[i] = new Color(Vector3.One * ((float) r.NextDouble()));
            //        float v = 0;
            //        foreach (PillarInfo pillarInfo in pillarInfos)
            //        {
            //            if ((new Vector2(Position.X, Position.Y) - new Vector2(x, y)).LengthSquared() < 100)
            //                v = 1;
            //        }
            //        f[i * resolution + j] = new Color(Vector3.One * v);
            //    }
            //}
            //temperatureTexture.SetData<Color>(f);
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

            //Console.WriteLine("initializing rand");
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

            //Console.WriteLine("off: "+randomOffset[1].ToString());
        }

        private Texture2D temperatureTexture;        

        protected override void ApplyCustomEffectParameters(Effect effect, Renderer renderer)
        {
            base.ApplyCustomEffectParameters(effect, renderer);

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
        private List<PillarInfo> pillarInfos;

        Random offsetRand;
    }
}
