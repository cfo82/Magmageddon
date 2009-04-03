using Microsoft.Xna.Framework;

namespace ProjectMagma.Simulation.Attributes
{
    public delegate void MatrixChangeEventHandler(MatrixAttribute sender, Matrix oldValue, Matrix newValue);
}
