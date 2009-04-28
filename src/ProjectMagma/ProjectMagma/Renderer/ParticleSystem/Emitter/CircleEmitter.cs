using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.ParticleSystem.Emitter
{
    public class CircleEmitter : ParticleEmitter
    {
        public NewParticle[] CreateParticles(GameTime gameTime)
        {
            elapsedTime += ((float)gameTime.ElapsedGameTime.Milliseconds / 1000.0f);

            if (lastTime < 0)
            {
                lastTime = elapsedTime;
            }

            int numParticles = (int)(((elapsedTime - lastTime) / 0.02) * 25.0f);

            lastTime = elapsedTime;

            NewParticle[] particles = new NewParticle[numParticles];
            for (int i = 0; i < particles.Length; ++i)
            {
                Vector3 velocity = Vector3.Zero;
                float horizontalVelocity = MathHelper.Lerp(0, 15, (float)random.NextDouble());
                double horizontalAngle = random.NextDouble() * MathHelper.Pi;
                velocity.X += horizontalVelocity * (float)Math.Cos(horizontalAngle);
                velocity.Z += horizontalVelocity * (float)Math.Sin(horizontalAngle);
                velocity.Y += MathHelper.Lerp(50, 100, (float)random.NextDouble());

                particles[i] = new NewParticle(
                    RandomPointOnCircle(gameTime),
                    velocity);
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

        private double lastTime = -20.0;
        private double elapsedTime = 0.0;
        private Random random = new Random();
        private float circleAngle = 0.0f;
    }
}
