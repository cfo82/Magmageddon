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
                Vector3 particlePosition = currentPoint + RandomOffset();
                Vector3 particleVelocity = RandomVelocity();
                int emitterIndex = EmitterIndex;

                for (int j = 0; j < 3; ++j)
                {
                    array[currentOffset * 3 + j].ParticlePosition = particlePosition;
                    array[currentOffset * 3 + j].ParticleVelocity = particleVelocity;
                    array[currentOffset * 3 + j].EmitterIndex = emitterIndex;
                }
                ++currentOffset;
            }

            for (int i = 0; i < secondaryEmitters.Length; ++i)
            {
                for (int j = 0; j < secondaryEmitters[i].lastParticleCount; ++j)
                {
                    Vector3 particlePosition = secondaryEmitters[i].point + RandomOffset();
                    Vector3 particleVelocity = RandomVelocity();
                    int emitterIndex = EmitterIndex;

                    for (int k = 0; k < 3; ++k)
                    {
                        array[currentOffset * 3 + k].ParticlePosition = particlePosition;
                        array[currentOffset * 3 + k].ParticleVelocity = particleVelocity;
                        array[currentOffset * 3 + k].EmitterIndex = emitterIndex;
                    }
                    ++currentOffset;
                }
            }

            Debug.Assert(currentOffset - start == length);
        }

        const int Prerandom = 1024;
        static int nextRandomOffset = 0;
        static readonly Vector3[] randomOffsets = new Vector3[Prerandom];
        static readonly Vector3[] randomSecondaryOffsets = new Vector3[Prerandom];
        static readonly Vector3[] velocities = new Vector3[Prerandom];

        static IceExplosionEmitter()
        {
            const float offset = 20;
            for (int i = 0; i < Prerandom; ++i)
            {
                randomOffsets[i] = new Vector3(
                    (float)(random.NextDouble() - 0.5) * 2 * offset,
                    (float)(random.NextDouble() - 0.5) * 2 * offset,
                    (float)(random.NextDouble() - 0.5) * 2 * offset);
            }
            const float soffset = 40;
            for (int i = 0; i < Prerandom; ++i)
            {
                randomSecondaryOffsets[i] = new Vector3(
                    (float)(random.NextDouble() - 0.5) * 2 * soffset,
                    (float)(random.NextDouble() - 0.5) * 2 * soffset,
                    (float)(random.NextDouble() - 0.5) * 2 * soffset);
            }
            const float speed = 100;

            double horizontalAngle = random.NextDouble() * MathHelper.Pi * 2.0;
            double verticalAngle = random.NextDouble() * MathHelper.Pi * 2.0;

            for (int i = 0; i < Prerandom; ++i)
            {
                velocities[i] = new Vector3(
                    (float)(System.Math.Cos(horizontalAngle) * System.Math.Cos(verticalAngle) * (random.NextDouble() * speed)),
                    (float)System.Math.Abs(System.Math.Sin(verticalAngle) * (random.NextDouble() * speed)),
                    (float)(System.Math.Sin(horizontalAngle) * System.Math.Cos(verticalAngle) * (random.NextDouble() * speed)));
            }
        }

        private static Vector3 RandomOffset()
        {
            nextRandomOffset = (nextRandomOffset + 1) % Prerandom;
            return randomOffsets[nextRandomOffset];
        }

        private static Vector3 SecondaryOffset()
        {
            nextRandomOffset = (nextRandomOffset + 1) % Prerandom;
            return randomSecondaryOffsets[nextRandomOffset];
        }

        private Vector3 RandomVelocity()
        {
            nextRandomOffset = (nextRandomOffset + 1) % Prerandom;
            return velocities[nextRandomOffset];
        }

        public override int EmitterIndex { set; get; }

        private double explosionTime = 0.0;
        private static Random random = new Random();
        private Vector3 currentPoint;
        private bool initial;
        private int primaryParticleCount;
        private SecondaryEmitter[] secondaryEmitters;
    }
}
