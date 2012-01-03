using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using ProjectMagma.Simulation.Collision;
using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
using ProjectMagma.Shared.Math.Primitives;
using System.Diagnostics;
using ProjectMagma.Shared.LevelData;

namespace ProjectMagma.Simulation
{
    public class SpawnControllerProperty : RobotBaseProperty
    {
        private float landedAt = -1;

        Entity spawnLight;

        public SpawnControllerProperty()
        {
        }

        public override void OnAttached(AbstractEntity player)
        {
            base.OnAttached(player);

            player.AddBoolAttribute("abortRespawning", false);

            this.OnActivated += OnSelfActivated;
            this.OnDeactivated += OnSelfDectivated;
        }

        public override void OnDetached(AbstractEntity player)
        {
            base.OnDetached(player);
        }

        private void OnSelfActivated(Property property)
        {
            player.SetBool("abortRespawning", false);
            Debug.Assert(spawnLight == null);
            if (spawnLight == null)
            {
                PositionOnRandomIsland();
                AddSpawnLight(player);
            }
            landedAt = -1;
        }

        private void OnSelfDectivated(Property property)
        {
            Debug.Assert(spawnLight != null);
            Game.Instance.Simulation.EntityManager.RemoveDeferred(spawnLight);
            spawnLight = null;

            player.GetProperty<Property>("controller").Activate();
            player.GetProperty<Property>("burnable").Activate();
            player.GetProperty<Property>("hud").Activate();
        }

        /// <summary>
        /// positions the player randomly on an island
        /// </summary>
        private void PositionOnRandomIsland()
        {
            int cnt = Game.Instance.Simulation.IslandManager.Count;
            Entity island = Game.Instance.Simulation.IslandManager[0];
            // try at most rounds times
            const int rounds = 3;
            int start = rand.Next(cnt - 1);
            for (int i = 0; i < cnt * rounds; i++)
            {
                bool valid = true;
                int islandNo = (start + i) % cnt;
                island = Game.Instance.Simulation.IslandManager[islandNo];

                // check if there is an island above this one
                foreach (Entity other in Game.Instance.Simulation.IslandManager)
                {
                    if (other != island)
                    {
                        float radius = (island.GetVector3(CommonNames.Scale) * new Vector3(1, 0, 1)).Length();
                        float otherRadius = (other.GetVector3(CommonNames.Scale) * new Vector3(1, 0, 1)).Length();
                        Vector3 pos = island.GetVector3(CommonNames.Position);
                        Vector3 opos = other.GetVector3(CommonNames.Position);
                        float dist = (pos - opos).Length();
                        // are they overlapping in xz?
                        if (dist < radius + otherRadius && opos.Y > pos.Y)
                        {
                            //                            Console.WriteLine("selected island "+other.Name+" above "+island.Name);
                            island = other;
                            break;
                        }
                    }
                }

                // check no players on island
                if (island.GetInt("players_targeting_island") > 0)
                {
                    valid = false;
                    //                    Console.WriteLine("player: " + player.Name + " rejected island: " + island.Name + " (" + island.GetInt("players_on_island") + ")");
                }

                // for 3rd round we accept low y islands
                if (i < cnt * 2)
                {
                    // check island is high enough
                    if (island.GetVector3(CommonNames.Position).Y < 100) // todo: extract constant
                    {
                        valid = false;
                    }

                    // for 2nd round (> cnt*2) we accept respawn on powerups
                    if (i < cnt)
                    {
                        // check no powerup on island
                        foreach (Entity powerup in Game.Instance.Simulation.PowerupManager)
                        {
                            if (island.Name == powerup.GetString("island_reference"))
                            {
                                valid = false;
                                break;
                            }
                        }

                        // check island is far enough away from other players,
                        foreach (Entity p in Game.Instance.Simulation.PlayerManager)
                        {
                            Vector3 dist = island.GetVector3(CommonNames.Position) - p.GetVector3(CommonNames.Position);
                            dist.Y = 0; // ignore y component
                            if (dist.Length() < constants.GetFloat("respawn_min_distance_to_players"))
                            {
                                valid = false;
                                break;
                            }
                        }
                    }
                }

                // re-random each round
                if (i % cnt == 0
                    && i > 0)
                {
                    start = rand.Next(cnt - 1);
                }

                if (valid)
                    break; // ok
                else
                    continue; // select another
            }

            SetDestinationIsland(island);

            player.SetVector3(CommonNames.Position, GetLandingPosition(island) + Vector3.UnitY * 500);
        }

        protected override void OnUpdate(Entity player, SimulationTime simTime)
        {
            if (player.GetBoolAttribute("ready").Value && landedAt > -1)
            {
                if (Game.Instance.Simulation.Phase != SimulationPhase.Intro)
                {
                    SetActiveIsland(destinationIsland);
                    Deactivate();
                }
                return;
            }

            PerformSpawnMovement(player, simTime);

            if (controllerInput.jumpButtonPressed)
            {
                player.SetBool("abortRespawning", true);
            }
        }

        private void AddSpawnLight(AbstractEntity player)
        {
            spawnLight = new Entity("spawn_light_" + player.Name);
            spawnLight.AddStringAttribute("player", player.Name);
            spawnLight.AddStringAttribute("island", destinationIsland.Name);

            Vector3 position = player.GetVector3(CommonNames.Position);
            Vector3 surfacePos;
            Simulation.GetPositionOnSurface(ref position, destinationIsland, out surfacePos);
            spawnLight.AddVector3Attribute(CommonNames.Position, surfacePos);

            Game.Instance.Simulation.EntityManager.AddDeferred(spawnLight, "spawn_light_base", templates);

            // and sound
            Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.Respawn);
        }

        private void PerformSpawnMovement(Entity player, SimulationTime simTime)
        {
            if (landedAt > -1)
            {
                if (simTime.At > landedAt + 2500) // extract constant
                {
                    player.SetBool("ready", true);
                }
                else
                    if (simTime.At > landedAt + 1000
                        && spawnLight != null
                        && spawnLight.GetBool(CommonNames.Hide) == false) // todo: extract constant
                    {
                        spawnLight.SetBool(CommonNames.Hide, true);
                    }
                return;
            }

            Vector3 velocity = -Vector3.UnitY * constants.GetFloat("max_gravity_speed");
            Vector3 position = player.GetVector3(CommonNames.Position) + velocity * simTime.Dt;

            Vector3 surfacePos;
            if (Simulation.GetPositionOnSurface(ref position, destinationIsland, out surfacePos))
            {
                if (position.Y < surfacePos.Y
                    || player.GetBool("abortRespawning"))
                {
                    position = surfacePos;

                    player.GetProperty<RobotRenderProperty>("render").NextOnceState = "jump_end";
                    destinationIsland.GetProperty<IslandRenderProperty>("render").Squash();

                    landedAt = simTime.At;
                }
            }
            else
            {
                Debug.Write("island's gone :(");
                position = GetLandingPosition(destinationIsland);
            }

            player.SetVector3(CommonNames.Position, position);
        }

    }
}

