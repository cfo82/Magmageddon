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

namespace ProjectMagmaContentPipeline
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

            BoundingBox bb = new BoundingBox();
            CalculateBoundingBox(input, context, bb);
            float heightDiff = CalculateHeightDiff(bb);
            Move(input, context, heightDiff);

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

        private void Move(
            NodeContent input,
            ContentProcessorContext context,
            float heightDiff
            )
        {
            MeshContent mesh = input as MeshContent;
            if (mesh != null)
            {
                for (int i = 0; i < mesh.Positions.Count; ++i)
                {
                    mesh.Positions[i] = new Vector3(
                        mesh.Positions[i].X,
                        mesh.Positions[i].Y + heightDiff,
                        mesh.Positions[i].Z);
                }
            }

            // Go through all childs
            foreach (NodeContent child in input.Children)
            {
                Move(child, context, heightDiff);
            }
        }

    }
}