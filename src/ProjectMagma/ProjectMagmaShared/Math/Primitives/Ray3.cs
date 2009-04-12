using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.Math.Primitives
{
    public class Ray3
    {
        public Ray3(Vector3 origin, Vector3 direction)
        {
            this.ray = new Ray(origin, direction);
        }

        public Vector3 Origin
        {
            get { return ray.Position; }
            set { ray.Position = value; }
        }

        public Vector3 Direction
        {
            get { return ray.Direction; }
            set { ray.Direction = value; }
        }

        private Ray ray;
    }
}
