using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Simulation;
using ProjectMagma.Shared.Math.Volume;
using ProjectMagma.Collision.CollisionTests;

namespace ProjectMagma.Collision
{
    public delegate void ContactHandler(GameTime gameTime, List<Contact> contacts);

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
                    Cylinder3 bvCylinder = GetBoundingCylinder(entity);
                    Game.Instance.Simulation.CollisionManager.AddCollisionEntity(entity, this, bvCylinder);
                }
                else if (bv_type == "alignedbox3tree")
                {
                    AlignedBox3Tree bvTree = GetAlignedBox3Tree(entity);
                    Game.Instance.Simulation.CollisionManager.AddCollisionEntity(entity, this, bvTree);
                }
                else if (bv_type == "sphere")
                {
                    Sphere3 bvSphere = GetBoundingSphere(entity);
                    Game.Instance.Simulation.CollisionManager.AddCollisionEntity(entity, this, bvSphere);
                }
            }
        }

        public void OnDetached(
            Entity entity
        )
        {
            Game.Instance.Simulation.CollisionManager.RemoveCollisionEntity(this);
        }

        public void FireContact(GameTime gameTime, List<Contact> contacts)
        {
            if (OnContact != null)
            {
                OnContact(gameTime, contacts);
            }
        }

        private Sphere3 GetBoundingSphere(Entity entity)
        {
            Model model = Game.Instance.Content.Load<Model>(entity.GetString("mesh"));
            VolumeCollection collection = (VolumeCollection)model.Tag;
            return (Sphere3)collection.GetVolume(VolumeType.Sphere3);
        }

        // calculates y-axis aligned bounding cylinder
        private Cylinder3 GetBoundingCylinder(Entity entity)
        {
            Model model = Game.Instance.Content.Load<Model>(entity.GetString("mesh"));
            VolumeCollection collection = (VolumeCollection)model.Tag;
            return (Cylinder3)collection.GetVolume(VolumeType.Cylinder3);
        }

        private AlignedBox3Tree GetAlignedBox3Tree(Entity entity)
        {
            Model model = Game.Instance.Content.Load<Model>(entity.GetString("mesh"));
            VolumeCollection collection = (VolumeCollection)model.Tag;
            return (AlignedBox3Tree)collection.GetVolume(VolumeType.AlignedBox3Tree);
        }

        private AlignedBox3 GetBoundingBox(Entity entity)
        {
            Model model = Game.Instance.Content.Load<Model>(entity.GetString("mesh"));
            VolumeCollection collection = (VolumeCollection)model.Tag;
            return (AlignedBox3)collection.GetVolume(VolumeType.AlignedBox3);
        }
        
        public event ContactHandler OnContact;
    }
}
