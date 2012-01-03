using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.LevelData;
using ProjectMagma.Renderer.Interface;
using ProjectMagma.Renderer.ParticleSystem.Emitter;
using ProjectMagma.Renderer.ParticleSystem.Stateful.Implementations;
using ProjectMagma.Renderer.Renderables;
using ProjectMagma.Bugslayer;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace ProjectMagma.Renderer
{
    public class Renderer : RendererInterface
    {
        public enum RendererPhase
        {
            Intro,
            Game,
            Outro,
            Closed
        }

        public class ChangeLevelUpdate : RendererUpdate
        {
            public ChangeLevelUpdate(string levelName)
            {
                this.levelName = levelName;
            }

            public void Apply(double timestamp)
            {
                Game.Instance.Renderer.ChangeLevel(levelName);
            }

            private string levelName;
        }

        public class ChangeToPhaseUpdate : RendererUpdate
        {
            public ChangeToPhaseUpdate(RendererPhase newPhase, string winningPlayer, RendererUpdatable winningUpdatable)
            {
                this.newPhase = newPhase;
                this.winningPlayer = winningPlayer;
                this.winningUpdatable = winningUpdatable;
            }

            public void Apply(double timestamp)
            {
                Game.Instance.Renderer.ChangeToPhase(newPhase, winningPlayer, winningUpdatable);
            }

            private RendererPhase newPhase;
            private string winningPlayer;
            private RendererUpdatable winningUpdatable;
        }

        public Renderer(
            WrappedContentManager wrappedContent,
            GraphicsDevice device
        )
        {
            EnablePostProcessing = true;

            renderTime = new RenderTime(Game.Instance.GlobalClock.ContinuousMilliseconds,
                Game.Instance.GlobalClock.PausableMilliseconds);

            entityManager = new RendererEntityManager();

            this.device = device;
            updateRenderables = new List<Renderable>();
            //            shadowCaster = new List<Renderable>();
            opaqueRenderables = new List<Renderable>();
            transparentRenderablesTEST = new List<Renderable>();
            transparentRenderables = new List<Renderable>();
            overlays = new List<Renderable>();

            shadowEffect = wrappedContent.Load<Effect>("Effects/ShadowEffect");

            lightPosition = new Vector3(0, 10000, 0); // later: replace by orthographic light, not lookAt
            lightTarget = Vector3.Zero;
            lightProjection = Matrix.CreateOrthographic(1500, 1500, 0.0f, 10000.0f);

            // Set the light to look at the center of the scene.
            lightView = Matrix.CreateLookAt(
                lightPosition,
                lightTarget,
                new Vector3(0, 0, -1));

            // later: replace by something like this:

            lightRenderTarget = new RenderTarget2D(
                                 device,
                                 shadowMapSize,
                                 shadowMapSize,
                                 false,
                                 SurfaceFormat.Single,
                                 DepthFormat.Depth24Stencil8
                                 );

            vectorCloudTexture = wrappedContent.Load<Texture2D>("Textures/Lava/vectorclouds");
            Color[] vectorCloudTextureData = new Color[vectorCloudTexture.Width * vectorCloudTexture.Height];
            vectorCloudTexture.GetData<Color>(vectorCloudTextureData);

            HalfVector4[] vectorCloudTexture4vsData = new HalfVector4[vectorCloudTexture.Width * vectorCloudTexture.Height];
            for (int i = 0; i < vectorCloudTexture.Width*vectorCloudTexture.Height; ++i)
            {
                Color current = vectorCloudTextureData[i];
                float r = (float)current.R / (float)Byte.MaxValue;
                float g = (float)current.G / (float)Byte.MaxValue;
                float b = (float)current.B / (float)Byte.MaxValue;
                vectorCloudTexture4vsData[i] = new HalfVector4(r, g, b, 1);
            }

            vectorCloudTextureForVertexShaders = new Texture2D(device, vectorCloudTexture.Width, vectorCloudTexture.Height, false, SurfaceFormat.HalfVector4);
            vectorCloudTextureForVertexShaders.SetData(vectorCloudTexture4vsData);

            // set up render targets
            PresentationParameters pp = Device.PresentationParameters;
            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;
            //SurfaceFormat format = SurfaceFormat.HalfVector4;
            //SurfaceFormat format = SurfaceFormat.Vector4;
            SurfaceFormat format = pp.BackBufferFormat;

            if (EnablePostProcessing)
            {
                //ResolveTarget = new ResolveTexture2D(Device, width, height, 1, format);
                targetOpaqueColorBuffer = new RenderTarget2D(Device, width, height, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
                targetAlphaColorBuffer = new RenderTarget2D(Device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                targetHDRColorBuffer = new RenderTarget2D(Device, width, height, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
                targetHorizontalBlurredHDRColorBuffer = new RenderTarget2D(Device, width / 2, height / 2, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
                targetBlurredHDRColorBuffer = new RenderTarget2D(Device, width / 2, height / 2, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);

                targetOpaqueRenderChannels = new RenderTarget2D(Device, width, height, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
                targetAlphaRenderChannels = new RenderTarget2D(Device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                targetRenderChannels = new RenderTarget2D(Device, width, height, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
                targetHorizontalBlurredRenderChannels = new RenderTarget2D(Device, width / 2, height / 2, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);
                targetBlurredRenderChannels = new RenderTarget2D(Device, width / 2, height / 2, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24Stencil8);

                targetOpaqueDepth = new RenderTarget2D(Device, width, height, false, SurfaceFormat.Vector2, DepthFormat.Depth24Stencil8);
                targetAlphaDepth = new RenderTarget2D(Device, width, height, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
                targetDepth = new RenderTarget2D(Device, width, height, false, SurfaceFormat.Vector2, DepthFormat.Depth24Stencil8);

                restoreDepthBufferPass = new RestoreDepthBufferPass(this);
                combinePass = new CombinePass(this);
                combineDepthPass = new CombineDepthPass(this);
                glowPass = new GlowPass(this);
                hdrCombinePass = new HdrCombinePass(this);
            }

            statefulParticleResourceManager = new ProjectMagma.Renderer.ParticleSystem.Stateful.ResourceManager(wrappedContent, device);


            updateQueues = new List<RendererUpdateQueue>();

            billboard = new Billboard(this, new Vector3(0, 200, 0), 250, 250, Vector4.One);
        }

        protected void ChangeLevel(
            string levelName
        )
        {
            EntityManager.Clear();
            EntityManager.Load(Game.Instance.ContentManager.Load<LevelData>(levelName));

            Camera = new Camera(this);

            LightManager = new LightManager(this);

            // recreate the lava fires system using the new level parameters
            if (explosionSystem != null)
                { explosionSystem.UnloadResources(); }
            explosionSystem = new LavaExplosion(this, Game.Instance.ContentManager, device,
                entityManager["lavafire"].GetFloat("size"),
                entityManager["lavafire"].GetFloat("rgb_multiplier"),
                entityManager["lavafire"].GetFloat("dot_multiplier"));
            for (int i = 0; i < 50; ++i)
            {
                explosionSystem.AddEmitter(new ProjectMagma.Renderer.ParticleSystem.Emitter.LavaExplosionEmitter());
            }

            // recreate the snow system using the new level parameters
            if (snowSystem != null)
                { snowSystem.UnloadResources(); }
            snowSystem = new Snow(this, Game.Instance.ContentManager, device,
                entityManager["snow"].GetFloat("particle_lifetime"),
                entityManager["snow"].GetFloat("max_alpha"),
                entityManager["snow"].GetFloat("base_size"),
                entityManager["snow"].GetFloat("random_size_modification"),
                entityManager["snow"].GetFloat("melting_start"),
                entityManager["snow"].GetFloat("melting_end"));
            snowSystem.AddEmitter(new SnowEmitter(EntityManager["snow"].GetFloat("particles_per_second")));
            for (int i = 0; i < 1000; ++i)
            {
                snowSystem.Update(-30d / 1000d + i * -30d / 1000d, -30d / 1000d + i * -30d / 1000d + 30d / 1000d);
            }

            if (iceExplosionSystem != null)
                { iceExplosionSystem.UnloadResources(); }
            iceExplosionSystem = new IceExplosion(this, Game.Instance.ContentManager, device);

            if (fireExplosionSystem != null)
               { fireExplosionSystem.UnloadResources(); }
            fireExplosionSystem = new FireExplosion(this, Game.Instance.ContentManager, device);

            if (flamethrowerSystem != null)
                { flamethrowerSystem.UnloadResources(); }
            flamethrowerSystem = new Flamethrower(this, Game.Instance.ContentManager, device);

            if (iceSpikeSystem != null)
                { iceSpikeSystem.UnloadResources(); }
            iceSpikeSystem = new IceSpike(this, Game.Instance.ContentManager, device);
        }

        protected void ChangeToPhase(
            RendererPhase phase,
            string winningPlayer,
            RendererUpdatable winningUpdatable
        )
        {
            switch (phase)
            {
                case RendererPhase.Outro:
                    {
                        WinningScreenRenderable renderable = new WinningScreenRenderable(0, winningPlayer);
                        //RobotRenderable winningRobot = winningUpdatable as RobotRenderable;
                        //Debug.Assert(winningRobot != null);
                        //winningRobot.ActivatePermanentState("win");
                        renderable.LoadResources(this);
                        updateRenderables.Add(renderable);
                        overlays.Add(renderable);
                        break;
                    }
                case RendererPhase.Intro:
                    {
                        for (int i = 0; i < overlays.Count; ++i)
                        {
                            if (overlays[i] is WinningScreenRenderable)
                            {
                                overlays.RemoveAt(i);
                                break;
                            }
                        }
                        break;
                    }
            }
        }

        public void AddUpdateQueue(RendererUpdateQueue updateQueue)
        {
            lock (updateQueues)
            {
                updateQueues.Add(updateQueue);
            }
        }

        [Conditional("DEBUG")]
        private void ValicateUpdateQueueCount()
        {
            if (updateQueues.Count > 10000)
            {
                throw new System.Exception("error: renderer has more than 10000 update queues to process!");
            }
        }

        private RendererUpdateQueue GetNextUpdateQueue()
        {
            lock (updateQueues)
            {
                if (updateQueues.Count == 0)
                { return null; }

                ValicateUpdateQueueCount();

                RendererUpdateQueue q = updateQueues[0];
                updateQueues.RemoveAt(0);
                return q;
            }
        }

        private double lastFrameTime = 0;
        private double currentFrameTime = 0;

        private void Update()
        {
            if (Camera != null)
            {
                // should only be called if a level has already been loaded
                Camera.Update(this);
                Camera.RecomputeFrame(ref opaqueRenderables);
            }

            RendererUpdateQueue q = GetNextUpdateQueue();
            while (q != null)
            {
                for (int i = 0; i < q.Count; ++i)
                {
                    q[i].Apply(q.Timestamp);
                }

                q = GetNextUpdateQueue();
            }

            foreach (Renderable renderable in updateRenderables)
            {
                renderable.Update(this);
            }

            Game.Instance.Profiler.BeginSection("particle_systems");
            PIXHelper.BeginEvent("Update Particles");

            PIXHelper.BeginEvent("Update lava flames");
            Game.Instance.Profiler.BeginSection("explosion_system");
            if (explosionSystem != null)
                { explosionSystem.Update(Time.Last / 1000d, Time.At / 1000d); }
            Game.Instance.Profiler.EndSection("explosion_system");
            PIXHelper.EndEvent();

            PIXHelper.BeginEvent("Update snow");
            Game.Instance.Profiler.BeginSection("snow_system");
            if (snowSystem != null)
                { snowSystem.Update(Time.Last / 1000d, Time.At / 1000d); }
            Game.Instance.Profiler.EndSection("snow_system");
            PIXHelper.EndEvent();

            PIXHelper.BeginEvent("Update ice explosions");
            Game.Instance.Profiler.BeginSection("ice_explosion_system");
            if (iceExplosionSystem != null)
                { iceExplosionSystem.Update(Time.PausableLast / 1000d, Time.PausableAt / 1000d); }
            Game.Instance.Profiler.EndSection("ice_explosion_system");
            PIXHelper.EndEvent();

            PIXHelper.BeginEvent("Update fire explosions");
            Game.Instance.Profiler.BeginSection("fire_explosion_system");
            if (fireExplosionSystem != null)
                { fireExplosionSystem.Update(Time.PausableLast / 1000d, Time.PausableAt / 1000d); }
            Game.Instance.Profiler.EndSection("fire_explosion_system");
            PIXHelper.EndEvent();

            PIXHelper.BeginEvent("Update flamethrowers");
            Game.Instance.Profiler.BeginSection("flamethrower_system");
            if (flamethrowerSystem != null)
                { flamethrowerSystem.Update(Time.PausableLast / 1000d, Time.PausableAt / 1000d); }
            Game.Instance.Profiler.EndSection("flamethrower_system");
            PIXHelper.EndEvent();

            PIXHelper.BeginEvent("Update ice spikes");
            Game.Instance.Profiler.BeginSection("ice_spike_system");
            if (iceSpikeSystem != null)
                { iceSpikeSystem.Update(Time.PausableLast / 1000d, Time.PausableAt / 1000d); }
            Game.Instance.Profiler.EndSection("ice_spike_system");
            PIXHelper.EndEvent();

            PIXHelper.EndEvent();
            Game.Instance.Profiler.EndSection("particle_systems");
        }

        public void Render()
        {
            PIXHelper.BeginEvent("Render Frame");

            renderTime.Update();
            //Console.WriteLine("rendering {0}", renderTime.PausableAt);

            Game.Instance.Profiler.BeginSection("beginning_stuff");
            Update();
            LightManager.Update(this);

            device.Clear(Color.White);

            // 1) Set the light's render target
            device.SetRenderTargets(lightRenderTarget);
            Game.Instance.Profiler.EndSection("beginning_stuff");

            Game.Instance.Profiler.BeginSection("rendering");

            // 2) Render the scene from the perspective of the light
            Game.Instance.Profiler.BeginSection("renderer_shadow");
            RenderShadow();
            Game.Instance.Profiler.EndSection("renderer_shadow");

            // 3) Set our render target back to the screen, and get the 
            //depth texture created by step 2
            Game.Instance.Profiler.BeginSection("change_render_target");
            device.SetRenderTargets(null);
            lightResolve = lightRenderTarget;
            Game.Instance.Profiler.EndSection("change_render_target");

            // 4) Render the scene from the view of the camera, 
            //and do depth comparisons in the shader to determine shadowing
            Game.Instance.Profiler.BeginSection("renderer_scene");
            RenderScene();
            Game.Instance.Profiler.EndSection("renderer_scene");

            // TODO: alpha blended objects...
            bool hasAlphaObjects = false;// transparentRenderablesTEST.Count > 0;
            Game.Instance.Profiler.BeginSection("render_scene_alpha");
            if (hasAlphaObjects)
            {
                RenderSceneAlpha();
            }
            Game.Instance.Profiler.EndSection("render_scene_alpha");

            if (EnablePostProcessing)
            {
                Game.Instance.Profiler.BeginSection("postprocessing");
                PIXHelper.BeginEvent("PostProcessing");

                // downscaling is done as part of the blur during the glow-pass
                if (hasAlphaObjects)
                {
                    Game.Instance.Profiler.BeginSection("combine");
                    combinePass.Render(targetOpaqueColorBuffer, targetAlphaColorBuffer, targetHDRColorBuffer);
                    combinePass.Render(targetOpaqueRenderChannels, targetAlphaRenderChannels, targetRenderChannels);
                    combineDepthPass.Render(targetOpaqueDepth, targetAlphaDepth, targetDepth);
                    Game.Instance.Profiler.EndSection("combine");

                    Game.Instance.Profiler.BeginSection("renderer_post_glow");
                    glowPass.Render(
                        targetHDRColorBuffer, targetRenderChannels,
                        targetHorizontalBlurredHDRColorBuffer, targetHorizontalBlurredRenderChannels,
                        targetBlurredHDRColorBuffer, targetBlurredRenderChannels
                        );
                    Game.Instance.Profiler.EndSection("renderer_post_glow");

                    // FIXME: do we really need to downscale and blur the renderchannels? we don't use
                    // the blurred version.

                    Game.Instance.Profiler.BeginSection("renderer_post_hdr");
                    hdrCombinePass.Render(
                        targetHDRColorBuffer, targetBlurredHDRColorBuffer, targetRenderChannels,
                        targetDepth
                        );
                    Game.Instance.Profiler.EndSection("renderer_post_hdr");
                }
                else
                {
                    Game.Instance.Profiler.BeginSection("renderer_post_glow");
                    PIXHelper.BeginEvent("Glow Pass");
                    glowPass.Render(
                        targetOpaqueColorBuffer, targetOpaqueRenderChannels,
                        targetHorizontalBlurredHDRColorBuffer, targetHorizontalBlurredRenderChannels,
                        targetBlurredHDRColorBuffer, targetBlurredRenderChannels
                        );
                    PIXHelper.EndEvent();
                    Game.Instance.Profiler.EndSection("renderer_post_glow");

                    // FIXME: do we really need to downscale and blur the renderchannels? we don't use
                    // the blurred version.

                    Game.Instance.Profiler.BeginSection("renderer_post_hdr");
                    PIXHelper.BeginEvent("HDR Combine Pass");
                    hdrCombinePass.Render(
                        targetOpaqueColorBuffer, targetBlurredHDRColorBuffer, targetOpaqueRenderChannels,
                        targetOpaqueDepth
                        );
                    PIXHelper.EndEvent();
                    Game.Instance.Profiler.EndSection("renderer_post_hdr");
                }

                PIXHelper.EndEvent();
                Game.Instance.Profiler.EndSection("postprocessing");
            }

            RenderParticles();
            RenderSceneAfterPost();

            Game.Instance.Profiler.EndSection("rendering");

            // 6) Render overlays
            Game.Instance.Profiler.BeginSection("overlay");
            RenderOverlays();
            Game.Instance.Profiler.EndSection("overlay");

            PIXHelper.EndEvent();
        }

        private void RenderShadow()
        {
            device.Clear(Color.Black);

            foreach (Renderable renderable in opaqueRenderables)
            {
                //Debug.Assert(renderable.RenderMode == RenderMode.RenderToShadowMap);
                Debug.Assert(renderable is ModelRenderable);
                if ((renderable as ModelRenderable).IsShadowCaster)
                    (renderable as ModelRenderable).DrawToShadowMap(this);
            }
        }

        private int TransparentRenderableComparison(
            Renderable r1,
            Renderable r2
        )
        {
            Vector3 cameraPosition = Camera.Position;

            Vector3 r1Diff = cameraPosition - r1.Position;
            Vector3 r2Diff = cameraPosition - r2.Position;

            if (System.Math.Abs(r1Diff.LengthSquared() - r2Diff.LengthSquared()) < 1e-05)
            {
                return 0;
            }
            else if (r1Diff.LengthSquared() > r2Diff.LengthSquared())
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        private void RenderScene()
        {
            PIXHelper.BeginEvent("Render Scene");
            
            if (EnablePostProcessing)
            {
                Device.SetRenderTargets(targetOpaqueColorBuffer, targetOpaqueRenderChannels, targetOpaqueDepth);
            }

            // clear the render targets
            Device.Clear(Color.White);

            // make sure to disable blending. XNA does not like alpha blending on floating point surfaces. And
            // it looks like it does not set them correctly when specified in .fx files. just disable it to make sure
            // that everything works as it should!
            Device.BlendState = BlendState.Opaque;

            foreach (Renderable renderable in opaqueRenderables)
            {
                Debug.Assert(renderable.RenderMode == RenderMode.RenderToScene);
                renderable.Draw(this);
            }

            if (EnablePostProcessing)
            {
                Device.SetRenderTargets(null);
            }

            //Texture2D texture = TargetDepth.GetTexture();
            //float[] pixelData = new float[texture.Width * texture.Height];
            //texture.GetData(pixelData, 0, texture.Width * texture.Height);
            //Console.WriteLine("start");
            //for (int i = 0; i < texture.Width * texture.Height; i++)
            //    if (pixelData[i] != 0.0f)
            //    {
            //        float g = pixelData[i];
            //        Console.WriteLine(g);
            //    }
            //Console.WriteLine("end");
            //int a = 0;

            PIXHelper.EndEvent();
        }

        private void RenderSceneAlpha()
        {
            if (EnablePostProcessing)
            {
                Device.SetRenderTargets(targetAlphaColorBuffer);
                Device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);
                restoreDepthBufferPass.Render(targetOpaqueDepth);
            }

            foreach (Renderable renderable in transparentRenderablesTEST)
            {
                Debug.Assert(renderable.RenderMode == RenderMode.RenderToSceneAlphaTEST);
                (renderable as AlphaIslandRenderable).CurrentPass = "IslandAlphaColor";
                renderable.Draw(this);
            }

            // need to sort transparent renderables by position and render them (back to front!!)
            // TODO: validate sorting... 

            if (EnablePostProcessing)
            {
                // draw render channel
                Device.SetRenderTargets(targetAlphaRenderChannels);
                Device.Clear(ClearOptions.Target, Color.Black, 0, 0);
                restoreDepthBufferPass.Render(targetOpaqueDepth);

                foreach (Renderable renderable in transparentRenderablesTEST)
                {
                    Debug.Assert(renderable.RenderMode == RenderMode.RenderToSceneAlphaTEST);
                    (renderable as AlphaIslandRenderable).CurrentPass = "IslandAlphaRenderChannel";
                    renderable.Draw(this);
                }

                // render depth
                Device.SetRenderTargets(targetAlphaDepth);
                Device.Clear(ClearOptions.Target, Color.Black, 0, 0);

                // the depth restore is not necessary here. it may add too many details later
                // but they can be filtered when the new depth-buffer is used.

                foreach (Renderable renderable in transparentRenderablesTEST)
                {
                    Debug.Assert(renderable.RenderMode == RenderMode.RenderToSceneAlphaTEST);
                    (renderable as AlphaIslandRenderable).CurrentPass = "IslandAlphaDepth";
                    renderable.Draw(this);
                }

                Device.SetRenderTargets(null);
            }
        }

        private void RenderSceneAfterPost()
        {
            foreach (Renderable renderable in opaqueRenderables)
            {
                renderable.DrawAfterPost(this);
            }
        }

        private void RenderParticles()
        {
            //transparentRenderables.Sort(TransparentRenderableComparison);
            PIXHelper.BeginEvent("Render particles");

            foreach (Renderable renderable in transparentRenderables)
            {
                Debug.Assert(renderable.RenderMode == RenderMode.RenderToSceneAlpha);
                renderable.Draw(this);
            }

            PIXHelper.BeginEvent("Render lava fires");
            if (explosionSystem != null)
                { explosionSystem.Render(lastFrameTime, currentFrameTime); }
            PIXHelper.EndEvent();

            PIXHelper.BeginEvent("Render Snow");
            if (snowSystem != null)
                { snowSystem.Render(lastFrameTime, currentFrameTime); }
            PIXHelper.EndEvent();

            PIXHelper.BeginEvent("Render ice explosion");
            if (iceExplosionSystem != null)
                { iceExplosionSystem.Render(Time.PausableLast / 1000d, Time.PausableAt / 1000d); }
            PIXHelper.EndEvent();

            PIXHelper.BeginEvent("Render robot explosion");
            if (fireExplosionSystem != null)
                { fireExplosionSystem.Render(Time.PausableLast / 1000d, Time.PausableAt / 1000d); }
            PIXHelper.EndEvent();

            PIXHelper.BeginEvent("Render flamethrower");
            if (flamethrowerSystem != null)
                { flamethrowerSystem.Render(Time.PausableLast / 1000d, Time.PausableAt / 1000d); }
            PIXHelper.EndEvent();

            PIXHelper.BeginEvent("Render ice spike");
            if (iceSpikeSystem != null)
                { iceSpikeSystem.Render(Time.PausableLast / 1000d, Time.PausableAt / 1000d); }
            PIXHelper.EndEvent();

            PIXHelper.EndEvent();
        }

        private void RenderOverlays()
        {
            PIXHelper.BeginEvent("Render overlays");
            foreach (Renderable renderable in overlays)
            {
                Debug.Assert(renderable.RenderMode == RenderMode.RenderOverlays);
                renderable.Draw(this);
            }
            PIXHelper.EndEvent();
        }

        private List<Renderable> GetMatchingRenderableList(
            Renderable renderable
        )
        {
            switch (renderable.RenderMode)
            {
                //case RenderMode.RenderToShadowMap: return shadowCaster;
                case RenderMode.RenderToScene: return opaqueRenderables;
                case RenderMode.RenderToSceneAlphaTEST: return transparentRenderablesTEST;
                case RenderMode.RenderToSceneAlpha: return transparentRenderables;
                case RenderMode.RenderOverlays: return overlays;
                default: throw new Exception(string.Format("invalid RenderMode constant: {0}", renderable.RenderMode));
            }
        }

        //public void RecomputeLavaTemperature(Renderable lava, List<Renderable> lava)
        //{
        //    Texture2D tex = new Texture2D(device,2 56, 256);

        //    lava;
        //}


        public void AddRenderable(
            Renderable renderable
        )
        {
            List<Renderable> renderables = GetMatchingRenderableList(renderable);

            if (
                renderables.Contains(renderable) ||
                (renderable.NeedsUpdate && updateRenderables.Contains(renderable))
                )
            {
                throw new Exception("invalid addition of already registered renderable");
            }

            renderable.LoadResources(this);

            if (renderable.NeedsUpdate)
            {
                updateRenderables.Add(renderable);
            }

            // insertion sort!
            int insertAtIndex = renderables.Count;
            for (int i = 0; i < renderables.Count; ++i)
            {
                if (
                    renderable.RenderPriority < renderables[i].RenderPriority &&
                    i < insertAtIndex
                    )
                {
                    insertAtIndex = i;
                    break;
                }
            }

            renderables.Insert(insertAtIndex, renderable);
        }

        public void RemoveRenderable(
            Renderable renderable
        )
        {
            List<Renderable> renderables = GetMatchingRenderableList(renderable);

            if (
                !renderables.Contains(renderable) ||
                (renderable.NeedsUpdate && !updateRenderables.Contains(renderable))
                )
            {
                throw new Exception("renderer does not contain the given renderable!");
            }

            renderable.UnloadResources(this);

            if (updateRenderables.Contains(renderable))
            {
                updateRenderables.Remove(renderable);
            }

            renderables.Remove(renderable);
        }

        public Vector3 CenterOfMass
        {
            get
            {
                Vector3 result = Vector3.Zero;
                int n = 0;
                foreach (Renderable renderable in updateRenderables)
                {
                    if (renderable is RobotRenderable)
                    {
                        result += renderable.Position;
                        n++;
                    }
                }
                return result / n;
            }
        }


        public GraphicsDevice Device
        {
            get { return device; }
        }

        public Texture2D LightResolve
        {
            get { return lightResolve; }
        }

        public RenderTarget2D LightRenderTarget
        {
            get { return lightRenderTarget; }
        }

        public Effect ShadowEffect
        {
            get { return shadowEffect; }
        }

        public Vector3 LightPosition
        {
            get { return lightPosition; }
        }

        public Matrix LightView
        {
            get { return lightView; }
        }

        public Matrix LightProjection
        {
            get { return lightProjection; }
        }

        public ParticleSystem.Stateful.ResourceManager StatefulParticleResourceManager
        {
            get { return statefulParticleResourceManager; }
        }

        public RenderTime Time
        {
            get { return renderTime; }
        }

        public LightManager LightManager { get; set; }

        private List<Renderable> updateRenderables;
        //private List<Renderable> shadowCaster;
        private List<Renderable> opaqueRenderables;
        private List<Renderable> transparentRenderablesTEST;
        private List<Renderable> transparentRenderables;
        private List<Renderable> overlays;

        private GraphicsDevice device;
        private Matrix lightView;
        private Matrix lightProjection;
        private Vector3 lightPosition;
        private Vector3 lightTarget;
        private Texture2D lightResolve;
        private Effect shadowEffect;
        private int shadowMapSize = 1024;
        private RenderTarget2D lightRenderTarget;

        private Texture2D vectorCloudTexture;
        private Texture2D vectorCloudTextureForVertexShaders;

        public bool EnablePostProcessing { get; set; }

        public Texture2D VectorCloudTexture
        {
            get { return vectorCloudTexture; }
        }

        public Texture2D VectorCloudTextureForVertexShaders
        {
            get { return vectorCloudTextureForVertexShaders; }
        }

        public Camera Camera { get; set; }

        // render
        private RenderTarget2D targetOpaqueColorBuffer;                 // opaque object colours are rendered into this buffer
        private RenderTarget2D targetAlphaColorBuffer;                  // transparent object colours are rendered into this buffer (in LDR)
        private RenderTarget2D targetHDRColorBuffer;                    // combined opaque&transparent buffer
        private RenderTarget2D targetHorizontalBlurredHDRColorBuffer;   // quarter-sized target containing colours which are horizontally blurred
        private RenderTarget2D targetBlurredHDRColorBuffer;             // quarter-sized target containing colours which are horizontally and vertically blurred


        private RenderTarget2D targetOpaqueRenderChannels;              // render-channel buffer for opaque objects
        private RenderTarget2D targetAlphaRenderChannels;               // render-channel buffer for transparent objects
        private RenderTarget2D targetRenderChannels;                    // combined render-channel target for both opaque and transparent objects
        private RenderTarget2D targetHorizontalBlurredRenderChannels;   // quarter sized target containing horizontally blurred render-channels
        private RenderTarget2D targetBlurredRenderChannels;             // quarter-sized target containing horizontally and vertically blurred render-channels

        private RenderTarget2D targetOpaqueDepth;
        private RenderTarget2D targetAlphaDepth;
        private RenderTarget2D targetDepth;

        public Texture2D DepthMap { get { return targetDepth; } }

        //public Texture2D ToolTexture { get; set; }

        //public ResolveTexture2D ResolveTarget { get; set; }

        public RendererEntityManager EntityManager
        {
            get { return entityManager; }
        }

        public Billboard Billboard
        {
            get { return billboard; }
        }

        public IceExplosion IceExplosionSystem
        {
            get { return iceExplosionSystem; }
        }

        public FireExplosion FireExplosionSystem
        {
            get { return fireExplosionSystem; }
        }

        public Flamethrower FlamethrowerSystem
        {
            get { return flamethrowerSystem; }
        }

        public IceSpike IceSpikeSystem
        {
            get { return iceSpikeSystem; }
        }

        private RestoreDepthBufferPass restoreDepthBufferPass;
        private CombinePass combinePass;
        private CombineDepthPass combineDepthPass;
        private GlowPass glowPass;
        private HdrCombinePass hdrCombinePass;

        private ParticleSystem.Stateful.Implementations.LavaExplosion explosionSystem;
        private ParticleSystem.Stateful.Implementations.Snow snowSystem;
        private ParticleSystem.Stateful.Implementations.IceExplosion iceExplosionSystem;
        private ParticleSystem.Stateful.Implementations.FireExplosion fireExplosionSystem;
        private ParticleSystem.Stateful.Implementations.Flamethrower flamethrowerSystem;
        private ParticleSystem.Stateful.Implementations.IceSpike iceSpikeSystem;
        private ParticleSystem.Stateful.ResourceManager statefulParticleResourceManager;

        //private LightManager lightManager;

        // ACCESS TO THIS LIST ONLY FOR SYNCHRONIZED THINGS!!
        private List<RendererUpdateQueue> updateQueues;

        private RenderTime renderTime;

        private RendererEntityManager entityManager;
        private Billboard billboard;
    }
}
