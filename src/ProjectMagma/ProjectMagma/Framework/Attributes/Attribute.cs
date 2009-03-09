using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Framework
{
    class Attribute
    {
        public Attribute(AttributeTemplate template)
        {
            this.template = template;
        }

        protected AttributeTemplate template;
    }
}
