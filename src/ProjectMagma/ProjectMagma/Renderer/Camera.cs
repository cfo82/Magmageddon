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
//            Target = new Vector3(0, 180, 0);
            Up = new Vector3(0, 1, 0);

            NearClip = 1.0f;
            FarClip = 10000.0f;
            FovRadians = MathHelper.ToRadians(33.0f);

            //centerPosition = new EaseVector3(Position, 0.003f);
            targetController = new EaseVector3(new Vector3(0, 180, 0), 0.04f);
            centerController = new EaseVector3(Position, 0.04f);
            CenterView = Matrix.CreateLookAt(centerController.Value, Target, Up);

            Viewport viewport = Game.Instance.GraphicsDevice.Viewport;
            AspectRatio = (float)viewport.Width / (float)viewport.Height;
            this.renderer = renderer;

            //GoTo(new Vector3(0,2000,1065));
            //GoTo(new Vector3(0, 450, 3065));
        }

        public void Update(Renderer renderer)
        {
            // update position
            centerController.Update(renderer.Time.DtMs);
            targetController.Update(renderer.Time.DtMs);

            // compute position

                        int a = 450;
            int b = 450;
            


            Vector3 circleOffset = (float)Math.Sin(renderer.Time.At * 0.002f) * 12.0f * Up
                   + (float)Math.Cos(renderer.Time.At * 0.002f) * 7.0f * Vector3.Left;

            //Position = centerController.Value + circleOffset + Vector3.Up * (a + (b - maxWorldY));
            Position = centerController.Value + circleOffset;// +Vector3.Up * (a + (b - maxWorldY));

            //centerPosition.X = renderer.CenterOfMass.X;
            
            // compute view and projection matrices
            View = Matrix.CreateLookAt(Position, Target, Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(FovRadians, AspectRatio, NearClip, FarClip);


        }

        public void GoTo(Vector3 targetPosition)
        {
            centerController.TargetValue = targetPosition;
        }

        public void GoToTarget(Vector3 targetPosition)
        {
            targetController.TargetValue = targetPosition;
        }
        float maxWorldY;
        public void RecomputeFrame(List<Renderable> renderables)
        {
            
            if (renderables.Count == 0)
                return;

            //Console.WriteLine("starting recomputing frame");

            Vector2 min = new Vector2(float.PositiveInfinity);
            Vector2 max = new Vector2(float.NegativeInfinity);
            Vector2 maxAbs = new Vector2(float.NegativeInfinity);
            maxWorldY = float.NegativeInfinity;

            for (int i = 0; i < renderables.Count; i++)
            {
                //if(!(renderables[i] is RobotRenderable || renderables[i] is IslandRenderable))
                if (!(renderables[i] is RobotRenderable))
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
                min.X = Math.Min(min.X, projection.X);
                min.Y = Math.Min(min.Y, projection.Y);
                max.X = Math.Max(max.X, projection.X);
                max.Y = Math.Max(max.Y, projection.Y);
                maxWorldY = Math.Max(maxWorldY, pos.Y);
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
            float xCenter = (min.X + max.X) * 0.5f;
            float yRange = (max.Y - min.Y);

            Vector3 normal = Vector3.Normalize(Target - centerController.Value);

            Vector3 correction = Vector3.Zero;
            Vector3 targetCorrection = Vector3.Zero;

            // zoom appropriately
            correction += -normal * (maxDeviation - 1.0f);

            // x-move appropriately
            correction += Vector3.Right * (xCenter);
            targetCorrection += Vector3.Right * (xCenter);

            // y-move appropriately
            //correction += Up * (-0.2f-min.Y + 0.2f-max.Y) * 0.2f;
            //targetCorrection += Vector3.Down * (max.Y - 0.5f) * 0.2f;


            GoTo(centerController.Value + correction * renderer.Device.Viewport.Width);
            GoToTarget(targetController.Value + targetCorrection * renderer.Device.Viewport.Width);

            //Console.WriteLine(maxDeviation);
            //Console.WriteLine(centerPosition.TargetValue.ToString());
        }


        private Renderer renderer;
        //private Vector3 centerPositionOld;
        private EaseVector3 centerController;
        private EaseVector3 targetController;

        public Vector3 CenterPosition
        {
            get { return centerController.Value; }
        }

        public Vector3 Position { get; set; }
        public Vector3 Target
        {
            get { return targetController.Value;  }
        }
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
