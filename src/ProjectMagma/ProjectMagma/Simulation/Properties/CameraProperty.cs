﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Simulation
{
    public class CameraProperty : Property
    {
        public CameraProperty()
        {
        }

        public void OnAttached(Entity entity)
        {
            entity.Update += OnUpdate;
            entity.AddMatrixAttribute("view", Matrix.Identity);
            entity.AddMatrixAttribute("projection", Matrix.Identity);
        }

        public void OnDetached(Entity entity)
        {
            entity.Update -= OnUpdate;
            // TODO: remove attributes!
        }

        private void OnUpdate(Entity entity, GameTime gameTime)
        {
            #region update view matrix

            // preconditions
            Debug.Assert(entity.HasAttribute("position"));
            Debug.Assert(entity.HasAttribute("target"));
            Debug.Assert(entity.HasAttribute("up"));

            Vector3 movingPosition =
                entity.GetVector3("position") +
                entity.GetVector3("up") * (float) Math.Sin(gameTime.TotalRealTime.TotalMilliseconds * 0.002f) * 10.0f;

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
            Viewport viewport = Game.Instance.Graphics.GraphicsDevice.Viewport;
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