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
    public abstract class RobotBaseProperty : ActiveProperty
    {
        internal Entity player;
        internal Entity constants;
        internal LevelData templates;
        internal PlayerIndex playerIndex;
        internal ControllerInput controllerInput;

        protected readonly Random rand = new Random(DateTime.Now.Millisecond);

        private Entity _activeIsland = null;
        private Entity _destinationIsland = null;

        protected Entity activeIsland
        {
            private set
            {
                player.SetString("active_island", (value != null)?value.Name:"");
            }

            get
            {
                return _activeIsland;
            }
        }
        protected Entity destinationIsland
        {
            private set
            {
                player.SetString("destination_island", (value != null) ? value.Name : "");
            }

            get
            {
                return _destinationIsland;
            }
        }

        public RobotBaseProperty()
        {
        }

        public override void OnAttached(AbstractEntity player)
        {
            base.OnAttached(player);

            this.player = player as Entity;
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];
            this.templates = Game.Instance.ContentManager.Load<LevelData>("Level/Common/DynamicTemplates");
            this.playerIndex = (PlayerIndex)player.GetInt(CommonNames.GamePadIndex);
            this.controllerInput = player.GetProperty<InputProperty>("input").ControllerInput;

            this.player.GetStringAttribute("active_island").ValueChanged += OnActiveIslandChanged;
            this.player.GetStringAttribute("destination_island").ValueChanged += OnDestinationIslandChanged;
        }

        public override void OnDetached(AbstractEntity player)
        {
            base.OnDetached(player);

            this.player.GetStringAttribute("active_island").ValueChanged -= OnActiveIslandChanged;
            this.player.GetStringAttribute("destination_island").ValueChanged -= OnDestinationIslandChanged;
        }

        private void OnActiveIslandChanged(StringAttribute sender, string oldValue, string newValue)
        {
            if (newValue != "")
            {
                Debug.Assert(Game.Instance.Simulation.EntityManager[newValue] != null);
                this._activeIsland = Game.Instance.Simulation.EntityManager[newValue];
            }
            else
            {
                this._activeIsland = null;
            }
        }

        private void OnDestinationIslandChanged(StringAttribute sender, string oldValue, string newValue)
        {
            if (newValue != "")
            {
                Debug.Assert(Game.Instance.Simulation.EntityManager[newValue] != null);
                this._destinationIsland = Game.Instance.Simulation.EntityManager[newValue];
            }
            else
            {
                this._destinationIsland = null;
            }
        }

        protected virtual void SetDestinationIsland(Entity island)
        {
            Debug.Assert(island != null);
            Debug.WriteLine("Set destination island of " + player.Name + " to " + island.Name);

            Debug.Assert(destinationIsland != island);

            island.GetAttribute<Vector3Attribute>(CommonNames.Position).ValueChanged += IslandPositionHandler;
            island.SetInt("players_targeting_island", island.GetInt("players_targeting_island") + 1);

            destinationIsland = island;
        }

        protected virtual void ResetDestinationIsland()
        {
            if (destinationIsland != null)
            {
                Debug.WriteLine("Reset destination island of " + player.Name + " from " + destinationIsland.Name);

                destinationIsland.GetAttribute<Vector3Attribute>(CommonNames.Position).ValueChanged -= IslandPositionHandler;
                destinationIsland.SetInt("players_targeting_island", destinationIsland.GetInt("players_targeting_island") - 1);

                destinationIsland = null;
            }
        }

        protected virtual void SetActiveIsland(Entity island)
        {
            Debug.Assert(island != null);
            Debug.WriteLine("Set active island of " + player.Name + " to " + island.Name);

            if (destinationIsland != null)
            {
                Debug.Assert(destinationIsland == island);
                ResetDestinationIsland();
            }

            Debug.Assert(activeIsland == null);
            Debug.Assert(player.GetStringAttribute("active_island").StringValue == "");

            // register with active
            island.GetAttribute<Vector3Attribute>(CommonNames.Position).ValueChanged += IslandPositionHandler;
            island.SetInt("players_on_island", island.GetInt("players_on_island") + 1);

            // set
            activeIsland = island;
        }

        protected virtual void LeaveActiveIsland()
        {
            if (activeIsland != null)
            {
                Debug.WriteLine("Reset active island of " + player.Name + " from " + activeIsland.Name);

                activeIsland.GetAttribute<Vector3Attribute>(CommonNames.Position).ValueChanged -= IslandPositionHandler;
                activeIsland.SetInt("players_on_island", activeIsland.GetInt("players_on_island") - 1);

                activeIsland = null;
            }
        }

        protected void IslandPositionHandler(Vector3Attribute sender, Vector3 oldValue, Vector3 newValue)
        {
            Vector3 delta = newValue - oldValue;
            player.SetVector3(CommonNames.PreviousPosition, player.GetVector3(CommonNames.PreviousPosition) + delta);
            player.SetVector3(CommonNames.Position, player.GetVector3(CommonNames.Position) + delta);
        }

        protected Vector3 GetLandingPosition(Entity island)
        {
            return GetLandingPosition(player, island);
        }

        public static Vector3 GetLandingPosition(Entity player, Entity island)
        {
            Vector3 pos;
            int pi = player.GetInt(CommonNames.GamePadIndex) + 1;
            if (island.HasAttribute("landing_offset_p" + pi))
            {
                pos = island.GetVector3(CommonNames.Position) + island.GetVector3("landing_offset_p" + pi);
            }
            else
            {
                pos = island.GetVector3(CommonNames.Position) + island.GetVector3("landing_offset");
            }
            return pos;
        }

    }
}

