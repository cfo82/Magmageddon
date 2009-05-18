using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.ParticleSystem.Emitter
{
    public class SnowEmitter : ParticleEmitter
    {
        public SnowEmitter(
            float particlesPerSecond
        )
        {
            this.particlesPerSecond = particlesPerSecond;
        }

        public int CalculateParticleCount(
            double lastFrameTime,
            double currentFrameTime
        )
        {
            double numParticles = (currentFrameTime - lastFrameTime) * particlesPerSecond + fragmentLost;
            fragmentLost = numParticles - System.Math.Floor(numParticles);
            return (int)System.Math.Floor(numParticles);
        }

        public void CreateParticles(
            double lastFrameTime,
            double currentFrameTime,
            CreateVertex[] array,
            int start,
            int length
        )
        {
            for (int i = 0; i < length; ++i)
            {
                array[start + i].ParticlePosition = RandomPoint();
                array[start + i].ParticleVelocity = Vector3.Zero;
            }
        }

        public Vector3 RandomPoint()
        {
            float planeSizeX = 1400;
            float planeSizeY = 30;
            float planeSizeZ = 1200;

            return new Vector3(
                ((float)random.NextDouble() - 0.5f) * planeSizeX,
                750.0f + ((float)random.NextDouble() - 0.5f) * planeSizeY,
                ((float)random.NextDouble() - 0.5f) * planeSizeZ
                );
        }

        private double particlesPerSecond;
        private double fragmentLost = 0.0;
        private static Random random = new Random();
    }
}
