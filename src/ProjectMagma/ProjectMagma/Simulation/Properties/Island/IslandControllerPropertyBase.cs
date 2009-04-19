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
                entity.AddIntAttribute("max_health", (int) (entity.GetVector3("scale").Length() * constants.GetFloat("scale_health_multiplier")));
            entity.AddIntAttribute("health", entity.GetInt("max_health"));

            entity.AddVector3Attribute("repulsion_velocity", Vector3.Zero);
            entity.AddVector3Attribute("pushback_velocity", Vector3.Zero);

            entity.AddStringAttribute("attracted_by", "");
            entity.AddVector3Attribute("attraction_velocity", Vector3.Zero);
            entity.AddIntAttribute("players_on_island", 0);

            entity.Update += OnUpdate;

            ((CollisionProperty)entity.GetProperty("collision")).OnContact += CollisionHandler;
            ((StringAttribute)entity.GetAttribute("attracted_by")).ValueChanged += AttracedByChangeHandler;
            ((Vector3Attribute)entity.GetAttribute("repulsion_velocity")).ValueChanged += RepulsionChangeHandler;

            originalPosition = entity.GetVector3("position");
        }

        public virtual void OnDetached(Entity entity)
        {
            entity.Update -= OnUpdate;
            ((CollisionProperty)entity.GetProperty("collision")).OnContact -= CollisionHandler;
            ((StringAttribute)entity.GetAttribute("attracted_by")).ValueChanged -= AttracedByChangeHandler;
        }

        protected virtual void OnUpdate(Entity island, SimulationTime simTime)
        {
            float dt = simTime.Dt;

            // get position
            Vector3 position = island.GetVector3("position");

            // if we are in normal state, constantly save current position
            if (state == IslandState.Normal)
            {
                originalPosition = position;
            }

            // implement sinking and rising islands
            int playersOnIsland = island.GetInt("players_on_island");
            if (!hadCollision && state == IslandState.Normal)
            {
                if (playersOnIsland > 0)
                {
                    state = IslandState.Influenced; 
                    position += dt * constants.GetFloat("sinking_speed") * playersOnIsland * (-Vector3.UnitY);
                    playerLeftAt = 0;
                }
                else
                {
                    if (playerLeftAt == 0)
                        playerLeftAt = simTime.At;
                    if (simTime.At > playerLeftAt + constants.GetInt("rising_delay"))
                    {
                        if (position.Y < originalPosition.Y)
                        {
                            position += dt * constants.GetFloat("sinking_speed") * (Vector3.UnitY);
                        }
                        else
                            state = IslandState.Normal;
                    }
                }
            }

            // apply repulsion from players 
            Vector3 repulsionVelocity = island.GetVector3("repulsion_velocity");
            Simulation.ApplyPushback(ref position, ref repulsionVelocity, constants.GetFloat("repulsion_deacceleration"),
                    OnRepulsionEnd);
            island.SetVector3("repulsion_velocity", repulsionVelocity);

            // apply pushback from other objects as long as there is a collision
            if (hadCollision)
            {
                position += island.GetVector3("pushback_velocity") * dt;
            }
            else
                island.SetVector3("pushback_velocity", Vector3.Zero);

            // perform attraction
            if (island.GetString("attracted_by") != "")
            {
                // get direction to player
                Entity player = Game.Instance.Simulation.EntityManager[island.GetString("attracted_by")];
                Vector3 dir = player.GetVector3("position") - position;
                dir.Normalize();

                if (!collisionWithDestination)
                {
                    Vector3 velocity = island.GetVector3("attraction_velocity");
                    // deaccelerate a bit first if > max_speed
                    if (velocity.Length() > constants.GetFloat("attraction_max_speed"))
                    {
                        velocity -= Vector3.Normalize(velocity) * constants.GetFloat("attraction_acceleration") * dt;
                    }
                    velocity += dir * constants.GetFloat("attraction_acceleration") * dt;
                    velocity.Y += dir.Y * constants.GetFloat("attraction_acceleration") * dt; // faster acceleration on y axis
                    island.SetVector3("attraction_velocity", velocity);

                    // change position according to velocity
                    position += velocity * dt;
                }
                else
                {
                    // hover on correct y
                    position.Y += dir.Y * constants.GetFloat("attraction_max_speed") * dt;
                }
            }

            // apply movement only if no collision
            if (!hadCollision)
            {
                // perform repositioning
                if (state == IslandState.Repositioning)
                {
                    // get direction of repositioning effort
                    Vector3 dir = Vector3.Normalize(originalPosition - position);

                    // calculate new position
                    Vector3 newPosition = position;
                    newPosition += dir * constants.GetFloat("repositioning_speed") * dt;

                    // check if we are there yet
                    if (Vector3.Dot(originalPosition - newPosition, originalPosition - position) < 0)
                    {
                        state = IslandState.Normal;
                    }
                    else
                    {
                        position = newPosition;
                    }
                }
                else
                    // normal movement
                    if (state != IslandState.InfluencedNoMovement)
                    {
                        // calculate new postion in subclass
                        CalculateNewPosition(island, ref position, dt);
                    }
            }

            island.SetVector3("position", position);

            hadCollision = false;
            collisionWithDestination = false;
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
                    // do nothing
                }
                else
                {
                    // only collision with objects which are not the player count
                    hadCollision = true;

                    // change direction of repulsion
                    Vector3 repulsionVelocity = island.GetVector3("repulsion_velocity");
                    if (repulsionVelocity.Length() > 0)
                    {
                        // only if collision normal is in opposite direction of current repulsion
                        if (Vector3.Dot(repulsionVelocity, contact[0].Normal) < 0)
                        {
                            island.SetVector3("repulsion_velocity", Vector3.Reflect(repulsionVelocity, contact[0].Normal)
                                * constants.GetFloat("collision_damping"));
                        }
                    }

                    if (island.GetString("attracted_by") != "")
                    {
                        Vector3 attractionVelocity = island.GetVector3("attraction_velocity");

                        if (kind == "island")
                        {
                            // check if collision island is island player who attracts is standing on
                            Entity player = Game.Instance.Simulation.EntityManager[island.GetString("attracted_by")];
                            if (other.Name == player.GetString("active_island"))
                            {
                                collisionWithDestination = true;
                                return;
                            }

                            // push the other island away (if contact normal is opposed)
                            if (Vector3.Dot(attractionVelocity, contact[0].Normal) < 0)
                            {
                                other.SetVector3("repulsion_velocity", island.GetVector3("attraction_velocity")
                                    + other.GetVector3("repulsion_velocity"));
                            }
                        }

                        // reflect for collision with pillar (if not already direction away from it)
                        if (Vector3.Dot(attractionVelocity, contact[0].Normal) < 0)
                        {
                            island.SetVector3("attraction_velocity", Vector3.Reflect(island.GetVector3("attraction_velocity"),
                                contact[0].Normal));
                        }
                    }
                    else
                        CollisionHandler(simTime, island, other, contact);

                    // always ensure we apply a bit of pushback out of other entity so we don't get stuck in there
                    if (!(other.GetString("kind") == "island"
                        && other.GetString("attracted_by") != ""))
                    {
                        Vector3 pushback = -contact[0].Normal * constants.GetFloat("contact_pushback_multiplier");
                        pushback.Y = 0; // only in xz plane
                        island.SetVector3("pushback_velocity", island.GetVector3("pushback_velocity") + pushback);
                    }
                }
            }
        }

        protected void RepulsionChangeHandler(Vector3Attribute sender, Vector3 oldValue, Vector3 newValue)
        {
            if (oldValue == Vector3.Zero
                && newValue != Vector3.Zero)
            {
                OnRepulsionStart();
            }
        }

        protected void AttracedByChangeHandler(StringAttribute sender, string oldValue, string newValue)
        {
            if (newValue != "" && oldValue == "")
            {
                OnAttractionStart();
            }
            else
            if (newValue == "" && oldValue != "")
            {
                OnAttractionEnd();
            }
        }

        public abstract void CalculateNewPosition(Entity island, ref Vector3 position, float dt);

        protected abstract void CollisionHandler(SimulationTime simTime, Entity island, Entity other, Contact co);

        protected virtual void OnRepulsionStart()
        {
            state = IslandState.Influenced;
        }

        protected virtual void OnAttractionStart()
        {
            state = IslandState.InfluencedNoMovement;
        }

        protected virtual void OnRepulsionEnd()
        {
            state = IslandState.Repositioning;
        }

        protected virtual void OnAttractionEnd()
        {
            state = IslandState.Repositioning;
            island.SetVector3("attraction_velocity", Vector3.Zero);
        }

        protected bool collisionWithDestination;
        protected bool hadCollision;

        protected Entity constants;
        protected Entity island;
        private double playerLeftAt;

        protected enum IslandState
        {
            Influenced,             // island's track is influensed by the outside
            InfluencedNoMovement,   // island's track is influensed by the outside, normal movement is prohibited
            Repositioning,          // island is hovering back to original position
            Normal                  // normal movement
        }

        protected IslandState state = IslandState.Normal;

        // position to hover back to
        private Vector3 originalPosition;
    }
}
