using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class RobotRenderable : BasicRenderable
    {
        public RobotRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model)
            : base(scale, rotation, position, model) {}

        protected override void SetBasicEffectParameters(BasicEffect basicEffect)
        {
            basicEffect.DiffuseColor = Vector3.One * 0.5f;
            basicEffect.SpecularColor = Vector3.One * 0.5f;
            basicEffect.EmissiveColor = Vector3.One * 0.0f;
        }
    }
}
