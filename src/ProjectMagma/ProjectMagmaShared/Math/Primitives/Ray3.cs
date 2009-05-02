using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.Math.Primitives
{
    public struct Ray3
    {
        public Ray3(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        public Vector3 Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        private Vector3 origin;
        private Vector3 direction;
    }
}
