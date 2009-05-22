
namespace ProjectMagma.Framework.Attributes
{
    public class StringAttribute : Attribute
    {
        public StringAttribute(string name)
        :   base(name)
        {
        }

        public StringAttribute(string name, string value)
        :   base(name)
        {
            this.value = value;
        }

        public override void Initialize(string value)
        {
            this.value = value;
        }

        public string Value
        {
            get
            {
                return value;
            }
            set
            {
                if (this.value != value)
                {
                    string oldValue = this.value;
                    this.value = value;
                    OnValueChanged(oldValue, this.value);
                }
            }
        }

        public override string StringValue
        {
            get
            {
                return value;
            }
        }

        private void OnValueChanged(string oldValue, string newValue)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, oldValue, newValue);
            }
        }

        public event StringChangeHandler ValueChanged;
        private string value;
    }
}
