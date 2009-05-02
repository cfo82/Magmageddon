using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using ProjectMagma.Shared.Math.Primitives;
using ProjectMagma.ContentPipeline.Model;

namespace ProjectMagma.ContentPipeline.ModelProcessors
{
    public abstract class MagmaModelProcessor<BaseClass> : ContentProcessor<NodeContent, MagmaModelContent> where BaseClass : ModelProcessor, new()
    {
        #region Main processing

        /// <summary>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override MagmaModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            ApplyParameters(context);

#if DEBUG
            // in debug mode we output the structure of this mesh
            context.Logger.LogImportantMessage("  Identity: " + input.Identity.SourceFilename);
            context.Logger.LogImportantMessage("  Mesh Hierarchy: ");
            LogStructure(input, context, 2);
#endif

            // test for an external collision mesh. If there is one we use it and abandon any internal
            // collision-mesh nodes.
            string absoluteSourceFilename = input.Identity.SourceFilename;
            string sourceFilename = absoluteSourceFilename.Substring(absoluteSourceFilename.LastIndexOf('\\') + 1);
            int sourceFilenameEndingIndex = absoluteSourceFilename.LastIndexOf('.');
            string collisionMeshFilename = absoluteSourceFilename.Substring(0, sourceFilenameEndingIndex) + "_col" + absoluteSourceFilename.Substring(sourceFilenameEndingIndex);
            if (File.Exists(collisionMeshFilename))
            {
                return ProcessFileWithSeparateCollisionMesh(input, context, collisionMeshFilename);
            }

            // if the file does not contain neither has a collision model on its side nor has any collision $
            // meshes stored inside it then this must be a legacy file...
            if (!ContainsCollisionNode(input))
            {
                return ProcessLegacyFile(input, context);
            }

            // if the call is not yet marked as recursively done it must be a container file
            if (!CallRecursive)
            {
                return ProcessContainerFile(input, context);
            }
            else // it must be the processing of a group inside a container
            {
                Debug.Assert(CallRecursive);
                Debug.Assert(CurrentGroup.Length > 0);

                return ProcessContainerGroup(input, context);
            }
        }

        private void ApplyParameters(
            ContentProcessorContext context
        )
        {
            CallRecursive = false;
            CurrentGroup = "";

            foreach (string key in context.Parameters.Keys)
            {
                if (key == "CallRecursive")
                {
                    CallRecursive = (bool)context.Parameters[key];
                }
                else if (key == "CurrentGroup")
                {
                    CurrentGroup = (string)context.Parameters[key];
                }
                else
                {
                    throw new ArgumentException(string.Format("found invalid key ({0}) in parameters", key));
                }
            }
        }

        private void LogStructure(
            NodeContent input,
            ContentProcessorContext context,
            int indent
        )
        {
            string indentString = "";
            for (int i = 0; i < indent; ++i)
            {
                indentString += "  ";
            }

            context.Logger.LogImportantMessage("{0}{1}", indentString, input.Name);
            foreach (NodeContent child in input.Children)
            {
                LogStructure(child, context, indent + 1);
            }
        }

        private bool ContainsCollisionNode(
            NodeContent input
        )
        {
            if (input == null)
            { return false; }

            if (IsCollisionNode(input))
                { return true; }

            foreach (NodeContent child in input.Children)
            {
                if (ContainsCollisionNode(child))
                    { return true; }
            }

            return false;
        }

        private MagmaModelContent ProcessFileWithSeparateCollisionMesh(
            NodeContent input,
            ContentProcessorContext context,
            string collisionMeshFilename
        )
        {
#if DEBUG
            context.Logger.LogImportantMessage("  processing file using a separate collision mesh");
            context.Logger.LogImportantMessage("  using '" + collisionMeshFilename + "' as collision mesh");
#endif

            NodeContent collisionMeshContent = collisionMeshContent = context.BuildAndLoadAsset<NodeContent, NodeContent>(
                new ExternalReference<NodeContent>(collisionMeshFilename), null);

            // calculate bounds because changes are based on the bounding box
            AlignedBox3 boundingBox = CalculateAlignedBox3(input, context, true);

            TransformMeshes(new NodeContent[] { input, collisionMeshContent }, context, ref boundingBox);

            // let the base class process the model
            MagmaModelContent modelContent = BaseProcessing(input, context);

            // add bounding volumes to the model
            modelContent.VolumeCollection = CalculateCollisionVolumes(new NodeContent[] { collisionMeshContent }, context);

            return modelContent;
        }

