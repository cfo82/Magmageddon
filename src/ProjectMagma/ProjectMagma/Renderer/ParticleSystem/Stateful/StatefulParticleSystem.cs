﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
            this.index = 0;
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
            this.createVertexLists = new List<CreateVertexArray>(32);
            spriteBatch = new SpriteBatch(device);

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
            positionTextures[0] = renderer.StatefulParticleResourceManager.AllocateStateTexture(systemSize);
            positionTextures[1] = renderer.StatefulParticleResourceManager.AllocateStateTexture(systemSize);
            velocityTextures = new RenderTarget2D[2];
            velocityTextures[0] = renderer.StatefulParticleResourceManager.AllocateStateTexture(systemSize);
            velocityTextures[1] = renderer.StatefulParticleResourceManager.AllocateStateTexture(systemSize);

            // initialize the first two textures
            RenderTarget2D currentRenderTarget0 = (RenderTarget2D)device.GetRenderTarget(0);
            RenderTarget2D currentRenderTarget1 = (RenderTarget2D)device.GetRenderTarget(1);
            device.SetRenderTarget(0, positionTextures[0]);
            device.SetRenderTarget(1, velocityTextures[0]);
            device.Clear(ClearOptions.Target, new Vector4(0, 0, 0, 0), 0, 0);
            device.SetRenderTarget(0, currentRenderTarget0);
            device.SetRenderTarget(1, currentRenderTarget1);

            //positionTextures[0].GetTexture().Save("PositionMaps/initial.dds", ImageFileFormat.Dds);

            // create effect
            localCreateVertices = new CreateVertex[createVertexBufferSize];
            createVertexBuffer = new DynamicVertexBuffer(device, CreateVertex.SizeInBytes * createVertexBufferSize, BufferUsage.Points | BufferUsage.WriteOnly);
            createVertexDeclaration = new VertexDeclaration(device, CreateVertex.VertexElements);

            particleCreateEffect = LoadCreateEffect(wrappedContent).Clone(device);
            particleCreateEffect.CurrentTechnique = particleCreateEffect.Techniques["CreateParticles"];

            // update effect
            particleUpdateEffect = LoadUpdateEffect(wrappedContent).Clone(device);
            particleUpdateEffect.CurrentTechnique = particleUpdateEffect.Techniques["UpdateParticles"];

            // rendering effect
            particleRenderingEffect = LoadRenderEffect(wrappedContent).Clone(device);
            particleRenderingEffect.CurrentTechnique = particleRenderingEffect.Techniques["RenderParticles"];
        }

        public virtual void UnloadResources()
        {
            // release texture maps
            renderer.StatefulParticleResourceManager.FreeStateTexture(systemSize, positionTextures[0]);
            renderer.StatefulParticleResourceManager.FreeStateTexture(systemSize, positionTextures[1]);
            renderer.StatefulParticleResourceManager.FreeStateTexture(systemSize, velocityTextures[0]);
            renderer.StatefulParticleResourceManager.FreeStateTexture(systemSize, velocityTextures[1]);

            // release the other resources
            spriteBatch.Dispose();
            createVertexBuffer.Dispose();
            createVertexDeclaration.Dispose();
            particleCreateEffect.Dispose();
            particleUpdateEffect.Dispose();
            particleRenderingEffect.Dispose();
        }

        public void AddEmitter(
            ParticleEmitter emitter
        )
        {
            if (this.emitters.Contains(emitter))
            {
                throw new ArgumentException("the given emitter is already registered!");
            }

            this.emitters.Add(emitter);
        }

        public void RemoveEmitter(
            ParticleEmitter emitter
        )
        {
            if (!this.emitters.Contains(emitter))
            {
                throw new ArgumentException("the given emitter is not registered!");
            }

            this.emitters.Remove(emitter);
        }

        void FlushCreateParticles(int count)
        {
            createVertexBuffer.SetData<CreateVertex>(createVertexBufferIndex * CreateVertex.SizeInBytes, localCreateVertices, 0, count, CreateVertex.SizeInBytes, SetDataOptions.NoOverwrite);

            device.Vertices[0].SetSource(createVertexBuffer, createVertexBufferIndex * CreateVertex.SizeInBytes, CreateVertex.SizeInBytes);
            device.VertexDeclaration = createVertexDeclaration;

            device.RenderState.AlphaBlendEnable = false;

            particleCreateEffect.Begin();

            foreach (EffectPass pass in particleCreateEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.DrawPrimitives(PrimitiveType.PointList, 0, count);

                pass.End();
            }

            particleCreateEffect.End();

            device.Vertices[0].SetSource(null, 0, 0);
        }

        void CreateParticles(
            double lastFrameTime,
            double currentFrameTime,
            bool render
        )
        {
            Vector2 positionHalfPixel = new Vector2(1.0f / (2.0f * positionTextures[0].Width), 1.0f / (2.0f * positionTextures[0].Height));

            int[] particleCount = new int[emitters.Count];

            int sumParticleCount = 0;
            for (int emitterIndex = 0; emitterIndex < emitters.Count; ++emitterIndex)
            {
                ParticleEmitter emitter = emitters[emitterIndex];
                particleCount[emitterIndex] = emitter.CalculateParticleCount(lastFrameTime, currentFrameTime);
                sumParticleCount += particleCount[emitterIndex];
            }

            CreateVertexArray vertices = renderer.StatefulParticleResourceManager.AllocateCreateVertexArray(sumParticleCount);
            for (int i = 0; i < vertices.OccupiedSize; ++i)
            {
                if (index >= positionTextures[0].Width * positionTextures[0].Height)
                {
                    index = 0;
                }

                int x = index % textureSize;
                int y = index / textureSize;

                vertices.Array[i] = new CreateVertex(Vector3.Zero, Vector3.Zero, new Vector2(
                    -1.0f + 2.0f * positionHalfPixel.X + 2.0f * 2.0f * x * positionHalfPixel.X,
                    -1.0f + 2.0f * positionHalfPixel.Y + 2.0f * 2.0f * y * positionHalfPixel.Y)
                );

                ++index;
            }

            int vertexIndex = 0;
            for (int emitterIndex = 0; emitterIndex < emitters.Count; ++emitterIndex)
            {
                ParticleEmitter emitter = emitters[emitterIndex];
                emitter.CreateParticles(lastFrameTime, currentFrameTime, vertices.Array, vertexIndex, particleCount[emitterIndex]);
                vertexIndex += particleCount[emitterIndex];
            }

            createVertexLists.Add(vertices);

            if (render)
            {
                int localCreateVerticesIndex = 0;
                for (int i = 0; i < createVertexLists.Count; ++i)
                {
                    CreateVertexArray currentVertices = createVertexLists[i];

                    int verticesCopied = 0;
                    while (verticesCopied < currentVertices.OccupiedSize)
                    {
                        int passVerticesCount = currentVertices.OccupiedSize - verticesCopied;
                        int passVerticesAvailable = createVertexBufferSize - localCreateVerticesIndex;
                        if (passVerticesCount > passVerticesAvailable)
                        {
                            passVerticesCount = passVerticesAvailable;
                        }

                        for (int j = 0; j < passVerticesCount; ++j)
                        {
                            localCreateVertices[localCreateVerticesIndex + j] = currentVertices.Array[verticesCopied + j];
                        }

                        verticesCopied += passVerticesCount;
                        localCreateVerticesIndex += passVerticesCount;

                        if (localCreateVerticesIndex >= createVertexBufferSize - createVertexBufferIndex)
                        {
                            FlushCreateParticles(verticesCopied);
                            localCreateVerticesIndex = 0;
                            createVertexBufferIndex = 0;
                        }
                    }
                }

                if (localCreateVerticesIndex > 0)
                {
                    FlushCreateParticles(localCreateVerticesIndex);
                }
            }
        }

        public void Update(
            double lastFrameTime,
            double currentFrameTime
        )
        {
            lastFrameTime = GetLastFrameTime();
            currentFrameTime = GetCurrentFrameTime();

            for (int i = 0; i < createVertexLists.Count; ++i)
            {
                renderer.StatefulParticleResourceManager.FreeCreateVertexArray(createVertexLists[i]);
            }
            createVertexLists.Clear();

            // calculate the timestep to make
            double intermediateLastFrameTime = lastFrameTime - remainingDt;
            double intermediateCurrentFrameTime = intermediateLastFrameTime + simulationStep;

            while (intermediateCurrentFrameTime < currentFrameTime)
            {
                int nextTexture = (activeTexture + 1) % 2;

                RenderTarget2D oldRenderTarget0 = (RenderTarget2D)device.GetRenderTarget(0);
                RenderTarget2D oldRenderTarget1 = (RenderTarget2D)device.GetRenderTarget(1);
                device.SetRenderTarget(0, positionTextures[nextTexture]);
                device.SetRenderTarget(1, velocityTextures[nextTexture]);

                particleUpdateEffect.Parameters["UpdateParticlesPositionTexture"].SetValue(positionTextures[activeTexture].GetTexture());
                particleUpdateEffect.Parameters["UpdateParticlesVelocityTexture"].SetValue(velocityTextures[activeTexture].GetTexture());
                particleUpdateEffect.Parameters["Dt"].SetValue((float)simulationStep);
                SetUpdateParameters(particleUpdateEffect.Parameters);

                Debug.Assert(particleUpdateEffect.CurrentTechnique.Passes.Count == 1);
                EffectPass pass = particleUpdateEffect.CurrentTechnique.Passes[0];

                Texture2D positionTexture = positionTextures[activeTexture].GetTexture();

                spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);

                particleUpdateEffect.Begin();
                pass.Begin();

                spriteBatch.Draw(positionTexture, new Rectangle(0, 0, positionTexture.Width, positionTexture.Height), Color.White);

                spriteBatch.End();

                pass.End();
                particleUpdateEffect.End();

                CreateParticles(intermediateLastFrameTime, intermediateCurrentFrameTime, (intermediateCurrentFrameTime + simulationStep) > currentFrameTime);

                device.SetRenderTarget(1, oldRenderTarget1);
                device.SetRenderTarget(0, oldRenderTarget0);

                activeTexture = nextTexture;

                //positionTextures[activeTexture].GetTexture().Save(string.Format("PositionMaps/{0:0000}.dds", fileindex++), ImageFileFormat.Dds);
                intermediateLastFrameTime = intermediateCurrentFrameTime;
                intermediateCurrentFrameTime += simulationStep;
            }

            intermediateCurrentFrameTime -= simulationStep; // undo last addition...
            remainingDt = currentFrameTime - intermediateCurrentFrameTime;
        }

        public void Render(
            double lastFrameTime,
            double currentFrameTime,
            Matrix viewMatrix,
            Matrix projectionMatrix
        )
        {
            lastFrameTime = GetLastFrameTime();
            currentFrameTime = GetCurrentFrameTime();

            SetParticleRenderStates(device.RenderState);

            VertexBuffer renderingVertexBuffer = renderer.StatefulParticleResourceManager.GetRenderingVertexBuffer(systemSize);
            VertexDeclaration renderingVertexDeclaration = renderer.StatefulParticleResourceManager.GetRenderingVertexDeclaration();

            device.Vertices[0].SetSource(renderingVertexBuffer, 0, RenderVertex.SizeInBytes);
            device.VertexDeclaration = renderingVertexDeclaration;

            particleRenderingEffect.Parameters["View"].SetValue(viewMatrix);
            particleRenderingEffect.Parameters["Projection"].SetValue(projectionMatrix);
            particleRenderingEffect.Parameters["RenderParticlesPositionTexture"].SetValue(positionTextures[activeTexture].GetTexture());
            particleRenderingEffect.Parameters["RandomTexture"].SetValue(renderer.VectorCloudTexture);
            particleRenderingEffect.Parameters["ViewportHeight"].SetValue(device.Viewport.Height);
            particleRenderingEffect.Parameters["StartSize"].SetValue(new Vector2(5.0f, 10.0f));
            particleRenderingEffect.Parameters["EndSize"].SetValue(new Vector2(50.0f, 200.0f));
            particleRenderingEffect.Parameters["Time"].SetValue((float)currentFrameTime);

            SetRenderingParameters(particleRenderingEffect.Parameters);

            particleRenderingEffect.Begin();

            foreach (EffectPass pass in particleRenderingEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.DrawPrimitives(PrimitiveType.PointList, 0, textureSize * textureSize);

                pass.End();
            }

            particleRenderingEffect.End();

            ResetParticleRenderStates(device.RenderState);
        }

        private void SetParticleRenderStates(RenderState renderState)
        {
            // Enable point sprites.
            renderState.PointSpriteEnable = true;
            renderState.PointSizeMax = 256;

            // Set the alpha blend mode.
            renderState.AlphaBlendEnable = true;
            renderState.AlphaBlendOperation = BlendFunction.Add;
            renderState.SourceBlend = Blend.SourceAlpha;
            renderState.DestinationBlend = Blend.InverseSourceAlpha;

            // Set the alpha test mode.
            renderState.AlphaTestEnable = true;
            renderState.AlphaFunction = CompareFunction.Greater;
            renderState.ReferenceAlpha = 0;

            // Enable the depth buffer (so particles will not be visible through
            // solid objects like the ground plane), but disable depth writes
            // (so particles will not obscure other particles).
            renderState.DepthBufferEnable = true;
            renderState.DepthBufferWriteEnable = false;
        }

        private void ResetParticleRenderStates(RenderState renderState)
        {
            renderState.PointSpriteEnable = false;
            renderState.DepthBufferWriteEnable = true;
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

        protected virtual void SetUpdateParameters(EffectParameterCollection parameters)
        {
        }

        protected virtual void SetRenderingParameters(EffectParameterCollection parameters)
        {
        }

        protected virtual double GetLastFrameTime()
        {
            return renderer.Time.Last/1000d;
        }

        protected virtual double GetCurrentFrameTime()
        {
            return renderer.Time.At/1000d;
        }

        #endregion

        private Renderer renderer;
        private List<ParticleEmitter> emitters;
        private int index;
        private GraphicsDevice device;

        private double remainingDt;
        private static readonly double simulationStep = 1.0 / 60.0;

        private static readonly int textureSize = 96;
        private static readonly Size systemSize = Size.Max9308;
        private int activeTexture;
        private RenderTarget2D[] positionTextures;
        private RenderTarget2D[] velocityTextures;

        private CreateVertex[] localCreateVertices;
        private DynamicVertexBuffer createVertexBuffer;
        private VertexDeclaration createVertexDeclaration;
        private Effect particleCreateEffect;
        private static readonly int createVertexBufferSize = 20000;
        private int createVertexBufferIndex = 0;

        private Effect particleUpdateEffect;

        private Effect particleRenderingEffect;

        private SpriteBatch spriteBatch;
        private List<CreateVertexArray> createVertexLists;
    }
}
