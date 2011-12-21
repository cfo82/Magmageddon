using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

using ProjectMagma.Simulation.Collision;
using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
using Microsoft.Xna.Framework.Audio;

namespace ProjectMagma.Simulation
{
    /// <summary>
    /// This class controls the position of a so called decoration object. A decoration
    /// entity is placed on the map and is attached to another entity. It then moves together
    /// with the entity it is attached to.
    /// 
    /// The decoration entity must declare two attributes:
    ///     - attached_to:string: specifies the name of the entity to which this decoration is attached
    ///     - attachment_point:string: specifies the name of the attachment point. 
    /// The entity to which this decoration is attached must declare a single attribute:
    ///     - [attachment_point_name]:float3: the attachment_point_name is the name specified above (through
    ///             the attribute attachment_point) and contains the position of the attachment point relative
    ///             to the entities position.
    /// </summary>
    public class DecorationPositionController : Property
    {
        public DecorationPositionController()
        {
        }

        public override void OnAttached(
            AbstractEntity entity
        )
        {
            decoration = entity as Entity;
            attachedTo = Game.Instance.Simulation.EntityManager[entity.GetString(CommonNames.AttachedTo)];

            Debug.Assert(decoration.HasString(CommonNames.AttachmentPoint), "must specify to which point this entity is to be attached");
            attachmentPointName = decoration.GetString(CommonNames.AttachmentPoint);

            Debug.Assert(attachedTo.HasVector3(attachmentPointName), "attachedTo must have the attachment point desired.");
            if (!attachedTo.HasVector3(attachmentPointName))
                { throw new Exception(string.Format("attachedTo {0} is missing the attachment point '{1}'", attachedTo.Name, attachmentPointName)); }

            attachedTo.GetVector3Attribute(CommonNames.Position).ValueChanged += OnAttachedToPositionChanged;

            // initialize properties
            if (!decoration.HasAttribute(CommonNames.Position))
                { decoration.AddVector3Attribute(CommonNames.Position, Vector3.Zero); }

            Vector3 islandPos = attachedTo.GetVector3(CommonNames.Position);
            PositionOnIsland(ref islandPos);
        }

        public override void OnDetached(
            AbstractEntity entity
        )
        {
            attachedTo.GetVector3Attribute(CommonNames.Position).ValueChanged -= OnAttachedToPositionChanged;
        }

        private void OnAttachedToPositionChanged(
            Vector3Attribute positionAttribute,
            Vector3 oldPosition,
            Vector3 newPosition
        )
        {
            PositionOnIsland(ref newPosition);
        }

        private void PositionOnIsland(ref Vector3 position)
        {
            Vector3 attachmentPoint = attachedTo.GetVector3(attachmentPointName);
            decoration.SetVector3(CommonNames.Position, position + attachmentPoint);
        }

        protected Entity decoration;
        protected Entity attachedTo;
        protected string attachmentPointName;
    }
}
