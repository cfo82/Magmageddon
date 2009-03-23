using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Framework;
using Microsoft.Xna.Framework;

namespace ProjectMagma
{
    class HUD
    {
        private static HUD instance = new HUD();

        private HUD()
        {
        }

        internal static HUD Instance
        {
            get { return instance; }
        }

        internal void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.Instance.GraphicsDevice);

            font = Game.Instance.Content.Load<SpriteFont>("Sprites/HUD/HUDFont");

            background = Game.Instance.Content.Load<Texture2D>("Sprites/HUD/background");
            healthBar = Game.Instance.Content.Load<Texture2D>("Sprites/HUD/health");
            energyBar = Game.Instance.Content.Load<Texture2D>("Sprites/HUD/energy");
            fuelBar = Game.Instance.Content.Load<Texture2D>("Sprites/HUD/fuel");

            playerConstants = Game.Instance.EntityManager["player_constants"];
        }

        internal void Draw(GameTime gameTime)
        {
            int screenWidth = Game.Instance.Graphics.GraphicsDevice.Viewport.Width;
            float screenscale = (float)screenWidth / 1280f;
            Matrix spriteScale = Matrix.CreateScale(screenscale, screenscale, 1);
            
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred,
                SaveStateMode.None, spriteScale);

            foreach (Entity player in Game.Instance.PlayerManager)
            {
                int no = player.GetInt("number");

                if (no == 1)
                {
                    SpriteEffects effects = SpriteEffects.None;
                    spriteBatch.Draw(background, new Vector2(0, 0), null, Color.White, 0f, Vector2.Zero, 1, effects, 1);

                    spriteBatch.DrawString(font, player.Name, new Vector2(14, 5), Color.Black);

                    spriteBatch.Draw(healthBar, new Vector2(14, 55), new Rectangle(0, 0, healthBar.Width * player.GetInt("health")
                        / playerConstants.GetInt("max_health"), healthBar.Height), Color.White, 0f, Vector2.Zero, 1, effects, 0);
                    spriteBatch.Draw(energyBar, new Vector2(14, 86), new Rectangle(0, 0, energyBar.Width * player.GetInt("energy")
                        / playerConstants.GetInt("max_energy"), energyBar.Height), Color.White, 0f, Vector2.Zero, 1, effects, 0);
                    spriteBatch.Draw(fuelBar, new Vector2(14, 117), new Rectangle(0, 0, fuelBar.Width * player.GetInt("fuel")
                        / playerConstants.GetInt("max_fuel"), fuelBar.Height), Color.White, 0f, Vector2.Zero, 1, effects, 0);
                }
                else
                {
                    SpriteEffects effects = SpriteEffects.FlipHorizontally;
                    spriteBatch.Draw(background, new Vector2(screenWidth / 2, 0), null, Color.White, 0f, Vector2.Zero, 1, effects, 1);

                    spriteBatch.DrawString(font, player.Name, new Vector2(screenWidth - 14 - font.MeasureString(player.Name).X, 5), Color.Black);

                    spriteBatch.Draw(healthBar, new Vector2(screenWidth - 14, 55), new Rectangle(0, 0,
                        healthBar.Width * player.GetInt("health") / playerConstants.GetInt("max_health"), healthBar.Height), 
                        Color.White, 0f, new Vector2(healthBar.Width, 0), 1, effects, 0);
                    spriteBatch.Draw(energyBar, new Vector2(screenWidth - 14, 86), new Rectangle(0, 0, energyBar.Width * player.GetInt("energy")
                        / playerConstants.GetInt("max_energy"), energyBar.Height), Color.White, 0f, new Vector2(energyBar.Width, 0), 1, effects, 0);
                    spriteBatch.Draw(fuelBar, new Vector2(screenWidth - 14, 117), new Rectangle(0, 0, fuelBar.Width * player.GetInt("fuel")
                        / playerConstants.GetInt("max_fuel"), fuelBar.Height), Color.White, 0f, new Vector2(fuelBar.Width, 0), 1, effects, 0);
                }


            }
            spriteBatch.DrawString(font, String.Format("{0:00.0} fps", (1000f / gameTime.ElapsedGameTime.Milliseconds)), 
                new Vector2(screenWidth / 2-20, 5), Color.Black);
            spriteBatch.End();
        }

        private SpriteFont font;
        private SpriteBatch spriteBatch;
        
        private Entity playerConstants;

        private Texture2D background;
        private Texture2D healthBar;
        private Texture2D energyBar;
        private Texture2D fuelBar;

    }
}
