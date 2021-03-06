﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Bugslayer;

namespace ProjectMagma.Renderer
{
    public class GlowPass : RenderPass
    {
        public GlowPass(
            Renderer renderer
            )
        : base(renderer)
        {
            gaussianBlurEffect = Game.Instance.ContentManager.Load<Effect>("Effects/BlurModified");
        }

        public void Render(
            Texture2D hdrColorBuffer, Texture2D renderChannelBuffer,
            RenderTarget2D targetIntermediateBlurredHDRColorBuffer, RenderTarget2D targetIntermediateBlurredRenderChannelBuffer,
            RenderTarget2D targetBlurredHDRColorBuffer, RenderTarget2D targetBlurredRenderChannelBuffer
            )
        {
            // pass 1 horizontal blur
            PIXHelper.BeginEvent("Horizontal Blur");
            SetBlurEffectParameters(
                1.0f / hdrColorBuffer.Width,
                0,
                hdrColorBuffer,
                renderChannelBuffer
                );
            DrawFullscreenQuad(
                hdrColorBuffer,
                targetIntermediateBlurredHDRColorBuffer, targetIntermediateBlurredRenderChannelBuffer,
                gaussianBlurEffect
                );
            PIXHelper.EndEvent();

            // result
            Texture2D intermediateBlurredHDRColorBuffer = targetIntermediateBlurredHDRColorBuffer;
            Texture2D intermediateBlurredRenderChannelBuffer = targetIntermediateBlurredRenderChannelBuffer;

            // pass 2 vertical blurr
            PIXHelper.BeginEvent("Vertical Blur");
            SetBlurEffectParameters(
                0,
                1.0f / hdrColorBuffer.Height,
                intermediateBlurredHDRColorBuffer,
                intermediateBlurredRenderChannelBuffer
                );
            DrawFullscreenQuad(
                intermediateBlurredHDRColorBuffer,
                targetBlurredHDRColorBuffer, targetBlurredRenderChannelBuffer,
                gaussianBlurEffect
                );
            PIXHelper.EndEvent();
        }

        private void SetBlurEffectParameters(
            float dx,
            float dy,
            Texture2D hdrColorBuffer,
            Texture2D renderChannelBuffer
            )
        {
            gaussianBlurEffect.Parameters["GeometryRender"].SetValue(hdrColorBuffer);
            gaussianBlurEffect.Parameters["RenderChannelColor"].SetValue(renderChannelBuffer);

            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter = gaussianBlurEffect.Parameters["SampleWeights"];
            EffectParameter offsetsParameter = gaussianBlurEffect.Parameters["SampleOffsets"];

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


        private Effect gaussianBlurEffect;
    }
}
