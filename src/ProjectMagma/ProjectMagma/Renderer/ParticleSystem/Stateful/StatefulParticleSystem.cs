using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Bugslayer;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace ProjectMagma.Renderer.ParticleSystem.Stateful
{
    public class StatefulParticleSystem : ParticleSystem
    {
        public StatefulParticleSystem(
            Renderer renderer,
            WrappedContentManager wrappedContent,
            GraphicsDevice device
        )
        {
            this.renderer = renderer;
            this.emitters = new List<ParticleEmitter>();
            this.nextParticleIndex = 0;
            this.device = device;
            this.remainingDt = 0.0;
            this.activeTexture = 0;
            this.positionTextures = null;
            this.velocityTextures = null;
            this.localCreateVertices = null;
            this.createVertexBuffer = null;
            this.createVertexDeclaration = null;
            this.particleCreateEffect = null;
            this.particleUpdateEffect = null;
            this.particleRenderingEffect = null;
            spriteBatch = new SpriteBatch(device);
            this.nextEmitterId = 0;
            this.freeEmitterIds = new List<int>();

            LoadResources(renderer, wrappedContent, device);
        }

        protected virtual void LoadResources(
            Renderer renderer,
            WrappedContentManager wrappedContent,
            GraphicsDevice device
        )
        {
            // create texture maps
            activeTexture = 0;

            positionTextures = new RenderTarget2D[2];
            positionTextures[0] = renderer.StatefulParticleResourceManager.AllocateStateTexture(GetSystemSize());
            positionTextures[1] = renderer.StatefulParticleResourceManager.AllocateStateTexture(GetSystemSize());
            velocityTextures = new RenderTarget2D[2];
            velocityTextures[0] = renderer.StatefulParticleResourceManager.AllocateStateTexture(GetSystemSize());
            velocityTextures[1] = renderer.StatefulParticleResourceManager.AllocateStateTexture(GetSystemSize());

            // initialize the first two textures
            RenderTargetBinding[] currentRenderTargets = device.GetRenderTargets();
            device.SetRenderTargets(positionTextures[0], velocityTextures[0]);
            device.Clear(ClearOptions.Target, new Vector4(0, 0, 0, 0), 0, 0);
            device.SetRenderTargets(currentRenderTargets);

            //positionTextures[0].GetTexture().Save("PositionMaps/initial.dds", ImageFileFormat.Dds);

            // create effect
            localCreateVertices = new CreateVertex[createVertexBufferSize];
            createVertexDeclaration = new VertexDeclaration(CreateVertex.SizeInBytes, CreateVertex.VertexElements);
            createVertexBuffer = new DynamicVertexBuffer(device, createVertexDeclaration, CreateVertex.SizeInBytes * createVertexBufferSize, /*BufferUsage.Points |*/ BufferUsage.WriteOnly);
            // no more point sprites... :-( TODO: fix

            particleCreateEffect = LoadCreateEffect(wrappedContent).Clone();
            particleCreateEffect.CurrentTechnique = particleCreateEffect.Techniques["CreateParticles"];

            // update effect
            particleUpdateEffect = LoadUpdateEffect(wrappedContent).Clone();
            particleUpdateEffect.CurrentTechnique = particleUpdateEffect.Techniques["UpdateParticles"];

            // rendering effect
            particleRenderingEffect = LoadRenderEffect(wrappedContent).Clone();
            particleRenderingEffect.CurrentTechnique = particleRenderingEffect.Techniques["RenderParticles"];

            spriteTexture = LoadSprite(wrappedContent);
        }

        public virtual void UnloadResources()
        {
            // release texture maps
            renderer.StatefulParticleResourceManager.FreeStateTexture(GetSystemSize(), positionTextures[0]);
            renderer.StatefulParticleResourceManager.FreeStateTexture(GetSystemSize(), positionTextures[1]);
            renderer.StatefulParticleResourceManager.FreeStateTexture(GetSystemSize(), velocityTextures[0]);
            renderer.StatefulParticleResourceManager.FreeStateTexture(GetSystemSize(), velocityTextures[1]);

            // release the other resources
            spriteBatch.Dispose();
            createVertexBuffer.Dispose();
            createVertexDeclaration.Dispose();
            particleCreateEffect.Dispose();
            particleUpdateEffect.Dispose();
            particleRenderingEffect.Dispose();
        }

        public virtual void AddEmitter(
            ParticleEmitter emitter
        )
        {
            if (this.emitters.Contains(emitter))
            {
                throw new ArgumentException("the given emitter is already registered!");
            }

            emitter.EmitterIndex = GetNextEmitterId();

            this.emitters.Add(emitter);
        }

        public virtual void RemoveEmitter(
            ParticleEmitter emitter
        )
        {
            if (emitter == null)
                { return; }
            if (!this.emitters.Contains(emitter))
                { throw new ArgumentException("the given emitter is not registered!"); }

            FreeEmitterId(emitter.EmitterIndex);

            this.emitters.Remove(emitter);
        }

        public int EmitterCount
        {
            get
            {
                return emitters.Count;
            }
        }

        public void ClearEmitters()
        {
            for (int i = 0; i < emitters.Count; ++i)
            {
                FreeEmitterId(emitters[i].EmitterIndex);
            }

            this.emitters.Clear();
        }

        void FlushCreateParticles(int count, double currentFrameTime, double dt)
        {
            createVertexBuffer.SetData<CreateVertex>(createVertexBufferIndex * CreateVertex.SizeInBytes, localCreateVertices, 0, count, CreateVertex.SizeInBytes, SetDataOptions.NoOverwrite);

            device.SetVertexBuffer(createVertexBuffer, createVertexBufferIndex * CreateVertex.SizeInBytes);
            //device.VertexDeclaration = createVertexDeclaration;

            device.BlendState = BlendState.Opaque;
            // device.RenderState.AlphaBlendEnable = false; TODO: set this parameter again!

            particleCreateEffect.Parameters["View"].SetValue(renderer.Camera.View);
            particleCreateEffect.Parameters["Projection"].SetValue(renderer.Camera.Projection);
            particleCreateEffect.Parameters["StartSize"].SetValue(new Vector2(5.0f, 10.0f));
            particleCreateEffect.Parameters["EndSize"].SetValue(new Vector2(50.0f, 200.0f));
            particleCreateEffect.Parameters["MinColor"].SetValue(Vector4.One);
            particleCreateEffect.Parameters["MaxColor"].SetValue(Vector4.One);
            particleCreateEffect.Parameters["ViewportHeight"].SetValue(device.Viewport.Height);
            particleCreateEffect.Parameters["ViewportWidth"].SetValue(device.Viewport.Width);
            particleCreateEffect.Parameters["Time"].SetValue((float)currentFrameTime);
            particleCreateEffect.Parameters["Dt"].SetValue((float)dt);
            particleCreateEffect.Parameters["PositionTexture"].SetValue(positionTextures[activeTexture]);
            particleCreateEffect.Parameters["VelocityTexture"].SetValue(velocityTextures[activeTexture]);
            particleCreateEffect.Parameters["RandomTexture"].SetValue(renderer.VectorCloudTexture);
            particleCreateEffect.Parameters["SpriteTexture"].SetValue(spriteTexture);
            SetCreateParameters(particleCreateEffect.Parameters);

            foreach (EffectPass pass in particleCreateEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.DrawPrimitives(PrimitiveType.TriangleList, 0, count/3);
                // TODO: fix: no more point sprites :-(
            }

            device.SetVertexBuffer(null);
        }

        private void UpdateParticles(
            double currentFrameTime,
            int nextTexture
            )
        {
            PIXHelper.BeginEvent("Update particles");

            particleUpdateEffect.Parameters["View"].SetValue(renderer.Camera.View);
            particleUpdateEffect.Parameters["Projection"].SetValue(renderer.Camera.Projection);
            particleUpdateEffect.Parameters["StartSize"].SetValue(new Vector2(5.0f, 10.0f));
            particleUpdateEffect.Parameters["EndSize"].SetValue(new Vector2(50.0f, 200.0f));
            particleUpdateEffect.Parameters["MinColor"].SetValue(Vector4.One);
            particleUpdateEffect.Parameters["MaxColor"].SetValue(Vector4.One);
            particleUpdateEffect.Parameters["ViewportHeight"].SetValue(device.Viewport.Height);
            particleUpdateEffect.Parameters["ViewportWidth"].SetValue(device.Viewport.Width);
            particleUpdateEffect.Parameters["Time"].SetValue((float)currentFrameTime);
            particleUpdateEffect.Parameters["Dt"].SetValue((float)SimulationStep);
            particleUpdateEffect.Parameters["PositionTexture"].SetValue(positionTextures[activeTexture]);
            particleUpdateEffect.Parameters["VelocityTexture"].SetValue(velocityTextures[activeTexture]);
            particleUpdateEffect.Parameters["RandomTexture"].SetValue(renderer.VectorCloudTexture);
            particleUpdateEffect.Parameters["SpriteTexture"].SetValue(spriteTexture);
            if (particleUpdateEffect.Parameters["ViewportSize"] != null)
            {
                particleUpdateEffect.Parameters["ViewportSize"].SetValue(new Vector2(positionTextures[nextTexture].Width, positionTextures[nextTexture].Height));
            }
            SetUpdateParameters(particleUpdateEffect.Parameters);

            Texture2D positionTexture = positionTextures[activeTexture];

            // TODO: fix rendering code!
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null, particleUpdateEffect);

            spriteBatch.Draw(positionTexture, new Rectangle(0, 0, positionTexture.Width, positionTexture.Height), Color.White);

            spriteBatch.End();

            PIXHelper.EndEvent();
        }

        private void CreateParticles(
            double lastFrameTime,
            double currentFrameTime,
            double dt
        )
        {
            Game.Instance.Profiler.BeginSection("create_particles");
            PIXHelper.BeginEvent("Create particles");

            Game.Instance.Profiler.BeginSection("count");
            Vector2 positionHalfPixel = new Vector2(1.0f / (2.0f * positionTextures[0].Width), 1.0f / (2.0f * positionTextures[0].Height));

            Vector2 positionPixel = new Vector2(1.0f / (positionTextures[0].Width), 1.0f / (positionTextures[0].Height));

            int[] particleCount = new int[emitters.Count];

            int sumParticleCount = 0;
            for (int emitterIndex = 0; emitterIndex < emitters.Count; ++emitterIndex)
            {
                ParticleEmitter emitter = emitters[emitterIndex];
                particleCount[emitterIndex] = emitter.CalculateParticleCount(lastFrameTime, currentFrameTime);
                sumParticleCount += particleCount[emitterIndex];
            }
            Game.Instance.Profiler.EndSection("count");

            // nothing to do if there are no new particles!
            if (sumParticleCount == 0)
            {
                PIXHelper.EndEvent();
                Game.Instance.Profiler.EndSection("create_particles");
                return;
            }

            Game.Instance.Profiler.BeginSection("allocate");
            CreateVertexArray vertices = renderer.StatefulParticleResourceManager.AllocateCreateVertexArray(sumParticleCount*3);
            Game.Instance.Profiler.EndSection("allocate");

            Game.Instance.Profiler.BeginSection("create");
            int maxindex = positionTextures[0].Width * positionTextures[0].Height;
            for (int i = 0; i < sumParticleCount; ++i)
            {
                if (nextParticleIndex >= maxindex)
                {
                    nextParticleIndex = 0;
                }

                int textureSize = textureSizes[(int)GetSystemSize()];
                int x = nextParticleIndex % textureSize;
                int y = nextParticleIndex / textureSize;

                Debug.Assert(textureSize == positionTextures[0].Width);
                Debug.Assert(textureSize == positionTextures[0].Height);

                Vector2[] offsets = {
                    new Vector2(-1,-1),
                    new Vector2(1,-1),
                    new Vector2(0,1)
                };

                for (int j = 0; j < 3; ++j)
                {
                    vertices.Array[i*3+j] = new CreateVertex(
                        Vector3.Zero,
                        Vector3.Zero,
                        new Vector2(
                            -1.0f + positionPixel.X + 2.0f * x * positionPixel.X + 2*offsets[j].X*positionPixel.X,
                            -1.0f + positionPixel.Y + 2.0f * y * positionPixel.Y + 2*offsets[j].Y*positionPixel.Y
                        ),
                        new Short2(
                            x,
                            y
                        ),
                        0 // emitterIndex
                    );
                }

                ++nextParticleIndex;
            }
            Game.Instance.Profiler.EndSection("create");

            Game.Instance.Profiler.BeginSection("emit");
            int vertexIndex = 0;
            for (int emitterIndex = 0; emitterIndex < emitters.Count; ++emitterIndex)
            {
                ParticleEmitter emitter = emitters[emitterIndex];

                emitter.CreateParticles(lastFrameTime, currentFrameTime, vertices.Array, vertexIndex, particleCount[emitterIndex]);
                vertexIndex += particleCount[emitterIndex];
            }
            Game.Instance.Profiler.EndSection("emit");

            Game.Instance.Profiler.BeginSection("render");
            int localCreateVerticesIndex = 0;

            int verticesCopied = 0;
            while (verticesCopied < vertices.OccupiedSize)
            {
                int passVerticesCount = vertices.OccupiedSize - verticesCopied;
                int passVerticesAvailable = createVertexBufferSize - localCreateVerticesIndex;
                if (passVerticesCount > passVerticesAvailable)
                {
                    passVerticesCount = passVerticesAvailable;
                }

                for (int j = 0; j < passVerticesCount; ++j)
                {
                    localCreateVertices[localCreateVerticesIndex + j] = vertices.Array[verticesCopied + j];
                }

                verticesCopied += passVerticesCount;
                localCreateVerticesIndex += passVerticesCount;

                if (localCreateVerticesIndex >= createVertexBufferSize - createVertexBufferIndex)
                {
                    FlushCreateParticles(verticesCopied, currentFrameTime, dt);
                    localCreateVerticesIndex = 0;
                    createVertexBufferIndex = 0;
                }
            }

            renderer.StatefulParticleResourceManager.FreeCreateVertexArray(vertices);

            if (localCreateVerticesIndex > 0)
            {
                FlushCreateParticles(localCreateVerticesIndex, currentFrameTime, dt);
            }

            Game.Instance.Profiler.EndSection("render");

            PIXHelper.EndEvent();
            Game.Instance.Profiler.EndSection("create_particles");
        }

        public void Update(
            double lastFrameTime,
            double currentFrameTime
        )
        {
            //Console.WriteLine("update {0}", this);

            // calculate the timestep to make
            double intermediateLastFrameTime = lastFrameTime - remainingDt;
            double intermediateCurrentFrameTime = intermediateLastFrameTime + SimulationStep;

            while (intermediateCurrentFrameTime < currentFrameTime)
            {
                //Console.WriteLine("update step");

                int nextTexture = (activeTexture + 1) % 2;

                RenderTargetBinding[] oldRenderTargets = device.GetRenderTargets();
                device.SetRenderTargets(positionTextures[nextTexture], velocityTextures[nextTexture]);

                UpdateParticles(intermediateCurrentFrameTime, nextTexture);

                CreateParticles(intermediateLastFrameTime, intermediateCurrentFrameTime, SimulationStep);

                device.SetRenderTargets(oldRenderTargets);

                activeTexture = nextTexture;

                //positionTextures[activeTexture].GetTexture().Save(string.Format("PositionMaps/{0:0000}.dds", fileindex++), ImageFileFormat.Dds);
                intermediateLastFrameTime = intermediateCurrentFrameTime;
                intermediateCurrentFrameTime += SimulationStep;
            }

            intermediateCurrentFrameTime -= SimulationStep; // undo last addition...
            remainingDt = currentFrameTime - intermediateCurrentFrameTime;
        }

        public void Render(
            double lastFrameTime,
            double currentFrameTime
        )
        {
            device.BlendState = BlendState.Additive;
            device.DepthStencilState = DepthStencilState.DepthRead;
            //SetParticleRenderStates(device.BlendState, device.RasterizerState, device.DepthStencilState);

            VertexBuffer renderingVertexBuffer = renderer.StatefulParticleResourceManager.GetRenderingVertexBuffer(GetSystemSize());
            VertexDeclaration renderingVertexDeclaration = renderer.StatefulParticleResourceManager.GetRenderingVertexDeclaration();

            device.SetVertexBuffer(renderingVertexBuffer, 0);
            //device.VertexDeclaration = renderingVertexDeclaration;

            particleRenderingEffect.Parameters["View"].SetValue(renderer.Camera.View);
            particleRenderingEffect.Parameters["Projection"].SetValue(renderer.Camera.Projection);
            particleRenderingEffect.Parameters["StartSize"].SetValue(new Vector2(5.0f, 10.0f));
            particleRenderingEffect.Parameters["EndSize"].SetValue(new Vector2(50.0f, 200.0f));
            particleRenderingEffect.Parameters["MinColor"].SetValue(Vector4.One);
            particleRenderingEffect.Parameters["MaxColor"].SetValue(Vector4.One);
            particleRenderingEffect.Parameters["ViewportHeight"].SetValue(device.Viewport.Height);
            particleRenderingEffect.Parameters["ViewportWidth"].SetValue(device.Viewport.Width);
            particleRenderingEffect.Parameters["Time"].SetValue((float)currentFrameTime);
            particleRenderingEffect.Parameters["Dt"].SetValue((float)(currentFrameTime-lastFrameTime));
            particleRenderingEffect.Parameters["PositionTexture"].SetValue(positionTextures[activeTexture]);
            particleRenderingEffect.Parameters["VelocityTexture"].SetValue(velocityTextures[activeTexture]);
            particleRenderingEffect.Parameters["RandomTexture"].SetValue(renderer.VectorCloudTexture);
            particleRenderingEffect.Parameters["RandomTextureForVertexShader"].SetValue(renderer.VectorCloudTextureForVertexShaders);
            particleRenderingEffect.Parameters["SpriteTexture"].SetValue(spriteTexture);
            if (particleRenderingEffect.Parameters["ViewPortScale"] != null)
            {
                particleRenderingEffect.Parameters["ViewPortScale"].SetValue(
                    new Vector2(2.0f / 1024.0f, 2.0f / 800.0f));
            }

            SetRenderingParameters(particleRenderingEffect.Parameters);

            foreach (EffectPass pass in particleRenderingEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                int textureSize = textureSizes[(int)GetSystemSize()];
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, textureSize * textureSize);
            }

            ResetParticleRenderStates(device.DepthStencilState);
        }

        private void SetParticleRenderStates(BlendState blendState, RasterizerState rasterizerState, DepthStencilState depthStencilState)
        {
            // Set the alpha blend mode.
            blendState.AlphaBlendFunction = BlendFunction.Add;
            blendState.ColorBlendFunction = BlendFunction.Add;
            blendState.AlphaSourceBlend = Blend.SourceAlpha;
            blendState.ColorSourceBlend = Blend.SourceAlpha;
            blendState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
            blendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
            //renderState.AlphaBlendEnable = true;
            //renderState.AlphaBlendOperation = BlendFunction.Add;
            //renderState.SourceBlend = Blend.SourceAlpha;
            //renderState.DestinationBlend = Blend.InverseSourceAlpha;

            // Set the alpha test mode.
            // TODO: fix
            //renderState.AlphaTestEnable = true;
            //renderState.AlphaFunction = CompareFunction.Greater;
            //renderState.ReferenceAlpha = 0;

            // Enable the depth buffer (so particles will not be visible through
            // solid objects like the ground plane), but disable depth writes
            // (so particles will not obscure other particles).
            // TODO: fix
            //renderState.DepthBufferEnable = true;
            //renderState.DepthBufferWriteEnable = false;
        }

        private void ResetParticleRenderStates(DepthStencilState depthStencilState)
        {
            // TODO: fix
            //renderState.PointSpriteEnable = false;
            //renderState.DepthBufferWriteEnable = true;
        }

        protected Renderer Renderer
        {
            get { return renderer; }
        }

        #region Configuration Parameters may be overwritten by subclasses to customize the behaviour!

        protected virtual Effect LoadCreateEffect(WrappedContentManager wrappedContent)
        {
            return wrappedContent.Load<Effect>("Effects/ParticleSystem/Stateful/Default");
        }

        protected virtual Effect LoadUpdateEffect(WrappedContentManager wrappedContent)
        {
            return wrappedContent.Load<Effect>("Effects/ParticleSystem/Stateful/Default");
        }

        protected virtual Effect LoadRenderEffect(WrappedContentManager wrappedContent)
        {
            return wrappedContent.Load<Effect>("Effects/ParticleSystem/Stateful/Default");
        }

        protected virtual Texture2D LoadSprite(WrappedContentManager wrappedContent)
        {
            return wrappedContent.Load<Texture2D>("Textures/xna_logo");
        }

        protected virtual void SetCreateParameters(EffectParameterCollection parameters)
        {
        }

        protected virtual void SetUpdateParameters(EffectParameterCollection parameters)
        {
        }

        protected virtual void SetRenderingParameters(EffectParameterCollection parameters)
        {
        }

        protected virtual double SimulationStep
        {
            get { return simulationStep; }
        }

        protected virtual Size GetSystemSize()
        {
            return Size.Max9308;
        }

        #endregion

        private void FreeEmitterId(int id)
        {
            freeEmitterIds.Add(id);
        }

        private int GetNextEmitterId()
        {
            if (freeEmitterIds.Count == 0)
            {
                int id = nextEmitterId;
                ++nextEmitterId;
                return id;
            }
            else
            {
                int id = freeEmitterIds[freeEmitterIds.Count - 1];
                freeEmitterIds.RemoveAt(freeEmitterIds.Count - 1);
                return id;
            }
        }

        protected Renderer renderer;
        protected List<ParticleEmitter> emitters;
        private int nextParticleIndex;
        private GraphicsDevice device;

        private double remainingDt;
        private static readonly double simulationStep = 1.0 / 30.0;

        private static readonly int[] textureSizes = { 16, 32, 48, 64, 96, 128, 256 };
        private int activeTexture;
        private RenderTarget2D[] positionTextures;
        private RenderTarget2D[] velocityTextures;

        private CreateVertex[] localCreateVertices;
        private DynamicVertexBuffer createVertexBuffer;
        private VertexDeclaration createVertexDeclaration;
        private Effect particleCreateEffect;
        private static readonly int createVertexBufferSize = 4000*3;
        private int createVertexBufferIndex = 0;

        private Effect particleUpdateEffect;

        private Effect particleRenderingEffect;

        private SpriteBatch spriteBatch;

        private Texture2D spriteTexture;

        // emitter Id management
        private int nextEmitterId = 0;
        private List<int> freeEmitterIds;
    }
}
