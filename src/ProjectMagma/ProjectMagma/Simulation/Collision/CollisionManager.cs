using System.Diagnostics;
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

        public void AddCollisionEntity(Entity entity, CollisionProperty property, Cylinder3 cylinder, bool needAllContacts)
        {
            AddCollisionEntity(new CollisionEntity(entity, property, cylinder, needAllContacts));
        }

        public void AddCollisionEntity(Entity entity, CollisionProperty property, AlignedBox3Tree tree, bool needAllContacts)
        {
            AddCollisionEntity(new CollisionEntity(entity, property, tree, needAllContacts));
        }

        public void AddCollisionEntity(Entity entity, CollisionProperty property, Sphere3 sphere, bool needAllContacts)
        {
            AddCollisionEntity(new CollisionEntity(entity, property, sphere, needAllContacts));
        }

        void AddCollisionEntity(CollisionEntity collisionEntity)
        {
            if (!collisionEntities.Contains(collisionEntity) && !ContainsCollisionEntity(collisionEntity.CollisionProperty))
            {
                collisionEntities.Add(collisionEntity);
            }
        }

        public bool ContainsCollisionEntity(CollisionProperty property)
        {
            for (int i = 0; i < collisionEntities.Count; ++i)
            {
                if (collisionEntities[i].CollisionProperty == property)
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
                if (collisionEntities[i].CollisionProperty == property)
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
                    CollisionEntity entity1 = collisionEntities[i];
                    CollisionEntity entity2 = collisionEntities[j];
                    ContactTest test = contactTests[BoundingVolumeTypeUtil.ToNumber(entity1.VolumeType), BoundingVolumeTypeUtil.ToNumber(entity2.VolumeType)];

                    Matrix worldTransform1;
                    Vector3 position1 = GetPosition(entity1.Entity);
                    Quaternion rotation1 = GetRotation(entity1.Entity);
                    Vector3 scale1 = GetScale(entity1.Entity);
                    CalculateWorldTransform(ref position1, ref rotation1, ref scale1, out worldTransform1);

                    Matrix worldTransform2;
                    Vector3 position2 = GetPosition(entity2.Entity);
                    Quaternion rotation2 = GetRotation(entity2.Entity);
                    Vector3 scale2 = GetScale(entity2.Entity);
                    CalculateWorldTransform(ref position2, ref rotation2, ref scale2, out worldTransform2);

                    Contact contact = new Contact(entity1.Entity, entity2.Entity);

                    test(
                        entity1.Entity, entity1.Volume, ref worldTransform1, ref position1, ref rotation1, ref scale1,
                        entity2.Entity, entity2.Volume, ref worldTransform2, ref position2, ref rotation2, ref scale2,
                        entity1.NeedAllContacts || entity2.NeedAllContacts, ref contact
                        );
                    if (contact.Count > 0)
                    {
                        //System.Console.WriteLine(contact.Count);
                        //if (
                        //    entity1.entity.HasString("kind") && entity2.entity.HasString("kind") &&(
                        //        entity1.entity.GetString("kind") == "pillar" ||
                        //        entity2.entity.GetString("kind") == "pillar" ||
                        //        (entity1.entity.GetString("kind") == "island" && entity2.entity.GetString("kind") == "island")
                        //    ))
                        //{
                        //    System.Console.WriteLine("Collision {0,4}: between {1} and {2}!", collisionCount, entity1.Entity.Name, entity2.Entity.Name);
                        //}
                        Debug.Assert(contact.EntityA == entity1.Entity && contact.EntityB == entity2.Entity);
                        entity1.CollisionProperty.FireContact(simTime, contact);
                        contact.Reverse();
                        Debug.Assert(contact.EntityA == entity2.Entity && contact.EntityB == entity1.Entity);
                        entity2.CollisionProperty.FireContact(simTime, contact);
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
            { ContactMeshCylinder.Test, ContactMeshMeshBox.Test, ContactMeshSphere.Test },
            { ContactSphereCylinder.Test, ContactSphereMesh.Test, ContactSphereSphere.Test } 
        };
        private List<CollisionEntity> collisionEntities;
    }
}
