﻿using System;
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
        private static readonly float cameraSpeed = 0.012f;

        public Camera(Renderer renderer)
        {
            Position = new Vector3(0, 500, 1065)*1.4f;
//            Target = new Vector3(0, 180, 0);
            Up = new Vector3(0, 1, 0);

            NearClip = 1.0f;
            FarClip = 10000.0f;
            //FovRadians = MathHelper.ToRadians(33.0f);
            FovRadians = MathHelper.ToRadians(27.0f);

            //centerPosition = new EaseVector3(Position, 0.003f);
            targetController = new EaseVector3(new Vector3(0, 180, 0), cameraSpeed);
            centerController = new EaseVector3(Position, cameraSpeed);
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
            centerController.Update(renderer.Time.PausableDtMs);
            targetController.Update(renderer.Time.PausableDtMs);

            // compute position

            //            int a = 450;
            //int b = 450;

            EyeToTarget = Vector3.Normalize(Target - CenterPosition);
            Right = Vector3.Cross(EyeToTarget, Up);

            Vector3 circleOffset = (float)Math.Sin(renderer.Time.At * 0.002f) * 12.0f * Up
                   + (float)Math.Cos(renderer.Time.At * 0.002f) * 7.0f * Vector3.Left;

            //Position = centerController.Value + circleOffset + Vector3.Up * (a + (b - maxWorldY));
            Position = centerController.Value + circleOffset;// +Vector3.Up * (a + (b - maxWorldY));

            //centerPosition.X = renderer.CenterOfMass.X;
            
            // compute view and projection matrices
            View = Matrix.CreateLookAt(Position, Target, Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(FovRadians, AspectRatio, NearClip, FarClip);


        }

        public void GoTo(Vector3 pos)
        {
            pos.X = Math.Max(pos.X, -100.0f);
            pos.X = Math.Min(pos.X, 100.0f);
            pos.Y = Math.Max(pos.Y, 400);
            pos.Y = Math.Min(pos.Y, 600.0f);
            pos.Z = Math.Max(pos.Z, -200.0f);
            pos.Z = Math.Min(pos.Z, 1300.0f);
            centerController.TargetValue = pos;
        }

        public void GoToTarget(Vector3 pos)
        {
            pos.X = Math.Max(pos.X, -100.0f);
            pos.X = Math.Min(pos.X, 100.0f);
            targetController.TargetValue = pos;
        }
        float maxWorldY;
        public void RecomputeFrame(List<Renderable> renderables)
        {
            return;
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
                //if (!(renderables[i] is RobotRenderable))
                if (renderables[i] is RobotRenderable)
                    ; // good, go on
                else if (renderables[i] is IslandRenderable && (renderables[i] as IslandRenderable).Interactable)
                    ; // good, go on
                else continue;

                //Vector3 ;

                Vector2 minProj, maxProj;
                ProjectRenderable(renderables[i], out minProj, out maxProj);

                // determine maximal deviation, could probably be made easier
                maxAbs.X = Math.Max(maxAbs.X, Math.Abs(minProj.X));
                maxAbs.Y = Math.Max(maxAbs.Y, Math.Abs(minProj.Y));
                maxAbs.X = Math.Max(maxAbs.X, Math.Abs(maxProj.X));
                maxAbs.Y = Math.Max(maxAbs.Y, Math.Abs(maxProj.Y));
                //float maxDeviation = Math.Max(
                //    maxAbs.X,
                //    maxAbs.Y
                min.X = Math.Min(min.X, minProj.X);
                min.Y = Math.Min(min.Y, minProj.Y);
                max.X = Math.Max(max.X, maxProj.X);
                max.Y = Math.Max(max.Y, maxProj.Y);

                //maxWorldY = Math.Max(maxWorldY, pos.Y);
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

            //Vector3 Normal = Vector3.Normalize(Target - centerController.Value);

            Vector3 correction = Vector3.Zero;
            Vector3 targetCorrection = Vector3.Zero;

            // zoom appropriately
            correction += -EyeToTarget * (maxDeviation - 1.0f);

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

        
        private void ProjectRenderable(Renderable renderable, out Vector2 minProj, out Vector2 maxProj)
        {
            // project: returns 2d vector in [-1,1]x[-1,1] independently of aspect ratio
            Vector3 pos = renderable.Position;
            Vector3 scale = (renderable as ModelRenderable).Scale;
            //scale.Y = 0.0f; // we ignore Y for auto zoom.
            float diagonalLength = scale.Length();


            Vector3 rawProjection;
            
            // transform min
            //rawProjection = Vector3.Transform(pos - Right*diagonalLength, View * Projection);

            Vector3 offset = (Up + Right + EyeToTarget) * diagonalLength;
            rawProjection = Vector3.Transform(pos - offset, View * Projection);
            minProj = new Vector2(
                rawProjection.X / rawProjection.Z,
                rawProjection.Y / rawProjection.Z
            );

            rawProjection = Vector3.Transform(pos + offset, View * Projection);
            maxProj = new Vector2(
                rawProjection.X / rawProjection.Z,
                rawProjection.Y / rawProjection.Z
            );

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
        public Vector3 Right { get; set; }
        public float NearClip { get; set; }
        public float FarClip { get; set; }
        public float FovRadians { get; set; }
        public float AspectRatio;
        public Vector3 EyeToTarget { get; set; }

        public Matrix CenterView { get; set; }
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }
    }
}
