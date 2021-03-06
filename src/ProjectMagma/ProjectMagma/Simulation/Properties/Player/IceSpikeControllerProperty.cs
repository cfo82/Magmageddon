﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ProjectMagma.Simulation.Collision;
using System;
using ProjectMagma.Shared.LevelData;
using ProjectMagma.Framework;

namespace ProjectMagma.Simulation
{
    public class IceSpikeControllerProperty : Property
    {
        private Entity constants;
        private LevelData templates;

        public IceSpikeControllerProperty()
        {
        }

        public override void OnAttached(AbstractEntity entity)
        {
            this.iceSpike = entity as Entity;
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];
            this.templates = Game.Instance.ContentManager.Load<LevelData>("Level/Common/DynamicTemplates");

            entity.GetProperty<CollisionProperty>("collision").OnContact += IceSpikeCollisionHandler;

            string targetPlayerName = entity.GetString("target_player");
            if (targetPlayerName != "")
            {
                targetPlayer = Game.Instance.Simulation.EntityManager[targetPlayerName];
                Game.Instance.Simulation.EntityManager.EntityRemoved += OnEntityRemoved;
            }
            else
                targetPlayer = null;

            shootingPlayer = Game.Instance.Simulation.EntityManager[entity.GetString("player")];

            createdAt = Game.Instance.Simulation.Time.At;

            (entity as Entity).OnUpdate += OnUpdate;
        }

        private float createdAt;

        public override void OnDetached(AbstractEntity entity)
        {
            entity.GetProperty<CollisionProperty>("collision").OnContact -= IceSpikeCollisionHandler;
            Game.Instance.Simulation.EntityManager.EntityRemoved -= OnEntityRemoved; 
            (entity as Entity).OnUpdate -= OnUpdate;
        }

        private void OnUpdate(Entity iceSpike, SimulationTime simTime)
        {
            if (state == IceSpikeState.Exploded)
            {
                if (simTime.At > explodedAt + constants.GetInt("ice_spike_death_timeout"))
                {
                    Game.Instance.Simulation.EntityManager.RemoveDeferred(iceSpike);
                }
                return;
            }

            // fetch required values
            Vector3 pos = iceSpike.GetVector3(CommonNames.Position);
            Vector3 v = iceSpike.GetVector3(CommonNames.Velocity);
            float dt = simTime.Dt;

            // accumulate forces
            Vector3 a = constants.GetVector3("ice_spike_gravity_acceleration");

            // in targeting mode (yet)?
            if (state == IceSpikeState.Rising
                && simTime.At > createdAt + constants.GetInt("ice_spike_rising_time"))
            {
                state = IceSpikeState.Targeting;
            }

            // perform targetting logic
            if (state == IceSpikeState.Targeting)
            {
                if (targetPlayer != null
                    && targetPlayer.GetFloat(CommonNames.Health) <= 0)
                {
                    AbortPlayerTargeting();
                }

                Vector3 dir;
                if (targetPlayer != null)
                {
                    // incorporate homing effect towards targeted player
                    Vector3 targetPlayerPos = targetPlayer.GetVector3(CommonNames.Position);
                    dir = Vector3.Normalize(targetPlayerPos - pos);
                }
                else
                {
                    // just accelerate in targeting direction
                    dir = iceSpike.GetVector3("target_direction");
                }

                // deaccelerate a bit if too fast
                if (v.Length() > constants.GetFloat("ice_spike_max_speed"))
                {
                    a = -Vector3.Normalize(v) * constants.GetFloat("ice_spike_homing_acceleration");
                }
                else
                {
                    // get acceleration to direction
                    float acc = constants.GetFloat("ice_spike_homing_acceleration");
                    a += dir * acc;
                    a.Y *= 0.6f; // don't accelerate as fast on y axis

                    // and add forces for islands
                    float islandForceRadius = constants.GetFloat("ice_spike_island_force_radius");
                    foreach (Entity island in Game.Instance.Simulation.IslandManager)
                    {
                        // island target player is standing on has no force
                        if (targetPlayer != null
                            && targetPlayer.GetString("active_island") == island.Name)
                            continue;

                        Vector3 idir = island.GetVector3(CommonNames.Position) - pos;
                        float dist = idir.Length();
                        idir.Normalize();
                        Vector3 ia = -idir * acc * (islandForceRadius * islandForceRadius / dist / dist);
                        a += ia;
                    }
                    // and pillars
                    float pillarForceRadius = constants.GetFloat("ice_spike_pillar_force_radius");
                    foreach (Entity island in Game.Instance.Simulation.PillarManager)
                    {
                        Vector3 idir = island.GetVector3(CommonNames.Position) - pos;
                        idir.Y = 0;
                        float dist = idir.Length();
                        idir.Normalize();
                        Vector3 ia = -idir * acc * (pillarForceRadius * pillarForceRadius / dist / dist);
                        a += ia;
                    }
                }
            }

            // integrate
            v += a * dt;
            pos += v * dt;

            // check lifetime
            if(state == IceSpikeState.Targeting
                && iceSpike.GetInt("creation_time") + constants.GetInt("ice_spike_lifetime") < simTime.At)
            {
                SetDead(iceSpike, simTime);
                return;
            }

            // remove if in lava
            if (pos.Y < Game.Instance.Simulation.EntityManager["lava"].GetVector3(CommonNames.Position).Y - 20)
            {
                SetDead(iceSpike, simTime);
                return;
            }

            // store computed values;
            iceSpike.SetVector3(CommonNames.Position, pos);
            iceSpike.SetVector3(CommonNames.Velocity, v);
        }

