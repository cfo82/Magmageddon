using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
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

        public void OnAttached(AbstractEntity flame)
        {
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];
            this.flame = flame as Entity;
            this.player = Game.Instance.Simulation.EntityManager[flame.GetString("player")];

            player.GetVector3Attribute(CommonNames.Position).ValueChanged += PlayerPositionHandler;

            flame.GetBoolAttribute(CommonNames.Fueled).ValueChanged += FlameFuelChangeHandler;

            flame.GetProperty<CollisionProperty>("collision").OnContact += FlamethrowerCollisionHandler;

            (flame as Entity).Update += OnUpdate;
        }

        public void OnDetached(AbstractEntity flame)
        {
            (flame as Entity).Update -= OnUpdate;
            flame.GetProperty<CollisionProperty>("collision").OnContact -= FlamethrowerCollisionHandler;
            flame.GetBoolAttribute(CommonNames.Fueled).ValueChanged -= FlameFuelChangeHandler;
            player.GetVector3Attribute(CommonNames.Position).ValueChanged -= PlayerPositionHandler;
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
                    flame.SetVector3(CommonNames.Scale, flame.GetVector3("full_scale") * ((float)((at - flameThrowerStateChangedAt) / warmupTime)));
                    if (at >= flameThrowerStateChangedAt + flameThrowerWarmupDeducted * (warmupTime / warmupCost))
                    {
                        player.SetFloat(CommonNames.Energy, player.GetFloat(CommonNames.Energy) - 1);
                        flameThrowerWarmupDeducted++;
                    }
                }
                else
                {
                    player.SetFloat(CommonNames.Energy, player.GetFloat(CommonNames.Energy) - (warmupCost - flameThrowerWarmupDeducted));
                    flameThrowerState = FlameThrowerState.Active;
                    flameThrowerStateChangedAt = at;
                }
            }
            else
            if(flameThrowerState == FlameThrowerState.Active)
            {
                player.SetFloat(CommonNames.Energy, player.GetFloat(CommonNames.Energy) - Game.Instance.Simulation.Time.Dt * constants.GetFloat("flamethrower_energy_per_second"));
            }
            // else cooldown -> do nothing


            if(flameThrowerState == FlameThrowerState.Cooldown)
            {
                // cooldown
                int cooldownTime = constants.GetInt("flamethrower_cooldown_time");
                if (at < flameThrowerStateChangedAt + constants.GetInt("flamethrower_cooldown_time"))
                {
                    flame.SetVector3(CommonNames.Scale, flame.GetVector3("full_scale") * ((float)(1 - (at - flameThrowerStateChangedAt) / cooldownTime)));
                }
                else
                {
                    flameThrowerState = FlameThrowerState.InActive;
                    Game.Instance.Simulation.EntityManager.RemoveDeferred(flame);
                }
            }

            if (flameThrowerState == FlameThrowerState.Active)
            {
                flame.SetVector3(CommonNames.Scale, flame.GetVector3("full_scale") * scaleFactor);
                if (scaleFactor < 1)
                    scaleFactor += constants.GetFloat("flamethrower_turn_flame_increase") * dt;
                else
                    scaleFactor = 1;
            }
        }

        private void PlayerPositionHandler(Vector3Attribute sender, Vector3 oldValue, Vector3 newValue)
        {
            Vector3 position = flame.GetVector3(CommonNames.Position);
            Vector3 delta = newValue - oldValue;
            position += delta;
            flame.SetVector3(CommonNames.Position, position);
        }

        private void FlameFuelChangeHandler(BoolAttribute sender, bool oldValue, bool newValue)
        {
            // activate cooldown
            if (flameThrowerState == FlameThrowerState.Active)
                flameThrowerStateChangedAt = at;
            flameThrowerState = FlameThrowerState.Cooldown;
        }

        private void FlamethrowerCollisionHandler(SimulationTime simTime, Contact contact)
        {
            Entity other = contact.EntityB;

            if (other.HasProperty("burnable")
                && other != player) // don't burn self
            {
                other.SetFloat("burnt_at", simTime.At);
            }
        }

    }
}
