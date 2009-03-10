﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Framework
{
    class Vector3Attribute : Attribute
    {
        public Vector3Attribute(string name, AttributeTemplate template)
        :   base(name, template)
        {
        }
            
        public override void Initialize(ContentManager content, string value)
        {
            string[] splitArray = value.Split(' ');
            v.X = float.Parse(splitArray[0]);
            v.Y = float.Parse(splitArray[1]);
            v.Z = float.Parse(splitArray[2]);
        }

        public Vector3 Vector
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

        private Vector3 v;
    }
}
