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
#if !XBOX && DEBUG
using xWinFormsLib;
#endif

using ProjectMagma.Renderer.Interface;
using ProjectMagma.Simulation;
using ProjectMagma.Shared.LevelData;
using ProjectMagma.Simulation.Attributes;
using System.Diagnostics;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Simulation
{
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
            iceSpikeManager = new EntityKindManager(entityManager, "ice_spike");
            collisionManager = new CollisionManager();
        }

        public RendererUpdateQueue Initialize(
            ContentManager content,
            String level
        )
        {
            currentUpdateQueue = new RendererUpdateQueue();

            paused = false;

            // load level data
            levelData = content.Load<LevelData>(level);
            entityManager.Load(levelData);

            simTime = new SimulationTime();

            RendererUpdateQueue returnValue = currentUpdateQueue;
            currentUpdateQueue = null;
            return returnValue;
        }

        public RendererUpdateQueue AddPlayers(Entity[] players)
        {
            currentUpdateQueue = new RendererUpdateQueue();

            String[] models = new String[players.Length];
            for(int i = 0; i < models.Length; i++)
            {
                models[i] = players[i].GetString("robot_entity");
            }
            entityManager.AddEntities(levelData, models, players);

            RendererUpdateQueue returnValue = currentUpdateQueue;
            currentUpdateQueue = null;
            return returnValue;
        }

        public void Close()
        {
            entityManager.Clear();
            collisionManager.Close();
        }

        public RendererUpdateQueue Update()
        {
            Game.Instance.Profiler.BeginSection("simulation_update");
            currentUpdateQueue = new RendererUpdateQueue();

            // pause simulation if explicitly paused or app changed
            if (!paused && Game.Instance.IsActive)
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
            {
                simTime.Pause();
            }

            Game.Instance.Profiler.EndSection("simulation_update");

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

        #region interval execution functionality

        private readonly Dictionary<string, float> appliedAt = new Dictionary<String, float>();

        public void ApplyPerSecondAddition(Entity source, String identifier, int perSecond, ref int value)
        {
            float interval = 1000f / perSecond;
            ApplyIntervalAddition(source, identifier, interval, ref value);
        }

        public void ApplyPerSecondAddition(Entity source, String identifier, int perSecond, IntAttribute attr)
        {
            float interval = 1000f / perSecond;
            ApplyIntervalAddition(source, identifier, interval, attr);
        }

        public void ApplyPerSecondSubstractrion(Entity source, String identifier, int perSecond, ref int value)
        {
            float interval = 1000f / perSecond;
            ApplyIntervalSubstraction(source, identifier, interval, ref value);
        }

        public void ApplyPerSecondSubstraction(Entity source, String identifier, int perSecond, IntAttribute attr)
        {
            float interval = 1000f / perSecond;
            ApplyIntervalSubstraction(source, identifier, interval, attr);
        }

        public void ApplyIntervalAddition(Entity source, String identifier, float interval, IntAttribute attr)
        {
            int val = attr.Value;
            ApplyIntervalAddition(source, identifier, interval, ref val);
            attr.Value = val;
        }

        public void ApplyIntervalAddition(Entity source, String identifier, float interval, ref int value)
        {
            int val = value;
            ExecuteAtInterval(source, identifier, interval, delegate(int diff) { val += diff; });
            value = val;
        }

        public void ApplyIntervalSubstraction(Entity source, String identifier, float interval, IntAttribute attr)
        {
            int val = attr.Value;
            ApplyIntervalSubstraction(source, identifier, interval, ref val);
            attr.Value = val;
        }

        public void ApplyIntervalSubstraction(Entity source, String identifier, float interval, ref int value)
        {
            int val = value;
            ExecuteAtInterval(source, identifier, interval, delegate(int diff) { val -= diff; });
            value = val;
        }

        public void ExecuteAtInterval(Entity source, String identifier, float interval, IntervalExecutionAction action)
        {
            String fullIdentifier = source.Name + "_" + identifier;
            float current = Time.At;

            // if appliedAt doesn't contain string this is first time we called this functin
            if (!appliedAt.ContainsKey(fullIdentifier))
            {
                appliedAt.Add(fullIdentifier, current);
                return;
            }

            float last = appliedAt[fullIdentifier];
            float nextUpdateTime = last + interval;
            // if we didnt adapt on last update, then there was no call to this method at that time
            // so we reset our time to current
            if (Time.Last >= nextUpdateTime)
            {
                appliedAt[fullIdentifier] = current;
                return;
            }

            // do we have to update yet?
            if (current >= nextUpdateTime)
            {
                // calculate how many updates would have happened in between
                int times = (int)((current - last) / interval);

                // execute action
                action(times);

                // update time
                appliedAt[fullIdentifier] = last + times * interval;
            }
        }
        #endregion

        #region collision pushback

        public static void ApplyPushback(ref Vector3 position, ref Vector3 pushbackVelocity, float deacceleration)
        {
            ApplyPushback(ref position, ref pushbackVelocity, deacceleration, delegate { });
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

        private readonly EntityManager entityManager;
        private EntityKindManager pillarManager;
        private EntityKindManager islandManager;
        private EntityKindManager playerManager;
        private EntityKindManager powerupManager;
        private EntityKindManager iceSpikeManager;
        private CollisionManager collisionManager;

        private SimulationTime simTime;
        private bool paused;

        private RendererUpdateQueue currentUpdateQueue;
    }
}
