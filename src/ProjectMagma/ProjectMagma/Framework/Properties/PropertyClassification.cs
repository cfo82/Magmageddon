using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Framework
{
    public class PropertyClassification : System.Attribute
    {
        public PropertyClassification(string classification)
        {
            this.classification = classification;
        }

        public string Classification
        {
            get
            {
                return this.classification;
            }
        }

        private string classification;
    }
}
