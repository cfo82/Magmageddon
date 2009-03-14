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
            if (value.Trim().Length == 0)
            {
                v = Quaternion.Identity;
            }
            else
            {
                float x, y, z, w;
                string[] splitArray = value.Split(' ');
                if (splitArray.Length > 0 && float.TryParse(splitArray[0], out x))
                {
                    v.X = x;
                }
                if (splitArray.Length > 1 && float.TryParse(splitArray[1], out y))
                {
                    v.Y = y;
                }
                if (splitArray.Length > 2 && float.TryParse(splitArray[2], out z))
                {
                    v.Z = z;
                }
                if (splitArray.Length > 3 && float.TryParse(splitArray[3], out w))
                {
                    v.W = w;
                }
            }
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

        public override string StringValue
        {
            get
            {
                return String.Format("{0} {1} {2} {3}", v.X, v.Y, v.Z, v.W);
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
