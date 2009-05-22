using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Model;
using ProjectMagma.Shared.Math.Primitives;
using ProjectMagma.Framework;

namespace ProjectMagma.Simulation.Collision
{
    public delegate void ContactHandler(SimulationTime simTime, Contact contact);

    public class CollisionProperty : Property
    {
        public CollisionProperty()
        {
        }

        public void OnAttached(
            AbstractEntity entity
        )
        {
            bool needAllContacts = entity.HasBool("need_all_contacts") && entity.GetBool("need_all_contacts");

            string modelName = entity.GetString("mesh");
            VolumeCollection[] collisionVolumes = Game.Instance.ContentManager.Load<MagmaModel>(modelName).VolumeCollection;
            if (collisionVolumes == null)
            {
                throw new System.Exception(string.Format("model {0} has no collision volumes!", modelName));
            }

            if (entity.HasString("bv_type"))
            {
                string bv_type = entity.GetString("bv_type");
                if (bv_type == "cylinder")
                {
                    object[] bvCylinders = new object[collisionVolumes.Length];
                    for (int i = 0; i < collisionVolumes.Length; ++i)
                    { bvCylinders[i] = collisionVolumes[i].GetVolume(VolumeType.Cylinder3); }
                    Game.Instance.Simulation.CollisionManager.AddCylinderCollisionEntity(entity as Entity, this, bvCylinders, needAllContacts);
                }
                else if (bv_type == "alignedbox3tree")
                {
                    object[] bvTrees = new object[collisionVolumes.Length];
                    for (int i = 0; i < collisionVolumes.Length; ++i)
                        { bvTrees[i] = collisionVolumes[i].GetVolume(VolumeType.AlignedBox3Tree); }
                    Game.Instance.Simulation.CollisionManager.AddAlignedBox3TreeCollisionEntity(entity as Entity, this, bvTrees, needAllContacts);
                }
                else if (bv_type == "sphere")
                {
                    object[] bvSpheres = new object[collisionVolumes.Length];
                    for (int i = 0; i < collisionVolumes.Length; ++i)
                        { bvSpheres[i] = collisionVolumes[i].GetVolume(VolumeType.Sphere3); }
                    Game.Instance.Simulation.CollisionManager.AddSphereCollisionEntity(entity as Entity, this, bvSpheres, needAllContacts);
                }
                else if (bv_type == "alignedbox3")
                {
                    AlignedBox3[] bvBoxes = new AlignedBox3[collisionVolumes.Length];
                    for (int i = 0; i < collisionVolumes.Length; ++i)
                        { bvBoxes[i] = (AlignedBox3)collisionVolumes[i].GetVolume(VolumeType.AlignedBox3); }
                    throw new System.Exception("bounding boxes not yet supported!");
                }
            }
        }

        public void OnDetached(
            AbstractEntity entity
        )
        {
            Game.Instance.Simulation.CollisionManager.RemoveCollisionEntity(this);
        }

        public void FireContact(SimulationTime simTime, Contact contact)
        {
            if (OnContact != null)
            {
                OnContact(simTime, contact);
            }
        }
        
        public event ContactHandler OnContact;
    }
}
