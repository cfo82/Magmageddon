using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Simulation
{
    public delegate void QuaternionChangeEventHandler(QuaternionAttribute sender, Quaternion oldValue, Quaternion newValue);
}
