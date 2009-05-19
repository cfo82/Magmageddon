using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class PowerupRenderable : BasicRenderable
    {
        public PowerupRenderable(
            double timestamp,
            Vector3 scale, Quaternion rotation, Vector3 position, Model model
        )
        :   base(timestamp, scale, rotation, position, model)
        {
            rotationTime = 0.0f;
        }

        public override void Update(Renderer renderer)
        {
            base.Update(renderer);

            rotationTime = renderer.Time.At / 1000.0;
        }

        protected override Matrix World {
            get
            {
                return Matrix.CreateRotationY((float) rotationTime) * Matrix.CreateTranslation(Vector3.Up * 0.25f) * originalWorld;
            }

            set
            {
                originalWorld = value;
            }
        }

        private Matrix originalWorld;
        private double rotationTime;
    }
}
