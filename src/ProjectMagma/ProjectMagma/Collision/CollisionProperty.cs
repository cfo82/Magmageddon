using Microsoft.Xna.Framework;
using ProjectMagma.Framework;
using ProjectMagma.Shared.BoundingVolume;
using ProjectMagma.Collision.CollisionTests;

namespace ProjectMagma.Collision
{
    public delegate void ContactHandler(GameTime gameTime, Contact c);

    public class CollisionProperty : Property
    {
        public CollisionProperty()
        {
        }

        public void OnAttached(
            Entity entity
        )
        {
            if (entity.HasString("bv_type"))
            {
                string bv_type = entity.GetString("bv_type");
                if (bv_type == "cylinder")
                {
                    BoundingCylinder bvCylinder = Game.CalculateBoundingCylinder(entity);
                    Game.Instance.CollisionManager.AddCollisionEntity(entity, this, bvCylinder);
                }
                else if (bv_type == "sphere")
                {
                    BoundingSphere bvSphere = Game.CalculateBoundingSphere(entity);
                    Game.Instance.CollisionManager.AddCollisionEntity(entity, this, bvSphere);
                }
            }
        }

        public void OnDetached(
            Entity entity
        )
        {
            Game.Instance.CollisionManager.RemoveCollisionEntity(this);
        }

        public void FireContact(GameTime gameTime, Contact c)
        {
            if (OnContact != null)
            {
                OnContact(gameTime, c);
            }
        }

        public event ContactHandler OnContact;
    }
}
