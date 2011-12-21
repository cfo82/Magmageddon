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

        public void Update(SimulationTime simTime)
        {
            if (OnUpdate != null)
            {
                OnUpdate(this, simTime);
            }
        }

        public event UpdateHandler OnUpdate;
    }
}
