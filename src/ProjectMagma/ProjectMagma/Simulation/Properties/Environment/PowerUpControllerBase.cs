﻿using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using ProjectMagma.Simulation.Collision;
using ProjectMagma.Simulation.Attributes;
using Microsoft.Xna.Framework.Audio;

namespace ProjectMagma.Simulation
{
    public abstract class PowerUpControllerBase : Property
    {

        private Random rand;

        public PowerUpControllerBase()
        {
            rand = new Random(234234);
        }

        public void OnAttached(
            Entity entity
        )
        {
            this.powerup = entity;
            this.constants = Game.Instance.Simulation.EntityManager["powerup_constants"];
            this.island = Game.Instance.Simulation.EntityManager[entity.GetString("island_reference")];

            // initialize properties
            Debug.Assert(this.powerup.HasVector3("relative_position"), "must have a relative translation attribute");
            if (!this.powerup.HasAttribute("position"))
            {
                this.powerup.AddVector3Attribute("position", Vector3.Zero);
            }

            // get position on surface
            powerup.AddFloatAttribute("surface_offset", 0f);
            CalculateSurfaceOffset();

            // add timeout respawn attribute
            this.powerup.AddFloatAttribute("respawn_at", 0);

            // register handlers
            this.island.GetVector3Attribute("position").ValueChanged += OnIslandPositionChanged;
            ((CollisionProperty)entity.GetProperty("collision")).OnContact += PowerupCollisionHandler;

            // load sound
            pickupSound = Game.Instance.ContentManager.Load<SoundEffect>("Sounds/" + powerup.GetString("pickup_sound"));

            powerup.Update += OnUpdate;
        }

        public void OnDetached(
            Entity entity
        )
        {
            powerup.Update -= OnUpdate;
            this.island.GetVector3Attribute("position").ValueChanged -= OnIslandPositionChanged;
        }

        private void OnUpdate(Entity powerup, SimulationTime simTime)
        {
            if (powerUsed
                && respawnAt == 0)
            {
                powerup.RemoveProperty("collision");
                powerup.RemoveProperty("render");
                powerup.RemoveProperty("shadow_cast");

                respawnAt = (float)(simTime.At + rand.NextDouble() * constants.GetFloat("respawn_random_time") + constants.GetFloat("respawn_min_time"));
            }
            else
            if (respawnAt != 0
                && simTime.At > respawnAt)
            {
                SelectNewIsland();

                powerup.AddProperty("collision", new CollisionProperty());
                powerup.AddProperty("render", new BasicRenderProperty());
                powerup.AddProperty("shadow_cast", new ShadowCastProperty());
                ((CollisionProperty)powerup.GetProperty("collision")).OnContact += PowerupCollisionHandler;

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
            this.powerup.SetVector3("position", newPosition + this.powerup.GetVector3("relative_position")
                + powerup.GetFloat("surface_offset") * Vector3.UnitY);
        }

        private void PowerupCollisionHandler(SimulationTime simTime, Contact contact)
        {
            Entity other = contact.EntityB;
            if (other.HasAttribute("kind")
                && "player".Equals(other.GetString("kind"))
                && !powerUsed)
            {
                // use the power
                GivePower(other);

                // check ranges
                ((PlayerControllerProperty)other.GetProperty("controller")).CheckPlayerAttributeRanges(other);

                // soundeffect
                pickupSound.Play(Game.Instance.EffectsVolume);

                // set to used
                powerUsed = true;
            }
        }

        protected abstract void GivePower(Entity player);

        private void SelectNewIsland()
        {
            Entity island;
            for (; ; )
            {
                int islandNo = rand.Next(Game.Instance.Simulation.IslandManager.Count - 1);
                island = Game.Instance.Simulation.IslandManager[islandNo];
                // check island is far enough away from players
                foreach (Entity p in Game.Instance.Simulation.PlayerManager)
                {
                    if ((island.GetVector3("position") - p.GetVector3("position")).Length() > constants.GetFloat("respawn_min_distance_to_players"))
                        continue; // select again
                }
                // no powerup on selected island -> break;
                break;
            }
            this.island = island;
            powerup.SetString("island_reference", island.Name);
            CalculateSurfaceOffset();
        }

        private void CalculateSurfaceOffset()
        {
            // get position on surface
            Vector3 islandPos = island.GetVector3("position");
            Vector3 checkPos = islandPos + powerup.GetVector3("relative_position");
            Vector3 surfacePos;
            Simulation.GetPositionOnSurface(ref checkPos, island, out surfacePos);
            powerup.SetFloat("surface_offset", surfacePos.Y - islandPos.Y);
        }

        private bool powerUsed = false;
        private float respawnAt = 0;
        private SoundEffect pickupSound;

        protected Entity powerup;
        protected Entity island;
        protected Entity constants;
    }
}