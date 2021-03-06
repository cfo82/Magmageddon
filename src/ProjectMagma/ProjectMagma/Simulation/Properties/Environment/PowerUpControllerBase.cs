﻿using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using ProjectMagma.Simulation.Collision;
using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
using Microsoft.Xna.Framework.Audio;

namespace ProjectMagma.Simulation
{
    public abstract class PowerUpControllerBase : Property
    {
        private Random rand;

        public PowerUpControllerBase()
        {
        }

        public override void OnAttached(
            AbstractEntity entity
        )
        {
            this.powerup = entity as Entity;
            this.constants = Game.Instance.Simulation.EntityManager["powerup_constants"];
            this.island = Game.Instance.Simulation.EntityManager[entity.GetString("island_reference")];

            rand = new Random(island.GetHashCode());

            // initialize properties
            Debug.Assert(this.powerup.HasVector3("relative_position"), "must have a relative translation attribute");
            if (!this.powerup.HasAttribute(CommonNames.Position))
            {
                this.powerup.AddVector3Attribute(CommonNames.Position, Vector3.Zero);
            }

            Debug.Assert(powerup.HasBool("fixed"));

            // get position on surface
            powerup.AddFloatAttribute("surface_offset", 0f);
            CalculateSurfaceOffset();
            Vector3 islandPos = island.GetVector3(CommonNames.Position);
            PositionOnIsland(ref islandPos);

            // add timeout respawn attribute
            this.powerup.AddFloatAttribute("respawn_at", 0);

            // register handlers
            this.island.GetVector3Attribute(CommonNames.Position).ValueChanged += OnIslandPositionChanged;
            entity.GetProperty<CollisionProperty>("collision").OnContact += PowerupCollisionHandler;

            powerup.OnUpdate += OnUpdate;
        }

        public override void OnDetached(
            AbstractEntity entity
        )
        {
            powerup.OnUpdate -= OnUpdate;
            this.island.GetVector3Attribute(CommonNames.Position).ValueChanged -= OnIslandPositionChanged;
            if(entity.HasProperty("collision"))
                entity.GetProperty<CollisionProperty>("collision").OnContact -= PowerupCollisionHandler;
        }

        private void OnUpdate(Entity powerup, SimulationTime simTime)
        {
            if (powerUsed && respawnAt == 0)
            {
                powerup.GetProperty<CollisionProperty>("collision").OnContact -= PowerupCollisionHandler;

                powerup.RemoveProperty("collision");
                powerup.RemoveProperty("render");
                powerup.RemoveProperty("shadow_cast");

                island.GetVector3Attribute(CommonNames.Position).ValueChanged -= OnIslandPositionChanged;

                respawnAt = (float)(simTime.At + rand.NextDouble() *
                    powerup.GetFloat("respawn_random_time") + powerup.GetFloat("respawn_min_time"));
            }
            else
                if (powerUsed && simTime.At > respawnAt)
            {
                if (!powerup.GetBool("fixed"))
                {
                    if (!SelectNewIsland())
                    {
                        // if we cannot find a suitable island: wait
                        return;
                    }
                }
                else
                {
                    Vector3 pos = island.GetVector3(CommonNames.Position);
                    PositionOnIsland(ref pos);
                }

                powerup.AddProperty("collision", new CollisionProperty(), true);
                powerup.AddProperty("render", new PowerupRenderProperty(), true);
                //powerup.AddProperty("shadow_cast", new ShadowCastProperty());

                this.island.GetVector3Attribute(CommonNames.Position).ValueChanged += OnIslandPositionChanged;
                powerup.GetProperty<CollisionProperty>("collision").OnContact += PowerupCollisionHandler;

                respawnAt = 0;
                powerUsed = false;
            }
        }

        private void OnIslandPositionChanged(
            Vector3Attribute positionAttribute,
            Vector3 oldPosition,
            Vector3 newPosition
        )
        {
            PositionOnIsland(ref newPosition);
        }

        private void PositionOnIsland(ref Vector3 position)
        {
            this.powerup.SetVector3(CommonNames.Position, position + GetRelativePosition(powerup, island) + powerup.GetFloat("surface_offset") * Vector3.UnitY);
        }

        private void PowerupCollisionHandler(SimulationTime simTime, Contact contact)
        {
            Entity other = contact.EntityB;
            if (other.HasAttribute(CommonNames.Kind)
                && "player".Equals(other.GetString(CommonNames.Kind))
                && !powerUsed)
            {
                // use the power
                GivePower(other);

                // notify hud
                if (other.HasProperty("hud"))
                {
                    other.GetProperty<HUDProperty>("hud").NotifyPowerupPickup(powerup.GetVector3(CommonNames.Position), NotificationString);
                }

                // check ranges
                other.GetProperty<PlayerControllerProperty>("controller").CheckPlayerAttributeRanges(other);

                // set to used
                powerUsed = true;
            }
        }

        private Vector3 GetRelativePosition(Entity powerup, Entity island)
        {
            if (island.HasVector3(powerup.Name + "_position"))
            {
                return island.GetVector3(powerup.Name + "_position");
            }
            else
            {
                return powerup.GetVector3("relative_position");
            }
        }

        protected abstract string NotificationString { get; }

        protected abstract void GivePower(Entity player);

        private bool SelectNewIsland()
        {
            int cnt = Game.Instance.Simulation.IslandManager.Count;
            Entity island;
            int i = 0;
            while(true)
            {
                int islandNo = rand.Next(Game.Instance.Simulation.IslandManager.Count - 1);
                island = Game.Instance.Simulation.IslandManager[islandNo];

                bool valid = true;

                // no players on it
                if (island.GetInt("players_on_island") > 0)
                    valid = false;

                // check we are far enough away from other powerups
                foreach (Entity p in Game.Instance.Simulation.PowerupManager)
                {
                    if ((p.GetVector3(CommonNames.Position) - island.GetVector3(CommonNames.Position)).Length()
                        < constants.GetFloat("respawn_min_distance_to_others"))
                    {
                        valid = false;
                        break;
                    }
                }

                i++;

                if (i == cnt)
                {
                    return false;
                }

                if (valid)
                    break; // ok
                else
                    continue; // select another
            }

            this.island = island;
            powerup.SetString("island_reference", island.Name);
            CalculateSurfaceOffset();
            Vector3 islandPos = island.GetVector3(CommonNames.Position);
            PositionOnIsland(ref islandPos);
            return true;
        }

        private void CalculateSurfaceOffset()
        {
            // get position on surface
            Vector3 islandPos = island.GetVector3(CommonNames.Position);
            Vector3 checkPos = islandPos + GetRelativePosition(powerup, island);
            Vector3 surfacePos;
            Simulation.GetPositionOnSurface(ref checkPos, island, out surfacePos);
            powerup.SetFloat("surface_offset", surfacePos.Y - islandPos.Y);
        }

        private bool powerUsed = false;
        private float respawnAt = 0;

        protected Entity powerup;
        protected Entity island;
        protected Entity constants;
    }
}
