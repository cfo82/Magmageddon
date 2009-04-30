using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Simulation.Collision;
using System;

namespace ProjectMagma.Simulation
{
    public class FlamethrowerControllerProperty : Property
    {
        private Entity constants;
        private Entity player;
        private Entity flame;

        private float scaleFactor = 1;
        private double at;

        FlameThrowerState flameThrowerState = FlameThrowerState.InActive;
        private double flameThrowerStateChangedAt = 0;
        private int flameThrowerWarmupDeducted = 0;

        enum FlameThrowerState
        {
            InActive, Warmup, Active, Cooldown
        }


        public FlamethrowerControllerProperty()
        {
        }

        public void OnAttached(Entity flame)
        {
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];
            this.flame = flame;
            this.player = Game.Instance.Simulation.EntityManager[flame.GetString("player")];

            player.GetVector3Attribute("position").ValueChanged += playerPositionHandler;
            player.GetQuaternionAttribute("rotation").ValueChanged += playerRotationHandler;

            flame.AddBoolAttribute("fueled", true);
            flame.GetBoolAttribute("fueled").ValueChanged += flameFuelChangeHandler;

            ((CollisionProperty)flame.GetProperty("collision")).OnContact += FlamethrowerCollisionHandler;

            flame.Update += OnUpdate;
        }

        public void OnDetached(Entity flame)
        {
            flame.Update -= OnUpdate;
            ((CollisionProperty)flame.GetProperty("collision")).OnContact -= FlamethrowerCollisionHandler;
            flame.GetBoolAttribute("fueled").ValueChanged -= flameFuelChangeHandler;
            player.GetQuaternionAttribute("rotation").ValueChanged -= playerRotationHandler;
            player.GetVector3Attribute("position").ValueChanged -= playerPositionHandler;
        }

        private void OnUpdate(Entity flame, SimulationTime simTime)
        {
            float dt = simTime.Dt;
            at = simTime.At;

            if (flameThrowerState == FlameThrowerState.InActive)
            {
                flameThrowerStateChangedAt = at;
                flameThrowerState = FlameThrowerState.Warmup;
            }
            
            if(flameThrowerState == FlameThrowerState.Warmup)
            {
                int warmupTime = constants.GetInt("flamethrower_warmup_time");
                int warmupCost = constants.GetInt("flamethrower_warmup_energy_cost");
                if (at < flameThrowerStateChangedAt + warmupTime)
                {
                    flame.SetVector3("scale", flame.GetVector3("full_scale") * ((float)((at - flameThrowerStateChangedAt) / warmupTime)));
                    if (at >= flameThrowerStateChangedAt + flameThrowerWarmupDeducted * (warmupTime / warmupCost))
                    {
                        player.SetInt("energy", player.GetInt("energy") - 1);
                        flameThrowerWarmupDeducted++;
                    }
                }
                else
                {
                    player.SetInt("energy", player.GetInt("energy") - (warmupCost-flameThrowerWarmupDeducted));
                    flame.SetBool("active", true);
                    flameThrowerState = FlameThrowerState.Active;
                    flameThrowerStateChangedAt = at;
                }
            }
            else
            if(flameThrowerState == FlameThrowerState.Active)
            {
                Game.Instance.Simulation.ApplyPerSecondSubstraction(flame, "energy_deducation", constants.GetInt("flamethrower_energy_per_second"),
                    player.GetIntAttribute("energy"));
            }
            // else cooldown -> do nothing


            if(flameThrowerState == FlameThrowerState.Cooldown)
            {
                // cooldown
                flame.SetBool("active", false);
                int cooldownTime = constants.GetInt("flamethrower_cooldown_time");
                if (at < flameThrowerStateChangedAt + constants.GetInt("flamethrower_cooldown_time"))
                {
                    flame.SetVector3("scale", flame.GetVector3("full_scale") * ((float)(1 - (at - flameThrowerStateChangedAt) / cooldownTime)));
                }
                else
                {
                    flameThrowerState = FlameThrowerState.InActive;
                    Game.Instance.Simulation.EntityManager.RemoveDeferred(flame);
                }
            }

            if (flameThrowerState == FlameThrowerState.Active)
            {
                flame.SetVector3("scale", flame.GetVector3("full_scale") * scaleFactor);
                if (scaleFactor < 1)
                    scaleFactor += constants.GetFloat("flamethrower_turn_flame_increase") * dt;
                else
                    scaleFactor = 1;
            }
        }

        private void playerPositionHandler(Vector3Attribute sender, Vector3 oldValue, Vector3 newValue)
        {
            Vector3 position = flame.GetVector3("position");
            Vector3 delta = newValue - oldValue;
            position += delta;
            flame.SetVector3("position", position);
        }

        private void playerRotationHandler(QuaternionAttribute sender, Quaternion oldValue, Quaternion newValue)
        {
            flame.SetQuaternion("rotation", newValue);

//            if (flameThrowerState == FlameThrowerState.Active)
//                scaleFactor = constants.GetFloat("flamethrower_turn_scale");
        }

        private void flameFuelChangeHandler(BoolAttribute sender, bool oldValue, bool newValue)
        {
            // activate cooldown
            if (flameThrowerState == FlameThrowerState.Active)
                flameThrowerStateChangedAt = at;
            flameThrowerState = FlameThrowerState.Cooldown;
        }

        private void FlamethrowerCollisionHandler(SimulationTime simTime, Contact contact)
        {
            Entity other = contact.EntityB;

            if (other.HasAttribute("health"))
            {
                Game.Instance.Simulation.ApplyPerSecondSubstraction(flame, other.Name + "_flamethrower_damage", constants.GetInt("flamethrower_damage_per_second"),
                    other.GetIntAttribute("health"));
                if (other.GetString("kind") == "player")
                    player.SetInt("frozen", 0);
            }
        }

    }
}
