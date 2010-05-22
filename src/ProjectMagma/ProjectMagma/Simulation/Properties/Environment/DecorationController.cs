using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using ProjectMagma.Simulation.Collision;
using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
using Microsoft.Xna.Framework.Audio;

namespace ProjectMagma.Simulation
{
    public class DecorationController : Property
    {
        public static readonly string AttachmentPoint = "attachment_point";
        public static readonly string Position = "position";
        public static readonly string IslandReference = "island_reference";

        public DecorationController()
        {
        }

        public void OnAttached(
            AbstractEntity entity
        )
        {
            this.decoration = entity as Entity;
            this.island = Game.Instance.Simulation.EntityManager[entity.GetString(IslandReference)];

            Debug.Assert(this.decoration.HasString(AttachmentPoint), "must specify to which point this entity is to be attached");
            attachmentPointName = decoration.GetString(AttachmentPoint);

            Debug.Assert(this.island.HasVector3(attachmentPointName), "island must have the attachment point desired.");
            if (!this.island.HasVector3(attachmentPointName))
                { throw new Exception(string.Format("island {0} is missing the attachment point '{1}'", island.Name, attachmentPointName)); }

            island.GetVector3Attribute(Position).ValueChanged += OnIslandPositionChanged;

            // initialize properties
            if (!this.decoration.HasAttribute(Position))
                { this.decoration.AddVector3Attribute(Position, Vector3.Zero); }

            Vector3 islandPos = island.GetVector3(Position);
            PositionOnIsland(ref islandPos);
        }

        public void OnDetached(
            AbstractEntity entity
        )
        {
            this.island.GetVector3Attribute(Position).ValueChanged -= OnIslandPositionChanged;
        }

        private void OnIslandPositionChanged(
            Vector3Attribute positionAttribute,
            Vector3 oldPosition,
            Vector3 newPosition
        )
        {
            PositionOnIsland(ref newPosition);
        }

        private void PositionOnIsland(ref Vector3 position)
        {
            Vector3 attachmentPoint = island.GetVector3(attachmentPointName);
            decoration.SetVector3(Position, position + attachmentPoint);
        }

        protected Entity decoration;
        protected Entity island;
        protected string attachmentPointName;
    }
}
