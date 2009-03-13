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
//            Vector3 a = new Vector3(straightAcceleration, 0f, 0f);

            // integrate
            v += gravityAcceleration * dt;
            pos += dt*v;

            // remove if in lava
            if (pos.Y < Game.Instance.EntityManager["lava"].GetVector3("position").Y)
            {
                Game.Instance.EntityManager.RemoveDeferred(entity);
                return;
            }

            BoundingSphere bs = Game.calculateBoundingSphere(Game.Instance.Content.Load<Model>(entity.GetString("mesh")),
                GetPosition(entity), GetRotation(entity), GetScale(entity));

            // detect collision
            foreach (Entity e in Game.Instance.EntityManager)
            {
                if (e.HasAttribute("position"))
                {
                    if (!e.Name.StartsWith("icespike") 
                        && !e.Name.Equals(entity.GetString("player")))
                    {
                        BoundingCylinder bc = Game.calculateBoundingCylinder(Game.Instance.Content.Load<Model>(e.GetString("mesh")),
                            GetPosition(e), GetRotation(e), GetScale(e));

                        if (bc.Intersects(bs))
                        {
                            // remove spike
                            Game.Instance.EntityManager.RemoveDeferred(entity);

                            // deduct health if player
                            if (e.Name.StartsWith("player"))
                                e.SetInt("health", e.GetInt("health") - iceSpikeDamage);

                            return;
                        }
                    }
                }
            }


            // store computed values;
            entity.SetVector3("position", pos);
            entity.SetVector3("velocity", v);
        }


        private Vector3 GetPosition(Entity entity)
        {
            return entity.GetVector3("position");
        }

        private Vector3 GetScale(Entity entity)
        {
            if (entity.HasVector3("scale"))
            {
                return entity.GetVector3("scale");
            }
            else
            {
                return Vector3.One;
            }
        }

        private Quaternion GetRotation(Entity entity)
        {
            if (entity.HasQuaternion("rotation"))
            {
                return entity.GetQuaternion("rotation");
            }
            else
            {
                return Quaternion.Identity;
            }
        }

        //private Entity target;
        private static readonly Vector3 gravityAcceleration = new Vector3(0, -500f, 0);
        private static readonly int iceSpikeDamage = 20;
        //private float straightAcceleration = 100f;
        //private float maxSpeed = 150f;
    }
}
