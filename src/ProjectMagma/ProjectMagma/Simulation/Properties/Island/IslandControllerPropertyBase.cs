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
            entity.AddVector3Attribute("repositioning_velocity", Vector3.Zero);

            entity.AddStringAttribute("attracted_by", "");
            entity.AddVector3Attribute("attraction_velocity", Vector3.Zero);
            entity.AddIntAttribute("players_on_island", 0);

            // approximation of island's radius and height
            Vector3 scale = island.GetVector3("scale");
            entity.AddFloatAttribute("height", scale.Y);
            scale.Y = 0;
            entity.AddFloatAttribute("radius", scale.Length());

            entity.AddBoolAttribute("stop_attraction", false);

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

        private double collisionAt = 0;

        protected virtual void OnUpdate(Entity island, SimulationTime simTime)
        {
            float dt = simTime.Dt;

            // get position
            Vector3 position = island.GetVector3("position");
            if (float.IsInfinity(position.X)
                || float.IsNaN(position.X))
            {
                Console.WriteLine("nan");
            }

            // implement sinking and rising islands
            int playersOnIsland = island.GetInt("players_on_island");
            if (!hadCollision)
            {
                if (playersOnIsland > 0)
                {
                    position -= dt * constants.GetFloat("sinking_speed") * playersOnIsland * (Vector3.UnitY);
                    playerLeftAt = 0;
                }
                else
                {
                    if (playerLeftAt == 0)
                        playerLeftAt = simTime.At;
                    if (simTime.At > playerLeftAt + constants.GetInt("rising_delay"))
                    {
                        state = IslandState.Repositioning;
                        playerLeftAt = double.MaxValue;
/*
                        if (position.Y < originalPosition.Y)
                        {
                            position += dt * constants.GetFloat("rising_speed") * (Vector3.UnitY);
                        }
                        else
                        {
                            if (state == IslandState.Influenced)
                            {
                                state = IslandState.Normal;
                            }
                        }
 */ 
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
                Entity playerIsland = Game.Instance.Simulation.EntityManager[player.GetString("active_island")];
                Vector3 destinationPos = playerIsland.GetVector3("position");
                Vector3 dir = destinationPos - position;
                float dist = dir.Length();
                if(dir != Vector3.Zero)
                    dir.Normalize();

                bool inRadius = (position - destinationPos).Length()
                        < island.GetFloat("radius") + playerIsland.GetFloat("radius");

                bool inHeight =
                    (position.Y < destinationPos.Y
                    && position.Y > destinationPos.Y - playerIsland.GetFloat("height"))
                    || (position.Y - island.GetFloat("height") < destinationPos.Y
                    && position.Y - island.GetFloat("height") > destinationPos.Y - playerIsland.GetFloat("height"));

                if (!collisionWithDestination)
                {
                    if (!inRadius || inHeight)
                    {
                        Vector3 velocity = island.GetVector3("attraction_velocity");
                        float maxSpeed = constants.GetFloat("attraction_max_speed");
                        if (dist < 200) // todo: extract constant
                            maxSpeed *= dist / 400;

                        // don't allow being faster than max-speed
                        if (velocity.Length() > maxSpeed
                            && maxSpeed > 0)
                        {
                            velocity = Vector3.Normalize(velocity) * maxSpeed;
                        }
                        if (!hadCollision)
                        {
//                            if (simTime.At > collisionAt + 1000)
                            {
                                velocity += dir * constants.GetFloat("attraction_acceleration") * dt;
//                                velocity.Y += dir.Y * constants.GetFloat("attraction_acceleration") * dt; // faster acceleration on y axis
                            }
                        }
                        else
                        {
                            collisionAt = simTime.At;
                        }
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
                else
                {
                    attractionAtDestination = true;
                    island.SetVector3("attraction_velocity", Vector3.Zero);
                }

                // check wheter to stop attraction (after timeout)
                if (island.GetBool("stop_attraction"))
                {
                    if (attractionEndTimoutStartedAt == 0)
                        attractionEndTimoutStartedAt = simTime.At;
                    if (simTime.At > attractionEndTimoutStartedAt + constants.GetFloat("attraction_timeout")
                        || !attractionAtDestination) // only wait for timeout if at destination
                    {
                        // finally stop attraction
                        island.SetString("attracted_by", "");
                    }
                }
            }

            // perform repositioning
            if (state == IslandState.Repositioning)
            {
                if (lastState != IslandState.Repositioning)
                {
                    repositioningPosition = GetNearestPointOnPath(ref position);
//                    Console.WriteLine("repositioning to: " + repositioningPosition);
                }

                // if players are standing on island, we only reposition in xz
                if (playersOnIsland > 0)
                {
                    // stay on y
                    repositioningPosition.Y = position.Y;
                }

                // get direction of repositioning effort
                Vector3 dir = repositioningPosition - position;
                if (dir != Vector3.Zero)
                    dir.Normalize();

                // calculate new position
                Vector3 newPosition = position;
                newPosition += island.GetVector3("repositioning_velocity") * dt;
                island.SetVector3("repositioning_velocity", dir * constants.GetFloat("repositioning_speed"));

                // check if we are there yet (respectivly a bit further)
                if (Vector3.Dot(repositioningPosition - newPosition, repositioningPosition - position) < 0)
                {
                    position = repositioningPosition;
                    state = IslandState.Normal;
                    island.SetVector3("repositioning_velocity", Vector3.Zero);
                    OnRepositioningEnded(dir);
                }
                else
                {
                    position = newPosition;
                }
            }
            else
            // apply movement only if no collision
            if (!hadCollision)
            {
                // normal movement
                if (state != IslandState.Influenced)
                {
                    // calculate new postion in subclass
                    CalculateNewPosition(island, ref position, dt);
                }
            }

            island.SetVector3("position", position);

            hadCollision = false;
            collisionWithDestination = false;
            lastState = state;
        }

        private void CollisionHandler(SimulationTime simTime, Contact contact)
        {
            Entity island = contact.EntityA;
            Entity other = contact.EntityB;
            if(other.HasString("kind"))
            {
                String kind = other.GetString("kind");

                if ((kind == "player"
                    && !PlayerControllerProperty.ImuneToIslandPush)
                    || kind == "powerup"
/*                    || kind == "cave"*/
                    )
                {
                    // do nothing
                }
                else 
                {
                    // only collision with objects which are not the player count
                    Vector3 normal = CalculatePseudoNormalIsland(island, other);
                    if (kind == "cave")
                    {
                        normal = island.GetVector3("position");
                        normal.Y = 0;
                        if(normal != Vector3.Zero)
                            normal.Normalize();
                    }

                    // change direction of repulsion
                    Vector3 repulsionVelocity = island.GetVector3("repulsion_velocity");
                    if (repulsionVelocity.Length() > 0)
                    {
                        // only if collision normal is in opposite direction of current repulsion
                        Vector3 xznormal = normal;
                        xznormal.Y = 0;
                        if (Vector3.Dot(repulsionVelocity, xznormal) > 0)
                        {
                            island.SetVector3("repulsion_velocity", Vector3.Reflect(repulsionVelocity, xznormal)
                                * constants.GetFloat("collision_damping"));
                        }
                    }

                    // if other is island, set players_on_island

                    // attraction?
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
                            if (Vector3.Dot(attractionVelocity, normal) > 0)
                            {
                                Vector3 velocity = normal * attractionVelocity.Length() * constants.GetFloat("collision_damping")
                                    // resulting velocity depends on angle
                                    * Vector3.Dot(normal, Vector3.Normalize(attractionVelocity));
                                velocity.Y = 0; // only in xz plane
                                other.SetVector3("repulsion_velocity", velocity + other.GetVector3("repulsion_velocity"));
                            }
                        }
                        else
                        // reflect for collision with pillar/island (if not already in direction away from it)
                        if (Vector3.Dot(attractionVelocity, normal) > 0)
                        {
                            Vector3 normXZ = normal;
                            normXZ.Y = 0;
                            if(normXZ != Vector3.Zero)
                                normXZ.Normalize();

                            // compute only on x/z plane for now
                            Vector3 velocityXZ = attractionVelocity;
                            velocityXZ.Y = 0.0f;

                            // project onto normal
                            Vector3 projection = Vector3.Dot(velocityXZ, normXZ) * normXZ;

                            // compute orthogonal repulsion
                            Vector3 newVelocity = -(projection - velocityXZ);
                            if (newVelocity != Vector3.Zero)
                                newVelocity.Normalize();
                            newVelocity.Y = 0; // -contact[0].Normal.Y * attractionVelocity.Y;
                            //newVelocity *= 600;
                            newVelocity *= attractionVelocity.Length();

                            // repulse in direction of normal too
                            if (Vector3.Dot(attractionVelocity, normal) > 0)
                            {
                                newVelocity += -normal * attractionVelocity.Length();
                                if (other.GetString("kind") == "pillar")
                                    newVelocity.Y = 0;
                            }
//                            newVelocity *= attractionVelocity.Length(); // *constants.GetFloat("collision_damping");

                            // todo: this will occur if projection is Vector3.Zero. what to do then?
                            if (float.IsInfinity(newVelocity.X))
                            {
                                Console.WriteLine("newvelocity: " + newVelocity);
                            }

                            island.SetVector3("attraction_velocity", newVelocity);
                        }
                    }
                    else
                    // adapt repositioning velocity
                    if (state == IslandState.Repositioning)
                    {
                        // get distance of destination point to sliding plane
                        float distance = Vector3.Dot(contact[0].Normal, repositioningPosition - contact[0].Point);

                        // calculate point on plane
                        Vector3 cpos = repositioningPosition - contact[0].Normal * distance;

                        // get sliding velocity
                        Vector3 slidingDir = (cpos - contact[0].Point);
                        if (slidingDir != Vector3.Zero)
                            slidingDir.Normalize();
                        Vector3 slidingVelocity = slidingDir * constants.GetFloat("repositioning_speed");

                        island.SetVector3("repositioning_velocity", slidingVelocity);
                    }
                    else
                        CollisionHandler(simTime, island, other, contact, ref normal);

                    hadCollision = true;
                }
            }
        }

        private Vector3 CalculatePseudoNormalIsland(Entity entityA, Entity entityB)
        {
            Vector3 normal;
            if(entityB.GetString("kind") == "pillar")
            {
                normal = entityB.GetVector3("position")-entityA.GetVector3("position");
                normal.Y = 0;
            }
            else
            {
                normal = entityB.GetVector3("position")-entityA.GetVector3("position");
            }
            if (normal != Vector3.Zero)
                normal.Normalize();
            return normal;
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

        /// <summary>
        /// gets nearest position on islands original path from given position 
        /// to reposition island to
        /// </summary>
        protected abstract Vector3 GetNearestPointOnPath(ref Vector3 position);

        protected abstract void CollisionHandler(SimulationTime simTime, Entity island, Entity other, Contact co, ref Vector3 normal);

        protected virtual void OnRepulsionStart()
        {
            state = IslandState.Influenced;
        }

        protected virtual void OnAttractionStart()
        {
            state = IslandState.Influenced;
        }

        protected virtual void OnRepulsionEnd()
        {
            state = IslandState.Repositioning;
        }

        protected virtual void OnAttractionEnd()
        {
            state = IslandState.Repositioning;
            island.SetVector3("attraction_velocity", Vector3.Zero);
            island.SetBool("stop_attraction", false);
            attractionEndTimoutStartedAt = 0;
            attractionAtDestination = false;
        }

        /// <summary>
        /// called when reposition ends
        /// </summary>
        /// <param name="dir">the direction from the last position</param>
        protected virtual void OnRepositioningEnded(Vector3 dir)
        {
            Console.WriteLine(" island " + island.Name + " back to normal state");
        }

        private float attractionEndTimoutStartedAt = 0;

        protected bool attractionAtDestination = false;
        protected bool collisionWithDestination;
        protected bool hadCollision;

        protected Entity constants;
        protected Entity island;
        private double playerLeftAt = double.MaxValue;

        protected enum IslandState
        {
            Influenced,             // island's track is influensed by the outside, normal movement is prohibited
            Repositioning,          // island is hovering back to original position
            Normal                  // normal movement
        }

        protected IslandState state = IslandState.Normal;
        protected IslandState lastState = IslandState.Normal;

        // position island started out at
        protected Vector3 originalPosition;

        // position to hover back to
        private Vector3 repositioningPosition;
    }
}
