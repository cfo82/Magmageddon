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

        public void Load(
            ContentManager content
        )
        {
            lastUpdateAt = 0.0;
            paused = false;

            // load level data
            LevelData levelData = content.Load<LevelData>("Level/TestLevel");
            entityManager.Load(levelData);

            int gi = 0;
            foreach (Entity e in playerManager)
            {
                e.AddIntAttribute("game_pad_index", gi++);
            }
            foreach (Entity e in powerupManager)
            {
                content.Load<SoundEffect>("Sounds/" + e.GetString("pickup_sound"));
            }
        }

        public void Update(GameTime gameTime)
        {
            currentGameTime = gameTime;
            
            if (!paused)
            {
                // update all entities
                foreach (Entity e in entityManager)
                {
                    e.OnUpdate(gameTime);
                }

                // perform collision detection
                collisionManager.Update(gameTime);

                // execute deferred add/remove orders on the entityManager
                entityManager.ExecuteDeferred();
            }

            // set lastupdate time
            lastUpdateAt = gameTime.TotalGameTime.TotalMilliseconds;
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

        public GameTime CurrentGameTime
        {
            get { return currentGameTime; }
        }

        public bool Paused
        {
            get { return paused; }
        }

        public double LastUpdateAt
        {
            get { return lastUpdateAt; }
        }

        private EntityManager entityManager;
        private EntityKindManager pillarManager;
        private EntityKindManager islandManager;
        private EntityKindManager playerManager;
        private EntityKindManager powerupManager;
        private EntityKindManager iceSpikeManager;
        private CollisionManager collisionManager;
        private GameTime currentGameTime;
        private double lastUpdateAt;
        private bool paused;
    }
}
