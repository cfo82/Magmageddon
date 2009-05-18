using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.ParticleSystem.Emitter
{
    public class IceExplosionEmitter : PointExplosionEmitter
    {
        private struct SecondaryEmitter
        {
            public Vector3 point;
            public int lastParticleCount;
        }

        public IceExplosionEmitter(
            Vector3 point,
            double explosionTime 
        )
        {
            currentPoint = point;
            this.explosionTime = explosionTime;
            initial = true;
            secondaryEmitters = new SecondaryEmitter[random.Next(3, 5)];
            for (int i = 0; i < secondaryEmitters.Length; ++i)
            {
                secondaryEmitters[i].point = currentPoint + SecondaryOffset();
            }
        }

        public override int CalculateParticleCount(
            double lastFrameTime,
            double currentFrameTime
        )
        {
            int numParticles = 0;

            { // primary
                if (initial)
                {
                    numParticles += 10 + random.Next(0, 10);
                }

                double diff = (currentFrameTime - explosionTime) / (0.6);
                if (diff > 1.0) { diff = 1.0; }
                numParticles += (int)((5 + random.Next(0, 10)) * (1.0 - diff));

                primaryParticleCount = numParticles;
            }

            for (int i = 0; i < secondaryEmitters.Length; ++i)
            {
                secondaryEmitters[i].lastParticleCount = 0;

                if (initial)
                {
                    secondaryEmitters[i].lastParticleCount += 5 + random.Next(0, 5);
                }

                double diff = (currentFrameTime - explosionTime) / (0.6);
                if (diff > 1.0) { diff = 1.0; }
                secondaryEmitters[i].lastParticleCount += (int)((2 + random.Next(0, 3)) * (1.0 - diff));

                numParticles += secondaryEmitters[i].lastParticleCount;
            }

            if (initial)
            {
                initial = false;
            }

            return numParticles;
        }

        public override void CreateParticles(
            double lastFrameTime,
            double currentFrameTime,
            CreateVertex[] array,
            int start,
            int length
        )
        {
            int currentOffset = start;

            for (int i = 0; i < primaryParticleCount; ++i)
            {
                array[currentOffset].ParticlePosition = currentPoint + RandomOffset();
                array[currentOffset].ParticleVelocity = RandomVelocity();
                ++currentOffset;
            }

            for (int i = 0; i < secondaryEmitters.Length; ++i)
            {
                for (int j = 0; j < secondaryEmitters[i].lastParticleCount; ++j)
                {
                    array[currentOffset].ParticlePosition = secondaryEmitters[i].point + RandomOffset();
                    array[currentOffset].ParticleVelocity = RandomVelocity();
                    ++currentOffset;
                }
            }

            Debug.Assert(currentOffset - start == length);
        }

        private Vector3 RandomOffset()
        {
            const float offset = 20;
            return new Vector3(
                (float)(random.NextDouble() - 0.5) * 2 * offset,
                (float)(random.NextDouble() - 0.5) * 2 * offset,
                (float)(random.NextDouble() - 0.5) * 2 * offset);
        }

        private Vector3 SecondaryOffset()
        {
            const float offset = 40;
            return new Vector3(
                (float)(random.NextDouble() - 0.5) * 2 * offset,
                (float)(random.NextDouble() - 0.5) * 2 * offset,
                (float)(random.NextDouble() - 0.5) * 2 * offset);
        }

        private Vector3 RandomVelocity()
        {
            const float speed = 100;

            double horizontalAngle = random.NextDouble() * MathHelper.Pi * 2.0;
            double verticalAngle = random.NextDouble() * MathHelper.Pi * 2.0;

            return new Vector3(
                (float)(System.Math.Cos(horizontalAngle) * System.Math.Cos(verticalAngle) * (random.NextDouble() * speed)),
                (float)System.Math.Abs(System.Math.Sin(verticalAngle) * (random.NextDouble() * speed)),
                (float)(System.Math.Sin(horizontalAngle) * System.Math.Cos(verticalAngle) * (random.NextDouble() * speed)));
        }

        private double explosionTime = 0.0;
        private static Random random = new Random();
        private Vector3 currentPoint;
        private bool initial;
        private int primaryParticleCount;
        private SecondaryEmitter[] secondaryEmitters;
    }
}
