using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Framework
{
    class Vector2Attribute : Attribute
    {
        public Vector2Attribute(AttributeTemplate template)
        :   base(template)
        {
        }
            
        public override void Initialize(float[] values)
        {
            v.X = values[0];
            v.Y = values[1];
        }

        public Vector2 Vector
        {
            get
            {
                return v;
            }
        }

        private Vector2 v;
    }
}
