using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Simulation.Collision
{
    public delegate void ContactHandler(SimulationTime simTime, Contact contact);

    public class CollisionProperty : Property
    {
        public CollisionProperty()
        {
        }

        public void OnAttached(
            Entity entity
        )
        {
            bool needAllContacts = entity.HasBool("need_all_contacts") && entity.GetBool("need_all_contacts");

            string modelName = entity.GetString("mesh");
            Model model = Game.Instance.Content.Load<Model>(modelName);
            List<VolumeCollection> collisionVolumes = (List<VolumeCollection>)model.Tag;
            if (collisionVolumes == null)
            {
                throw new System.Exception(string.Format("model {0} has no collision volumes!", modelName));
            }

            if (entity.HasString("bv_type"))
            {
                string bv_type = entity.GetString("bv_type");
                if (bv_type == "cylinder")
                {
                    object[] bvCylinders = new object[collisionVolumes.Count];
                    for (int i = 0; i < collisionVolumes.Count; ++i)
                    { bvCylinders[i] = collisionVolumes[i].GetVolume(VolumeType.Cylinder3); }
                    Game.Instance.Simulation.CollisionManager.AddCylinderCollisionEntity(entity, this, bvCylinders, needAllContacts);
                }
                else if (bv_type == "alignedbox3tree")
                {
                    object[] bvTrees = new object[collisionVolumes.Count];
                    for (int i = 0; i < collisionVolumes.Count; ++i)
                        { bvTrees[i] = collisionVolumes[i].GetVolume(VolumeType.AlignedBox3Tree); }
                    Game.Instance.Simulation.CollisionManager.AddAlignedBox3TreeCollisionEntity(entity, this, bvTrees, needAllContacts);
                }
                else if (bv_type == "sphere")
                {
                    object[] bvSpheres = new object[collisionVolumes.Count];
                    for (int i = 0; i < collisionVolumes.Count; ++i)
                        { bvSpheres[i] = collisionVolumes[i].GetVolume(VolumeType.Sphere3); }
                    Game.Instance.Simulation.CollisionManager.AddSphereCollisionEntity(entity, this, bvSpheres, needAllContacts);
                }
                else if (bv_type == "alignedbox3")
                {
                    AlignedBox3[] bvBoxes = new AlignedBox3[collisionVolumes.Count];
                    for (int i = 0; i < collisionVolumes.Count; ++i)
                        { bvBoxes[i] = (AlignedBox3)collisionVolumes[i].GetVolume(VolumeType.AlignedBox3); }
                    throw new System.Exception("bounding boxes not yet supported!");
                }
            }
        }

        public void OnDetached(
            Entity entity
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
