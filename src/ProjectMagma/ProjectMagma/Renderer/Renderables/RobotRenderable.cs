using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class RobotRenderable : BasicRenderable
    {
        public RobotRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model)
            : base(scale, rotation, position, model) { }

        protected override void SetDefaultMaterialParameters()
        {
            Alpha = 1.0f;
            SpecularPower = 10.0f;
            DiffuseColor = Vector3.One * 0.5f;
            SpecularColor = Vector3.One * 1.0f;
            EmissiveColor = Vector3.One * 0.0f;
        }
    }
}
