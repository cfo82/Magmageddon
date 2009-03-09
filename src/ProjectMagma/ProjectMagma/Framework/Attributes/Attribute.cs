using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Framework
{
    public abstract class Attribute
    {
        public Attribute(AttributeTemplate template)
        {
            this.template = template;
        }

        public abstract void Initialize(float[] values);

        public String Name
        {
            get
            {
                return this.template.Name;
            }
        }

        protected AttributeTemplate template;
    }
}
