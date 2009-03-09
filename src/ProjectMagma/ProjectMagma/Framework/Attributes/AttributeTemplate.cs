using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ProjectMagma.Framework
{
    class AttributeTemplate
    {
        public AttributeTemplate(string name)
        {
            this.Name = name;
        }

        public AttributeTemplate(Type attributeType)
        {
            this.AttributeType = attributeType;
        }

        public Attribute createAttribute()
        {
            ConstructorInfo constructor = this.attributeType.GetConstructor(new Type[] { this.GetType() });
            object newAttribute = constructor.Invoke(new object[] { this });
            return newAttribute as Attribute;
        }

        public string Name
        {
            get
            {
                return this.attributeType.FullName;
            }
            set
            {
                Type type = Type.GetType(value);
                if (type != null)
                {
                    this.attributeType = type;
                }
            }
        }

        public Type AttributeType
        {
            get
            {
                return this.attributeType;
            }
            set
            {
                this.attributeType = value;
            }
        }

        protected Type attributeType;        
    }
}
