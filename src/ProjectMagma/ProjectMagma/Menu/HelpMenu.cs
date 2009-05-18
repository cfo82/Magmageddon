using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Simulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ProjectMagma
{
    class HelpMenu: MenuScreen
    {
        private readonly Texture2D helpScreen;

        public HelpMenu(Menu menu)
            : base(menu, new Vector2(190, 75))
        {
            helpScreen = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Menu/help_screen");

            DrawPrevious = false;
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(helpScreen, this.Position, Color.White);
        }

        public override void OnOpen()
        {
        }
    }
}