using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.MathHelpers;

namespace ProjectMagma.Renderer.Renderables
{
    class WinningScreenRenderable : Renderable
    {
        public WinningScreenRenderable(int renderPriority, string name)
        {
            this.renderPriority = renderPriority;
            this.str = name.ToUpper() + " HAS WON!";
            scale = new SineFloat(0.9f, 1.0f, 7.0f);
        }

        public override void LoadResources(Renderer renderer)
        {
            base.LoadResources(renderer);

            spriteBatch = new SpriteBatch(Game.Instance.GraphicsDevice);
            font = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/winning_screen");
            this.pos = new Vector2(640, 360);
            scale.Start(renderer.Time.PausableAt);
        }

        public override void Update(Renderer renderer)
        {
            base.Update(renderer);
            scale.Update(renderer.Time.PausableAt);
        }


        public override void Draw(Renderer renderer)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred,
               SaveStateMode.None, Matrix.Identity*((float)Game.Instance.GraphicsDevice.Viewport.Width)/1280f);

            DrawTools.DrawCenteredBorderedShadowString(spriteBatch, font, str, pos - Vector2.UnitY * 50, Color.White, 1.0f * scale.Value);
            DrawTools.DrawCenteredBorderedShadowString(spriteBatch, font, "CONGRATULATIONS!", pos + Vector2.UnitY * 50, Color.White, 0.65f * scale.Value);
            spriteBatch.End();
        }

        public override RenderMode RenderMode { get { return RenderMode.RenderOverlays;  } }
        public override Vector3 Position { get { return Vector3.Zero; } }
        public override int RenderPriority
        {
            get { return renderPriority; }
        }
        string str;
        Vector2 pos;
        SpriteFont font;
        SpriteBatch spriteBatch;
        SineFloat scale;
        private int renderPriority;
    }
}
