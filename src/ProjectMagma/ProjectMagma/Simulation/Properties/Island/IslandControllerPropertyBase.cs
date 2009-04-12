using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Collision;
using ProjectMagma.Simulation.Attributes;


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

            this.island = entity;
            this.constants = Game.Instance.Simulation.EntityManager["island_constants"];

            if (!entity.HasAttribute("max_health"))
                entity.AddIntAttribute("max_health", (int) (Game.GetScale(entity).Length() * constants.GetFloat("scale_health_multiplier")));
            entity.AddIntAttribute("health", entity.GetInt("max_health"));

            entity.AddVector3Attribute("repulsion_velocity", Vector3.Zero);
            entity.AddVector3Attribute("pushback_velocity", Vector3.Zero);
            entity.AddStringAttribute("attracted_by", "");

            entity.Update += OnUpdate;

            ((CollisionProperty)entity.GetProperty("collision")).OnContact += CollisionHandler;
            ((StringAttribute)entity.GetAttribute("attracted_by")).ValueChanged += AttracedByChangeHandler;

            originalPosition = entity.GetVector3("position");
        }

        public virtual void OnDetached(Entity entity)
        {
            entity.Update -= OnUpdate;
            ((CollisionProperty)entity.GetProperty("collision")).OnContact -= CollisionHandler;
            ((StringAttribute)entity.GetAttribute("attracted_by")).ValueChanged += AttracedByChangeHandler;
        }

        protected virtual void OnUpdate(Entity island, SimulationTime simTime)
        {
            float dt = simTime.Dt;

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
                    playerLeftAt = simTime.At;
                if (position.Y < originalPosition.Y &&
                    simTime.At > playerLeftAt + constants.GetInt("rising_delay"))
                {
                    position += dt * constants.GetFloat("rising_speed") * Vector3.UnitY;

                    if (position.Y > originalPosition.Y)
                    {
                        position.Y = originalPosition.Y;
                    }
                }
            }

            // apply pushback from players 
            Vector3 repulsionVelocity = island.GetVector3("repulsion_velocity");
            Game.ApplyPushback(ref position, ref repulsionVelocity, constants.GetFloat("repulsion_deacceleration"),
                    OnPushbackEnded);
            island.SetVector3("repulsion_velocity", repulsionVelocity);

            // apply pushback from other objects as long as there is a collision
            if (hadCollision)
            {
                position += island.GetVector3("pushback_velocity") * dt;
            }
            else
                island.SetVector3("pushback_velocity", Vector3.Zero);

            // apply movement only if no collision
            if (!hadCollision)
            {
                // apply attraction by players
                if (island.GetString("attracted_by") != "")
                {
                    Entity player = Game.Instance.Simulation.EntityManager[island.GetString("attracted_by")];
                    Vector3 dir = player.GetVector3("position") - position;
                    dir.Normalize();

                    position += dir * constants.GetFloat("attraction_speed") * dt;
                }
                else
                {
                    // calculate new postion in subclass
                    CalculateNewPosition(island, ref position, dt);
                }
            }

            island.SetVector3("position", position);

            playersOnIsland = 0;
            hadCollision = false;
        }

        private void CollisionHandler(SimulationTime simTime, Contact contact)
        {
            Entity island = contact.EntityA;
            Entity other = contact.EntityB;
            if(other.HasString("kind"))
            {
                String kind = other.GetString("kind");
                if (kind == "player")
                {
                    // collision with player
                    if(Vector3.Dot(Vector3.UnitY, contact[0].Normal) > 0) // on top
                        playersOnIsland++;
                }
                else
                {
                    // only collision with objects which are not the player count
                    hadCollision = true;

//                    Console.WriteLine("collision: " + island.Name + " with " + other.Name);

                    // change direction of repulsion
                    Vector3 repulsionVelocity = island.GetVector3("repulsion_velocity");
                    if (repulsionVelocity.Length() > 0)
                    {
                        // only if collision normal is in opposite direction of current repulsion
                        if (Vector3.Dot(repulsionVelocity, contact[0].Normal) < 0)
                        {
                            island.SetVector3("repulsion_velocity", -repulsionVelocity * constants.GetFloat("collision_damping"));
                        }
                    }

                    // if attracted, apply repulsion to colliding islands
                    if (island.GetString("attracted_by") != "")
                    {
                        if (kind == "island")
                        {
                            // todo: check here if collision island is island player who attracts is standing on

                            // push the other island away
                            other.SetVector3("repulsion_velocity", contact[0].Normal * constants.GetFloat("attraction_speed")
                                + other.GetVector3("repulsion_velocity"));
                        }
                        else
                            if (kind == "pillar")
                            {
                                // repulse if collision with pillar
                                island.SetVector3("repulsion_velocity", -contact[0].Normal * constants.GetFloat("attraction_speed")
                                    + island.GetVector3("repulsion_velocity"));
                            }
                    }

                    CollisionHandler(simTime, island, other, contact);
                }
            }
        }

        protected void AttracedByChangeHandler(StringAttribute sender, string oldValue, string newValue)
        {
            if (newValue == "" && oldValue != "")
            {
                OnAttractionEnded();
            }
        }

        public abstract void CalculateNewPosition(Entity island, ref Vector3 position, float dt);

        protected abstract void CollisionHandler(SimulationTime simTime, Entity island, Entity other, Contact co);

        protected virtual void OnPushbackEnded()
        {
        }

        protected virtual void OnAttractionEnded()
        {
        }

        protected bool hadCollision;

        protected Entity constants;
        protected Entity island;
        private int playersOnIsland;
        private double playerLeftAt;
        private Vector3 originalPosition;
    }
}
