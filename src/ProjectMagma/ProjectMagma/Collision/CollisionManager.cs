using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Framework;
using ProjectMagma.Shared.BoundingVolume;
using ProjectMagma.Collision.CollisionTests;

namespace ProjectMagma.Collision
{
    public class CollisionManager
    {
        public CollisionManager()
        {
            collisionEntities = new List<CollisionEntity>();
        }

        public void AddCollisionEntity(Entity entity, CollisionProperty property, BoundingCylinder cylinder)
        {
            AddCollisionEntity(new CollisionEntity(entity, property, cylinder));
        }

        public void AddCollisionEntity(Entity entity, CollisionProperty property, BoundingSphere sphere)
        {
            AddCollisionEntity(new CollisionEntity(entity, property, sphere));
        }

        public void AddCollisionEntity(CollisionEntity collisionEntity)
        {
            if (!collisionEntities.Contains(collisionEntity) && !ContainsCollisionEntity(collisionEntity.collisionProperty))
            {
                collisionEntities.Add(collisionEntity);
            }
        }

        public bool ContainsCollisionEntity(CollisionProperty property)
        {
            for (int i = 0; i < collisionEntities.Count; ++i)
            {
                if (collisionEntities[i].collisionProperty == property)
                {
                    return true;
                }
            }

            return false;
        }

        public void RemoveCollisionEntity(CollisionProperty property)
        {
            for (int i = 0; i < collisionEntities.Count; ++i)
            {
                if (collisionEntities[i].collisionProperty == property)
                {
                    collisionEntities.RemoveAt(i);
                    return;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < collisionEntities.Count; ++i)
            {
                for (int j = i + 1; j < collisionEntities.Count; ++j)
                {
                    CollisionEntity entity1 = collisionEntities[i];
                    CollisionEntity entity2 = collisionEntities[j];
                    ContactTest test = contactTests[BoundingVolumeTypeUtil.ToNumber(entity1.volumeType)][BoundingVolumeTypeUtil.ToNumber(entity2.volumeType)];
                    Contact c = test(entity1.entity, entity1.volume, entity2.entity, entity2.volume);
                    if (c != null)
                    {
                        //Console.WriteLine("COLLISION!!");
                        entity1.collisionProperty.FireContact(gameTime, c);
                        entity2.collisionProperty.FireContact(gameTime, c);
                    }
                }
            }
        }

        public readonly ContactTest[][] contactTests = new ContactTest[2][] {
            new ContactTest[2] { ContactSphereSphere.Test, ContactSphereCylinder.Test },
            new ContactTest[2] { ContactCylinderSphere.Test, ContactCylinderCylinder.Test } 
        };
        public List<CollisionEntity> collisionEntities;
    }
}
