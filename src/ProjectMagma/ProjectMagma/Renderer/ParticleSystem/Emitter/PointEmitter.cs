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

        public NewParticle[] CreateParticles(GameTime gameTime)
        {
            elapsedTime += ((float)gameTime.ElapsedGameTime.Milliseconds / 1000.0f);

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

        /// <summary>
        /// Helper used by the UpdateFire method. Chooses a random location
        /// around a circle, at which a fire particle will be created.
        /// </summary>
        private Vector3 RandomPointOnCircle(GameTime gameTime)
        {
            //circleAngle += ((float)gameTime.ElapsedGameTime.Milliseconds / 500000.0f);

            const float radius = 30;
            const float height = 40;

            double angle = random.NextDouble() * Math.PI * 2;

            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);

            return new Vector3(
                x * radius * (float)Math.Cos(circleAngle),
                y * radius + height,
                x * radius * (float)Math.Sin(circleAngle));
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
