using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Framework
{
    public class Vector3Attribute : Attribute
    {
        public Vector3Attribute(string name)
        :   base(name)
        {
        }
            
        public override void Initialize(string value)
        {
            if (value.Trim().Length == 0)
            {
                v = Vector3.Zero;
            }
            else
            {
                float x, y, z;
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
            }
        }

        public Vector3 Value
        {
            get
            {
                return v;
            }

            set
            {
                if (v != value)
                {
                    Vector3 oldValue = v;
                    v = value;
                    OnValueChanged(oldValue, v);
                }
            }
        }

        public override string StringValue
        {
            get
            {
                return String.Format("{0} {1} {2}", v.X, v.Y, v.Z);
            }
        }

        private void OnValueChanged(Vector3 oldValue, Vector3 newValue)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, oldValue, newValue);
            }
        }

        public event Vector3ChangeHandler ValueChanged;
        private Vector3 v;
    }
}
