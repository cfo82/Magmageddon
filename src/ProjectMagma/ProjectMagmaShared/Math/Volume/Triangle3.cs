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

        public Vector3 Normal
        {
            get
            {
                Vector3 e1, e2, n;
                Vector3.Subtract(ref Vertex2, ref Vertex0, out e1);
                Vector3.Subtract(ref Vertex1, ref Vertex0, out e2);
                Vector3.Cross(ref e1, ref e2, out n);
                n.Normalize();
                return n;
            }
        }

        public Vector3 Vertex0;
        public Vector3 Vertex1;
        public Vector3 Vertex2;
    }
}
