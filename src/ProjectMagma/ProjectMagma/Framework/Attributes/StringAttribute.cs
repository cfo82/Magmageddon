using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Framework
{
    class StringAttribute : Attribute
    {
        public StringAttribute(string name)
        :   base(name)
        {
        }
            
        public override void Initialize(ContentManager content, string value)
        {
            this.value = value;
        }

        public string Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        private string value;
    }
}
