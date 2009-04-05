﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Collision;


namespace ProjectMagma.Simulation
{
    public abstract class IslandControllerPropertyBase : Property
    {
        public IslandControllerPropertyBase()
        {
        }

        public virtual void OnAttached(Entity entity)
        {
            Debug.Assert(entity.HasVector3("position"));

            this.constants = Game.Instance.Simulation.EntityManager["island_constants"];

            if (!entity.HasAttribute("max_health"))
                entity.AddIntAttribute("max_health", (int) (Game.GetScale(entity).Length() * constants.GetFloat("scale_health_multiplier")));
            entity.AddIntAttribute("health", entity.GetInt("max_health"));

            entity.AddVector3Attribute("repulsion_velocity", Vector3.Zero);
            entity.AddStringAttribute("attracted_by", "");

            entity.Update += OnUpdate;

            ((CollisionProperty)entity.GetProperty("collision")).OnContact += CollisionHandler;

            originalPosition = entity.GetVector3("position");
        }

        public virtual void OnDetached(Entity entity)
        {
            entity.Update -= OnUpdate;
            if (entity.HasProperty("collision"))
            {
                ((CollisionProperty)entity.GetProperty("collision")).OnContact -= CollisionHandler;
             }
        }

        protected virtual void OnUpdate(Entity island, GameTime gameTime)
        {
            float dt = ((float)gameTime.ElapsedGameTime.Milliseconds)/1000.0f;

            // implement sinking/rising islands...
            Vector3 position = island.GetVector3("position");
            if (playersOnIsland > 0)
            {
                position += dt * constants.GetFloat("sinking_speed") * playersOnIsland * (-Vector3.UnitY);
                playerLeftAt = 0;
            }
            else
            {
                if (playerLeftAt == 0)
                    playerLeftAt = gameTime.TotalGameTime.TotalMilliseconds;
                if (position.Y < originalPosition.Y && 
                    gameTime.TotalGameTime.TotalMilliseconds > playerLeftAt + constants.GetInt("rising_delay"))
                {
                    position += dt * constants.GetFloat("rising_speed") * Vector3.UnitY;
                }
            }

            if (position.Y > originalPosition.Y)
            {
                position.Y = originalPosition.Y;
            }

            // apply pushback from players
            Vector3 repulsionVelocity = island.GetVector3("repulsion_velocity");
            Game.ApplyPushback(ref position, ref repulsionVelocity, constants.GetFloat("repulsion_deacceleration"));
            island.SetVector3("repulsion_velocity", repulsionVelocity);
            
            // apply attraction by players
            if (island.GetString("attracted_by") != "")
            {
                Entity player = Game.Instance.Simulation.EntityManager[island.GetString("attracted_by")];
                Vector3 dir = player.GetVector3("position") - position;
                dir.Normalize();

                // todo: make constant
                float attractionSpeed = 100;

                position += dir * attractionSpeed * dt;
            }

            island.SetVector3("position", position);

            playersOnIsland = 0;
        }

        private void CollisionHandler(GameTime gameTime, List<Contact> contacts)
        {
            Contact contact = contacts[0];
            Entity island = contact.EntityA;
            Entity other = contact.EntityB;
            if(other.HasString("kind"))
            {
                if (other.GetString("kind") == "player")
                {
                    // collision with player
                    if(Vector3.Dot(Vector3.UnitY, contact.Normal) > 0) // on top
                        playersOnIsland++;
                }
                else
                {
                    // change direction of repulsion
                    Vector3 repulsionVelocity = island.GetVector3("repulsion_velocity");
                    if (repulsionVelocity.Length() > 0)
                    {
                        island.SetVector3("repulsion_velocity", -repulsionVelocity);
                    }
                    
                    // if attracted, apply repulsion to colliding islands
                    if (island.GetString("attracted_by") != "")
                    {
                        // Entity player = Game.Instance.Simulation.EntityManager[island.GetString("attracted_by")];
                        float attractionSpeed = 100;
                        other.SetVector3("repulsion_velocity", contact.Normal * attractionSpeed + other.GetVector3("repulsion_velocity"));
                    }

                    CollisionHandler(gameTime, other, contact);
                }
            }
        }

        protected abstract void CollisionHandler(GameTime gameTime, Entity entity, Contact co);

        protected Entity constants;
        private int playersOnIsland;
        private double playerLeftAt;
        private Vector3 originalPosition;
    }
}
