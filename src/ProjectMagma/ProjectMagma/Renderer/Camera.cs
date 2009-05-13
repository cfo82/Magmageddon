using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Integration;
using ProjectMagma.MathHelpers;

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

            //centerPosition = new EaseVector3(Position, 0.003f);
            centerPosition = new EaseVector3(Position, 0.004f);
            CenterView = Matrix.CreateLookAt(centerPosition.Value, Target, Up);

            Viewport viewport = Game.Instance.GraphicsDevice.Viewport;
            AspectRatio = (float)viewport.Width / (float)viewport.Height;
            this.renderer = renderer;

            //GoTo(new Vector3(0,2000,1065));
            //GoTo(new Vector3(0, 450, 3065));
        }

        public void Update(GameTime gameTime)
        {
            // update position
            centerPosition.Update(gameTime);

            // compute position
            Position = centerPosition.Value + (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds * 0.002f) * 12.0f * Up
                + (float)Math.Cos(gameTime.TotalGameTime.TotalMilliseconds * 0.002f) * 7.0f * (new Vector3(1,0,0));

            //centerPosition.X = renderer.CenterOfMass.X;
            
            // compute view and projection matrices
            View = Matrix.CreateLookAt(Position, Target, Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(FovRadians, AspectRatio, NearClip, FarClip);


        }

        public void GoTo(Vector3 targetPosition)
        {
            centerPosition.TargetValue = targetPosition;
        }

        public void RecomputeFrame(List<Renderable> renderables)
        {
            if (renderables.Count == 0)
                return;

            //Console.WriteLine("starting recomputing frame");

            //Vector2 min = new Vector2(float.PositiveInfinity);
            Vector2 maxAbs = new Vector2(float.NegativeInfinity);

            for (int i = 0; i < renderables.Count; i++)
            {
                if(!(renderables[i] is RobotRenderable || renderables[i] is IslandRenderable))
                    continue;

                Vector3 pos = renderables[i].Position;

                // project: returns 2d vector in [-1,1]x[-1,1] independently of aspect ratio
                Vector3 rawProjection = Vector3.Transform(pos, View * Projection);
                Vector2 projection = new Vector2(
                    rawProjection.X / rawProjection.Z,
                    rawProjection.Y / rawProjection.Z
                );

                // determine maximal deviation
                maxAbs.X = Math.Max(maxAbs.X, Math.Abs(projection.X));
                maxAbs.Y = Math.Max(maxAbs.Y, Math.Abs(projection.Y));
                //float maxDeviation = Math.Max(
                //    maxAbs.X,
                //    maxAbs.Y
                //max.X = Math.Max(min.X, projection.X);
                //max.Y = Math.Max(min.Y, projection.Y);

                #region debug: comparison of projection operations

                //Vector3 proj = renderer.Device.Viewport.Project(pos, Projection, View, Matrix.Identity);
                //proj.X /= renderer.Device.Viewport.Width;
                //proj.Y /= renderer.Device.Viewport.Height;
                //proj *= 2;
                //proj -= Vector3.One;

                //Vector3 proj2 = Vector3.Transform(pos, View * Projection);
                //proj2.X /= proj2.Z;
                //proj2.Y /= -proj2.Z;                
                //Console.WriteLine(proj.ToString() + "/" + proj2.ToString() + "/"+(proj.X-proj2.X)+"/"+(proj.Y-proj2.Y));

                #endregion
            }
            //Console.WriteLine("ending recomputing frame");
            float maxDeviation = Math.Max(maxAbs.X, maxAbs.Y) * 1.2f;

            Vector3 normal = Vector3.Normalize(Target - centerPosition.Value) * renderer.Device.Viewport.Width;
            GoTo(centerPosition.Value - normal * (maxDeviation - 1.0f));

            //Console.WriteLine(maxDeviation);
            //Console.WriteLine(centerPosition.TargetValue.ToString());
        }


        private Renderer renderer;
        //private Vector3 centerPositionOld;
        private EaseVector3 centerPosition;

        public Vector3 CenterPosition
        {
            get { return centerPosition.Value; }
        }

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
