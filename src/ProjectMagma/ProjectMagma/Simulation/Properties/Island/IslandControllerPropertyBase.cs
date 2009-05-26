using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Collision;
using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;


namespace ProjectMagma.Simulation
{
    public abstract class IslandControllerPropertyBase : Property
    {
        public IslandControllerPropertyBase()
        {
            OnRepulsionEndAction = new PushBackFinishedHandler(OnRepulsionEnd);
        }

        public virtual void OnAttached(AbstractEntity entity)
        {
            Debug.Assert(entity.HasVector3("position"));

            this.island = entity as Entity;
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

            (entity as Entity).Update += OnUpdate;

            entity.GetProperty<CollisionProperty>("collision").OnContact += CollisionHandler;
//            ((Vector3Attribute)entity.GetAttribute("repulsion_velocity")).ValueChanged += RepulsionChangeHandler;
            entity.GetAttribute<StringAttribute>("repulsed_by").ValueChanged += RepulsedByChangeHandler;
            entity.GetAttribute<IntAttribute>("players_on_island").ValueChanged += PlayersOnIslandChangeHandler;

            originalPosition = entity.GetVector3("position");
        }

        public virtual void OnDetached(AbstractEntity entity)
        {
            (entity as Entity).Update -= OnUpdate;
            entity.GetProperty<CollisionProperty>("collision").OnContact -= CollisionHandler;
//            ((Vector3Attribute)entity.GetAttribute("repulsion_velocity")).ValueChanged -= RepulsionChangeHandler;
            entity.GetAttribute<StringAttribute>("repulsed_by").ValueChanged -= RepulsedByChangeHandler;
            entity.GetAttribute<IntAttribute>("players_on_island").ValueChanged -= PlayersOnIslandChangeHandler;
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
            bool interactable;
            if (Game.Instance.Simulation.Phase == SimulationPhase.Game)
            {
                interactable = playersOnIsland > 0;
                if (!interactable)
                {
                    foreach (Entity player in Game.Instance.Simulation.PlayerManager)
                    {
                        float dist = (position - player.GetVector3("position")).Length();
                        if (dist < playerConstants.GetFloat("island_jump_free_range"))
                            interactable = true;
                    }
                }
            }
            else
            {
                // islands are interactable only during game phase
                interactable = false;
            }
            island.SetBool("interactable", interactable);

            // implement sinking islands
            if (!HadCollision(simTime)
                && Game.Instance.Simulation.Phase == SimulationPhase.Game) // only sink in game-phase
            {
                if (playersOnIsland > 0)
                {
                    position -= dt * island.GetFloat("sinking_speed") * playersOnIsland * (Vector3.UnitY);
                }
            }

            // set repositioning on players left
            if (playersOnIsland == 0)
            {
                if (simTime.At > playerLeftAt + constants.GetInt("rising_delay"))
                {
                    // rising using normal repositioning
                    state = IslandState.Repositioning;
                    playerLeftAt = double.MaxValue;
                }
            }

            // apply repulsion from players 
            Vector3 repulsionVelocity = island.GetVector3("repulsion_velocity");
            if (repulsionVelocity.Length() > island.GetFloat("repulsion_speed"))
                repulsionVelocity = Vector3.Normalize(repulsionVelocity) * island.GetFloat("repulsion_speed");
            Simulation.ApplyPushback(ref position, ref repulsionVelocity, constants.GetFloat("repulsion_deacceleration"));
            island.SetVector3("repulsion_velocity", repulsionVelocity);

            // apply pushback from other objects as long as there is a collision
            if (HadCollision(simTime))
            {
                Vector3 pushbackVelocity = island.GetVector3("pushback_velocity");
                if (pushbackVelocity.Length() > constants.GetFloat("collision_max_speed"))
                {
                    pushbackVelocity.Normalize();
                    pushbackVelocity *= constants.GetFloat("collision_max_speed");
                    island.SetVector3("pushback_velocity", pushbackVelocity);
                }
                Simulation.ApplyPushback(ref position, ref pushbackVelocity, constants.GetFloat("collision_deacceleration"), CheckDistance);
            }
            else
            {
                if (island.GetVector3("pushback_velocity") != Vector3.Zero)
                {
                    island.SetVector3("pushback_velocity", Vector3.Zero);
                    CheckDistance();
                }
            }
           
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
                    island.SetVector3("repositioning_velocity", Vector3.Zero);
                }

