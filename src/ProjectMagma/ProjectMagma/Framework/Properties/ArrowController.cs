using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Collision;

namespace ProjectMagma.Framework
{
    public class ArrowControllerProperty : Property
    {

        public ArrowControllerProperty()
        {
        }

        private void OnUpdate(Entity powerupEntity, GameTime gameTime)
        {
        }

        public void OnAttached(Entity arrow)
        {
            this.arrow = arrow;
            this.island = null;

            // register island change handler
            arrow.GetStringAttribute("island").ValueChanged += OnIslandChanged;

            arrow.Update += OnUpdate;
        }

        public void OnDetached(
            Entity entity
        )
        {
            arrow.Update -= OnUpdate;
            if(island != null)
                island.GetVector3Attribute("position").ValueChanged -= OnIslandPositionChanged;
        }

        private void OnIslandChanged(StringAttribute sender,
            String oldIsland, String newIsland)
        {
            if(island != null)
                island.GetVector3Attribute("position").ValueChanged -= OnIslandPositionChanged;


            if ("" == newIsland)
            {
                island = null;
            }
            else
            {
                island = Game.Instance.EntityManager[newIsland];
                island.GetVector3Attribute("position").ValueChanged += OnIslandPositionChanged;
            }
        }

        private void OnIslandPositionChanged(
            Vector3Attribute positionAttribute,
            Vector3 oldPosition,
            Vector3 newPosition
        )
        {
            // todo get position from model
            this.arrow.SetVector3("position", newPosition + Vector3.UnitY*30);
        }

        private Entity arrow;
        private Entity island;
    }
}
