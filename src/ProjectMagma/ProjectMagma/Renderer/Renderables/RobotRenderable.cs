using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class RobotRenderable : TexturedRenderable
    {
        public RobotRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model, Vector3 color1, Vector3 color2)
            : base(scale, rotation, position, model, null, null, null)
        {
            this.color1 = color1;
            this.color2 = color2;
        }

        protected override void SetDefaultMaterialParameters()
        {
            Alpha = 1.0f;
            SpecularPower = 10.0f;
            DiffuseColor = color1;
            SpecularColor = color2;
            EmissiveColor = Vector3.One * 0.0f;
        }

        private Vector3 color1;
        private Vector3 color2;
    }
}
