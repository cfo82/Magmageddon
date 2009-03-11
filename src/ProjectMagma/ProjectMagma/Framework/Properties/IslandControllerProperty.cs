using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Framework
{
    [PropertyClassification("simulation")]
    public class IslandControllerProperty : Property
    {
        public void OnAttached(Entity entity)
        {
            entity.Update += new UpdateHandler(OnUpdate);
        }

        public void OnDetached(Entity entity)
        {
            entity.Update -= new UpdateHandler(OnUpdate);
        }

        public void OnUpdate(Entity entity, GameTime gameTime)
        {
            int dt = gameTime.ElapsedGameTime.Milliseconds;

            // control this island
        }
    }
}
