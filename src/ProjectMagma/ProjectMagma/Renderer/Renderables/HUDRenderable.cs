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
        }

        public override void LoadResources()
        {
            base.LoadResources();

            spriteBatch = new SpriteBatch(Game.Instance.GraphicsDevice);
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

        public override void Draw(
            Renderer renderer,
            GameTime gameTime
        )
        {
            int screenWidth = Game.Instance.GraphicsDevice.Viewport.Width;
            float screenscale = (float)screenWidth / 1280f;
            Matrix spriteScale = Matrix.CreateScale(screenscale, screenscale, 1);

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
                bgX = screenWidth / 2;
                textX = screenWidth - 14 - font.MeasureString(nameString).X;
                healthX = screenWidth - 14 - healthBar.Width + (healthBar.Width - healthBarWidth);
                energyX = screenWidth - 14 - energyBar.Width + (energyBar.Width - energyBarWidth); ;
                fuelX = screenWidth - 14 - fuelBar.Width + (fuelBar.Width - fuelBarWidth); ;
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
    }
}
