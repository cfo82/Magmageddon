using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer.Renderables
{
    class WinningScreenRenderable : Renderable
    {
        public WinningScreenRenderable(string name)
        {
            this.str = name.ToUpper() + " HAS WON!";
        }

        public override void LoadResources(Renderer renderer)
        {
            base.LoadResources(renderer);

            spriteBatch = new SpriteBatch(Game.Instance.GraphicsDevice);
            font = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/winning_screen");
            //viewportSize = new Vector2(Game.Instance.GraphicsDevice.Viewport.Width,
            //    Game.Instance.GraphicsDevice.Viewport.Height);
            this.pos = new Vector2(640, 360) - font.MeasureString(str)/2;
        }

        public override void Draw(Renderer renderer)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred,
               SaveStateMode.None, Matrix.Identity*((float)Game.Instance.GraphicsDevice.Viewport.Width)/1280f);

            DrawTools.DrawCenteredShadowString(spriteBatch, font, str, pos,Color.White, 1.0f);
            spriteBatch.End();
            //throw new NotImplementedException();
        }

        public override RenderMode RenderMode { get { return RenderMode.RenderToScene;  } }
        public override Vector3 Position { get { return Vector3.Zero; } }
        string str;
        Vector2 pos;
        SpriteFont font;
        SpriteBatch spriteBatch;
        //Vector2 viewportSize;
    }
}
