using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.BoundingVolume;
using ProjectMagma.Collision;

namespace ProjectMagma.Framework
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

        private Dictionary<string, double> entityHitAt;

        enum FlameThrowerState
        {
            InActive, Warmup, Active, Cooldown
        }


        public FlamethrowerControllerProperty()
        {
        }

        public void OnAttached(Entity flame)
        {
            this.constants = Game.Instance.EntityManager["player_constants"];
            this.flame = flame;
            this.player = Game.Instance.EntityManager[flame.GetString("player")];

            player.GetVector3Attribute("position").ValueChanged += new Vector3ChangeHandler(playerPositionHandler);
            player.GetQuaternionAttribute("rotation").ValueChanged += new QuaternionChangeEventHandler(playerRotationHandler);

            flame.AddBoolAttribute("fueled", true);
            flame.GetBoolAttribute("fueled").ValueChanged += new BoolChangeHandler(flameFuelChangeHandler);

            entityHitAt = new Dictionary<string, double>(Game.Instance.EntityManager.Count<Entity>());

            ((CollisionProperty)flame.GetProperty("collision")).OnContact += new ContactHandler(FlamethrowerCollisionHandler);

            flame.Update += OnUpdate;
        }

        public void OnDetached(Entity flame)
        {
            flame.Update -= OnUpdate;
            ((CollisionProperty)flame.GetProperty("collision")).OnContact -= new ContactHandler(FlamethrowerCollisionHandler);
        }

        private void OnUpdate(Entity flame, GameTime gameTime)
        {
            float dt = ((float)gameTime.ElapsedGameTime.Milliseconds)/1000.0f;
            at = gameTime.TotalGameTime.TotalMilliseconds;

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
                int energy = player.GetInt("energy") ;
                flameThrowerStateChangedAt = Game.ApplyPerSecond(at, flameThrowerStateChangedAt, constants.GetInt("flamethrower_energy_per_second"),
                    ref energy);
                player.SetInt("energy", energy);
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
                    Game.Instance.EntityManager.RemoveDeferred(flame);
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

            if (flameThrowerState == FlameThrowerState.Active)
                scaleFactor = constants.GetFloat("flamethrower_turn_scale");
        }

        private void flameFuelChangeHandler(BoolAttribute sender, bool oldValue, bool newValue)
        {
            // activate cooldown
            if (flameThrowerState == FlameThrowerState.Active)
                flameThrowerStateChangedAt = at;
            flameThrowerState = FlameThrowerState.Cooldown;
        }

        private void FlamethrowerCollisionHandler(GameTime gameTime, Contact c)
        {
            Entity other = c.entityB;

            if (other.HasAttribute("health"))
            {
                if (!entityHitAt.ContainsKey(other.Name))
                    entityHitAt[other.Name] = at;
                else
                {
                    int health = other.GetInt("health");
                    entityHitAt[other.Name] = Game.ApplyPerSecond(at, entityHitAt[other.Name],
                        constants.GetInt("flamethrower_damage_per_second"), ref health);
                    player.SetInt("health", health);
                }

                if (other.GetString("kind") == "player")
                    player.SetInt("frozen", 0);
            }
        }

    }
}
