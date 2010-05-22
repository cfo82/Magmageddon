using System;
using Microsoft.Xna.Framework;
using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
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
                explosion.GetProperty<CollisionProperty>("collision").OnContact -= ExplosionCollisionHandler;
            }
            
            if(simTime.At > liveTo)
            {
                Game.Instance.Simulation.EntityManager.RemoveDeferred(explosion);
            }
        }

        public void OnAttached(AbstractEntity explosion)
        {
            liveTo = Game.Instance.Simulation.Time.At + explosion.GetInt("live_span");

            explosion.GetProperty<CollisionProperty>("collision").OnContact += ExplosionCollisionHandler;
         
            (explosion as Entity).Update += OnUpdate;
        }

        public void OnDetached(AbstractEntity explosion)
        {
            (explosion as Entity).Update -= OnUpdate;

            explosion.GetProperty<CollisionProperty>("collision").OnContact -= ExplosionCollisionHandler;
        }

        private void ExplosionCollisionHandler(SimulationTime simTime, Contact contact)
        {
            Entity explosion = contact.EntityA;
            Entity other = contact.EntityB;
            if (other.HasAttribute(CommonNames.Kind) && other.GetString(CommonNames.Kind) == "player")
            {
                // apply damage to player
                if (explosion.HasAttribute("damage"))
                {
                    other.SetFloat(CommonNames.Health, other.GetFloat(CommonNames.Health) - explosion.GetFloat("damage"));
                }
                if(explosion.HasAttribute("freeze_time"))
                {
                    other.SetInt(CommonNames.Frozen, explosion.GetInt("freeze_time"));
                }
                other.GetProperty<PlayerControllerProperty>("controller").CheckPlayerAttributeRanges(other);
            }
            hadCollision = true;
        }

        private bool hadCollision = false;
        private float liveTo;
    }
}
