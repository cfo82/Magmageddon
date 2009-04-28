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
            int frozen
        )
        {
            // graphics resources
            spriteBatch = new SpriteBatch(Game.Instance.GraphicsDevice);
            font = Game.Instance.Content.Load<SpriteFont>("Sprites/HUD/HUDFont");
            background = Game.Instance.Content.Load<Texture2D>("Sprites/HUD/background");
            healthBar = Game.Instance.Content.Load<Texture2D>("Sprites/HUD/health");
            energyBar = Game.Instance.Content.Load<Texture2D>("Sprites/HUD/energy");
            fuelBar = Game.Instance.Content.Load<Texture2D>("Sprites/HUD/fuel");

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
            string frozenString = string.Format(" (frozen: {0:00.00})", frozenSeconds);
            string nameString = this.playerName + frozenString;

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

            //spriteBatch.DrawString(font, String.Format("{0:00.0} fps", (1000f / gameTime.ElapsedGameTime.TotalMilliseconds))
            //    + " " + String.Format("{0:00.0} sps", (1000f / Game.Instance.Simulation.Time.DtMs)),
            //  new Vector2(screenWidth / 2 - 20, 5), Color.Silver);

#if DEBUG
            numFrames++;
            totalMilliSeconds += gameTime.ElapsedGameTime.TotalMilliseconds;

            // only start after 2 sec "warmup"
            if (totalMilliSeconds > 2000)
            {
                float fps = (float)(1000f / gameTime.ElapsedGameTime.TotalMilliseconds);
                if (fps > maxFPS)
                    maxFPS = fps;
                if (fps < minFPS)
                    minFPS = fps;
                spriteBatch.DrawString(
                    font,
                    String.Format("{0:000.0} fps", fps) + " " +
                    String.Format("{0:00.0} avg", (1000.0f * numFrames / totalMilliSeconds)) + " " +
                    String.Format("{0:00.0} min", minFPS) + " " +
                    String.Format("{0:00.0} max", maxFPS),
                    new Vector2(screenWidth / 2 - 150, 5), Color.Silver
                );
            }
#endif

            spriteBatch.End();
        }

        private float minFPS = float.MaxValue;
        private float maxFPS = 0;

        static int numFrames = 0;
        static double totalMilliSeconds = 0;

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

        public int GamePadIndex
        {
            set { gamePadIndex = value; }
        }

        public int Health
        {
            set { health = value; }
        }

        public int MaxHealth
        {
            set { maxHealth = value; }
        }

        public int Energy
        {
            set { energy = value; }
        }

        public int MaxEnergy
        {
            set { maxEnergy = value; }
        }

        public int Fuel
        {
            set { fuel = value; }
        }

        public int MaxFuel
        {
            set { maxFuel = value; }
        }

        public int Frozen
        {
            set { frozen = value; }
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
    }
}
