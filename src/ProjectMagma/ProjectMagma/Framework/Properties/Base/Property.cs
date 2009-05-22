using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Simulation
{
    public interface Property
    {
        void OnAttached(AbstractEntity entity);
        void OnDetached(AbstractEntity entity);
    }
}
