using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Framework
{
    public class QuaternionAttribute : Attribute
    {
        public QuaternionAttribute(string name)
        :   base(name)
        {
        }
            
        public override void Initialize(string value)
        {
            string[] splitArray = value.Split(' ');
            v.X = float.Parse(splitArray[0]);
            v.Y = float.Parse(splitArray[1]);
            v.Z = float.Parse(splitArray[2]);
            v.Z = float.Parse(splitArray[2]);
        }

        public Quaternion Value
        {
            get
            {
                return v;
            }

            set
            {
                if (v != value)
                {
                    Quaternion oldValue = v;
                    v = value;
                    OnValueChanged(oldValue, v);
                }
            }
        }

        private void OnValueChanged(Quaternion oldValue, Quaternion newValue)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, oldValue, newValue);
            }
        }

        public event QuaternionChangeEventHandler ValueChanged;
        private Quaternion v;
    }
}
