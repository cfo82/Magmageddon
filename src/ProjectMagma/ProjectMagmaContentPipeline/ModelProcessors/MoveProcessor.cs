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
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            // calculate bounds because changes are based on the bounding box
            BoundingBox bb = new BoundingBox();
            bb.Min = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
            bb.Max = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
            CalculateBoundingBox(input, context, ref bb);

            // first center the models (I think they are actually already centered...
            Vector3 diff = Vector3.Zero - (bb.Min + ((bb.Max - bb.Min) / 2.0f));
            MoveModel(input, context, diff);
            bb.Max += diff;
            bb.Min += diff;

            // now that the models are centered scale them
            float scaleFactor = bb.Max.X;
            if (bb.Max.Y > scaleFactor) scaleFactor = bb.Max.Y;
            if (bb.Max.Z > scaleFactor) scaleFactor = bb.Max.Z;
            scaleFactor = 1.0f / scaleFactor;
            ScaleModel(input, context, scaleFactor);
            bb.Max *= scaleFactor;
            bb.Min *= scaleFactor;

            // now let the subclass decide on how to modify the box (aligning bottom/top to zero)
            float heightDiff = CalculateHeightDiff(ref bb);
            MoveModel(input, context, new Vector3(0, heightDiff, 0));

            ModelContent modelContent = base.Process(input, context);
            modelContent.Tag = bb;
            return modelContent;
        }

        protected abstract float CalculateHeightDiff(ref BoundingBox bb);

        private void CalculateBoundingBox(
            NodeContent input,
            ContentProcessorContext context,
            ref BoundingBox box
            )
        {
            MeshContent mesh = input as MeshContent;
            if (mesh != null)
            {
                foreach (Vector3 pos in mesh.Positions)
                {
                    Vector3 transformed = Vector3.Transform(pos, mesh.AbsoluteTransform);
                    if (transformed.X < box.Min.X) { box.Min.X = transformed.X; }
                    if (transformed.Y < box.Min.Y) { box.Min.Y = transformed.Y; }
                    if (transformed.Z < box.Min.Z) { box.Min.Z = transformed.Z; }
                    if (transformed.X > box.Max.X) { box.Max.X = transformed.X; }
                    if (transformed.Y > box.Max.Y) { box.Max.Y = transformed.Y; }
                    if (transformed.Z > box.Max.Z) { box.Max.Z = transformed.Z; }
                }
            }

            // Go through all childs
            foreach (NodeContent child in input.Children)
            {
                CalculateBoundingBox(child, context, ref box);
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
                    Vector3 translation;
                    Quaternion rotation;
                    Vector3 scale;

                    mesh.Transform.Decompose(out scale, out rotation, out translation);
                    translation *= scaleFactor;

                    mesh.Transform = Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(translation);

                    mesh.Positions[i] = mesh.Positions[i] * scaleFactor;
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