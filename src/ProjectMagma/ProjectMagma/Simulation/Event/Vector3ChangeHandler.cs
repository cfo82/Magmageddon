using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Attributes;

namespace ProjectMagma.Simulation
{
    public delegate void Vector3ChangeHandler(Vector3Attribute sender, Vector3 oldValue, Vector3 newValue);
}
