using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.Math.Volume
{
    public class Cylinder3 : Volume
    {
        public Cylinder3()
        {
        }

        public Cylinder3(Vector3 c1, Vector3 c2, float radius)
        {
            this.c1 = c1;
            this.c2 = c2;
            this.radius = radius;
        }
        
        public Vector3 Top
        {
            get { return c1; }
        }

        public Vector3 Bottom
        {
            get { return c2; }
        }

        public float Radius
        {
            get { return radius; }
        }

        private Vector3 c1;
        private Vector3 c2;
        private float radius;
    }
}
