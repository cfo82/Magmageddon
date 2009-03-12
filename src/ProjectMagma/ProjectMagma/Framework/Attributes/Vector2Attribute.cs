using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Framework
{
    class Vector2Attribute : Attribute
    {
        public Vector2Attribute(string name)
        :   base(name)
        {
        }
            
        public override void Initialize(ContentManager content, string value)
        {
            string[] splitArray = value.Split(' ');
            v.X = float.Parse(splitArray[0]);
            v.Y = float.Parse(splitArray[1]);
        }

        public Vector2 Value
        {
            get
            {
                return v;
            }
            set
            {
                v = value;
            }
        }

        private Vector2 v;
    }
}
