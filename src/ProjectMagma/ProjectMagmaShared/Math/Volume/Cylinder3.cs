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
            set { c1 = value; }
        }

        public Vector3 Bottom
        {
            get { return c2; }
            set { c2 = value; }
        }

        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        public VolumeType Type
        {
            get { return VolumeType.Cylinder3; }
        }

        private Vector3 c1;
        private Vector3 c2;
        private float radius;
    }
}
