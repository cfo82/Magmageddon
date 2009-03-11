using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Framework
{
    class IntAttribute : Attribute
    {
        public IntAttribute(string name, AttributeTemplate template)
        :   base(name, template)
        {
        }
            
        public override void Initialize(ContentManager content, string value)
        {
            v = int.Parse(value);
        }

        public int Value
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

        private int v;
    }
}
