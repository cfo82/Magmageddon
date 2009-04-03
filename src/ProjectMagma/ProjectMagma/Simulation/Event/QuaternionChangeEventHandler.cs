using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Attributes;

namespace ProjectMagma.Simulation
{
    public delegate void QuaternionChangeEventHandler(QuaternionAttribute sender, Quaternion oldValue, Quaternion newValue);
}
