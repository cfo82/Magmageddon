using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Attributes;

namespace ProjectMagma.Simulation
{
    public delegate void Vector2ChangeHandler(Vector2Attribute sender, Vector2 oldValue, Vector2 newValue);
}
