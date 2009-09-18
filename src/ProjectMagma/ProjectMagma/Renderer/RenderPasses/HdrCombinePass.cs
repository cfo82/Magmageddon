using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Integration;

namespace ProjectMagma.Renderer
{
    public class HdrCombinePass : RenderPass
    {
        public HdrCombinePass(Renderer renderer)
            : base(renderer)
        {
            hdrCombineEffect = Game.Instance.ContentManager.Load<Effect>("Effects/HdrCombine");
            randomOffset = new DoublyIntegratedVector2
            (
               Vector2.Zero, new Vector2(0.002f, 0.002f), 0.0f, 0.0f, -0.4f, 0.4f
            );
        }

        public void Render()
        {
            randomOffset.RandomlyIntegrate(Renderer.Time.DtMs, 0.2f, 0.0f);

            hdrCombineEffect.Parameters["GeometryRender"].SetValue(GeometryRender);
            hdrCombineEffect.Parameters["BlurGeometryRender"].SetValue(BlurGeometryRender);
            hdrCombineEffect.Parameters["RenderChannelColor"].SetValue(RenderChannelColor);
            hdrCombineEffect.Parameters["ToolTexture"].SetValue(ToolTexture);
            hdrCombineEffect.Parameters["CloudTexture"].SetValue(Renderer.VectorCloudTexture);
            hdrCombineEffect.Parameters["DepthTexture"].SetValue(DepthTexture);

            hdrCombineEffect.Parameters["RandomOffset"].SetValue(randomOffset.Value);

            SetArray3FromEntity("BloomSensitivity", "tonemapping", "bloom_sensitivity");
            SetArray3FromEntity("BloomIntensity", "tonemapping", "bloom_intensity");
            SetArray3FromEntity("BaseIntensity", "tonemapping", "base_intensity");
            SetArray3FromEntity("BloomSaturation", "tonemapping", "bloom_saturation");
            SetArray3FromEntity("BaseSaturation", "tonemapping", "base_saturation");
            SetArray3FromEntity("In1", "tonemapping", "in1");
            SetArray3FromEntity("Out1", "tonemapping", "out1");
            SetArray3FromEntity("In2", "tonemapping", "in2");
            SetArray3FromEntity("Out2", "tonemapping", "out2");

            // precomp In1_Precomp and In2_Precomp
            Vector3 vIn1 = Renderer.EntityManager["tonemapping"].GetVector3("in1");
            Vector3 vOut1 = Renderer.EntityManager["tonemapping"].GetVector3("out1");
            Vector3 vIn2 = Renderer.EntityManager["tonemapping"].GetVector3("in2");
            Vector3 vOut2 = Renderer.EntityManager["tonemapping"].GetVector3("out2");
            Vector3 vIn1Precomp = vOut1 / (vIn1 * (vIn1 - vIn2));
            Vector3 vIn2Precomp = vOut2 / (vIn2 * (vIn2 - vIn1));

            hdrCombineEffect.Parameters["In1_Precomp"].SetValue(new float[] { vIn1Precomp.X, vIn1Precomp.Y, vIn1Precomp.Z });
            hdrCombineEffect.Parameters["In2_Precomp"].SetValue(new float[] { vIn2Precomp.X, vIn2Precomp.Y, vIn2Precomp.Z });
            
            SetFloatFromEntity("FogZOff", "fog", "fog_z_off");
            SetFloatFromEntity("FogZMul", "fog", "fog_z_mul");
            SetFloatFromEntity("FogYOff", "fog", "fog_y_off");
            SetFloatFromEntity("FogYMul", "fog", "fog_y_mul");
            SetVector3FromEntity("FogColor", "fog", "fog_color");

            hdrCombineEffect.Parameters["FogGlobMul"].SetValue(
                Renderer.EntityManager["fog"].GetFloat("fog_glob_mul") *
                Renderer.EntityManager["camera"].GetFloat("fog_multiplier")
            );

            SetFloatFromEntity("BlueTopOverlayStrength", "topoverlay", "strength");

            DrawFullscreenQuad(BlurGeometryRender, hdrCombineEffect);
        }

        private void SetFloatFromEntity(string paramName, string entityName, string attributeName)
        {
            hdrCombineEffect.Parameters[paramName].SetValue(
                Renderer.EntityManager[entityName].GetFloat(attributeName)
            );
        }

        private void SetVector3FromEntity(string paramName, string entityName, string attributeName)
        {
            hdrCombineEffect.Parameters[paramName].SetValue(
                Renderer.EntityManager[entityName].GetVector3(attributeName)
            );
        }

        private void SetArray3FromEntity(string paramName, string entityName, string attributeName)
        {
            Vector3 vector = Renderer.EntityManager[entityName].GetVector3(attributeName);
            float[] array = new float[3];
            array[0] = vector.X;
            array[1] = vector.Y;
            array[2] = vector.Z;
            hdrCombineEffect.Parameters[paramName].SetValue(
                array
            );
        }


        private DoublyIntegratedVector2 randomOffset;
        private Effect hdrCombineEffect;

        public Texture2D GeometryRender { get; set; }
        public Texture2D BlurGeometryRender { get; set; }
        public Texture2D RenderChannelColor { get; set; }
        public Texture2D ToolTexture { get; set; }
        public Texture2D DepthTexture { get; set; }
    }
}
