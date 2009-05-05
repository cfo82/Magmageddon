using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.ParticleSystem.Emitter
{
    public class FireExplosionEmitter : ParticleEmitter
    {
        public FireExplosionEmitter()
        {
            currentPoint = RandomPoint();
        }

        public NewParticle[] CreateParticles(double lastFrameTime, double currentFrameTime)
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
            numParticles += (int) ((15 + random.Next(0, 10)) * (1.0 - diff)); 

            NewParticle[] particles = new NewParticle[numParticles];
            for (int i = 0; i < particles.Length; ++i)
            {
                particles[i] = new NewParticle(currentPoint + RandomOffset(), RandomVelocity());
            }

            return particles;
        }

        /// <summary>
        /// Helper used by the UpdateFire method. Chooses a random location
        /// around a circle, at which a fire particle will be created.
        /// </summary>
        private Vector3 RandomPoint()
        {
            const float radius = 1200;
            const float height = 0;
            return new Vector3(
                (float)(random.NextDouble()-0.5) * 2 * radius, height, (float)(random.NextDouble()-0.8) * radius);
        }

        private Vector3 RandomOffset()
        {
            const float offset = 40;
            return new Vector3(
                (float)random.NextDouble() * 2 * offset - offset,
                (float)random.NextDouble() * 2 * offset - offset,
                (float)random.NextDouble() * 2 * offset - offset);
        }

        private Vector3 RandomVelocity()
        {
            const float speed = 150;

            double horizontalAngle = random.NextDouble() * MathHelper.Pi * 2.0;
            double verticalAngle = random.NextDouble() * MathHelper.Pi * 2.0;

            return new Vector3(
                (float)(System.Math.Cos(horizontalAngle) * System.Math.Cos(verticalAngle) * (random.NextDouble() * speed)),
                (float)System.Math.Abs(System.Math.Sin(verticalAngle) * (random.NextDouble() * speed)),
                (float)(System.Math.Sin(horizontalAngle) * System.Math.Cos(verticalAngle) * (random.NextDouble() * speed)));
        }

        private float untilNextExplosion = (float)random.NextDouble() * 3.0f;
        private double lastExplosion = 0.0;
        private static Random random = new Random();
        private Vector3 currentPoint;
    }
}
