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
            OnRepulsionEndAction = new PushBackFinishedHandler(OnRepulsionEnd);
        }

        public virtual void OnAttached(Entity entity)
        {
            Debug.Assert(entity.HasVector3("position"));

            this.island = entity;
            this.constants = Game.Instance.Simulation.EntityManager["island_constants"];
            this.playerConstants = Game.Instance.Simulation.EntityManager["player_constants"];

            if (!entity.HasAttribute("max_health"))
                entity.AddIntAttribute("max_health", (int) (entity.GetVector3("scale").Length() * constants.GetFloat("scale_health_multiplier")));
            entity.AddIntAttribute("health", entity.GetInt("max_health"));

            hasFixedMovementPath = entity.GetBool("fixed");

            entity.AddVector3Attribute("repulsion_velocity", Vector3.Zero);
            entity.AddVector3Attribute("pushback_velocity", Vector3.Zero);
            entity.AddVector3Attribute("repositioning_velocity", Vector3.Zero);

            entity.AddStringAttribute("repulsed_by", "");
            entity.AddIntAttribute("players_on_island", 0);

            // approximation of island's radius and height
            Vector3 scale = island.GetVector3("scale");
            entity.AddFloatAttribute("height", scale.Y);
            scale.Y = 0;
            entity.AddFloatAttribute("radius", scale.Length());

            entity.Update += OnUpdate;

            ((CollisionProperty)entity.GetProperty("collision")).OnContact += CollisionHandler;
            ((Vector3Attribute)entity.GetAttribute("repulsion_velocity")).ValueChanged += RepulsionChangeHandler;

            originalPosition = entity.GetVector3("position");
        }

        public virtual void OnDetached(Entity entity)
        {
            entity.Update -= OnUpdate;
            ((CollisionProperty)entity.GetProperty("collision")).OnContact -= CollisionHandler;
            ((Vector3Attribute)entity.GetAttribute("repulsion_velocity")).ValueChanged -= RepulsionChangeHandler;
        }

        protected virtual void OnUpdate(Entity island, SimulationTime simTime)
        {
            if (Game.Instance.Simulation.Phase == SimulationPhase.Intro)
            {
                return;
            }

            float dt = simTime.Dt;

            int playersOnIsland = island.GetInt("players_on_island");
            Vector3 position = island.GetVector3("position");

            // check if any player can interact with this island to provide an attribute so dominik is happy
            bool interactable = playersOnIsland > 0;
            if (!interactable)
            {
                foreach (Entity player in Game.Instance.Simulation.PlayerManager)
                {
                    float dist = (position - player.GetVector3("position")).Length();
                    if (dist < playerConstants.GetFloat("island_jump_free_range"))
                        interactable = true;
                }
            }
            island.SetBool("interactable", interactable);

            // implement sinking and rising islands
            if (!HadCollision(simTime)
                && Game.Instance.Simulation.Phase == SimulationPhase.Game) // only sink in game-phase
            {
                if (playersOnIsland > 0)
                {
                    position -= dt * island.GetFloat("sinking_speed") * playersOnIsland * (Vector3.UnitY);
                    playerLeftAt = 0;
                }
                else
                {
                    if (playerLeftAt == 0)
                        playerLeftAt = simTime.At;
                    if (simTime.At > playerLeftAt + constants.GetInt("rising_delay"))
                    {
                        // rising using normal repositioning
                        state = IslandState.Repositioning;
                        playerLeftAt = double.MaxValue;
                    }
                }
            }

            // apply repulsion from players 
            Vector3 repulsionVelocity = island.GetVector3("repulsion_velocity");
            if (repulsionVelocity.Length() > constants.GetFloat("repulsion_max_speed"))
                repulsionVelocity = Vector3.Normalize(repulsionVelocity) * constants.GetFloat("repulsion_max_speed");
            Simulation.ApplyPushback(ref position, ref repulsionVelocity, constants.GetFloat("repulsion_deacceleration"),
                    OnRepulsionEndAction);
            island.SetVector3("repulsion_velocity", repulsionVelocity);

            // apply pushback from other objects as long as there is a collision
            if (HadCollision(simTime))
            {
                Vector3 pushbackVelocity = island.GetVector3("pushback_velocity");
                if (pushbackVelocity.Length() > constants.GetFloat("repulsion_max_speed"))
                {
                    pushbackVelocity.Normalize();
                    pushbackVelocity *= constants.GetFloat("repulsion_max_speed");
                    island.SetVector3("pushback_velocity", pushbackVelocity);
                }
                position += pushbackVelocity * dt;
            }
            else
                island.SetVector3("pushback_velocity", Vector3.Zero);
           
            // perform repositioning
            if (state == IslandState.Repositioning)
            {
                if (lastState != IslandState.Repositioning)
                {
                    if (hasFixedMovementPath)
                    {
                        repositioningPosition = GetNearestPointOnPath(ref position);
                    }
                    else
                    {
                        repositioningPosition = position;
                        repositioningPosition.Y = originalPosition.Y; // ensure we still maintian y position
                    }
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

                    // only update velocity if no collision
                    if (!HadCollision(simTime))
                    {
                        island.SetVector3("repositioning_velocity", dir * constants.GetFloat("repositioning_speed"));
                    }
                }
            }
            else
            // apply movement only if no collision
            if (!HadCollision(simTime))
            {
                // normal movement
                if (state != IslandState.Influenced)
                {
                    // calculate new postion in subclass
                    CalculateNewPosition(island, ref position, dt);
                }
            }

            island.SetVector3("position", position);

            lastState = state;
        }

        private void CollisionHandler(SimulationTime simTime, Contact contact)
        {
            Entity island = contact.EntityA;
            Entity other = contact.EntityB;
            if(other.HasString("kind"))
            {
                String kind = other.GetString("kind");

                if ((kind == "player" // don't do any collision response with player if set so
                    && !PlayerControllerProperty.ImuneToIslandPush)
                    // never do collision response with player who is standing on island
                    || (other.HasString("active_island") && other.GetString("active_island") == island.Name)
                    || (other.HasString("jump_island") && other.GetString("jump_island") == island.Name)
                    || kind == "powerup"
                    )
                {
                    // do nothing
                }
                else 
                {
                    // other objects
                    Vector3 normal = CalculatePseudoNormalIsland(island, other);

                    // change direction of repulsion
                    Vector3 repulsionVelocity = island.GetVector3("repulsion_velocity");
                    if (repulsionVelocity.Length() > 0)
                    {
                        // only if collision normal is in opposite direction of current repulsion
                        Vector3 xznormal = normal;
                        xznormal.Y = 0;
                        if (Vector3.Dot(Vector3.Normalize(repulsionVelocity), xznormal) > 0)
                        {
                            island.SetVector3("repulsion_velocity", Vector3.Reflect(repulsionVelocity, xznormal)
                                * constants.GetFloat("collision_damping"));
                        }
                        island.SetVector3("pushback_velocity", island.GetVector3("pushback_velocity")
                            - xznormal * 50); // todo: extract constant
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

                        // push a bit out too
                        normal.Y = 0;
                        island.SetVector3("pushback_velocity", island.GetVector3("pushback_velocity")
                                                   - normal * 10); // todo: extract constant
                    }
                    else
                        CollisionHandler(simTime, island, other, contact, ref normal);

                    collisionAt = simTime.At;
                }
            }
        }

        private Vector3 CalculatePseudoNormalIsland(Entity entityA, Entity entityB)
        {
            Vector3 normal;
            String kind = entityB.GetString("kind");
            if (kind == "pillar")
            {
                normal = entityB.GetVector3("position")-entityA.GetVector3("position");
                normal.Y = 0;
            }
            else
            if (kind == "cave")
            {
                normal = entityA.GetVector3("position");
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

        private readonly PushBackFinishedHandler OnRepulsionEndAction;

        protected virtual void OnRepulsionEnd()
        {
            state = IslandState.Repositioning;
        }

        /// <summary>
        /// called when reposition ends
        /// </summary>
        /// <param name="dir">the direction from the last position</param>
        protected virtual void OnRepositioningEnded(Vector3 dir)
        {
            //Console.WriteLine(" island " + island.Name + " back to normal state");
        }

        protected float collisionAt;

        // todo: extract constant
        protected static readonly float CollisionTimeout = 100;
        protected bool HadCollision(SimulationTime simTime)
        {
            return simTime.At < collisionAt + CollisionTimeout;
        }

        protected Entity constants;
        protected Entity playerConstants;
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


        // has island a fixed path or can it start its path from wherever it is?
        protected bool hasFixedMovementPath = true; 

        // position island started out at
        protected Vector3 originalPosition;

        // position to hover back to
        private Vector3 repositioningPosition;
    }
}
