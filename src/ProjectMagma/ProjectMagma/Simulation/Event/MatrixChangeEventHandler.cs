using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Framework
{
    public delegate void MatrixChangeEventHandler(MatrixAttribute sender, Matrix oldValue, Matrix newValue);
}