                Vector3 desiredPosition = repositioningPosition;
                // if players are standing on island, we only reposition in xz
                if (playersOnIsland > 0
                    || (simTime.At < playerLeftAt + constants.GetInt("rising_delay") // also wait for the delay
                    && playerLeftAt < float.MaxValue)) 
                {
                    // stay on y
                    desiredPosition.Y = position.Y;
                }

                // get direction of new repositioning effort
                Vector3 dir = desiredPosition - position;
                float dist = dir.Length();
                if (dir != Vector3.Zero)
                    dir.Normalize();

                Vector3 oldVelocity = island.GetVector3("repositioning_velocity");
                if (oldVelocity.Length() > island.GetFloat("repositioning_speed"))
                {
                    oldVelocity = Vector3.Normalize(oldVelocity) * island.GetFloat("repositioning_speed");
                }
                if (dist < 80 && oldVelocity != Vector3.Zero) // todo: extract constant
                {
                    oldVelocity -= oldVelocity * 0.6f * simTime.Dt * 25;
                }

                Vector3 velocity = oldVelocity;

                // acceleration
                Vector3 acc = dir * constants.GetFloat("repositioning_acceleraton");
                velocity += acc * dt;

                // calculate new position 
                Vector3 newPosition = position;
                newPosition += velocity * dt;

                // if acceleration takes us further away from object we stop
                if ((newPosition - desiredPosition).Length() > (position - desiredPosition).Length()
                    && (newPosition-desiredPosition).Length() < 40)
                {
//                    position = desiredPosition;
                    state = IslandState.Normal;
                    island.SetVector3("repositioning_velocity", Vector3.Zero);
                    OnRepositioningEnded(dir);
                }
                else
                {
                    position = newPosition;

                    island.SetVector3("repositioning_velocity", velocity);
                }
            }
            else
            {
                Vector3 velocity = island.GetVector3("velocity");
                // apply movement only if no collision
                if (!HadCollision(simTime))
                {
                    // normal movement
                    if (state != IslandState.Repulsed)
                    {
                        // calculate acceleration direction in subclass
                        Vector3 dir = CalculateAccelerationDirection(island, ref position, ref velocity, constants.GetFloat("movement_acceleration"), dt);
                        velocity += dir * constants.GetFloat("movement_acceleration") * dt;

                        if (velocity.Length() > island.GetFloat("movement_speed"))
                        {
                            velocity.Normalize();
                            velocity *= island.GetFloat("movement_speed");
                        }

                        island.SetVector3("velocity", velocity);
                    }
                }
                position += velocity * dt;
            }

            island.SetVector3("position", position);

