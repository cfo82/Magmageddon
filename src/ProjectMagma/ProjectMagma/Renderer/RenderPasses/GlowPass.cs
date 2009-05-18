using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class GlowPass : RenderPass
    {
        public GlowPass(Renderer renderer, RenderTarget2D target0, RenderTarget2D target1)
            : base(renderer, target0, target1)
        {
            originalBlurEffect = Game.Instance.ContentManager.Load<Effect>("Effects/GaussianBlur");
            gaussianBlurEffect = Game.Instance.ContentManager.Load<Effect>("Effects/BlurModified");
        }

        public override void Render()
        {
            //SetBlurEffectParameters(1.0f / Renderer.Device.Viewport.Width, 0);
            //DrawFullscreenQuad(Renderer.RenderChannels, Renderer.Device.Viewport.Width, Renderer.Device.Viewport.Height / 2, gaussianBlurEffect);
            //SetBlurEffectParameters(0, 1.0f / Renderer.Device.Viewport.Height);
            //DrawFullscreenQuad(Renderer.ResolveTarget, Renderer.Device.Viewport.Width / 2, Renderer.Device.Viewport.Height, gaussianBlurEffect);

            //SetBlurEffectParameters(1.0f / Renderer.Device.Viewport.Width, 0, originalBlurEffect, null);
            //DrawFullscreenQuad(Renderer.RenderChannels, Renderer.Target1, originalBlurEffect);
            //SetBlurEffectParameters(0, 1.0f / Renderer.Device.Viewport.Height, originalBlurEffect, null);
            //DrawFullscreenQuad(Renderer.Target1.GetTexture(), Renderer.Target1, originalBlurEffect);
            ////DrawFullscreenQuad(Renderer.Target1.GetTexture(), originalBlurEffect);

            //gaussianBlurEffect.Parameters["geom"].SetValue(Renderer.ResolveTarget);
            //SetBlurEffectParameters(1.0f / Renderer.Device.Viewport.Width, 0, gaussianBlurEffect, Renderer.Target1.GetTexture());
            //DrawFullscreenQuad(Renderer.Target1.GetTexture(), Renderer.Target1, gaussianBlurEffect);
            //SetBlurEffectParameters(0, 1.0f / Renderer.Device.Viewport.Height, gaussianBlurEffect, Renderer.Target1.GetTexture());
            //DrawFullscreenQuad(Renderer.Target1.GetTexture(), gaussianBlurEffect);
            ////DrawFullscreenQuad(Renderer.ResolveTarget, gaussianBlurEffect);


            gaussianBlurEffect.Parameters["GeometryRender"].SetValue(GeometryRender);
            SetBlurEffectParameters(1.0f / Renderer.Device.Viewport.Width, 0, gaussianBlurEffect, Renderer.RenderChannels);
            DrawFullscreenQuad(Renderer.GeometryRender, Target0, Target1, gaussianBlurEffect);

            gaussianBlurEffect.Parameters["GeometryRender"].SetValue(Target0.GetTexture());
            SetBlurEffectParameters(0, 1.0f / Renderer.Device.Viewport.Height, gaussianBlurEffect, Target1.GetTexture());
            DrawFullscreenQuad(Renderer.GeometryRender, Target0, Target1, gaussianBlurEffect);

            BlurGeometryRender = Target0.GetTexture();
            BlurRenderChannelColor = Target1.GetTexture();

            //DrawFullscreenQuad(BlurRenderChannelColor, null);

            //gaussianBlurEffect.Parameters["geom"].SetValue(Renderer.ResolveTarget);
            //SetBlurEffectParameters(1.0f / Renderer.Device.Viewport.Width, 0, gaussianBlurEffect, Renderer.RenderChannels);
            //DrawFullscreenQuad(Renderer.GeometryRender, gaussianBlurEffect);

//            gaussianBlurEffect.Parameters["geom"].SetValue(Renderer.GeometryRender);


            //// Pass 2: draw from rendertarget 1 into rendertarget 2,
            //// using a shader to apply a horizontal gaussian blur filter.

            //DrawFullscreenQuad(renderTarget1.GetTexture(), renderTarget2,
            //                   gaussianBlurEffect,
            //                   IntermediateBuffer.BlurredHorizontally);

            //// Pass 3: draw from rendertarget 2 back into rendertarget 1,
            //// using a shader to apply a vertical gaussian blur filter.

            //DrawFullscreenQuad(renderTarget2.GetTexture(), renderTarget1,
            //                   gaussianBlurEffect,
            //                   IntermediateBuffer.BlurredBothWays);


        }



        void SetBlurEffectParameters(float dx, float dy, Effect effect, Texture2D pRenderChannelColor)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter, renderChannelColor;

            weightsParameter = effect.Parameters["SampleWeights"];
            offsetsParameter = effect.Parameters["SampleOffsets"];
            renderChannelColor = effect.Parameters["RenderChannelColor"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
            if(effect==gaussianBlurEffect) // HACK
                renderChannelColor.SetValue(pRenderChannelColor);            
        }


        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        float ComputeGaussian(float n)
        {
            float theta = 3.0f;//Settings.BlurAmount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }


        private Effect originalBlurEffect, gaussianBlurEffect;

        public Texture2D GeometryRender { get; set; }
        public Texture2D BlurGeometryRender { get; set; }
        public Texture2D BlurRenderChannelColor { get; set; }
    }
}
