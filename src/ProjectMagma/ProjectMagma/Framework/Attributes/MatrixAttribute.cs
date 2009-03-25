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
                string[] splitArray = value.Split(' ');
                float[] values = new float[] { 1, 0, 0, 0,  0, 1, 0, 0,  0, 0, 1, 0,  0, 0, 0, 1 };
                for (int i = 0; i < 16; ++i)
                {
                    if (splitArray.Length > i)
                    {
                        values[i] = float.Parse(splitArray[i]);
                    }
                }

                v.M11 = values[ 0]; v.M12 = values[ 1]; v.M13 = values[ 2]; v.M14 = values[ 3];
                v.M21 = values[ 4]; v.M22 = values[ 5]; v.M23 = values[ 6]; v.M24 = values[ 7];
                v.M31 = values[ 8]; v.M32 = values[ 9]; v.M33 = values[10]; v.M34 = values[11];
                v.M41 = values[12]; v.M42 = values[13]; v.M43 = values[14]; v.M44 = values[15]; 
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
                    "{0} {1} {2} {3}" +
                    "{4} {5} {6} {7}" +
                    "{8} {9} {10} {11}" +
                    "{12} {13} {14} {15}",
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
