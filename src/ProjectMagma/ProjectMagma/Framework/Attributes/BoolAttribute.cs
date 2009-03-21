using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Framework
{
    public class BoolAttribute : Attribute
    {
        public BoolAttribute(string name) :   base(name)
        {
        }
            
        public override void Initialize(string value)
        {
            v = "true".Equals(value);

        }

        public bool Value
        {
            get
            {
                return v;
            }

            set
            {
                if (v != value)
                {
                    bool oldValue = v;
                    v = value;
                    OnValueChanged(oldValue, v);
                }
            }
        }

        public override string StringValue
        {
            get
            {
                return String.Format("{0}", v);
            }
        }

        private void OnValueChanged(bool oldValue, bool newValue)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, oldValue, newValue);
            }
        }

        public event BoolChangeHandler ValueChanged;
        private bool v;
    }
}
