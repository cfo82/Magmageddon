using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Integration;

namespace ProjectMagma.Renderer
{
    public class LightManager
    {
        public LightManager()
        {
            // initialize blueish sky light
            SkyLight = new ParallelLight(
                new Vector3(112, 213, 255) / 255.0f * 1f,
                Vector3.One * 1.0f,
                new Vector3(0, -1, 0)
            );

            // initialize lava light
            lavaBaseColor = new Vector3(1.0f, 0.5f, 0.1f);
            lavaBrightness = new DoublyIntegratedFloat(1.0f, 0.0f, 0.5f, 1.5f, -1.0f, 1.0f);
            LavaLight = new ParallelLight
            (
                lavaBaseColor,
                Vector3.One * 1.0f,
                new Vector3(0, 1, -1)
            );

            // initialize moving spot light
            spotLightPhase = new DoublyIntegratedFloat(0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 1.0f);
            SpotLight = new ParallelLight
            (
                new Vector3(1.0f, 1.0f, 1.0f) * 1.2f,
                Vector3.One * 1.0f,
                SpotLightDirection()
            );
        }

        public void Update(GameTime gameTime)
        {
            // update lava light
            lavaBrightness.RandomlyIntegrate(gameTime, 30.0f, 0.0f);
            LavaLight.DiffuseColor = lavaBaseColor * lavaBrightness.Value;

            // update moving spot light
            spotLightPhase.RandomlyIntegrate(gameTime, 10.0f, 0.2f);
            SpotLight.Direction = SpotLightDirection();
        }

        private Vector3 SpotLightDirection()
        {
            Vector3 dir = new Vector3((float)Math.Cos(spotLightPhase.Value), -0.2f, (float)Math.Sin(spotLightPhase.Value));
            return Vector3.Normalize(dir);
        }

        public ParallelLight SkyLight { get; set; }
        public ParallelLight LavaLight { get; set; }
        public ParallelLight SpotLight { get; set; }

        private Vector3 lavaBaseColor;

        private DoublyIntegratedFloat spotLightPhase;
        private DoublyIntegratedFloat lavaBrightness;
    }
}
