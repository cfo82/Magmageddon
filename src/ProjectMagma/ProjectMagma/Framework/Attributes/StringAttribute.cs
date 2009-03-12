using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Framework
{
    public class StringAttribute : Attribute
    {
        public StringAttribute(string name)
        :   base(name)
        {
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
