using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.ParticleSystem.Emitter
{
    public class IceExplosionEmitter : ParticleEmitter
    {
        public IceExplosionEmitter(
            Vector3 point,
            double explosionTime 
        )
        {
            currentPoint = point;
            this.explosionTime = explosionTime;
            initial = true;
        }

        public NewParticle[] CreateParticles(double lastFrameTime, double currentFrameTime)
        {
            int numParticles = 0;

            if (initial)
            {
                numParticles += 20 + random.Next(0, 10);
                initial = false;
            }

            double diff = (currentFrameTime - explosionTime) / (0.6);
            if (diff > 1.0) { diff = 1.0; }
            numParticles += (int) ((5 + random.Next(0, 20)) * (1.0 - diff)); 

            NewParticle[] particles = new NewParticle[numParticles];
            for (int i = 0; i < particles.Length; ++i)
            {
                particles[i] = new NewParticle(currentPoint + RandomOffset(), RandomVelocity());
            }

            return particles;
        }

        private Vector3 RandomOffset()
        {
            const float offset = 40;
            return new Vector3(
                (float)(random.NextDouble()-0.5) * 2 * offset,
                (float)(random.NextDouble() - 0.5) * 2 * offset,
                (float)(random.NextDouble() - 0.5) * 2 * offset);
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

        private double explosionTime = 0.0;
        private static Random random = new Random();
        private Vector3 currentPoint;
        private bool initial;
    }
}
