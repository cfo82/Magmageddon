using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Integration;
using ProjectMagma.Framework;

namespace ProjectMagma.Simulation
{
    public class CameraProperty : Property
    {
        //DoublyIntegratedFloat phi, theta; // for later use

        public CameraProperty()
        {
        }

        public void OnAttached(AbstractEntity entity)
        {
            (entity as Entity).Update += OnUpdate;
            entity.AddMatrixAttribute("view", Matrix.Identity);
            entity.AddMatrixAttribute("projection", Matrix.Identity);
        }

        public void OnDetached(AbstractEntity entity)
        {
            (entity as Entity).Update -= OnUpdate;
        }

        private void OnUpdate(Entity entity, SimulationTime simTime)
        {
            #region update view matrix

            // preconditions
            Debug.Assert(entity.HasAttribute(CommonNames.Position));
            Debug.Assert(entity.HasAttribute("target"));
            Debug.Assert(entity.HasAttribute("up"));

            Vector3 movingPosition =
                entity.GetVector3(CommonNames.Position) +
                //entity.GetVector3("up") * (float)Math.Sin(simTime.At * 0.002f) * 10.0f;
                entity.GetVector3("up") * (float)Math.Sin(1 * 0.002f) * 10.0f;

            // compute matrix
            view = Matrix.CreateLookAt(
                movingPosition,
                entity.GetVector3("target"),
                entity.GetVector3("up")
            );

            #endregion

            #region update projection matrix

            // preconditions
            Debug.Assert(entity.HasAttribute("fov"));
            Debug.Assert(entity.HasAttribute("near_clip"));
            Debug.Assert(entity.HasAttribute("far_clip"));

            // get aspect ratio
            Viewport viewport = Game.Instance.GraphicsDevice.Viewport;
            float aspectRatio = (float)viewport.Width / (float)viewport.Height;

            // compute matrix
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(entity.GetFloat("fov")),
                aspectRatio,
                entity.GetFloat("near_clip"),
                entity.GetFloat("far_clip")
            );

            entity.SetMatrix("view", view);
            entity.SetMatrix("projection", projection);

            #endregion
        }

        private Matrix view;
        private Matrix projection;

        public Matrix View
        {
            get { return view; }
            set { view = value; }
        }

        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; }
        }
    }
}
