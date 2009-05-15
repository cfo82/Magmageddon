﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.ParticleSystem.Emitter
{
    public class PointEmitter : ParticleEmitter
    {
        public PointEmitter(
            double time,
            Vector3 point,
            float particlesPerSecond
        )
        {
            this.times[1] = time;
            this.points[1] = point;
            this.particlesPerSecond = particlesPerSecond;
        }

        public int CalculateParticleCount(
            double lastFrameTime,
            double currentFrameTime
        )
        {
            Debug.Assert(currentFrameTime >= times[0]);
            double amount = currentFrameTime - times[0];
            double interval = times[1] - times[0];
            double interpolation = amount / interval;
            Vector3 point = points[0] + (points[1] - points[0]) * (float)interpolation;

            double exactNumParticles = (currentFrameTime - lastFrameTime) * particlesPerSecond + fragmentLost;
            double floorNumParticles = System.Math.Floor(exactNumParticles);
            fragmentLost = exactNumParticles - floorNumParticles;
            int numParticles = (int)floorNumParticles;
            return numParticles;
        }

        public void CreateParticles(
            double lastFrameTime,
            double currentFrameTime,
            CreateVertex[] array,
            int start,
            int length
        )
        {
            Debug.Assert(currentFrameTime > times[0]);
            double amount = currentFrameTime - times[0];
            double interval = times[1] - times[0];
            double interpolation = amount / interval;
            Vector3 point = points[0] + (points[1] - points[0]) * (float)interpolation;

            for (int i = 0; i < length; ++i)
            {
                double horizontalAngle = random.NextDouble() * MathHelper.Pi * 2.0;
                double verticalAngle = random.NextDouble() * MathHelper.Pi * 2.0;

                Vector3 velocity = new Vector3(
                    (float)(System.Math.Cos(horizontalAngle) * System.Math.Cos(verticalAngle)),
                    (float)System.Math.Sin(verticalAngle),
                    (float)(System.Math.Sin(horizontalAngle) * System.Math.Cos(verticalAngle)));
                velocity = velocity * 75;

                array[start+i].ParticlePosition = point;
                array[start+i].ParticleVelocity = velocity;
            }
        }

        public void SetPoint(
            double time,
            Vector3 point
        )
        {
            Debug.Assert(time >= times[1]);
            times[0] = times[1];
            points[0] = points[1];
            times[1] = time;
            points[1] = point;
        }

        public double[] Times
        {
            get { return times; }
        }

        private double[] times = new double[2];
        private Vector3[] points = new Vector3[2];
        private double particlesPerSecond;
        private double fragmentLost = 0.0;
        private Random random = new Random();
    }
}
