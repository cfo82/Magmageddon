using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class HUDRenderable : Renderable
    {
        public HUDRenderable(
            string playerName,
            int gamePadIndex,
            int health,
            int maxHealth,
            int energy,
            int maxEnergy,
            int fuel,
            int maxFuel,
            int frozen,
            int jumps
        )
        {
            // initialize variables
            this.playerName = playerName;
            this.gamePadIndex = gamePadIndex;
            this.health = health;
            this.maxHealth = maxHealth;
            this.energy = energy;
            this.maxEnergy = maxEnergy;
            this.fuel = fuel;
            this.maxFuel = maxFuel;
            this.frozen = frozen;
            this.jumps = jumps;

            barEffect = Game.Instance.ContentManager.Load<Effect>("Effects/Hud/Bars");
            barBackgroundTexture = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Hud/bar_background");
            barHighlightTexture = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Hud/bar_highlight");
        }

        private SpriteFont playerNameFont;
        private SpriteFont powerupFont;

        public override void LoadResources()
        {
            base.LoadResources();

            spriteBatch = new SpriteBatch(Game.Instance.GraphicsDevice);
            playerNameFont = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/hud_playername");
            powerupFont = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/hud_powerup");
            font = Game.Instance.ContentManager.Load<SpriteFont>("Sprites/HUD/HUDFont");
            background = Game.Instance.ContentManager.Load<Texture2D>("Sprites/HUD/background");
            healthBar = Game.Instance.ContentManager.Load<Texture2D>("Sprites/HUD/health");
            energyBar = Game.Instance.ContentManager.Load<Texture2D>("Sprites/HUD/energy");
            fuelBar = Game.Instance.ContentManager.Load<Texture2D>("Sprites/HUD/fuel");
        }

        public override void UnloadResources()
        {
            spriteBatch.Dispose();

            base.UnloadResources();
        }

        public void DrawOld(
            Renderer renderer,
            GameTime gameTime
        )
        {
            Viewport viewPort = Game.Instance.GraphicsDevice.Viewport;

            float screenScale = (float)viewPort.Width / 1280f;
            Matrix spriteScale = Matrix.CreateScale(screenScale, screenScale, 1);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred,
                SaveStateMode.None, spriteScale);

            float bgX, textX, healthX, energyX, fuelX;

            int healthBarWidth = healthBar.Width * health / maxHealth;
            int energyBarWidth = energyBar.Width * energy / maxEnergy;
            int fuelBarWidth = fuelBar.Width * fuel / maxFuel;

            double frozenSeconds = (double)this.frozen / 1000.0;
            string frozenString = string.Format("frozen: {0:00.00}", frozenSeconds);
            string jumpsString = string.Format("jumps: {0:00}", jumps);
            string nameString = this.playerName + " (" + frozenString + ", " +jumpsString+ ")";

            SpriteEffects effects;
            if (gamePadIndex == 0)
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
                bgX = viewPort.Width / 2;
                textX = viewPort.Width - 14 - font.MeasureString(nameString).X;
                healthX = viewPort.Width - 14 - healthBar.Width + (healthBar.Width - healthBarWidth);
                energyX = viewPort.Width - 14 - energyBar.Width + (energyBar.Width - energyBarWidth); ;
                fuelX = viewPort.Width - 14 - fuelBar.Width + (fuelBar.Width - fuelBarWidth); ;
            }

            spriteBatch.Draw(background, new Vector2(bgX, 0), null, Color.White, 0f, Vector2.Zero, 1, effects, 1);
            spriteBatch.DrawString(font, nameString, new Vector2(textX, 5), Color.Black);

            spriteBatch.Draw(healthBar, new Vector2(healthX, 55), new Rectangle(0, 0, healthBarWidth, healthBar.Height),
                Color.White, 0f, Vector2.Zero, 1, effects, 0);
            spriteBatch.Draw(energyBar, new Vector2(energyX, 86), new Rectangle(0, 0, energyBarWidth, energyBar.Height),
                Color.White, 0f, Vector2.Zero, 1, effects, 0);
            spriteBatch.Draw(fuelBar, new Vector2(fuelX, 117), new Rectangle(0, 0, fuelBarWidth, fuelBar.Height),
                Color.White, 0f, Vector2.Zero, 1, effects, 0);

            spriteBatch.End();
        }


        public override void Draw(
            Renderer renderer,
            GameTime gameTime
        )
        {
            Viewport viewport = Game.Instance.GraphicsDevice.Viewport;
            float screenScale = (float)viewport.Width / 1280f;
            Matrix spriteScale = Matrix.CreateScale(screenScale, screenScale, 1);

            // begin drawing of gui elements
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate,
                SaveStateMode.None, spriteScale);

            barEffect.Parameters["BackgroundTexture"].SetValue(barBackgroundTexture);
            barEffect.Parameters["HighlightTexture"].SetValue(barHighlightTexture);

            int horOff = 100, vertOff = 70;
            //int hudWidth = 271, hudHeight = 50;

            Vector2 BarAreaSize = new Vector2(271, 50);

            barEffect.Parameters["TotalSize"].SetValue(BarAreaSize);
            barEffect.Parameters["HighlightTexture"].SetValue(barHighlightTexture);

            health += maxHealth/30;
            if (health > maxHealth) health = 0;
            barEffect.Parameters["HealthValue"].SetValue(((float) health)/maxHealth);

            int xStart, yStart;
            if(gamePadIndex==0 || gamePadIndex==2)
            {
                xStart = horOff;
            } else {
                xStart = viewport.Width - (int) BarAreaSize.X - horOff;
            }
            if(gamePadIndex==0 || gamePadIndex==1)
            {
                yStart = vertOff;
            } else {
                yStart = viewport.Height - (int) BarAreaSize.Y - vertOff;
            }

            Rectangle rect = new Rectangle(xStart, yStart+40, (int) BarAreaSize.X, (int) BarAreaSize.Y);

            //switch(gamePadIndex)
            //{
            //    case 0:
            //        break;
            //    case 1:
            //        break;
            //    case 2:
            //        break;
            //    case 3:
            //        break;
            //}


            float bgX, textX, healthX, energyX, fuelX;

            int healthBarWidth = healthBar.Width * health / maxHealth;
            int energyBarWidth = energyBar.Width * energy / maxEnergy;
            int fuelBarWidth = fuelBar.Width * fuel / maxFuel;

            double frozenSeconds = (double)this.frozen / 1000.0;
            string frozenString = string.Format("frozen: {0:00.00}", frozenSeconds);
            string jumpsString = string.Format("jumps: {0:00}", jumps);
            string nameString = this.playerName + " (" + frozenString + ", " + jumpsString + ")";

            //SpriteEffects effects;
            //if (gamePadIndex == 0)
            //{
            //    effects = SpriteEffects.None;
            //    bgX = 0;
            //    textX = 14;
            //    healthX = 14;
            //    energyX = 14;
            //    fuelX = 14;
            //}
            //else
            //{
            //    effects = SpriteEffects.FlipHorizontally;
            //    bgX = viewport.Width / 2;
            //    textX = viewport.Width - 14 - font.MeasureString(nameString).X;
            //    healthX = viewport.Width - 14 - healthBar.Width + (healthBar.Width - healthBarWidth);
            //    energyX = viewport.Width - 14 - energyBar.Width + (energyBar.Width - energyBarWidth); ;
            //    fuelX = viewport.Width - 14 - fuelBar.Width + (fuelBar.Width - fuelBarWidth); ;
            //}

            //spriteBatch.Draw(background, new Vector2(bgX, 0), null, Color.White, 0f, Vector2.Zero, 1, effects, 1);
            Vector2 playerNamePos = new Vector2(xStart, yStart);
            Vector2 playerNameShadowPos = playerNamePos + new Vector2(2,2);
            //spriteBatch.DrawString(playerNameFont, this.playerName.ToUpper(), playerNamePos-(new Vector2(1,1)), Color.DimGray);
            spriteBatch.DrawString(playerNameFont, this.playerName.ToUpper(), playerNameShadowPos, Color.DimGray);
            spriteBatch.DrawString(playerNameFont, this.playerName.ToUpper(), playerNamePos, Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate,
                SaveStateMode.None, spriteScale);
            barEffect.Begin();
            barEffect.CurrentTechnique.Passes[0].Begin();
            spriteBatch.Draw(barBackgroundTexture, rect, Color.White);
            barEffect.CurrentTechnique.Passes[0].End();
            barEffect.End();
            spriteBatch.End();

            //spriteBatch.Draw(healthBar, new Vector2(healthX, 55), new Rectangle(0, 0, healthBarWidth, healthBar.Height),
            //    Color.White, 0f, Vector2.Zero, 1, effects, 0);
            //spriteBatch.Draw(energyBar, new Vector2(energyX, 86), new Rectangle(0, 0, energyBarWidth, energyBar.Height),
            //    Color.White, 0f, Vector2.Zero, 1, effects, 0);
            //spriteBatch.Draw(fuelBar, new Vector2(fuelX, 117), new Rectangle(0, 0, fuelBarWidth, fuelBar.Height),
            //    Color.White, 0f, Vector2.Zero, 1, effects, 0);



        }

        public override void UpdateInt(string id, int value)
        {
            base.UpdateInt(id, value);

            if (id == "GamePadIndex")
            {
                gamePadIndex = value;
            }
            else if (id == "Health")
            {
                health = value;
            }
            else if (id == "MaxHealth")
            {
                maxHealth = value;
            }
            else if (id == "Energy")
            {
                energy = value;
            }
            else if (id == "MaxEnergy")
            {
                maxEnergy = value;
            }
            else if (id == "Fuel")
            {
                fuel = value;
            }
            else if (id == "MaxFuel")
            {
                maxFuel = value;
            }
            else if (id == "Frozen")
            {
                frozen = value;
            }
            else if (id == "Jumps")
            {
                jumps = value;
            }
        }

        public override void UpdateString(string id, string value)
        {
            base.UpdateString(id, value);

            if (id == "PlayerName")
            {
                playerName = value;
            }
        }

        public override RenderMode RenderMode
        {
            get { return ProjectMagma.Renderer.RenderMode.RenderOverlays; }
        }

        public override Vector3 Position
        {
            get { return Vector3.Zero; }
            set { }
        }

        public string PlayerName
        {
            set { playerName = value; }
        }

        private SpriteFont font;
        private SpriteBatch spriteBatch;

        private Texture2D background;
        private Texture2D healthBar;
        private Texture2D energyBar;
        private Texture2D fuelBar;

        private string playerName;
        private int gamePadIndex;
        private int health;
        private int maxHealth;
        private int energy;
        private int maxEnergy;
        private int fuel;
        private int maxFuel;
        private int frozen;
        private int jumps;

        private Effect barEffect;

        private Texture2D barBackgroundTexture;
        private Texture2D barHighlightTexture;


    }
}
