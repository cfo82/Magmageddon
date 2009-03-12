using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Framework
{
    public abstract class Attribute
    {
        public Attribute(string name)
        {
            this.name = name;
        }

        public abstract void Initialize(string value);

        public String Name
        {
            get
            {
                return this.name;
            }
        }

        private string name;
    }
}
