using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.ParticleSystem.Emitter
{
    public class LavaExplosionEmitter : ParticleEmitter
    {
        public LavaExplosionEmitter()
        {
            currentPoint = RandomPoint();
        }

        public int CalculateParticleCount(
            double lastFrameTime,
            double currentFrameTime
        )
        {
            int numParticles = 0;

            if (currentFrameTime - lastExplosion > untilNextExplosion)
            {
                lastExplosion = currentFrameTime;
                currentPoint = RandomPoint();
                numParticles += 20 + random.Next(0, 10);
                untilNextExplosion = 3.0f + (float)random.NextDouble() * 3;
            }

            double diff = (currentFrameTime - lastExplosion) / (2.0);
            if (diff > 1.0) { diff = 1.0; }
            numParticles += (int)((15 + random.Next(0, 10)) * (1.0 - diff));

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
            for (int i = 0; i < length; ++i)
            {
                array[start + i].ParticlePosition = currentPoint + RandomOffset();
                array[start + i].ParticleVelocity = RandomVelocity();
                array[start + i].EmitterIndex = EmitterIndex;
            }
        }


        const int Prerandom = 1024;
        static int nextRandomOffset = 0;
        static readonly Vector3[] randomPositions = new Vector3[Prerandom];
        static readonly Vector3[] randomOffsets = new Vector3[Prerandom];
        static readonly Vector3[] velocities = new Vector3[Prerandom];

        static LavaExplosionEmitter()
        {
            const float radius = 1200;
            const float height = 0;
            for (int i = 0; i < Prerandom; ++i)
            {
                randomPositions[i] = new Vector3(
                    (float)(random.NextDouble()-0.5) * 2 * radius, height, (float)(random.NextDouble()-0.8) * radius);
            }

            const float offset = 40;
            for (int i = 0; i < Prerandom; ++i)
            {
                randomOffsets[i] = new Vector3(
                    (float)(random.NextDouble() - 0.5) * 2 * offset,
                    (float)(random.NextDouble() - 0.5) * 2 * offset,
                    (float)(random.NextDouble() - 0.5) * 2 * offset);
            }

            const float speed = 150;

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

        private static Vector3 RandomPoint()
        {
            nextRandomOffset = (nextRandomOffset + 1) % Prerandom;
            return randomPositions[nextRandomOffset];
        }

        private static Vector3 RandomOffset()
        {
            nextRandomOffset = (nextRandomOffset + 1) % Prerandom;
            return randomOffsets[nextRandomOffset];
        }

        private Vector3 RandomVelocity()
        {
            nextRandomOffset = (nextRandomOffset + 1) % Prerandom;
            return velocities[nextRandomOffset];
        }

        public int EmitterIndex { set; get; }

        private float untilNextExplosion = (float)random.NextDouble() * 3.0f;
        private double lastExplosion = 0.0;
        private static Random random = new Random();
        private Vector3 currentPoint;
    }
}
