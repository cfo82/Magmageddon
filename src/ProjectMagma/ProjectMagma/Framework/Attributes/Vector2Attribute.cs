using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Framework
{
    public class Vector2Attribute : Attribute
    {
        public Vector2Attribute(string name)
        :   base(name)
        {
        }
            
        public override void Initialize(string value)
        {
            string[] splitArray = value.Split(' ');
            v.X = float.Parse(splitArray[0]);
            v.Y = float.Parse(splitArray[1]);
        }

        public Vector2 Value
        {
            get
            {
                return v;
            }
            set
            {
                if (v != value)
                {
                    Vector2 oldValue = v;
                    v = value;
                    OnValueChanged(oldValue, v);
                }
            }
        }

        private void OnValueChanged(Vector2 oldValue, Vector2 newValue)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, oldValue, newValue);
            }
        }

        public event Vector2ChangeHandler ValueChanged;
        private Vector2 v;
    }
}
