﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ProjectMagma.Simulation.Collision;

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

            entity.Update += OnUpdate;
        }

        public void OnDetached(Entity entity)
        {
            ((CollisionProperty)entity.GetProperty("collision")).OnContact -= IceSpikeCollisionHandler;

            entity.Update -= OnUpdate;
        }

        private void OnUpdate(Entity iceSpike, SimulationTime simTime)
        {
            int iat = (int)simTime.At;

            // fetch required values
            Vector3 pos = iceSpike.GetVector3("position");
            Vector3 v = iceSpike.GetVector3("velocity");
            float dt = simTime.Dt;

            // accumulate forces
            Vector3 a = constants.GetVector3("ice_spike_gravity_acceleration");

            string targetPlayerName = iceSpike.GetString("target_player");
            if (targetPlayerName != "")
            {
                // incorporate homing effect towards targeted player
                Entity targetPlayer = Game.Instance.Simulation.PlayerManager[0];
                foreach (Entity e in Game.Instance.Simulation.PlayerManager)
                {
                    if (e.Name == targetPlayerName)
                    {
                        targetPlayer = e;
                        break;
                    }
                }
                Vector3 targetPlayerPos = targetPlayer.GetVector3("position");
                Vector3 diff = targetPlayerPos - pos;
                diff.Normalize();
                a += diff * constants.GetFloat("ice_spike_homing_acceleration");
            }
            else
            {
                // incorporate uniform acceleration
                Vector3 a_uniform = v;
                a_uniform.Normalize();
                a += a_uniform * constants.GetFloat("ice_spike_uniform_acceleration");
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

        //private Entity target;

    }
}
