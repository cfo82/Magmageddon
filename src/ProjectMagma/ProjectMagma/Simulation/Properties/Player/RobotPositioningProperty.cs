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
    public sealed class RobotPositioningProperty : Property
    {
        internal Entity player;
        internal Entity constants;

        public RobotPositioningProperty()
        {
        }

        public override void OnAttached(AbstractEntity player)
        {
            this.player = player as Entity;
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];
        }

        public override void OnDetached(AbstractEntity player)
        {
        }


        public void registerIslandPositionHandler(Entity island)
        {
            Debug.Assert(island != null);
            island.GetAttribute<Vector3Attribute>(CommonNames.Position).ValueChanged += IslandPositionHandler;
        }

        public void unregisterIslandPositionHandler(Entity island)
        {
            Debug.Assert(island != null);
            island.GetAttribute<Vector3Attribute>(CommonNames.Position).ValueChanged -= IslandPositionHandler;
        }

        private void IslandPositionHandler(Vector3Attribute sender, Vector3 oldValue, Vector3 newValue)
        {
//            Debug.WriteLine("@"+this.GetType()+","+Game.Instance.Simulation.Time.At+": position of " + player.Name + " changed to " + newValue);
            Vector3 delta = newValue - oldValue;
            player.SetVector3(CommonNames.PreviousPosition, player.GetVector3(CommonNames.PreviousPosition) + delta);
            player.SetVector3(CommonNames.Position, player.GetVector3(CommonNames.Position) + delta);
        }

    }
}

