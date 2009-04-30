using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.ParticleSystem.Emitter
{
    public class PointEmitter : ParticleEmitter
    {
        public PointEmitter(Vector3 point, float particlesPerSecond)
        {
            this.point = point;
            this.particlesPerSecond = particlesPerSecond;
        }

        public NewParticle[] CreateParticles(
            double dt
        )
        {
            elapsedTime += dt;

            if (lastTime < 0)
            {
                lastTime = elapsedTime;
            }


            double exactNumParticles = (elapsedTime - lastTime) * particlesPerSecond + fragmentLost;
            double floorNumParticles = System.Math.Floor(exactNumParticles);
            fragmentLost = exactNumParticles - floorNumParticles;
            int numParticles = (int)floorNumParticles;

            lastTime = elapsedTime;

            NewParticle[] particles = new NewParticle[numParticles];
            for (int i = 0; i < particles.Length; ++i)
            {
                double horizontalAngle = random.NextDouble() * MathHelper.Pi * 2.0;
                double verticalAngle = random.NextDouble() * MathHelper.Pi * 2.0;

                Vector3 velocity = new Vector3(
                    (float)(System.Math.Cos(horizontalAngle) * System.Math.Cos(verticalAngle)),
                    (float)System.Math.Sin(verticalAngle),
                    (float)(System.Math.Sin(horizontalAngle) * System.Math.Cos(verticalAngle)));
                velocity = velocity * 75;

                particles[i] = new NewParticle(
                    point, velocity);
            }

            return particles;
        }

        public Vector3 Point
        {
            get { return point; }
            set { point = value; }
        }

        private Vector3 point;
        private double particlesPerSecond;
        private double lastTime = -20.0;
        private double elapsedTime = 0.0;
        private double fragmentLost = 0.0;
        private Random random = new Random();
        private float circleAngle = 0.0f;
    }
}
