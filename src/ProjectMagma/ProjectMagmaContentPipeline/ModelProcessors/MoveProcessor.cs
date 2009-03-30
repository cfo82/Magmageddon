using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using ProjectMagma.Shared.Math.Volume;

namespace ProjectMagma.ContentPipeline.ModelProcessors
{
    public abstract class MoveProcessor : ModelProcessor
    {
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            // calculate bounds because changes are based on the bounding box
            AlignedBox3 bb = CalculateAlignedBox3(input, context);

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

            // now let the subclass decide on how to modify the position
            Vector3 scaledOrigDiff = diff * scaleFactor;
            Vector3 diffCorrector = CalculateDiff(ref scaledOrigDiff, ref bb);
            MoveModel(input, context, diffCorrector);
            bb.Min += diffCorrector;
            bb.Max += diffCorrector;

            // let the base class process the model
            ModelContent modelContent = base.Process(input, context);

            // add bounding volumes to the model
            VolumeCollection collection = new VolumeCollection();
            collection.AddVolume(CalculateAlignedBox3(input, context));
            collection.AddVolume(CalculateCylinder3(input, context));
            collection.AddVolume(CalculateSphere3(input, context));
            modelContent.Tag = collection;
            return modelContent;
        }

        protected virtual Vector3 CalculateDiff(ref Vector3 origDiff, ref AlignedBox3 bb)
        {
            return Vector3.Zero;
        }

        private AlignedBox3 CalculateAlignedBox3(
            NodeContent input,
            ContentProcessorContext context
        )
        {
            AlignedBox3 alignedBox = new AlignedBox3();
            alignedBox.Min = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
            alignedBox.Max = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
            CalculateAlignedBox3(input, context, ref alignedBox);
            return alignedBox;
        }

        private void CalculateAlignedBox3(
            NodeContent input,
            ContentProcessorContext context,
            ref AlignedBox3 box
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

            // Go through all children
            foreach (NodeContent child in input.Children)
            {
                CalculateAlignedBox3(child, context, ref box);
            }
        }

        // calculates y-axis aligned bounding cylinder
        private Cylinder3 CalculateCylinder3(
            NodeContent input,
            ContentProcessorContext context
        )
        {
            // calculate center
            AlignedBox3 bb = CalculateAlignedBox3(input, context);
            Vector3 center = (bb.Min + bb.Max) / 2;

            float top = bb.Max.Y;
            float bottom = bb.Min.Y;

            // calculate radius
            // a valid cylinder here is an extruded circle (not an oval) therefore extents in 
            // x- and z-direction should be equal.
            float radius = bb.Max.X - center.X;

            return new Cylinder3(new Vector3(center.X, top, center.Z),
                new Vector3(center.X, bottom, center.Z),
                radius);
        }

        private Sphere3 CalculateSphere3(
            NodeContent input,
            ContentProcessorContext context
        )
        {
            // calculate center
            AlignedBox3 bb = CalculateAlignedBox3(input, context);
            Vector3 center = (bb.Min + bb.Max) / 2;

            // calculate radius
            //            float radius = (bb.Max-bb.Min).Length() / 2;
            float radius = (bb.Max.Y - bb.Min.Y) / 2; // HACK: hack for player

            return new Sphere3(center, radius);
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
                    Matrix inverseTransform = Matrix.Invert(mesh.AbsoluteTransform);
                    Vector3 position = Vector3.Transform(mesh.Positions[i], mesh.AbsoluteTransform);
                    position += diff;
                    position = Vector3.Transform(position, inverseTransform);

                    mesh.Positions[i] = position;
                }
            }

            // Go through all children
            foreach (NodeContent child in input.Children)
            {
                MoveModel(child, context, diff);
            }
        }

        private void ScaleModel(
            NodeContent input,
            ContentProcessorContext context,
            float scaleFactor
        ) {
            MeshContent mesh = input as MeshContent;
            if (mesh != null)
            {
                for (int i = 0; i < mesh.Positions.Count; ++i)
                {
                    Matrix inverseTransform = Matrix.Invert(mesh.AbsoluteTransform);
                    Vector3 position = Vector3.Transform(mesh.Positions[i], mesh.AbsoluteTransform);
                    position *= scaleFactor;
                    position = Vector3.Transform(position, inverseTransform);

                    mesh.Positions[i] = position;
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