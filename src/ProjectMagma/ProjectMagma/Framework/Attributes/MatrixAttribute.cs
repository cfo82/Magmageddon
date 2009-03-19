using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Framework
{
    public class MatrixAttribute : Attribute
    {
        public MatrixAttribute(string name)
        :   base(name)
        {
        }
            
        public override void Initialize(string value)
        {
            if (value.Trim().Length == 0)
            {
                v = Matrix.Identity;
            }
            else
            {
                Debug.Assert(false, "String initialization of matrices is currently not supported."); // TODO
            }
        }

        public Matrix Value
        {
            get
            {
                return v;
            }

            set
            {
                if (v != value)
                {
                    Matrix oldValue = v;
                    v = value;
                    OnValueChanged(oldValue, v);
                }
            }
        }

        public override string StringValue
        {
            get
            {
                return String.Format(
                    "{0} {1} {2} {3}\n" +
                    "{0} {1} {2} {3}\n" +
                    "{0} {1} {2} {3}\n" +
                    "{0} {1} {2} {3}\n",
                    v.M11, v.M12, v.M13, v.M14,
                    v.M21, v.M22, v.M23, v.M24,
                    v.M31, v.M32, v.M33, v.M34,
                    v.M41, v.M42, v.M43, v.M44);
            }
        }

        private void OnValueChanged(Matrix oldValue, Matrix newValue)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, oldValue, newValue);
            }
        }

        public event MatrixChangeEventHandler ValueChanged;
        private Matrix v;
    }
}
