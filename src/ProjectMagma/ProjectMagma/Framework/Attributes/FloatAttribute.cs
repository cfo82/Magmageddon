using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Framework
{
    public class FloatAttribute : Attribute
    {
        public FloatAttribute(string name)
        :   base(name)
        {
        }
            
        public override void Initialize(string value)
        {
            v = float.Parse(value);
        }

        public float Value
        {
            get
            {
                return v;
            }

            set
            {
                if (v != value)
                {
                    float oldValue = v;
                    v = value;
                    OnValueChanged(oldValue, v);
                }
            }
        }

        private void OnValueChanged(float oldValue, float newValue)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, oldValue, newValue);
            }
        }

        public event FloatChangeHandler ValueChanged;
        private float v;
    }
}
