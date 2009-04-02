using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Collision;


namespace ProjectMagma.Framework
{
    public abstract class IslandControllerPropertyBase : Property
    {
        public IslandControllerPropertyBase()
        {
        }

        public virtual void OnAttached(Entity entity)
        {
            Debug.Assert(entity.HasVector3("position"));

            this.constants = Game.Instance.EntityManager["island_constants"];

            if (!entity.HasAttribute("max_health"))
                entity.AddIntAttribute("max_health", (int) (Game.GetScale(entity).Length() * constants.GetFloat("scale_health_multiplier")));
            entity.AddIntAttribute("health", entity.GetInt("max_health"));

            entity.AddVector3Attribute("repulsion_velocity", Vector3.Zero);

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
            
            island.SetVector3("position", position);

            playersOnIsland = 0;
        }

        private void CollisionHandler(GameTime gameTime, List<Contact> contacts)
        {
            Contact contact = contacts[0];
            Entity entity = contact.EntityB;
            if (entity.HasString("kind") && // other entity has a kind-attribute
                entity.GetString("kind") == "player" && // other entity is a player
                contact.Normal.Y > 0 // player is above island
            )
            {
                // collision with player
                playersOnIsland++;
            }
            else
            {
                CollisionHandler(gameTime, entity, contact);
            }
        }

        protected abstract void CollisionHandler(GameTime gameTime, Entity entity, Contact co);

        protected Entity constants;
        private int playersOnIsland;
        private double playerLeftAt;
        private Vector3 originalPosition;
    }
}
