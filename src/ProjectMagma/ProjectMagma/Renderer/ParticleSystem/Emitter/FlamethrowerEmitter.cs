﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.ParticleSystem.Emitter
{
    public class FlamethrowerEmitter : ParticleEmitter
    {
        public FlamethrowerEmitter(
            Vector3 point,
            Vector3 direction,
            float particlesPerSecond
        )
        {
            this.point = point;
            this.direction = direction;
            this.particlesPerSecond = particlesPerSecond;
        }

        public int CalculateParticleCount(
            double lastFrameTime,
            double currentFrameTime
        )
        {
            int numParticles = 0;

            double exactNumParticles = (currentFrameTime - lastFrameTime) * particlesPerSecond + fragmentLost;
            double floorNumParticles = System.Math.Floor(exactNumParticles);
            fragmentLost = exactNumParticles - floorNumParticles;
            numParticles = (int)floorNumParticles;
           
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
            float innerSpeed = 540;
            float outerSpeed = 470;

            for (int i = 0; i < length; ++i)
            {
                float angle = 0.125f;
                //double horizontalAngle = random.NextDouble() * MathHelper.Pi * angle - MathHelper.Pi * (angle/2);
                //double verticalAngle = random.NextDouble() * MathHelper.Pi * angle - MathHelper.Pi * (angle/2);
                double circleAngle = random.NextDouble() * MathHelper.Pi * 2;

                Vector3 up = Vector3.Up;
                Vector3 right = Vector3.Cross(up, direction);
                up = Vector3.Cross(direction, right);

                Vector3 displacement =
                    right * (float)System.Math.Cos(circleAngle) * (float)random.NextDouble() * angle +
                    up * (float)System.Math.Sin(circleAngle) * (float)random.NextDouble() * angle;
                float amount = displacement.Length() / angle;

                Vector3 velocity = direction + displacement;
                velocity *= ((1f - amount) * innerSpeed + amount * outerSpeed) * (0.7f + (float)random.NextDouble() * 0.3f);

                array[start + i].ParticlePosition = point;
                array[start + i].ParticleVelocity = velocity;
            }
        }

        public Vector3 Point
        {
            get { return point; }
            set { point = value; }
        }

        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        private Vector3 point;
        private Vector3 direction;
        private double particlesPerSecond;
        private double fragmentLost = 0.0;
        private static Random random = new Random();
    }
}
