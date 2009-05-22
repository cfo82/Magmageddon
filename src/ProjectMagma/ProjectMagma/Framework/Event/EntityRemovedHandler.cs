using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectMagma.Simulation;

namespace ProjectMagma.Simulation
{
    public delegate void EntityRemovedHandler<EntityType>(AbstractEntityManager<EntityType> manager, EntityType entity)
        where EntityType : AbstractEntity;
}
