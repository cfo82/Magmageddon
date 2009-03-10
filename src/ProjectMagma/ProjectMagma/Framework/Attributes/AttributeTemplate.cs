using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ProjectMagma.Framework
{
    public class AttributeTemplate
    {
        public AttributeTemplate(string name, Type attributeType)
        {
            this.name = name;
            this.attributeType = attributeType;
        }

        public Attribute CreateAttribute(string attributeName)
        {
            ConstructorInfo constructor = this.attributeType.GetConstructor(new Type[] { typeof(string), this.GetType() });
            object newAttribute = constructor.Invoke(new object[] { attributeName, this });
            return newAttribute as Attribute;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public Type AttributeType
        {
            get
            {
                return this.attributeType;
            }
        }

        protected string name;
        protected Type attributeType;        
    }
}
