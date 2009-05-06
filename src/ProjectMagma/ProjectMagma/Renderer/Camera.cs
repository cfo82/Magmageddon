using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class Camera
    {
        public Camera(Renderer renderer)
        {
            Position = new Vector3(0, 450, 1065);
            Target = new Vector3(0, 180, 0);
            Up = new Vector3(0, 1, 0);

            NearClip = 1.0f;
            FarClip = 10000.0f;
            FovRadians = MathHelper.ToRadians(33.0f);

            centerPosition = Position;
            CenterView = Matrix.CreateLookAt(centerPosition, Target, Up);

            Viewport viewport = Game.Instance.GraphicsDevice.Viewport;
            AspectRatio = (float)viewport.Width / (float)viewport.Height;
            this.renderer = renderer;
        }

        public void Update(GameTime gameTime)
        {
            // compute position
            Position = centerPosition + (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds * 0.002f) * 12.0f * Up
                + (float)Math.Cos(gameTime.TotalGameTime.TotalMilliseconds * 0.002f) * 7.0f * (new Vector3(1,0,0));

            //centerPosition.X = renderer.CenterOfMass.X;
            
            // compute view and projection matrices
            View = Matrix.CreateLookAt(Position, Target, Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(FovRadians, AspectRatio, NearClip, FarClip);
        }


        private Renderer renderer;
        private Vector3 centerPosition;

        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; }
        public float NearClip { get; set; }
        public float FarClip { get; set; }
        public float FovRadians { get; set; }
        public float AspectRatio;

        public Matrix CenterView { get; set; }
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }
    }
}