        private MagmaModelContent ProcessLegacyFile(
            NodeContent input,
            ContentProcessorContext context
        )
        {
            context.Logger.LogWarning(null, input.Identity, "processing legacy file using the graphical mesh as collision mesh");

            // calculate bounds because changes are based on the bounding box
            AlignedBox3 bb = CalculateAlignedBox3(input, context, false);

            TransformMeshes(new NodeContent[] { input }, context, ref bb);

            // let the base class process the model
            MagmaModelContent modelContent = BaseProcessing(input, context);

            // add bounding volumes to the model
            modelContent.VolumeCollection = CalculateCollisionVolumes(new NodeContent[] { input }, context);

            return modelContent;
        }

        private MagmaModelContent ProcessContainerFile(
            NodeContent input,
            ContentProcessorContext context
        )
        {
#if DEBUG
            context.Logger.LogImportantMessage("  processing file as a model container");
#endif

            int startIndex = input.Identity.SourceFilename.LastIndexOf("Content\\") + "Content\\".Length;
            int endIndex = input.Identity.SourceFilename.LastIndexOf('\\') + 1;
            string dir = input.Identity.SourceFilename.Substring(startIndex, endIndex - startIndex);

            // parameters to be passed into the processor
            OpaqueDataDictionary dictionary = new OpaqueDataDictionary();
            dictionary.Add("CallRecursive", true);

            foreach (NodeContent child in input.Children)
            {
#if DEBUG
                context.Logger.LogImportantMessage("  spawning a new build process for {0}", child.Name);
#endif

                dictionary["CurrentGroup"] = child.Name;

                context.BuildAsset<NodeContent, MagmaModelContent>(
                    new ExternalReference<NodeContent>(input.Identity.SourceFilename),
                    this.GetType().Name, dictionary, GetContainerGroupImporter(), dir + child.Name);
            }

            RemoveTextureReferences(input, context);

            return BaseProcessing(input, context);
        }

        private MagmaModelContent ProcessContainerGroup(
            NodeContent input,
            ContentProcessorContext context
        )
        {
#if DEBUG
            context.Logger.LogImportantMessage("  processing one group contained in a model container");
#endif

            // find the child to process!
            NodeContent currentGroupNode = GetChild(input, CurrentGroup);
            Debug.Assert(currentGroupNode != null);
            if (currentGroupNode == null)
            {
                throw new ArgumentException(string.Format("unable to find referenced child ({0})!", CurrentGroup));
            }

            // copy the identity
            currentGroupNode.Identity = input.Identity;

            // calculate bounds because changes are based on the bounding box
            AlignedBox3 bb = CalculateAlignedBox3(currentGroupNode, context, true);

            // transform the graphical mesh and the collision meshes in one step!
            TransformMeshes(new NodeContent[] { currentGroupNode }, context, ref bb);

            // extract all collision meshes
            List<NodeContent> collisionNodes = new List<NodeContent>();
            foreach (NodeContent child in currentGroupNode.Children)
            {
                if (IsCollisionNode(child))
                {
                    collisionNodes.Add(child);
                }
            }
            foreach (NodeContent collisionMesh in collisionNodes)
            {
#if DEBUG
                context.Logger.LogImportantMessage("  using {0} as a collision fragment", collisionMesh.Name);
#endif
                currentGroupNode.Children.Remove(collisionMesh);
            }

            // let the base class process the graphical model
            MagmaModelContent modelContent = BaseProcessing(currentGroupNode, context);

            // now we process our collision meshes... 
            // TODO: take all collision meshes not only the first one!
            VolumeCollection collection = new VolumeCollection();

            if (collisionNodes.Count == 0)
            {
                context.Logger.LogWarning(null, input.Identity, "unable to find a collision mesh in group {0}", CurrentGroup);
                modelContent.VolumeCollection = CalculateCollisionVolumes(new NodeContent[] { currentGroupNode }, context);
            }
            else
            {
                NodeContent[] collisionNodesArray = new NodeContent[collisionNodes.Count];
                for (int i = 0; i < collisionNodes.Count; ++i)
                    { collisionNodesArray[i] = collisionNodes[i]; }
                modelContent.VolumeCollection = CalculateCollisionVolumes(collisionNodesArray, context);
            }

            return modelContent;
        }

