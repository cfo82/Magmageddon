using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Shared.Math.Volume;
using ProjectMagma.Simulation.Collision.CollisionTests;

namespace ProjectMagma.Simulation.Collision
{
    public class CollisionManager
    {
        public CollisionManager()
        {
            collisionEntities = new List<CollisionEntity>();
        }

        public void AddCollisionEntity(Entity entity, CollisionProperty property, Cylinder3 cylinder)
        {
            AddCollisionEntity(new CollisionEntity(entity, property, cylinder));
        }

        public void AddCollisionEntity(Entity entity, CollisionProperty property, AlignedBox3Tree tree)
        {
            AddCollisionEntity(new CollisionEntity(entity, property, tree));
        }

        public void AddCollisionEntity(Entity entity, CollisionProperty property, Sphere3 sphere)
        {
            AddCollisionEntity(new CollisionEntity(entity, property, sphere));
        }

        void AddCollisionEntity(CollisionEntity collisionEntity)
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

        public void Update(SimulationTime simTime)
        {
            for (int i = 0; i < collisionEntities.Count; ++i)
            {
                for (int j = i + 1; j < collisionEntities.Count; ++j)
                {
                    List<Contact> contacts = new List<Contact>();
                    CollisionEntity entity1 = collisionEntities[i];
                    CollisionEntity entity2 = collisionEntities[j];
                    ContactTest test = contactTests[BoundingVolumeTypeUtil.ToNumber(entity1.volumeType), BoundingVolumeTypeUtil.ToNumber(entity2.volumeType)];

                    Matrix worldTransform1;
                    Vector3 position1 = GetPosition(entity1.entity);
                    Quaternion rotation1 = GetRotation(entity1.entity);
                    Vector3 scale1 = GetScale(entity1.entity);
                    CalculateWorldTransform(ref position1, ref rotation1, ref scale1, out worldTransform1);

                    Matrix worldTransform2;
                    Vector3 position2 = GetPosition(entity2.entity);
                    Quaternion rotation2 = GetRotation(entity2.entity);
                    Vector3 scale2 = GetScale(entity2.entity);
                    CalculateWorldTransform(ref position2, ref rotation2, ref scale2, out worldTransform2);

                    test(
                        entity1.entity, entity1.volume, ref worldTransform1, ref position1, ref rotation1, ref scale1,
                        entity2.entity, entity2.volume, ref worldTransform2, ref position2, ref rotation2, ref scale2,
                        contacts
                        );
                    if (contacts.Count > 0)
                    {
                        //if (
                        //    entity1.entity.HasString("kind") && entity2.entity.HasString("kind") &&(
                        //        entity1.entity.GetString("kind") == "pillar" ||
                        //        entity2.entity.GetString("kind") == "pillar" ||
                        //        (entity1.entity.GetString("kind") == "island" && entity2.entity.GetString("kind") == "island")
                        //    ))
                        //{
                        //    System.Console.WriteLine("Collision {0,4}: between {1} and {2}!", collisionCount, entity1.entity.Name, entity2.entity.Name);
                        //}
                        entity1.collisionProperty.FireContact(simTime, contacts);
                        foreach (Contact c in contacts)
                        {
                            c.Reverse();
                        }
                        entity2.collisionProperty.FireContact(simTime, contacts);
                        //++collisionCount;
                    }
                }
            }
        }

        private void CalculateWorldTransform(
            ref Vector3 translation,
            ref Quaternion rotation,
            ref Vector3 scale,
            out Matrix world
        )
        {
            Matrix scaleMatrix, rotationMatrix, translationMatrix, tempMatrix;
            Matrix.CreateScale(ref scale, out scaleMatrix);
            Matrix.CreateFromQuaternion(ref rotation, out rotationMatrix);
            Matrix.CreateTranslation(ref translation, out translationMatrix);
            Matrix.Multiply(ref scaleMatrix, ref rotationMatrix, out tempMatrix);
            Matrix.Multiply(ref tempMatrix, ref translationMatrix, out world);
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

        private readonly ContactTest[,] contactTests = new ContactTest[3, 3] {
            { ContactCylinderCylinder.Test, ContactCylinderMesh.Test, ContactCylinderSphere.Test },
            { ContactMeshCylinder.Test, ContactMeshMesh.Test, ContactMeshSphere.Test },
            { ContactSphereCylinder.Test, ContactSphereMesh.Test, ContactSphereSphere.Test } 
        };
        private List<CollisionEntity> collisionEntities;
    }
}
