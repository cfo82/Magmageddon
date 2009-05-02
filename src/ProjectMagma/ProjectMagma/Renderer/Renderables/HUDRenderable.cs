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
            barComponentTexture = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Hud/bar_highlight");

            ComputePositions();
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

        private Vector2 BarAreaSize;
        private Vector2 multiplier, invMultiplier;
        private Matrix spriteScale, playerMirror;
        private int xStart, yStart;

        private void ComputePositions()
        {
            Viewport viewport = Game.Instance.GraphicsDevice.Viewport;
            float screenScale = (float)viewport.Width / 1280f;
            spriteScale = Matrix.CreateScale(screenScale, screenScale, 1);
            BarAreaSize = new Vector2(271, 52);
            Vector2 TotalSize = BarAreaSize + new Vector2(0, 50);
            int horOff = 100, vertOff = 50;

            playerMirror = Matrix.Identity;
            multiplier = new Vector2(0, 0);
            if (gamePadIndex == 0 || gamePadIndex == 2)
            {
                xStart = horOff;
            }
            else
            {
                xStart = viewport.Width - (int)TotalSize.X - horOff;
                playerMirror *= new Matrix(
                    -1, 0, 1, 0,
                     0, 1, 0, 0,
                     0, 0, 1, 0,
                     0, 0, 0, 1
                );
                multiplier.X = 1;
            }
            if (gamePadIndex == 0 || gamePadIndex == 1)
            {
                yStart = vertOff;
            }
            else
            {
                yStart = viewport.Height - (int)TotalSize.Y - vertOff;
                playerMirror *= new Matrix(
                    1, 0, 0, 0,
                    0,-1, 1, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1
                );
                multiplier.Y = 1;
            }

            invMultiplier = new Vector2(1, 1) - multiplier;
        }


        private void ApplyBarEffectParameters()
        {
            barEffect.Parameters["BackgroundTexture"].SetValue(barBackgroundTexture);
            barEffect.Parameters["ComponentTexture"].SetValue(barComponentTexture);
            barEffect.Parameters["Size"].SetValue(BarAreaSize);
            health = 80;
            barEffect.Parameters["HealthValue"].SetValue(((float)health) / maxHealth);
            barEffect.Parameters["EnergyValue"].SetValue(((float)energy) / maxEnergy);
            Console.WriteLine("health: " + health + " " + maxHealth + " " + ((float)health) / maxHealth);
            barEffect.Parameters["HealthColor1"].SetValue(new Vector3(0.91f, 0.08f, 0.64f));
            barEffect.Parameters["HealthColor2"].SetValue(new Vector3(0.77f, 0.08f, 0.86f));
            barEffect.Parameters["PlayerMirror"].SetValue(playerMirror);
        }


        public override void Draw(
            Renderer renderer,
            GameTime gameTime
        )
        {

            // TESTHACK
            //health += maxHealth/30;
            //if (health > maxHealth) health = 0;
            // /TESTHACK

            // set effect parameters


            //double frozenSeconds = (double)this.frozen / 1000.0;
            //string frozenString = string.Format("frozen: {0:00.00}", frozenSeconds);
            //string jumpsString = string.Format("jumps: {0:00}", jumps);
            //string nameString = this.playerName + " (" + frozenString + ", " + jumpsString + ")";
            //Vector4 a = Vector4.Transform(new Vector4(1, 1, 1, 0), playerMirror);
            //Vector2 t = (new Vector2(1, 1)) - a;

            #region draw strings and bars

            // begin of first sprite batch - no effects, just strings
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, spriteScale);

            // draw the player name 
            Vector2 playerNameSize = playerNameFont.MeasureString(playerName.ToUpper());
            Vector2 playerNamePos = new Vector2(xStart + 50, yStart) + multiplier * (new Vector2(170, 117) - playerNameSize);
            Vector2 playerNameShadowPos = playerNamePos + new Vector2(2, 2);
            spriteBatch.DrawString(playerNameFont, playerName.ToUpper(), playerNameShadowPos, Color.DimGray);
            spriteBatch.DrawString(playerNameFont, playerName.ToUpper(), playerNamePos, Color.White);

            // draw the current powerup status, if any
            string powerupString = PowerupString();
            Vector2 powerupStringSize = playerNameFont.MeasureString(powerupString);
            Vector2 powerupPos = new Vector2(xStart + 52, yStart + 20) + invMultiplier * (powerupStringSize -(new Vector2(120, 5)));
            //Vector2 powerupPos = new Vector2(xStart + 52, yStart + 20) + invMultiplier * (new Vector2(209, 97) - powerupStringSize);
            Vector2 powerupShadowPos = powerupPos + new Vector2(2, 2);
            spriteBatch.DrawString(powerupFont, powerupString, powerupShadowPos, Color.DimGray);
            spriteBatch.DrawString(powerupFont, powerupString, powerupPos, Color.White);            
            
            // we're done with all strings
            spriteBatch.End();

            ApplyBarEffectParameters();

            // draw bar
            Rectangle barRect = new Rectangle(xStart, yStart + 30, (int)BarAreaSize.X, (int)BarAreaSize.Y);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, spriteScale);
            barEffect.Begin();
            barEffect.CurrentTechnique.Passes[0].Begin();
            spriteBatch.Draw(barBackgroundTexture, barRect, Color.White);
            barEffect.CurrentTechnique.Passes[0].End();
            barEffect.End();
            spriteBatch.End();

            #endregion
        }

        private string PowerupString()
        {
            if (jumps > 0)
                return "JUMPS: " + jumps;
            else
                return "";
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
        private Texture2D barComponentTexture;


    }
}