        private MagmaModelContent BaseProcessing(
            NodeContent input, 
            ContentProcessorContext context
        )
        {
            BaseClass baseClass = new BaseClass();

            MagmaModelContent content = new MagmaModelContent();
            content.XnaModel = baseClass.Process(input, context);

            return content;
        }

        protected virtual string GetContainerGroupImporter()
        {
            return null;
        }

        #endregion

        #region Mesh Processing

        private void TransformMeshes(
            NodeContent[] inputNodes,
            ContentProcessorContext context,
            ref AlignedBox3 boundingBox
        )
        {
            foreach (NodeContent input in inputNodes)
            {
                RemoveTextureReferences(input, context);
            }

            // first center the models (I think they are actually already centered...
            Vector3 diff = Vector3.Zero - (boundingBox.Min + ((boundingBox.Max - boundingBox.Min) / 2.0f));
            foreach (NodeContent input in inputNodes)
            {
                MoveModel(input, context, diff);
            }
            boundingBox.Max += diff;
            boundingBox.Min += diff;

            // now that the models are centered scale them
            float scaleFactor = boundingBox.Max.X;
            if (boundingBox.Max.Y > scaleFactor) scaleFactor = boundingBox.Max.Y;
            if (boundingBox.Max.Z > scaleFactor) scaleFactor = boundingBox.Max.Z;
            scaleFactor = 1.0f / scaleFactor;
            foreach (NodeContent input in inputNodes)
            {
                ScaleModel(input, context, scaleFactor);
            }
            boundingBox.Max *= scaleFactor;
            boundingBox.Min *= scaleFactor;

            // now let the subclass decide on how to modify the position
            Vector3 scaledOrigDiff = diff * scaleFactor;
            Vector3 diffCorrector = CalculateDiff(ref scaledOrigDiff, ref boundingBox);
            foreach (NodeContent input in inputNodes)
            {
                MoveModel(input, context, diffCorrector);
            }
            boundingBox.Min += diffCorrector;
            boundingBox.Max += diffCorrector;
        }

        private void RemoveTextureReferences(
            NodeContent input,
            ContentProcessorContext context
        )
        {
            MeshContent mesh = input as MeshContent;
            if (mesh != null)
            {
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    geometry.Material.Textures.Clear();
                }
            }

            // Go through all children
            foreach (NodeContent child in input.Children)
            {
                RemoveTextureReferences(child, context);
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
        )
        {
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

        protected virtual Vector3 CalculateDiff(ref Vector3 origDiff, ref AlignedBox3 bb)
        {
            return Vector3.Zero;
        }

        #endregion

        #region Volume Calculations

        private VolumeCollection[] CalculateCollisionVolumes(
            NodeContent[] collisionNodes,
            ContentProcessorContext context
        )
        {
            List<VolumeCollection> collectionList = new List<VolumeCollection>();
            foreach (NodeContent currentNode in collisionNodes)
            {
                VolumeCollection collection = new VolumeCollection();
                collection.AddVolume(CalculateAlignedBox3(currentNode, context, false));
                collection.AddVolume(CalculateAlignedBox3Tree(currentNode, context, false));
                collection.AddVolume(CalculateCylinder3(currentNode, context, false));
                collection.AddVolume(CalculateSphere3(currentNode, context, false));
                collectionList.Add(collection);
            }
            return collectionList.ToArray();
        }

        private AlignedBox3 CalculateAlignedBox3(
            NodeContent input,
            ContentProcessorContext context,
            bool ignoreCollisionNodes
        )
        {
            AlignedBox3 alignedBox = new AlignedBox3();
            alignedBox.Min = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
            alignedBox.Max = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
            CalculateAlignedBox3(input, context, ignoreCollisionNodes, ref alignedBox);
            return alignedBox;
        }

        private void CalculateAlignedBox3(
            NodeContent input,
            ContentProcessorContext context,
            bool ignoreCollisionNodes,
            ref AlignedBox3 box
            )
        {
            if (ignoreCollisionNodes && IsCollisionNode(input)) { return; }

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
                CalculateAlignedBox3(child, context, ignoreCollisionNodes, ref box);
            }
        }

