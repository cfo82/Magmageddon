using Microsoft.Xna.Framework;

namespace ProjectMagma.Simulation.Attributes
{
    public class Vector2Attribute : Attribute
    {
        public Vector2Attribute(string name)
        :   base(name)
        {
        }

        public Vector2Attribute(string name, Vector2 value)
        :   base(name)
        {
            this.v = value;
        }

        public override void Initialize(string value)
        {
            if (value.Trim().Length == 0)
            {
                v = Vector2.Zero;
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
            }
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

        public override string StringValue
        {
            get
            {
                return string.Format("{0} {1}", v.X, v.Y);
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
