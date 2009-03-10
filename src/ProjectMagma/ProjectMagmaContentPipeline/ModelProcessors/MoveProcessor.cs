using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

// TODO: replace these with the processor input and output types.
using TInput = System.String;
using TOutput = System.String;

namespace ProjectMagmaContentPipeline.ModelProcessors
{
    public abstract class MoveProcessor : ModelProcessor
    {
        protected class BoundingBox
        {
            public BoundingBox()
            {
                min = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
                max = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
            }

            public Vector3 min;
            public Vector3 max;
        }

        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            // calculate bounds because changes are based on the bounding box
            BoundingBox bb = new BoundingBox();
            CalculateBoundingBox(input, context, bb);

            // first center the models (I think they are actually already centered...
            Vector3 diff = Vector3.Zero - (bb.min + ((bb.max - bb.min) / 2.0f));
            MoveModel(input, context, diff);
            bb.max += diff;
            bb.min += diff;

            // now that the models are centered scale them
            float scaleFactor = bb.max.X;
            if (bb.max.Y > scaleFactor) scaleFactor = bb.max.Y;
            if (bb.max.Z > scaleFactor) scaleFactor = bb.max.Z;
            scaleFactor = 1.0f / scaleFactor;
            ScaleModel(input, context, scaleFactor);
            bb.max *= scaleFactor;
            bb.min *= scaleFactor;

            // now let the subclass decide on how to modify the box (aligning bottom/top to zero)
            float heightDiff = CalculateHeightDiff(bb);
            MoveModel(input, context, new Vector3(0, heightDiff, 0));

            return base.Process(input, context);
        }

        protected abstract float CalculateHeightDiff(BoundingBox bb);

        private void CalculateBoundingBox(
            NodeContent input,
            ContentProcessorContext context,
            BoundingBox box
            )
        {
            MeshContent mesh = input as MeshContent;
            if (mesh != null)
            {
                foreach (Vector3 pos in mesh.Positions)
                {
                    if (pos.X < box.min.X) { box.min.X = pos.X; }
                    if (pos.Y < box.min.Y) { box.min.Y = pos.Y; }
                    if (pos.Z < box.min.Z) { box.min.Z = pos.Z; }
                    if (pos.X > box.max.X) { box.max.X = pos.X; }
                    if (pos.Y > box.max.Y) { box.max.Y = pos.Y; }
                    if (pos.Z > box.max.Z) { box.max.Z = pos.Z; }
                }
            }

            // Go through all childs
            foreach (NodeContent child in input.Children)
            {
                CalculateBoundingBox(child, context, box);
            }
        }

        private void MoveModel(
            NodeContent input,
            ContentProcessorContext context,
            Vector3 diff
            )
        {
            MeshContent mesh = input as MeshContent;
            if (mesh != null)
            {
                for (int i = 0; i < mesh.Positions.Count; ++i)
                {
                    mesh.Positions[i] = mesh.Positions[i] + diff;
                }
            }

            // Go through all childs
            foreach (NodeContent child in input.Children)
            {
                MoveModel(child, context, diff);
            }
        }

        private void ScaleModel(
            NodeContent input,
            ContentProcessorContext context,
            float scaleFactor
            )
        {
            MeshContent mesh = input as MeshContent;
            if (mesh != null)
            {
                for (int i = 0; i < mesh.Positions.Count; ++i)
                {
                    mesh.Positions[i] = mesh.Positions[i]*scaleFactor;
                }
            }

            // Go through all childs
            foreach (NodeContent child in input.Children)
            {
                ScaleModel(child, context, scaleFactor);
            }
        }
    }
}