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
            string[] splitArray = value.Split(' ');
            v.X = float.Parse(splitArray[0]);
            v.Y = float.Parse(splitArray[1]);
            v.Z = float.Parse(splitArray[2]);
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