        private AlignedBox3Tree CalculateAlignedBox3Tree(
            NodeContent input,
            ContentProcessorContext context,
            bool ignoreCollisionNodes
        )
        {
            // collect positions and indices
            List<Vector3> positionList = new List<Vector3>();
            List<UInt16> indexList = new List<UInt16>();
            CalculateAlignedBox3Tree(input, context, ignoreCollisionNodes, positionList, indexList);
            if (indexList.Count % 3 != 0)
                { throw new Exception("invalid number of indices!"); }

            Vector3[] positions = new Vector3[positionList.Count];
            for (int i = 0; i < positionList.Count; ++i)
                { positions[i] = positionList[i]; }
            UInt16[] indices = new UInt16[indexList.Count];
            for (int i = 0; i < indexList.Count; ++i)
                { indices[i] = indexList[i]; }
            
            AlignedBox3Tree tree = new AlignedBox3Tree(positions, indices);
#if DEBUG
            context.Logger.LogImportantMessage("  average number of triangles per leaf in collision mesh is {0}", tree.Root.AverageNumTrianglesPerLeaf);
#endif
            return tree;
        }

        private void CalculateAlignedBox3Tree(
            NodeContent input,
            ContentProcessorContext context,
            bool ignoreCollisionNodes,
            List<Vector3> positions,
            List<UInt16> indices
            )
        {
            if (ignoreCollisionNodes && IsCollisionNode(input)) { return; }

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
                CalculateAlignedBox3Tree(child, context, ignoreCollisionNodes ,positions, indices);
            }
        }

        // calculates y-axis aligned bounding cylinder
        private Cylinder3 CalculateCylinder3(
            NodeContent input,
            ContentProcessorContext context,
            bool ignoreCollisionNodes
        )
        {
            // calculate center
            AlignedBox3 bb = CalculateAlignedBox3(input, context, ignoreCollisionNodes);
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
            ContentProcessorContext context,
            bool ignoreCollisionNodes
        )
        {
            // calculate center
            AlignedBox3 bb = CalculateAlignedBox3(input, context, ignoreCollisionNodes);
            Vector3 center = (bb.Min + bb.Max) / 2;

            // calculate radius
            //            float radius = (bb.Max-bb.Min).Length() / 2;
            float radius = (bb.Max.Y - bb.Min.Y) / 2; // HACK: hack for player

            return new Sphere3(center, radius);
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// returns a flag indicating if the node (and all its children) belong to the collision
        /// mesh of a model. 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool IsCollisionNode(
            NodeContent node
        )
        {
            return node.Name != null && node.Name.EndsWith("_col");
        }

        private NodeContent GetChild(
            NodeContent node,
            string childName
        )
        {
            foreach (NodeContent child in node.Children)
            {
                if (child.Name == childName)
                {
                    return child;
                }
            }

            return null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Property storing if this is a recursive call to the processor (thus we only have to process one of
        /// the sub-groups!)
        /// </summary>
        public bool CallRecursive
        {
            set;
            get;
        }

        /// <summary>
        /// Property identifying which of the subgroups have to be processed
        /// </summary>
        public string CurrentGroup
        {
            set;
            get;
        }

        #endregion
    }
}