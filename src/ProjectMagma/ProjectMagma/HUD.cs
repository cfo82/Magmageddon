using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Simulation;
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

            playerConstants = Game.Instance.Simulation.EntityManager["player_constants"];
        }

        internal void Draw(GameTime gameTime)
        {
            int screenWidth = Game.Instance.GraphicsDevice.Viewport.Width;
            float screenscale = (float)screenWidth / 1280f;
            Matrix spriteScale = Matrix.CreateScale(screenscale, screenscale, 1);
            
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred,
                SaveStateMode.None, spriteScale);

            foreach (Entity player in Game.Instance.Simulation.PlayerManager)
            {
                int no = player.GetInt("number");

                float bgX, textX, healthX, energyX, fuelX;
                
                int healthBarWidth = healthBar.Width * player.GetInt("health") / playerConstants.GetInt("max_health");
                int energyBarWidth = energyBar.Width * player.GetInt("energy") / playerConstants.GetInt("max_energy");
                int fuelBarWidth = fuelBar.Width * player.GetInt("fuel") / playerConstants.GetInt("max_fuel");

                SpriteEffects effects;
                if (no == 1)
                {
                    effects = SpriteEffects.None;
                    bgX = 0;
                    textX = 14;
                    healthX = 14;
                    energyX = 14;
                    fuelX = 14;
                }
                else
                {
                    effects = SpriteEffects.FlipHorizontally;
                    bgX = screenWidth / 2;
                    textX = screenWidth - 14 - font.MeasureString(player.Name).X;
                    healthX = screenWidth - 14 - healthBar.Width + (healthBar.Width - healthBarWidth);
                    energyX = screenWidth - 14 - energyBar.Width + (energyBar.Width - energyBarWidth); ;
                    fuelX = screenWidth - 14 - fuelBar.Width + (fuelBar.Width - fuelBarWidth); ;
                }

                spriteBatch.Draw(background, new Vector2(bgX, 0), null, Color.White, 0f, Vector2.Zero, 1, effects, 1);
                spriteBatch.DrawString(font, player.GetString("player_name"), new Vector2(textX, 5), Color.Black);

                spriteBatch.Draw(healthBar, new Vector2(healthX, 55), new Rectangle(0, 0, healthBarWidth, healthBar.Height), 
                    Color.White, 0f, Vector2.Zero, 1, effects, 0);
                spriteBatch.Draw(energyBar, new Vector2(energyX, 86), new Rectangle(0, 0, energyBarWidth, energyBar.Height), 
                    Color.White, 0f, Vector2.Zero, 1, effects, 0);
                spriteBatch.Draw(fuelBar, new Vector2(fuelX, 117), new Rectangle(0, 0, fuelBarWidth, fuelBar.Height), 
                    Color.White, 0f, Vector2.Zero, 1, effects, 0);
            }
            spriteBatch.DrawString(font, String.Format("{0:00.0} fps", (1000f / (Game.Instance.Simulation.CurrentGameTime.TotalGameTime.TotalMilliseconds -
                Game.Instance.Simulation.LastUpdateAt))), 
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
