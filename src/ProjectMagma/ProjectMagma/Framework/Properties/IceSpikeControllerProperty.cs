using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ProjectMagma.Framework
{
    public class IceSpikeControllerProperty : Property
    {
        public IceSpikeControllerProperty()
        {
        }

        public void OnAttached(Entity entity)
        {
            entity.Update += new UpdateHandler(OnUpdate);
        }

        public void OnDetached(Entity entity)
        {
            entity.Update -= new UpdateHandler(OnUpdate);
        }

        private void OnUpdate(Entity entity, GameTime gameTime)
        {

            // fetch required values
            Vector3 pos = entity.GetVector3("position");
            Vector3 v = entity.GetVector3("velocity");
            float dt = ((float)gameTime.ElapsedGameTime.Milliseconds)/1000.0f;

            // compute acceleration
            Vector3 a = new Vector3(straightAcceleration, 0f, 0f);

            // integrate
            v += dt*a;
            pos += dt*v;

            // store computed values;
            entity.SetVector3("position", pos);
            entity.SetVector3("velocity", v);
        }

        //private Entity target;
        private float straightAcceleration = 100f;
        //private float maxSpeed = 150f;
    }
}
