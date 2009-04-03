using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Collision;

namespace ProjectMagma.Framework
{
    public class DeathProperty : Property
    {

        public DeathProperty()
        {
        }

        private void OnUpdate(Entity entity, GameTime gameTime)
        {
            if (entity.GetInt("health") <= 0)
            {
                Game.Instance.EntityManager.RemoveDeferred(entity);
                return;
            }
        }

        public void OnAttached(Entity arrow)
        {
            arrow.Update += OnUpdate;
        }

        public void OnDetached(Entity entity)
        {
            entity.Update -= OnUpdate;
        }

    }
}
