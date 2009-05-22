using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectMagma.Shared.LevelData;
using ProjectMagma.Framework;
using System.Diagnostics;

namespace ProjectMagma.Simulation
{
    public class EntityManager : AbstractEntityManager<Entity>
    {
        public override Entity CreateEntity(string name)
        {
            return new Entity(name);
        }
    }
}