        private void IceSpikeCollisionHandler(SimulationTime simTime, Contact contact)
        {
            Entity iceSpike = contact.EntityA;
            Entity other = contact.EntityB;
       
            // ignore collision with island player is standing on during warmup phase
            if (simTime.At < createdAt + constants.GetInt("ice_spike_rising_time")
                && shootingPlayer.HasAttribute("active_island")
                && other.Name == shootingPlayer.GetString("active_island"))
            {
                return;
            }

            // remove spike
            if (other != shootingPlayer // dont collide with shooter
                && !other.HasAttribute("dynamic") // don't collide with moving other spikes, explosions, etc.
                ) // don't collide with other explosions
            {
                if (other.GetString(CommonNames.Kind) == "player")
                    IceSpikePlayerCollisionHandler(simTime, iceSpike, other);
                SetDead(iceSpike, simTime);
            }
        }

        private void SetDead(Entity iceSpike, SimulationTime simTime)
        {
            iceSpike.SetBool(CommonNames.Dead, true);
            iceSpike.GetProperty<CollisionProperty>("collision").OnContact -= IceSpikeCollisionHandler;

            // make boom
            Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.IceSpikeExplosionOnEnvironment);

            // add explosion
            Entity iceSpikeExplosion = new Entity(iceSpike.Name+"_explosion");
            iceSpikeExplosion.AddStringAttribute("player", iceSpike.GetString("player"));

            // only do damage when in targeting mode
            if (state == IceSpikeState.Targeting)
            {
                iceSpikeExplosion.AddFloatAttribute("damage", constants.GetFloat("ice_spike_damage"));
                iceSpikeExplosion.AddIntAttribute("freeze_time", constants.GetInt("ice_spike_freeze_time"));
            }

            iceSpikeExplosion.AddVector3Attribute(CommonNames.Position, iceSpike.GetVector3(CommonNames.Position));

            Game.Instance.Simulation.EntityManager.AddDeferred(iceSpikeExplosion, "ice_spike_explosion_base", templates);

            explodedAt = simTime.At;
            state = IceSpikeState.Exploded;
        }

        private void IceSpikePlayerCollisionHandler(SimulationTime simTime, Entity iceSpike, Entity player)
        {
            // indicate hit
            Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.IceSpikeExplosionOnPlayer);
        }

        private void OnEntityRemoved(AbstractEntityManager<Entity> manager, Entity entity)
        {
            if (entity == targetPlayer)
            {
                AbortPlayerTargeting();
            }
        }

        private void AbortPlayerTargeting()
        {
            targetPlayer = null;
            Vector3 dir = iceSpike.GetVector3(CommonNames.Velocity);
            if (dir != Vector3.Zero)
                dir.Normalize();
            else
                dir = new Vector3(0, -1, 0);
            iceSpike.AddVector3Attribute("target_direction", dir);
        }

        private Vector3 GetPosition(Entity entity)
        {
            return entity.GetVector3(CommonNames.Position);
        }

        private Vector3 GetScale(Entity entity)
        {
            if (entity.HasVector3(CommonNames.Scale))
            {
                return entity.GetVector3(CommonNames.Scale);
            }
            else
            {
                return Vector3.One;
            }
        }

        private Quaternion GetRotation(Entity entity)
        {
            if (entity.HasQuaternion(CommonNames.Rotation))
            {
                return entity.GetQuaternion(CommonNames.Rotation);
            }
            else
            {
                return Quaternion.Identity;
            }
        }

        private Entity iceSpike;
        private Entity shootingPlayer;
        private Entity targetPlayer;
        private double explodedAt = 0;
        private IceSpikeState state = IceSpikeState.Rising;

        private enum IceSpikeState
        {
            Rising,
            Targeting,
            Exploded
        }
    }
}
