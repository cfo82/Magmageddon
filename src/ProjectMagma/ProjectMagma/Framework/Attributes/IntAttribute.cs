using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Framework
{
    public class IntAttribute : Attribute
    {
        public IntAttribute(string name)
        :   base(name)
        {
        }
            
        public override void Initialize(string value)
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
                if (v != value)
                {
                    int oldValue = v;
                    v = value;
                    OnValueChanged(oldValue, v);
                }
            }
        }

        private void OnValueChanged(int oldValue, int newValue)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, oldValue, newValue);
            }
        }

        public event IntChangeHandler ValueChanged;
        private int v;
    }
}
