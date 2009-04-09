using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Simulation.Collision
{
    public struct ContactPoint
    {
        public ContactPoint(Vector3 point, Vector3 normal)
        {
            this.Point = point;
            this.Normal = normal;
        }

        public readonly Vector3 Point;
        public readonly Vector3 Normal;
    }
}
