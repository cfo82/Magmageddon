
namespace ProjectMagma.Simulation.Attributes
{
    public class FloatAttribute : Attribute
    {
        public FloatAttribute(string name)
        :   base(name)
        {
        }

        public FloatAttribute(string name, float value)
        :   base(name)
        {
            this.v = value;
        }

        public override void Initialize(string value)
        {
            if (value.Trim().Length == 0)
            {
                v = 0.0f;
            }
            else
            {
                v = float.Parse(value);
            }
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

        public override string StringValue
        {
            get
            {
                return string.Format("{0}", v);
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
