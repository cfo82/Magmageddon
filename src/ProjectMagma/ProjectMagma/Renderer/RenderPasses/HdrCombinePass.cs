using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Integration;

namespace ProjectMagma.Renderer
{
    public class HdrCombinePass : RenderPass
    {
        public HdrCombinePass(Renderer renderer, RenderTarget2D target0, RenderTarget2D target1)
            : base(renderer, target0, target1)
        {
            hdrCombineEffect = Game.Instance.ContentManager.Load<Effect>("Effects/HdrCombine");
            randomOffset = new DoublyIntegratedVector2
            (
               Vector2.Zero, new Vector2(0.002f, 0.002f), 0.0f, 0.0f, -0.4f, 0.4f
            );
        }

        public override void Render()
        {
            randomOffset.RandomlyIntegrate(Renderer.Time.DtMs, 0.2f, 0.0f);

            hdrCombineEffect.Parameters["GeometryRender"].SetValue(GeometryRender);
            hdrCombineEffect.Parameters["BlurGeometryRender"].SetValue(BlurGeometryRender);
            hdrCombineEffect.Parameters["RenderChannelColor"].SetValue(RenderChannelColor);
            hdrCombineEffect.Parameters["DepthTexture"].SetValue(DepthTexture);
            hdrCombineEffect.Parameters["CloudTexture"].SetValue(Renderer.VectorCloudTexture);

            hdrCombineEffect.Parameters["RandomOffset"].SetValue(randomOffset.Value);

            hdrCombineEffect.Parameters["BloomSensitivity"].SetValue(BloomSensitivity);
            hdrCombineEffect.Parameters["BloomIntensity"].SetValue(BloomIntensity);
            hdrCombineEffect.Parameters["BaseIntensity"].SetValue(BaseIntensity);
            hdrCombineEffect.Parameters["BloomSaturation"].SetValue(BloomSaturation);
            hdrCombineEffect.Parameters["BaseSaturation"].SetValue(BaseSaturation);
            hdrCombineEffect.Parameters["In1"].SetValue(In1);
            hdrCombineEffect.Parameters["Out1"].SetValue(Out1);
            hdrCombineEffect.Parameters["In2"].SetValue(In2);
            hdrCombineEffect.Parameters["Out2"].SetValue(Out2);

            hdrCombineEffect.Parameters["FogZOff"].SetValue(0.2f);
            hdrCombineEffect.Parameters["FogZMul"].SetValue(1.0f);
            hdrCombineEffect.Parameters["FogYOff"].SetValue(0.2f);
            hdrCombineEffect.Parameters["FogYMul"].SetValue(0.1f);
            hdrCombineEffect.Parameters["FogGlobMul"].SetValue(1.0f);
            hdrCombineEffect.Parameters["FogColor"].SetValue(new Vector4(1,1,1,1));
            //FogGlobMul
            //float FogZOff = 0.2, FogZMul = 1.0, FogYOff = 0.2, FogYMul = 0.1, FogGlobMul = 1.0;
            //float FogColor = float4(1, 1, 1, 1);

            DrawFullscreenQuad(BlurGeometryRender, hdrCombineEffect);
            //DrawFullscreenQuad(GeometryRender, null);
        }

        private DoublyIntegratedVector2 randomOffset;
        private Effect hdrCombineEffect;

        public Texture2D GeometryRender { get; set; }
        public Texture2D BlurGeometryRender { get; set; }
        public Texture2D RenderChannelColor { get; set; }
        public Texture2D DepthTexture { get; set; }

        private float[] BloomSensitivity = { 0.25f, 0.0f, 0.0f };

        //private float[] BloomIntensity = { 1.15f, 0.7f, 2.0f };
        //private float[] BaseIntensity = { 0.75f, 0.8f, 1.0f };

        private float[] BloomIntensity = { 2.8f, 0.7f, 0.0f };
        private float[] BaseIntensity = { 0.87f, 0.8f, 1.0f };

        private float[] BloomSaturation = { 2.5f, 0.8f, 1.0f };
        private float[] BaseSaturation = { 1.0f, 1.0f, 1.0f };

        private float[] In1 = { 1.0f, 1.4f, 1.0f };
        //private float[] Out1 = { 1.0f, 0.8f, 1.0f };
        private float[] Out1 = { 1.0f, 1.1f, 1.0f };
        private float[] In2 = { 2.0f, 2.7f, 2.0f };
        //private float[] Out2 = { 2.0f, 20.0f, 1.0f };
        private float[] Out2 = { 2.0f, 14.0f, 2.0f };
    }
}
