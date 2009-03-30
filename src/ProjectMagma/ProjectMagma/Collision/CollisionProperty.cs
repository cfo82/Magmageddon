using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Framework;
using ProjectMagma.Shared.Math.Volume;
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
                    Cylinder3 bvCylinder = CalculateBoundingCylinder(entity);
                    Game.Instance.CollisionManager.AddCollisionEntity(entity, this, bvCylinder);
                }
                else if (bv_type == "sphere")
                {
                    BoundingSphere bvSphere = CalculateBoundingSphere(entity);
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

        private BoundingSphere CalculateBoundingSphere(Entity entity)
        {
            Model mesh = Game.Instance.Content.Load<Model>(entity.GetString("mesh"));

            // calculate center
            BoundingBox bb = CalculateBoundingBox(mesh);
            Vector3 center = (bb.Min + bb.Max) / 2;

            // calculate radius
            //            float radius = (bb.Max-bb.Min).Length() / 2;
            float radius = (bb.Max.Y - bb.Min.Y) / 2; // HACK: hack for player

            return new BoundingSphere(center, radius);
        }

        // calculates y-axis aligned bounding cylinder
        private Cylinder3 CalculateBoundingCylinder(Entity entity)
        {
            Model mesh = Game.Instance.Content.Load<Model>(entity.GetString("mesh"));

            // calculate center
            BoundingBox bb = CalculateBoundingBox(mesh);
            Vector3 center = (bb.Min + bb.Max) / 2;

            float top = bb.Max.Y;
            float bottom = bb.Min.Y;

            // calculate radius
            // a valid cylinder here is an extruded circle (not an oval) therefore extents in 
            // x- and z-direction should be equal.
            float radius = bb.Max.X - center.X;

            return new Cylinder3(new Vector3(center.X, top, center.Z),
                new Vector3(center.X, bottom, center.Z),
                radius);
        }

        private BoundingBox CalculateBoundingBox(Entity entity)
        {
            Model mesh = Game.Instance.Content.Load<Model>(entity.GetString("mesh"));
            return CalculateBoundingBox(mesh);
        }

        private BoundingBox CalculateBoundingBox(Model model)
        {
            return (BoundingBox)model.Tag;
        }
        
        public event ContactHandler OnContact;
    }
}
