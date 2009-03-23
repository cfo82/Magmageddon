﻿using System;
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

        //static int collisionCount = 0;

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < collisionEntities.Count; ++i)
            {
                for (int j = i + 1; j < collisionEntities.Count; ++j)
                {
                    CollisionEntity entity1 = collisionEntities[i];
                    CollisionEntity entity2 = collisionEntities[j];
                    Matrix worldTransform1 = CalculateWorldTransform(entity1);
                    Matrix worldTransform2 = CalculateWorldTransform(entity2);
                    ContactTest test = contactTests[BoundingVolumeTypeUtil.ToNumber(entity1.volumeType)][BoundingVolumeTypeUtil.ToNumber(entity2.volumeType)];
                    Contact c = test(
                        entity1.entity, entity1.volume, worldTransform1, GetPosition(entity1.entity), GetRotation(entity1.entity), GetScale(entity1.entity),
                        entity2.entity, entity2.volume, worldTransform2, GetPosition(entity2.entity), GetRotation(entity2.entity), GetScale(entity2.entity)
                        );
                    if (c != null)
                    {
                        //Console.WriteLine("Collision {0,4}: between {1} and {2}!", collisionCount, entity1.entity.Name, entity2.entity.Name);
                        entity1.collisionProperty.FireContact(gameTime, c);
                        entity2.collisionProperty.FireContact(gameTime, c.Reverse());
                        //++collisionCount;
                    }
                }
            }
        }

        private Matrix CalculateWorldTransform(CollisionEntity e)
        {
            return 
                Matrix.CreateScale(GetScale(e.entity)) *
                Matrix.CreateFromQuaternion(GetRotation(e.entity)) * 
                Matrix.CreateTranslation(GetPosition(e.entity));
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

        private readonly ContactTest[][] contactTests = new ContactTest[2][] {
            new ContactTest[2] { ContactSphereSphere.Test, ContactSphereCylinder.Test },
            new ContactTest[2] { ContactCylinderSphere.Test, ContactCylinderCylinder.Test } 
        };
        private List<CollisionEntity> collisionEntities;
    }
}