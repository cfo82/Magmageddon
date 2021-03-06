﻿using System;
using System.Collections.Generic;
using ProjectMagma.Simulation.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

using ProjectMagma.Renderer.Interface;
using ProjectMagma.Shared.LevelData;
using ProjectMagma.Framework.Attributes;
using System.Diagnostics;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Simulation
{
    public delegate void IntervalExecutionAction(int times);
    public delegate void PushBackFinishedHandler();

    public class Simulation
    {
        private LevelData levelData;

        public Simulation()
        {
            entityManager = new EntityManager();
            pillarManager = new EntityKindManager(entityManager, "pillar");
            islandManager = new EntityKindManager(entityManager, "island");
            playerManager = new EntityKindManager(entityManager, "player");
            powerupManager = new EntityKindManager(entityManager, "powerup");
            collisionManager = new CollisionManager();
            soundRegistry = new SoundRegistry();
        }

        public RendererUpdateQueue Initialize(
            WrappedContentManager wrappedContent,
            string simulationLevel,
            string rendererLevel,
            double startTime
        )
        {
            try
            {
                simTime = new SimulationTime(startTime);

                StartOperation(); // needs a valid simTime!

                paused = false;

                currentUpdateQueue.AddUpdate(new Renderer.Renderer.ChangeLevelUpdate(rendererLevel));
                SetPhase(SimulationPhase.Intro, "", null);

                // load level data
                levelData = wrappedContent.Load<LevelData>(simulationLevel);
                entityManager.Load(levelData);
                soundRegistry.Load();
                LevelLoaded();

                // start to play sounds
                MusicSettingsLoaded();

                Game.Instance.AudioPlayer.Play(soundRegistry.CaveBackgroundLoop, true);
                Game.Instance.AudioPlayer.Play(soundRegistry.LavaBackgroundLoop, true);

                return EndOperation();
            }
            finally
            {
                currentUpdateQueue = null;
            }
        }

        public void MusicSettingsLoaded()
        {
            if (soundRegistry.BackgroundMusic.Trim().Length == 0)
                { MediaPlayer.Stop(); }
            else
            {
                string backgroundMusic = soundRegistry.BackgroundMusic;
                float volume = 1.0f;
                if (backgroundMusic.IndexOf(':') >= 0)
                {
                    volume = float.Parse(backgroundMusic.Substring(backgroundMusic.IndexOf(':') + 1));
                    backgroundMusic = backgroundMusic.Substring(0, backgroundMusic.IndexOf(':'));
                }

                MediaPlayer.Play(Game.Instance.ContentManager.Load<Song>(backgroundMusic));
                MediaPlayer.Volume = Game.Instance.MusicVolume * volume;
                MediaPlayer.IsRepeating = true;
            }
        }

        public RendererUpdateQueue AddPlayers(Entity[] players)
        {
            try
            {
                StartOperation();

                String[] models = new String[players.Length];
                for (int i = 0; i < models.Length; i++)
                {
                    Entity player = players[i];
                    Debug.Assert(player.HasInt(CommonNames.GamePadIndex));
                    // TODO: jbernet: I uncommented this assert. lives seems to be set later on?
                    //Debug.Assert(player.HasInt(CommonNames.Lives));
                    Debug.Assert(player.HasString("robot_entity"));
                    Debug.Assert(player.HasString(CommonNames.PlayerName));
                    player.AddBoolAttribute("ready", false);
                    player.GetAttribute<BoolAttribute>("ready").ValueChanged += OnPlayerReady;
                    models[i] = player.GetString("robot_entity");
                }
                entityManager.AddEntities(levelData, models, players);
                playersWaiting = models.Length;

                return EndOperation();
            }
            finally
            {
                currentUpdateQueue = null;
            }
        }

        public RendererUpdateQueue Close()
        {
            try
            {
                StartOperation();

                SetPhase(SimulationPhase.Closed, "", null);

                foreach (Entity player in playerManager)
                {
                    player.GetAttribute<BoolAttribute>("ready").ValueChanged -= OnPlayerReady;
                }

                entityManager.Clear();
                pillarManager.Close();
                islandManager.Close();
                playerManager.Close();
                powerupManager.Close();
                collisionManager.Close();

                return EndOperation();
            }
            finally
            {
                currentUpdateQueue = null;
            }
        }

        public RendererUpdateQueue Update()
        {
            try
            {
                Game.Instance.SimulationThread.Profiler.BeginSection("simulation_update");
                StartOperation();

                // pause simulation if explicitly paused or app changed
                // removed app-changed thing (Game.Instance.IsActive) since this should be already handled
                // by the game class
                if (!paused)
                {
                    // update simulation time
                    simTime.Update();
                    //Console.WriteLine("simulating {0}", simTime.At);

                    if (simTime.DtMs < 15)
                    {
                        System.Threading.Thread.Sleep(15 - (int)simTime.DtMs);
                    }

                    // update all entities
                    foreach (Entity e in entityManager)
                    {
                        e.Update(simTime);
                    }

                    // perform collision detection
                    collisionManager.Update(simTime);

                    // execute deferred add/remove orders on the entityManager
                    entityManager.ExecuteDeferred();

                    if (phase == SimulationPhase.Intro
                        && playersWaiting == 0)
                    {
                        SetPhase(SimulationPhase.Game, "", null);
                    }

                    //System.Threading.Thread.Sleep(60);
                }
                else
                {
                    // safety measurement:
                    //   - first we should never enter this methode since we pause simulation and simulation thread simultaneously
                    //    => assert if we arrive here
                    //   - second take precautions and pause vor 10 milliseconds if we arrive here in a release build

                    Debug.Assert(false);
                    System.Threading.Thread.Sleep(10);
                }

                RendererUpdateQueue returnValue = EndOperation();
                Game.Instance.SimulationThread.Profiler.EndSection("simulation_update");
                return returnValue;
            }
            finally
            {
                currentUpdateQueue = null;
            }
        }

        private void OnPlayerReady(BoolAttribute attr, bool oldValue, bool newValue)
        {
            if (newValue == true)
            {
                playersWaiting--;
            }
        }

        private void StartOperation()
        {
            if (currentUpdateQueue != null)
            {
                throw new Exception("synchronisation error");
            }

            currentUpdateQueue = new RendererUpdateQueue(simTime.At);
        }

        private RendererUpdateQueue EndOperation()
        {
            RendererUpdateQueue returnValue = currentUpdateQueue;
            currentUpdateQueue = null;
            return returnValue;
        }

        public EntityManager EntityManager
        {
            get
            {
                return entityManager;
            }
        }

        public EntityKindManager PillarManager
        {
            get
            {
                return pillarManager;
            }
        }

        public EntityKindManager IslandManager
        {
            get
            {
                return islandManager;
            }
        }

        public EntityKindManager PlayerManager
        {
            get
            {
                return playerManager;
            }
        }

        public EntityKindManager PowerupManager
        {
            get
            {
                return powerupManager;
            }
        }

        public CollisionManager CollisionManager
        {
            get
            {
                return collisionManager;
            }
        }

        public SoundRegistry SoundRegistry
        {
            get { return soundRegistry; }
        }


        public SimulationTime Time
        {
            get { return simTime; }
        }

        public bool Paused
        {
            get { return paused; }
        }

        public void Pause()
        {
            paused = true;
        }

        public void Resume()
        {
            paused = false;
        }

        public void SetPhase(SimulationPhase phase, string winningPlayer, RendererUpdatable winningUpdatable)
        {
            Renderer.Renderer.RendererPhase rendererPhase;

            switch (phase)
            {
                case SimulationPhase.Intro: rendererPhase = ProjectMagma.Renderer.Renderer.RendererPhase.Intro; break;
                case SimulationPhase.Game: rendererPhase = ProjectMagma.Renderer.Renderer.RendererPhase.Game; break;
                case SimulationPhase.Outro: rendererPhase = ProjectMagma.Renderer.Renderer.RendererPhase.Outro; break;
                case SimulationPhase.Closed: rendererPhase = ProjectMagma.Renderer.Renderer.RendererPhase.Closed; break;
                default: throw new System.ArgumentException(string.Format("{0} is not a valid phase", phase));
            }

            this.phase = phase;
            currentUpdateQueue.AddUpdate(new ProjectMagma.Renderer.Renderer.ChangeToPhaseUpdate(rendererPhase, winningPlayer, winningUpdatable));
        }

        public SimulationPhase Phase
        {
            get { return phase; }
        } 

        #region collision pushback

        private static void DoNothingPushBackFinishedHandler()
        {
        }

        private static readonly PushBackFinishedHandler doNothingPushBackFinishedHandler = new PushBackFinishedHandler(DoNothingPushBackFinishedHandler);

        public static void ApplyPushback(ref Vector3 position, ref Vector3 pushbackVelocity, float deacceleration)
        {
            ApplyPushback(ref position, ref pushbackVelocity, deacceleration, doNothingPushBackFinishedHandler);
        }

        public static void ApplyPushback(ref Vector3 position, ref Vector3 pushbackVelocity, float deacceleration,
            PushBackFinishedHandler ev)
        {
            if (pushbackVelocity.Length() > 0)
            {
                float dt = Game.Instance.Simulation.Time.Dt;

                //                Console.WriteLine("pushback applied: "+pushbackVelocity);

                Vector3 oldVelocity = pushbackVelocity;

                // apply de-acceleration
                pushbackVelocity -= Vector3.Normalize(pushbackVelocity) * deacceleration * dt;

                // if length increases we accelerate in opposite direction -> stop
                if (pushbackVelocity.Length() > oldVelocity.Length())
                {
                    // apply old velocity again
                    position += oldVelocity * dt;
                    // set zero
                    pushbackVelocity = Vector3.Zero;
                    // inform 
                    ev();
                }

                // apply velocity
                position += pushbackVelocity * dt;
            }
        }
        #endregion

        #region other helper functions
        /// <summary>
        /// gets position on surface of specified entity
        /// </summary>
        /// <param name="position">the position to start looking at</param>
        /// <param name="entity">the entity to query</param>
        /// <returns></returns>
        public static bool GetPositionOnSurface(ref Vector3 position, Entity entity, out Vector3 surfacePosition)
        {
            Debug.Assert(entity.HasProperty("collision"));
            Ray3 ray = new Ray3(position + 1000 * Vector3.UnitY, -Vector3.UnitY);
            return Game.Instance.Simulation.CollisionManager.GetIntersectionPoint(ref ray, entity, out surfacePosition);
        }
        #endregion

        public RendererUpdateQueue CurrentUpdateQueue
        {
            get
            {
                return currentUpdateQueue;
            }
        }

        private void LevelLoaded()
        {
            if (OnLevelLoaded != null)
            {
                OnLevelLoaded(this);
            }
        }

        public event LevelLoadedHandler OnLevelLoaded;

        private readonly EntityManager entityManager;
        private EntityKindManager pillarManager;
        private EntityKindManager islandManager;
        private EntityKindManager playerManager;
        private EntityKindManager powerupManager;
        private CollisionManager collisionManager;

        private SimulationTime simTime;
        private bool paused;
        private int playersWaiting;
        private SimulationPhase phase = SimulationPhase.Intro;

        private RendererUpdateQueue currentUpdateQueue;

        private SoundRegistry soundRegistry;
    }

    public enum SimulationPhase
    {
        Intro,
        Game,
        Outro,
        Closed
    }
}
