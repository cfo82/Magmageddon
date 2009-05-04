using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ProjectMagma.Simulation.Collision;
using System;

namespace ProjectMagma.Simulation
{
    public class IceSpikeControllerProperty : Property
    {
        private Entity constants;

        public IceSpikeControllerProperty()
        {
        }

        public void OnAttached(Entity entity)
        {
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];

            ((CollisionProperty)entity.GetProperty("collision")).OnContact += IceSpikeCollisionHandler;

            string targetPlayerName = entity.GetString("target_player");
            if (targetPlayerName != "")
            {
                targetPlayer = Game.Instance.Simulation.EntityManager[targetPlayerName];
            }
            else
                targetPlayer = null;

            shootingPlayer = Game.Instance.Simulation.EntityManager[entity.GetString("player")];

            createdAt = Game.Instance.Simulation.Time.At;

            entity.Update += OnUpdate;
        }

        private float createdAt;

        public void OnDetached(Entity entity)
        {
            ((CollisionProperty)entity.GetProperty("collision")).OnContact -= IceSpikeCollisionHandler;

            entity.Update -= OnUpdate;
        }

        private void OnUpdate(Entity iceSpike, SimulationTime simTime)
        {
            if (diedAt > 0)
            {
                if (simTime.At > diedAt + constants.GetInt("ice_spike_death_timeout"))
                {
                    Game.Instance.Simulation.EntityManager.RemoveDeferred(iceSpike);
                }
                return;
            }

            // fetch required values
            Vector3 pos = iceSpike.GetVector3("position");
            Vector3 v = iceSpike.GetVector3("velocity");
            float dt = simTime.Dt;

            // accumulate forces
            Vector3 a = constants.GetVector3("ice_spike_gravity_acceleration");

            // in targeting mode (yet)?
            if (targetPlayer != null
                && simTime.At > createdAt + constants.GetInt("ice_spike_rising_time"))
            {
                // incorporate homing effect towards targeted player
                Vector3 targetPlayerPos = targetPlayer.GetVector3("position");

                // deaccelerate a bit if too fast
                if (v.Length() > constants.GetFloat("ice_spike_max_speed"))
                {
                    a = -Vector3.Normalize(v) * constants.GetFloat("ice_spike_homing_acceleration");
                }
                else
                {
                    // get acceleration to direction
                    Vector3 dir = Vector3.Normalize(targetPlayerPos - pos);
                    float acc = constants.GetFloat("ice_spike_homing_acceleration");
                    a += dir * acc;
                    a.Y *= 0.6f; // don't accelerate as fast on y axis

                    // and add forces for islands
                    float pillarForceRadius = constants.GetFloat("ice_spike_pillar_force_radius");
                    foreach (Entity island in Game.Instance.Simulation.IslandManager)
                    {
                        // island target player is standing on has no force
                        if (targetPlayer.GetString("active_island") == island.Name)
                            continue;

                        Vector3 idir = island.GetVector3("position") - pos;
                        float dist = idir.Length();
                        idir.Normalize();
                        Vector3 ia = -idir * acc * (pillarForceRadius * pillarForceRadius / dist / dist);
                        a += ia;
                    }
                    // and pillars
                    float islandForceRadius = constants.GetFloat("ice_spike_island_force_radius");
                    foreach (Entity island in Game.Instance.Simulation.PillarManager)
                    {
                        Vector3 idir = island.GetVector3("position") - pos;
                        idir.Y = 0;
                        float dist = idir.Length();
                        idir.Normalize();
                        Vector3 ia = -idir * acc * (islandForceRadius * islandForceRadius / dist / dist);
                        a += ia;
                    }
                }
            }

            // integrate
            v += a * dt;
            pos += v * dt;

            // check lifetime
            if(iceSpike.GetInt("creation_time") + constants.GetInt("ice_spike_lifetime") < simTime.At)
            {
                SetDead(iceSpike, simTime);
                return;
            }

            // remove if in lava
            if (pos.Y < Game.Instance.Simulation.EntityManager["lava"].GetVector3("position").Y - 20)
            {
                SetDead(iceSpike, simTime);
                return;
            }

            // store computed values;
            iceSpike.SetVector3("position", pos);
            iceSpike.SetVector3("velocity", v);
        }

        private void IceSpikeCollisionHandler(SimulationTime simTime, Contact contact)
        {
            Entity iceSpike = contact.EntityA;
            Entity other = contact.EntityB;
            // ignore collision with island player is standing on during warmup phae
            if (simTime.At < createdAt + constants.GetInt("ice_spike_rising_time")
                && other.Name == shootingPlayer.GetString("active_island"))
            {
                return;
            }

            // remove spike
            if (other != shootingPlayer) // dont collide with shooter
            {
                if (other.HasAttribute("kind") && other.GetString("kind") == "player")
                    IceSpikePlayerCollisionHandler(simTime, iceSpike, other);
                SetDead(iceSpike, simTime);
            }
        }

        private void SetDead(Entity iceSpike, SimulationTime simTime)
        {
            diedAt = simTime.At;
            iceSpike.SetBool("dead", true);
            ((CollisionProperty)iceSpike.GetProperty("collision")).OnContact -= IceSpikeCollisionHandler;

            // add explosion
            Entity iceSpikeExplosion = new Entity(iceSpike.Name+"_explosion");
            iceSpikeExplosion.AddStringAttribute("player", iceSpike.GetString("player"));

            // todo: constant
            iceSpikeExplosion.AddIntAttribute("live_span", 2000);
            iceSpikeExplosion.AddIntAttribute("damage", constants.GetInt("ice_spike_damage"));
            iceSpikeExplosion.AddIntAttribute("freeze_time", constants.GetInt("ice_spike_freeze_time"));

            iceSpikeExplosion.AddVector3Attribute("position", iceSpike.GetVector3("position"));

            iceSpikeExplosion.AddStringAttribute("mesh", "Models/Sfx/icespike_explosion");
            iceSpikeExplosion.AddVector3Attribute("scale", new Vector3(20, 20, 20));

            iceSpikeExplosion.AddStringAttribute("bv_type", "sphere");

            // todo: change this
            iceSpikeExplosion.AddProperty("render", new BasicRenderProperty());
            iceSpikeExplosion.AddProperty("collision", new CollisionProperty());
            iceSpikeExplosion.AddProperty("controller", new ExplosionControllerProperty());

            Game.Instance.Simulation.EntityManager.AddDeferred(iceSpikeExplosion);
        }

        private void IceSpikePlayerCollisionHandler(SimulationTime simTime, Entity iceSpike, Entity player)
        {
            // indicate hit
            SoundEffect soundEffect = Game.Instance.ContentManager.Load<SoundEffect>("Sounds/sword-clash");
            soundEffect.Play(Game.Instance.EffectsVolume);
        }

        private Vector3 GetPosition(Entity entity)
        {
            return entity.GetVector3("position");
        }

        private Vector3 GetScale(Entity entity)
        {
            if (entity.HasVector3("scale"))
            {
                return entity.GetVector3("scale");
            }
            else
            {
                return Vector3.One;
            }
        }

        private Quaternion GetRotation(Entity entity)
        {
            if (entity.HasQuaternion("rotation"))
            {
                return entity.GetQuaternion("rotation");
            }
            else
            {
                return Quaternion.Identity;
            }
        }

        private Entity shootingPlayer;
        private Entity targetPlayer;
        private double diedAt = 0;
    }
}
