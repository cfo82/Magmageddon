using Microsoft.Xna.Framework;

namespace ProjectMagma.Simulation.Attributes
{
    public delegate void QuaternionChangeHandler(QuaternionAttribute sender, Quaternion oldValue, Quaternion newValue);
}
