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
                if (splitArray.Length > 0)
                {
                    v.X = float.Parse(splitArray[0]);
                }
                if (splitArray.Length > 1)
                {
                    v.Y = float.Parse(splitArray[1]);
                }
                if (splitArray.Length > 2)
                {
                    v.Z = float.Parse(splitArray[2]);
                }
                if (splitArray.Length > 3)
                {
                    v.W = float.Parse(splitArray[3]);
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
