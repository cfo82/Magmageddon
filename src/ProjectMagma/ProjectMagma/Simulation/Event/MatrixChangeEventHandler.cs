using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Attributes;

namespace ProjectMagma.Simulation
{
    public delegate void MatrixChangeEventHandler(MatrixAttribute sender, Matrix oldValue, Matrix newValue);
}