            lastState = state;
        }

        // checks if distance to point on path is to far away so we need repositioning
        private void CheckDistance()
        {
            Vector3 pos = island.GetVector3("position");
            Vector3 ppos = GetNearestPointOnPath(ref pos);
            if ((pos - ppos).Length() > constants.GetFloat("repositioning_threshold")
                && state == IslandState.Normal)
            {
                state = IslandState.Repositioning;
            }
        }

        private void CollisionHandler(SimulationTime simTime, Contact contact)
        {
            Entity island = contact.EntityA;
            Entity other = contact.EntityB;
            if(other.HasString("kind"))
            {
                String kind = other.GetString("kind");

                if (// never do collision response with player who is standing or jumping on island
                    (other.HasString("active_island") && other.GetString("active_island") == island.Name)
                    || (other.HasString("jump_island") && other.GetString("jump_island") == island.Name)
                    || kind == "powerup" // with powerup neather
                    || other.HasBool("dynamic") // neither with dynamic entities
                    )
                {
                    // do nothing
                }
                else 
                {
                    if (kind == "island" && simTime.At > collisionAt + 250) // don't make to much noise
                    {
                        // play sound
                        Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.IslandHitsIsland);
                    }

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
                            - xznormal * constants.GetFloat("collision_acceleration") * simTime.Dt); // todo: extract constant
                    }

                    if (state == IslandState.Repositioning)
                    {
                        Vector3 repositioningVelocity = island.GetVector3("repositioning_velocity");
                        if (Vector3.Dot(Vector3.Normalize(repositioningVelocity), normal) > 0)
                        {
                            island.SetVector3("repositioning_velocity", Vector3.Reflect(repositioningVelocity, normal)
                                * constants.GetFloat("collision_damping"));
                        }
                    }
                    else
                        if (state == IslandState.Normal)
                        {
                            // pusbhack a bit
                            island.SetVector3("pushback_velocity", island.GetVector3("pushback_velocity")
                                - normal * constants.GetFloat("collision_acceleration") * simTime.Dt); // todo: extract constant

                            // call handler of child class
                            if (!HandleCollision(simTime, island, other, contact, ref normal))
                            {
                                // do simple reflection if no special collision handler provided by subclass
                                // or explicitly return of false
                                Vector3 xznormal = normal;
                                xznormal.Y = 0;
                                if (xznormal != Vector3.Zero)
                                    xznormal.Normalize();
                                // reflect velocity
                                Vector3 velocity = island.GetVector3("velocity");
                                if (velocity != Vector3.Zero)
                                {
                                    if (Vector3.Dot(Vector3.Normalize(velocity), xznormal) > 0)
                                    {
                                        island.SetVector3("velocity", Vector3.Reflect(velocity, xznormal)
                                            * constants.GetFloat("collision_damping"));
                                    }
                                }
                            }
                        }

                    collisionAt = simTime.At;
                }
            }
        }

        private Vector3 CalculatePseudoNormalIsland(Entity entityA, Entity entityB)
        {
            Vector3 normal;
            String kind = entityB.GetString("kind");
            if (kind == "pillar" || kind == "player")
            {
                normal = entityB.GetVector3("position")-entityA.GetVector3("position");
                normal.Y = 0;
            }
            else
            if (kind == "cave")
            {
                normal = entityA.GetVector3("position");
                normal.Y /= 2;
            }
            else
            {
                normal = entityB.GetVector3("position")-entityA.GetVector3("position");
            }
            if (normal != Vector3.Zero)
                normal.Normalize();
            return normal;
        }

        protected void RepulsedByChangeHandler(StringAttribute sender, String oldValue, String newValue)
        {
            if (oldValue == "" && newValue != "")
            {
                OnRepulsionStart();
            }
            else
                if (oldValue != "" && newValue == "")
                {
                    OnRepulsionEnd();
                }
        }

        protected void PlayersOnIslandChangeHandler(IntAttribute sender, int oldVlaue, int newValue)
        {
            if (newValue == 0)
            {
                playerLeftAt = Game.Instance.Simulation.Time.At;
            }
        }

        public abstract Vector3 CalculateAccelerationDirection(Entity island, ref Vector3 position, ref Vector3 velocity, float acceleration, float dt);

        /// <summary>
        /// gets nearest position on islands original path from given position 
        /// to reposition island to
        /// </summary>
        protected abstract Vector3 GetNearestPointOnPath(ref Vector3 position);

        protected virtual bool HandleCollision(SimulationTime simTime, Entity island, Entity other, Contact co, ref Vector3 normal)
        {
            return false;
        }

        protected virtual void OnRepulsionStart()
        {
            // reset normal movement
            island.SetVector3("velocity", Vector3.Zero);
            state = IslandState.Repulsed;
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
            Repulsed,             // island's track is influensed by the outside, normal movement is prohibited
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
