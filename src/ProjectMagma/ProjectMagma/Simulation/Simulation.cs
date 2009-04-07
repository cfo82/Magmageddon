using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectMagma.Simulation.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
#if !XBOX
using xWinFormsLib;
#endif

using ProjectMagma.Simulation;
using ProjectMagma.Shared.LevelData;

namespace ProjectMagma.Simulation
{
    public class Simulation
    {
        public Simulation()
        {
            entityManager = new EntityManager();
            pillarManager = new EntityKindManager(entityManager, "pillar");
            islandManager = new EntityKindManager(entityManager, "island");
            playerManager = new EntityKindManager(entityManager, "player");
            powerupManager = new EntityKindManager(entityManager, "powerup");
            iceSpikeManager = new EntityKindManager(entityManager, "ice_spike");
            collisionManager = new CollisionManager();
        }

        public void Initialize(
            ContentManager content,
            String level,
            Entity[] players
        )
        {
            paused = false;

            // load level data
            LevelData levelData = content.Load<LevelData>(level);
            String[] models = new String[players.Length];
            for(int i = 0; i < models.Length; i++)
            {
                models[i] = players[i].GetString("robot_entity");
            }
            entityManager.Load(levelData, models, players);

            // load soundeffects
            foreach (Entity e in powerupManager)
            {
                content.Load<SoundEffect>("Sounds/" + e.GetString("pickup_sound"));
            }

            simTime = new SimulationTime();
        }

        public void Update(GameTime gameTime)
        {
            if (!paused)
            {
                // update simulation time
                simTime.Update();

                // update all entities
                foreach (Entity e in entityManager)
                {
                    e.OnUpdate(simTime);
                }

                // perform collision detection
                collisionManager.Update(simTime);

                // execute deferred add/remove orders on the entityManager
                entityManager.ExecuteDeferred();
            }
            else
                simTime.Pause();
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

        public CollisionManager CollisionManager
        {
            get
            {
                return collisionManager;
            }
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

        private readonly EntityManager entityManager;
        private EntityKindManager pillarManager;
        private EntityKindManager islandManager;
        private EntityKindManager playerManager;
        private EntityKindManager powerupManager;
        private EntityKindManager iceSpikeManager;
        private CollisionManager collisionManager;

        private SimulationTime simTime;
        private bool paused;
    }

    public class SimulationTime
    {
        private long lastTick = DateTime.Now.Ticks;

        private int frame = 0;

        private float at = 0;
        private float last = 0;

        private float dt = 0;
        private float dtMs = 0;

        /// <summary>
        /// the how manieth frame this is since simulation start
        /// </summary>
        public int Frame
        {
            get { return frame; }
        }

        /// <summary>
        /// current time in total milliseconds passed since simulation start
        /// </summary>
        public float At
        {
            get { return at; }
        }

        /// <summary>
        /// last time in total milliseconds passed since simulation start
        /// </summary>
        public float Last
        {
            get { return last; }
        }

        /// <summary>
        /// difference between last and current update in fraction of a second
        /// </summary>
        public float Dt
        {
            get { return dt; }
        }

        /// <summary>
        /// difference between last and current update in milliseconds
        /// </summary>
        public float DtMs
        {
            get { return dt; }
        }

        internal void Update()
        {
            // reset last
            last = at;

            // dt in milliseconds
            dtMs = (float)((DateTime.Now.Ticks - lastTick) / 10000d);

            // dt in seconds
            dt = dtMs / 1000f;

            // at is in milliseconds
            at += dtMs;

            // increase frame counter
            frame++;

            // reset lastTick
            lastTick = DateTime.Now.Ticks;
        }

        internal void Pause()
        {
            lastTick = DateTime.Now.Ticks;
        }

    }
}
