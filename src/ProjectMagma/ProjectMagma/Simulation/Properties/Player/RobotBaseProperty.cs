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
    public abstract class RobotBaseProperty : Property
    {
        internal Entity player;
        internal Entity constants;
        internal LevelData templates;
        internal PlayerIndex playerIndex;
        internal ControllerInput controllerInput;

        protected readonly Random rand = new Random(DateTime.Now.Millisecond);

        protected Entity activeIsland = null;

        public RobotBaseProperty()
        {
        }

        public override void OnAttached(AbstractEntity player)
        {
            (player as Entity).OnUpdate += OnUpdate;

            this.player = player as Entity;
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];
            this.templates = Game.Instance.ContentManager.Load<LevelData>("Level/Common/DynamicTemplates");
            this.playerIndex = (PlayerIndex)player.GetInt(CommonNames.GamePadIndex);
            this.controllerInput = player.GetProperty<InputProperty>("input").ControllerInput;
        }

        public override void OnDetached(AbstractEntity player)
        {
            (player as Entity).OnUpdate -= OnUpdate;
        }

        protected abstract void OnUpdate(Entity player, SimulationTime simTime);

        /// <summary>
        /// sets the activeisland
        /// </summary>
        protected virtual void SetActiveIsland(Entity island)
        {
            // register with active
            island.GetAttribute<Vector3Attribute>(CommonNames.Position).ValueChanged += IslandPositionHandler;
            island.SetInt("players_on_island", island.GetInt("players_on_island") + 1);

            // set
            activeIsland = island;
            player.SetString("active_island", island.Name);
        }

        protected void IslandPositionHandler(Vector3Attribute sender, Vector3 oldValue, Vector3 newValue)
        {
            Vector3 position = player.GetVector3(CommonNames.Position);
            Vector3 delta = newValue - oldValue;
            position += delta;
            player.SetVector3(CommonNames.PreviousPosition, position);
            player.SetVector3(CommonNames.Position, position);
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

