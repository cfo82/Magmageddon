using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.Math.Volume
{
    public struct Triangle3
    {
        public Triangle3(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vertex0 = v0;
            Vertex1 = v1;
            Vertex2 = v2;
        }

        public Vector3 Vertex0;
        public Vector3 Vertex1;
        public Vector3 Vertex2;
    }
}
