using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer.ParticleSystem.Stateful.Implementations
{
    public class IceSpike : StatefulParticleSystem
    {
        public IceSpike(
            Renderer renderer,
            WrappedContentManager wrappedContent,
            GraphicsDevice device
        )
        :   base(renderer, wrappedContent, device)
        {
            Effect effect = LoadEffect(wrappedContent);
            int rowCount = effect.Parameters["IceSpikePositionArray"].Elements.Count;
            positionArray = new Vector3[rowCount];
            directionArray = new Vector3[rowCount];
            deadArray = new bool[rowCount];
            gravityStartArray = new float[rowCount];
        }

        protected override void LoadResources(Renderer renderer, WrappedContentManager wrappedContent, GraphicsDevice device)
        {
            base.LoadResources(renderer, wrappedContent, device);
        }

        public override void UnloadResources()
        {
            // no need to release the trailSprite since its managed by the resource manager!
            base.UnloadResources();
        }

        private Effect LoadEffect(WrappedContentManager wrappedContent)
        {
            return wrappedContent.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/Implementations/IceSpike");
        }

        protected override Effect LoadCreateEffect(WrappedContentManager wrappedContent)
        {
            return LoadEffect(wrappedContent);
        }

        protected override Effect LoadUpdateEffect(WrappedContentManager wrappedContent)
        {
            return LoadEffect(wrappedContent);
        }

        protected override Effect LoadRenderEffect(WrappedContentManager wrappedContent)
        {
            return LoadEffect(wrappedContent);
        }

        protected override Texture2D LoadSprite(WrappedContentManager wrappedContent)
        {
            return wrappedContent.Load<Texture2D>("Textures/Sfx/IceSpikeTrail");
        }

        protected override double SimulationStep
        {
            get
            {
                return 1d / 60d;
            }
        }

        protected override Size GetSystemSize()
        {
            return Size.Max65536;
        }

        protected override void SetUpdateParameters(EffectParameterCollection parameters)
        {
            base.SetUpdateParameters(parameters);

            /*for (int i = 0; i < positionArray.Length; ++i)
            {
                positionArray[i] = Vector3.Zero;
                directionArray[i] = Vector3.Zero;
                gravityStartArray[i] = 0.0f;
            }*/

            parameters["IceSpikePositionArray"].SetValue(positionArray);
            parameters["IceSpikeDirectionArray"].SetValue(directionArray);
            parameters["IceSpikeGravityStartArray"].SetValue(gravityStartArray);
        }

        protected override void SetRenderingParameters(EffectParameterCollection parameters)
        {
            base.SetRenderingParameters(parameters);
        }

        public void SetPosition(int emitterIndex, Vector3 position)
        {
            positionArray[emitterIndex] = position;
        }

        public void SetDirection(int emitterIndex, Vector3 direction)
        {
            directionArray[emitterIndex] = direction;
            directionArray[emitterIndex].Normalize();
        }

        public void SetDead(int emitterIndex, bool dead)
        {
            deadArray[emitterIndex] = dead;
            gravityStartArray[emitterIndex] = dead ? 0.0f : 0.1f;
        }

        public bool IsDead(int emitterIndex)
        {
            return deadArray[emitterIndex];
        }

        private Vector3[] positionArray;
        private Vector3[] directionArray;
        private bool[] deadArray;
        private float[] gravityStartArray;
    }
}
