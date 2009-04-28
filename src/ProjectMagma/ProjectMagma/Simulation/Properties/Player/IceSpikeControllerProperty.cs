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

        private float origDist;

        public void OnAttached(Entity entity)
        {
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];

            ((CollisionProperty)entity.GetProperty("collision")).OnContact += IceSpikeCollisionHandler;

            string targetPlayerName = entity.GetString("target_player");
            if (targetPlayerName != "")
            {
                targetPlayer = Game.Instance.Simulation.EntityManager[targetPlayerName];
                origDist = (targetPlayer.GetVector3("position") - entity.GetVector3("position")).Length();
            }
            else
                targetPlayer = null;

            entity.Update += OnUpdate;
        }

        public void OnDetached(Entity entity)
        {
            ((CollisionProperty)entity.GetProperty("collision")).OnContact -= IceSpikeCollisionHandler;

            entity.Update -= OnUpdate;
        }

        private float correctedAt = 0;

        private void OnUpdate(Entity iceSpike, SimulationTime simTime)
        {
            int iat = (int)simTime.At;

            // fetch required values
            Vector3 pos = iceSpike.GetVector3("position");
            Vector3 v = iceSpike.GetVector3("velocity");
            float dt = simTime.Dt;

            // accumulate forces
            Vector3 a = constants.GetVector3("ice_spike_gravity_acceleration");

            if (targetPlayer != null)
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
                    a += dir * constants.GetFloat("ice_spike_homing_acceleration");// *(1 - factor);
                    a.Y *= 0.6f; // don't accelerate as fast on y axis
                }

                correctedAt = simTime.At;
            }
            else
            {
                /*
                // incorporate uniform acceleration
                Vector3 a_uniform = v;
                a_uniform.Normalize();
                a += a_uniform * constants.GetFloat("ice_spike_uniform_acceleration");
                 */
            }

            // integrate
            v += a * dt;
            pos += v * dt;

            // check lifetime
            if(iceSpike.GetInt("creation_time") + constants.GetInt("ice_spike_lifetime") < iat)
            {
                Game.Instance.Simulation.EntityManager.RemoveDeferred(iceSpike);
                return;
            }

            // remove if in lava
            if (pos.Y < Game.Instance.Simulation.EntityManager["lava"].GetVector3("position").Y)
            {
                Game.Instance.Simulation.EntityManager.RemoveDeferred(iceSpike);
                return;
            }

            // store computed values;
            iceSpike.SetVector3("position", pos);
            iceSpike.SetVector3("velocity", v);
        }

        private void IceSpikeCollisionHandler(SimulationTime simTime, Contact contact)
        {
            // remove spike
            Entity iceSpike = contact.EntityA;
            Entity other = contact.EntityB;
            if (!(other.Name == iceSpike.Name) // dont collide with self
                && !(other.Name == iceSpike.GetString("player"))) // dont collide with shooter
            {
                if (other.HasAttribute("kind") && other.GetString("kind") == "player")
                    IceSpikePlayerCollisionHandler(simTime, iceSpike, other);
                Game.Instance.Simulation.EntityManager.RemoveDeferred(iceSpike);
            }
        }

        private void IceSpikePlayerCollisionHandler(SimulationTime simTime, Entity iceSpike, Entity player)
        {
            // indicate hit
            SoundEffect soundEffect = Game.Instance.Content.Load<SoundEffect>("Sounds/sword-clash");
            soundEffect.Play(Game.Instance.EffectsVolume);

            // buja
            player.SetInt("health", player.GetInt("health") - constants.GetInt("ice_spike_damage"));
            player.SetInt("frozen", player.GetInt("frozen") + constants.GetInt("ice_spike_freeze_time"));
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

        private Entity targetPlayer;
    }
}
