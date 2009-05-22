using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;

using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;

namespace ProjectMagma.Simulation
{
    public class Entity : AbstractEntity
    {
        public Entity(string name)
        :   base(name)
        {
        }

        public void OnUpdate(SimulationTime simTime)
        {
            if (Update != null)
            {
                Update(this, simTime);
            }
        }

        public event UpdateHandler Update;
    }
}
