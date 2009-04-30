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
                throw new Exception(string.Format("model {0} has no collision volumes!", modelName));
            }

            if (entity.HasString("bv_type"))
            {
                string bv_type = entity.GetString("bv_type");
                if (bv_type == "cylinder")
                {
                    Cylinder3[] bvCylinders = new Cylinder3[collisionVolumes.Count];
                    for (int i = 0; i < collisionVolumes.Count; ++i)
                    { bvCylinders[i] = (Cylinder3)collisionVolumes[i].GetVolume(VolumeType.Cylinder3); }
                    Game.Instance.Simulation.CollisionManager.AddCollisionEntity(entity, this, bvCylinders, needAllContacts);
                }
                else if (bv_type == "alignedbox3tree")
                {
                    AlignedBox3Tree[] bvTrees = new AlignedBox3Tree[collisionVolumes.Count];
                    for (int i = 0; i < collisionVolumes.Count; ++i)
                        { bvTrees[i] = (AlignedBox3Tree)collisionVolumes[i].GetVolume(VolumeType.AlignedBox3Tree); }
                    Game.Instance.Simulation.CollisionManager.AddCollisionEntity(entity, this, bvTrees, needAllContacts);
                }
                else if (bv_type == "sphere")
                {
                    Sphere3[] bvSpheres = new Sphere3[collisionVolumes.Count];
                    for (int i = 0; i < collisionVolumes.Count; ++i)
                        { bvSpheres[i] = (Sphere3)collisionVolumes[i].GetVolume(VolumeType.Sphere3); }
                    Game.Instance.Simulation.CollisionManager.AddCollisionEntity(entity, this, bvSpheres, needAllContacts);
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
