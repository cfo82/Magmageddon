using Microsoft.Xna.Framework;

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
                return string.Format("{0} {1} {2}", v.X, v.Y, v.Z);
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
