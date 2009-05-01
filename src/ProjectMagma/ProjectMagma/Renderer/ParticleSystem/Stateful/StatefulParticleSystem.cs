using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace ProjectMagma.Renderer.ParticleSystem.Stateful
{
    public class StatefulParticleSystem : ParticleSystem
    {
        public StatefulParticleSystem(
            Renderer renderer,
            ContentManager content,
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
            this.createVertexBuffer = null;
            this.createVertexDeclaration = null;
            this.particleCreateEffect = null;
            this.particleUpdateEffect = null;
            this.particleRenderingEffect = null;
            spriteBatch = new SpriteBatch(device);

            LoadResources(renderer, content, device);
        }

        protected virtual void LoadResources(
            Renderer renderer,
            ContentManager content,
            GraphicsDevice device
        )
        {
            // create texture maps
            activeTexture = 0;
            HalfVector4[] array = new HalfVector4[textureSize * textureSize];
            for (int i = 0; i < textureSize * textureSize; ++i)
            {
                array[i] = new HalfVector4(0, 0, 0, 0);
            }
            positionTextures = new RenderTarget2D[2];
            positionTextures[0] = renderer.StatefulParticleResourceManager.AllocateStateTexture(systemSize);
            positionTextures[1] = renderer.StatefulParticleResourceManager.AllocateStateTexture(systemSize);
            velocityTextures = new RenderTarget2D[2];
            velocityTextures[0] = renderer.StatefulParticleResourceManager.AllocateStateTexture(systemSize);
            velocityTextures[1] = renderer.StatefulParticleResourceManager.AllocateStateTexture(systemSize);
            device.SetRenderTarget(0, positionTextures[0]);
            device.SetRenderTarget(0, null);
            device.SetRenderTarget(0, velocityTextures[0]);
            device.SetRenderTarget(0, null);
            positionTextures[0].GetTexture().SetData<HalfVector4>(array);
            velocityTextures[0].GetTexture().SetData<HalfVector4>(array);
            //positionTextures[0].GetTexture().Save("PositionMaps/initial.dds", ImageFileFormat.Dds);

            // create effect
            createVertexBuffer = new VertexBuffer(device, CreateVertex.SizeInBytes * createVertexBufferSize, BufferUsage.WriteOnly);
            createVertexDeclaration = new VertexDeclaration(device, CreateVertex.VertexElements);

            particleCreateEffect = LoadCreateEffect(content).Clone(device);
            particleCreateEffect.CurrentTechnique = particleCreateEffect.Techniques["CreateParticles"];

            // update effect
            particleUpdateEffect = LoadUpdateEffect(content).Clone(device);
            particleUpdateEffect.CurrentTechnique = particleUpdateEffect.Techniques["UpdateParticles"];

            // rendering effect
            particleRenderingEffect = LoadRenderEffect(content).Clone(device);
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

        void CreateParticles(
            double lastFrameTime,
            double currentFrameTime
        )
        {
            Vector2 positionHalfPixel = new Vector2(1.0f / (2.0f * positionTextures[0].Width), 1.0f / (2.0f * positionTextures[0].Height));

            foreach (ParticleEmitter emitter in emitters)
            {
                NewParticle[] list = emitter.CreateParticles(lastFrameTime, currentFrameTime);
                if (list.Length == 0)
                    { continue; }

                CreateVertex[] vertices = new CreateVertex[list.Length];
                for (int i = 0; i < list.Length; ++i)
                {
                    if (index >= positionTextures[0].Width * positionTextures[0].Height)
                    {
                        index = 0;
                    }

                    int x = index % textureSize;
                    int y = index / textureSize;

                    vertices[i] = new CreateVertex(list[i].Position, list[i].Velocity, new Vector2(
                        -1.0f + 2.0f * positionHalfPixel.X + 2.0f * 2.0f * x * positionHalfPixel.X,
                        -1.0f + 2.0f * positionHalfPixel.Y + 2.0f * 2.0f * y * positionHalfPixel.Y)
                    );

                    ++index;
                }

                for (int i = 0; i < vertices.Length; i += createVertexBufferSize)
                {
                    int passVerticesCount = vertices.Length - i;
                    if (passVerticesCount > createVertexBufferSize)
                        { passVerticesCount = createVertexBufferSize; }
                    createVertexBuffer.SetData<CreateVertex>(vertices, i, passVerticesCount);

                    device.Vertices[0].SetSource(createVertexBuffer, 0, CreateVertex.SizeInBytes);
                    device.VertexDeclaration = createVertexDeclaration;

                    device.RenderState.AlphaBlendEnable = false;

                    particleCreateEffect.Begin();

                    foreach (EffectPass pass in particleCreateEffect.CurrentTechnique.Passes)
                    {
                        pass.Begin();

                        device.DrawPrimitives(PrimitiveType.PointList, 0, passVerticesCount);

                        pass.End();
                    }

                    particleCreateEffect.End();

                    device.Vertices[0].SetSource(null, 0, 0);
                }
            }
        }

        public void Update(
            double lastFrameTime,
            double currentFrameTime
        )
        {
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

                CreateParticles(intermediateLastFrameTime, intermediateCurrentFrameTime);

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
            Matrix viewMatrix,
            Matrix projectionMatrix
        )
        {
            SetParticleRenderStates(device.RenderState);

            VertexBuffer renderingVertexBuffer = renderer.StatefulParticleResourceManager.GetRenderingVertexBuffer(systemSize);
            VertexDeclaration renderingVertexDeclaration = renderer.StatefulParticleResourceManager.GetRenderingVertexDeclaration();

            device.Vertices[0].SetSource(renderingVertexBuffer, 0, RenderVertex.SizeInBytes);
            device.VertexDeclaration = renderingVertexDeclaration;

            particleRenderingEffect.Parameters["View"].SetValue(viewMatrix);
            particleRenderingEffect.Parameters["Projection"].SetValue(projectionMatrix);
            particleRenderingEffect.Parameters["RenderParticlesPositionTexture"].SetValue(positionTextures[activeTexture].GetTexture());
            particleRenderingEffect.Parameters["ViewportHeight"].SetValue(device.Viewport.Height);
            particleRenderingEffect.Parameters["StartSize"].SetValue(new Vector2(5.0f, 10.0f));
            particleRenderingEffect.Parameters["EndSize"].SetValue(new Vector2(50.0f, 200.0f));

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

        #region Configuration Parameters may be overwritten by subclasses to customize the behaviour!

        protected virtual Effect LoadCreateEffect(ContentManager content)
        {
            return content.Load<Effect>("Effects/ParticleSystem/Stateful/Default");
        }

        protected virtual Effect LoadUpdateEffect(ContentManager content)
        {
            return content.Load<Effect>("Effects/ParticleSystem/Stateful/Default");
        }

        protected virtual Effect LoadRenderEffect(ContentManager content)
        {
            return content.Load<Effect>("Effects/ParticleSystem/Stateful/Default");
        }

        protected virtual void SetUpdateParameters(EffectParameterCollection parameters)
        {
        }

        protected virtual void SetRenderingParameters(EffectParameterCollection parameters)
        {
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

        private VertexBuffer createVertexBuffer;
        private VertexDeclaration createVertexDeclaration;
        private Effect particleCreateEffect;
        private static readonly int createVertexBufferSize = 2048;

        private Effect particleUpdateEffect;

        private Effect particleRenderingEffect;

        private SpriteBatch spriteBatch;
    }
}
