using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Framework
{
    public class PowerUpController : Property
    {
        public PowerUpController()
        {
        }

        public void OnAttached(
            Entity entity
        )
        {
            this.powerupEntity = entity;
            this.islandEntity = Game.Instance.EntityManager[entity.GetString("island_reference")];

            // initialize properties
            Debug.Assert(this.powerupEntity.HasVector3("relative_position"), "must have a relative translation attribute");
            if (!this.powerupEntity.HasAttribute("position"))
            {
                this.powerupEntity.AddAttribute("position", "float3", "0 0 0"); // position is set later on...
            }

            // initialize position on island
            Debug.Assert(islandEntity.HasVector3("position"), "the island must have a position attribute.");
            this.powerupEntity.SetVector3("position", islandEntity.GetVector3("position"));

            // register change handler
            this.islandEntity.GetVector3Attribute("position").ValueChanged += new Vector3ChangeHandler(OnIslandPositionChanged);
        }

        public void OnDetached(
            Entity entity
        )
        {
            entity.Update -= new UpdateHandler(OnUpdate);
        }

        private void OnUpdate(
            Entity entity,
            GameTime gameTime
        )
        {
        }

        private void OnIslandPositionChanged(
            Vector3Attribute positionAttribute,
            Vector3 oldPosition,
            Vector3 newPosition
        )
        {
            this.powerupEntity.SetVector3("position", newPosition + this.powerupEntity.GetVector3("relative_position"));
        }

        private Entity powerupEntity;
        private Entity islandEntity;
    }
}
