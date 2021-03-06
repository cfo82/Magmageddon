﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Framework;
using ProjectMagma.Simulation.Collision;

namespace ProjectMagma.Simulation
{
    public class DeathProperty : Property
    {

        public DeathProperty()
        {
        }

        private void OnUpdate(Entity entity, SimulationTime simTime)
        {
            if (entity.GetFloat(CommonNames.Health) <= 0)
            {
                Game.Instance.Simulation.EntityManager.RemoveDeferred(entity);
                return;
            }
        }

        public override void OnAttached(AbstractEntity arrow)
        {
            (arrow as Entity).OnUpdate += OnUpdate;
        }

        public override void OnDetached(AbstractEntity entity)
        {
            (entity as Entity).OnUpdate -= OnUpdate;
        }

    }
}
