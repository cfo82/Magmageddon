using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Renderer.Interface;
using ProjectMagma.MathHelpers;

namespace ProjectMagma.Renderer.Renderables
{
    public class RespawnLightRenderable : Renderable
    {
        public RespawnLightRenderable(
            double timestamp,
            Vector3 position
        )
        {
            this.position = new Vector3InterpolationHistory(timestamp, position);
            this.fadeInOut = new EaseFloat(0, 0.025f);
            this.fadeInOut.TargetValue = 120;
        }

        public override void LoadResources(Renderer renderer)
        {
            texture = Game.Instance.ContentManager.Load<Texture2D>("Textures/Sfx/RespawnSpot");
            base.LoadResources(renderer);
        }

        public override void UnloadResources()
        {
            base.UnloadResources();
        }

        public override void Draw(Renderer renderer)
        {
            fadeInOut.Update(renderer.Time.PausableDtMs);

            float alpha = Game.Instance.Renderer.EntityManager["respawn_spot"].GetFloat("alpha");
            Vector3 color = Game.Instance.Renderer.EntityManager["respawn_spot"].GetVector3("color");

            Billboard billboard = Game.Instance.Renderer.Billboard;
            billboard.Texture = texture;
            billboard.Reposition(Position, fadeInOut.Value, 1000, new Vector4(color.X, color.Y, color.Z, alpha));
            billboard.Draw(Game.Instance.Renderer.Camera.View, Game.Instance.Renderer.Camera.Projection);
        }

        public override void UpdateBool(string id, double timestamp, bool value)
        {
            base.UpdateBool(id, timestamp, value);

            if (id == "Hide")
            {
                fadeInOut.TargetValue = 0;
            }
        }

        public override void UpdateVector3(string id, double timestamp, Vector3 value)
        {
            base.UpdateVector3(id, timestamp, value);

            if (id == "Position")
            {
                position.AddKeyframe(timestamp, value);
            }
        }

        public override RenderMode RenderMode
        {
            get { return RenderMode.RenderToSceneAlpha; }
        }

        public override Vector3 Position
        {
            get { return position.Evaluate(Game.Instance.Renderer.Time.PausableAt); }
        }

        private Vector3InterpolationHistory position;
        private Texture2D texture;
        private EaseFloat fadeInOut;
    }
}
