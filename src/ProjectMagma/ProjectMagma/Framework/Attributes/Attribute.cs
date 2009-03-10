using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Framework
{
    public abstract class Attribute
    {
        public Attribute(string name, AttributeTemplate template)
        {
            this.name = name;
            this.template = template;
        }

        public abstract void Initialize(ContentManager content, string value);

        public String Name
        {
            get
            {
                return this.name;
            }
        }

        private string name;
        private AttributeTemplate template;
    }
}
