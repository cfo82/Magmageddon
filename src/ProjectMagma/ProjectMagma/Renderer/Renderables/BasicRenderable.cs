using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class BasicRenderable : ModelRenderable
    {
        public BasicRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model)
            : base(scale, rotation, position, model) {}

        protected override void DrawMesh(Renderer renderer, GameTime gameTime, ModelMesh mesh)
        {
            //SetEffect(mesh, new BasicEffect(Game.Instance.GraphicsDevice, null));
            foreach (BasicEffect basicEffect in mesh.Effects)
            {
                // set lighting settings
                basicEffect.EnableDefaultLighting();
                basicEffect.PreferPerPixelLighting = true;

                // set matrices
                basicEffect.World = BoneTransformMatrix(mesh) * World;
                basicEffect.View = Game.Instance.View;
                basicEffect.Projection = Game.Instance.Projection;

                // set lights
                SetLights(basicEffect, renderer.LightManager);

                // set inherited parameters
                SetBasicEffectParameters(basicEffect);
            }
            mesh.Draw();
        }

        protected override void ApplyEffectsToModel()
        {
            // nothing to do, BasicEffect is applied by default.
        }

        protected virtual void SetBasicEffectParameters(BasicEffect basicEffect)
        {
        }

    }
}
