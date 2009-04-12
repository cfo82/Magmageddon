using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using ProjectMagma.Shared.Math.Primitives;

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
            collection.AddVolume(CalculateAlignedBox3Tree(input, context));
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

        private AlignedBox3Tree CalculateAlignedBox3Tree(
            NodeContent input,
            ContentProcessorContext context
        )
        {
            // collect positions and indices
            List<Vector3> positionList = new List<Vector3>();
            List<UInt16> indexList = new List<UInt16>();
            CalculateAlignedBox3Tree(input, context, positionList, indexList);
            if (indexList.Count % 3 != 0)
                { throw new Exception("invalid number of indices!"); }

            Vector3[] positions = new Vector3[positionList.Count];
            for (int i = 0; i < positionList.Count; ++i)
                { positions[i] = positionList[i]; }
            UInt16[] indices = new UInt16[indexList.Count];
            for (int i = 0; i < indexList.Count; ++i)
                { indices[i] = indexList[i]; }
            
            return new AlignedBox3Tree(positions, indices);
        }

        private void CalculateAlignedBox3Tree(
            NodeContent input,
            ContentProcessorContext context,
            List<Vector3> positions,
            List<UInt16> indices
            )
        {
            MeshContent mesh = input as MeshContent;
            if (mesh != null)
            {
                int basePosition = positions.Count;
                int baseIndex = indices.Count;

                // I think I may need to document this part a bit (hope it works as it is!). The internal XNA representation
                // of a model (using this MeshContent, etc.) is as follows:
                //   - a model is built of hierarchical nodes (transformations, meshes, etc.)
                //   - a MeshContent (which is of interest here) stores his transformation relative to the parent. to get the
                //     absolute transformation (regarding to the model space) use mesh.AbsoluteTransform
                //   - a mesh has a set of position vectors
                //   - each mesh consists of one or several GeometryContent instances. these represent single batches of 
                //     renderable geometry. they will later index into the 'global' MeshContent position array
                //   - GeometryContent as an array of vertices. Vertices may consist of several channels (this is like it's commonly
                //     done. The GeometryData index-array is a triangle list indexing into the Vertex data.
                //   - The vertex data maintains instead of a position channel a position index channel. this position index channel
                //     then indexes into the MeshContent position array. 
                // => to get correct position indices we need to first copy the position data into the list. then we have to 
                //    to iterate over all GeometryContent instances, iterate over the respective indices and use each index to 
                //    address a vertex-component-position index which in turn can then be used for the index array.

                foreach (Vector3 pos in mesh.Positions)
                {
                    Vector3 transformed = Vector3.Transform(pos, mesh.AbsoluteTransform);
                    positions.Add(transformed);
                }

                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    if (geometry.Indices.Count % 3 != 0)
                        { throw new Exception(string.Format("invalid number ({0}) of indices!", geometry.Vertices.PositionIndices.Count)); }

                    //geometry.Vertices.pos
                    for (int i = 0; i < geometry.Indices.Count; ++i)
                    {
                        int vertexIndex = geometry.Indices[i];
                        if (vertexIndex >= geometry.Vertices.VertexCount)
                            { throw new Exception(string.Format("invalid vertexIndex {0}!", vertexIndex)); }

                        UInt16 index = (UInt16)(geometry.Vertices.PositionIndices[vertexIndex] + basePosition);
                        if (index >= positions.Count)
                            { throw new Exception(string.Format("invalid index {0}!", index)); }
                        indices.Add(index);
                    }
                }
            }

            // Go through all children
            foreach (NodeContent child in input.Children)
            {
                CalculateAlignedBox3Tree(child, context, positions, indices);
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