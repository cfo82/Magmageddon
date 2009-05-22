
namespace ProjectMagma.Framework.Attributes
{
    public class IntAttribute : Attribute
    {
        public IntAttribute(string name)
        :   base(name)
        {
        }

        public IntAttribute(string name, int value)
        :   base(name)
        {
            this.v = value;
        }

        public override void Initialize(string value)
        {
            if (value.Trim().Length == 0)
            {
                v = 0;
            }
            else
            {
                this.v = int.Parse(value);
            }
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

        public override string StringValue
        {
            get
            {
                return string.Format("{0}", v);
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
