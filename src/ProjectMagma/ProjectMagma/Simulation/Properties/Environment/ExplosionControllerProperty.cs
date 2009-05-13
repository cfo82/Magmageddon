using System;
using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Simulation.Collision;

namespace ProjectMagma.Simulation
{
    public class ExplosionControllerProperty : Property
    {

        public ExplosionControllerProperty()
        {
        }

        private void OnUpdate(Entity explosion, SimulationTime simTime)
        {
            if (hadCollision)
            {
                ((CollisionProperty)explosion.GetProperty("collision")).OnContact -= ExplosionCollisionHandler;
            }
            
            if(simTime.At > liveTo)
            {
                Game.Instance.Simulation.EntityManager.RemoveDeferred(explosion);
            }
        }

        public void OnAttached(Entity explosion)
        {
            liveTo = Game.Instance.Simulation.Time.At + explosion.GetInt("live_span");

            ((CollisionProperty)explosion.GetProperty("collision")).OnContact += ExplosionCollisionHandler;
         
            explosion.Update += OnUpdate;
        }

        public void OnDetached(Entity explosion)
        {
            explosion.Update -= OnUpdate;

            ((CollisionProperty)explosion.GetProperty("collision")).OnContact -= ExplosionCollisionHandler;
        }

        private void ExplosionCollisionHandler(SimulationTime simTime, Contact contact)
        {
            Entity explosion = contact.EntityA;
            Entity other = contact.EntityB;
            if (other.HasAttribute("kind") && other.GetString("kind") == "player")
            {
                // apply damage to player
                if (explosion.HasAttribute("damage"))
                {
                    other.SetInt("health", other.GetInt("health") - explosion.GetInt("damage"));
                }
                if(explosion.HasAttribute("freeze_time"))
                {
                    other.SetInt("frozen", explosion.GetInt("freeze_time"));
                }
            }
            hadCollision = true;
        }

        private bool hadCollision = false;
        private float liveTo;
    }
}
