﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class HUDRenderable : Renderable
    {
        #region constructor and resource loading/unloading

        public HUDRenderable(
            string playerName, int gamePadIndex,                     // player identification
            int health, int maxHealth, int energy, int maxEnergy,    // displayed in bars
            int lives, int frozen,                                   // displayed as text
            int jumps, int repulsion_seconds,                        // displayed as powerup text          
            Vector3 color1, Vector3 color2                           // player color specifices
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
            this.lives = lives;
            this.color1 = color1;
            this.color2 = color2;
            this.repulsion_seconds = repulsion_seconds;

            displayedEnergy = energy;
            displayedHealth = health;

            barEffect = Game.Instance.ContentManager.Load<Effect>("Effects/Hud/Bars");
            barBackgroundTexture = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Hud/bar_background");
            barComponentTexture = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Hud/bar_highlight");

            ComputePositions();
        }

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


        #endregion constructor

        #region external value updates

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
            else if (id == "Lives")
            {
                lives = value;
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

        #endregion

        public override void Draw(
            Renderer renderer,
            GameTime gameTime
        )
        {
            // TESTHACK
            //health += maxHealth/30;
            //if (health > maxHealth) health = 0;
            // /TESTHACK

            // TODO: ADD FROZEN INDICATION. OLD CODE:
            //double frozenSeconds = (double)this.frozen / 1000.0;
            //string frozenString = string.Format("frozen: {0:00.00}", frozenSeconds);

            // TODO: ADD LIVES DISPLAY

            // TODO: ADD EVEN MORE BLINKI-BLINKI ON LOW HEALTH!!!!!!1!!

            // TODO: FIX REPULSION SECONDS

            UpdateDisplayedValues();
            ApplyBarEffectParameters();
            DrawStrings();
            DrawBars();
        }

        private void UpdateDisplayedValues()
        {
            const float c = 0.15f;
            displayedHealth = MathHelper.Lerp(displayedHealth, (float)health, c);
            displayedEnergy = MathHelper.Lerp(displayedEnergy, (float)energy, c);
            if (displayedHealth - health > 0.01)
                healthBlink = !healthBlink;
            else
                healthBlink = false;
            if (displayedEnergy - energy > 0.01)
                energyBlink = !energyBlink;
            else
                energyBlink = false;
        }

        private string PowerupString()
        {
            if (jumps > 0)
            {
                return "JUMPS: " + jumps;
            }
            else if (repulsion_seconds > 0)
            {
                return "REPULSION: " + repulsion_seconds + "SEC";
            }
            else
            {
                return "TEMP :P";
            }
        }

        private void DrawBars()
        {
            Rectangle barRect = new Rectangle(xStart, yStart + 30, (int)barAreaSize.X, (int)barAreaSize.Y);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, spriteScale);
            barEffect.Begin();
            barEffect.CurrentTechnique.Passes[0].Begin();
            spriteBatch.Draw(barBackgroundTexture, barRect, Color.White);
            barEffect.CurrentTechnique.Passes[0].End();
            barEffect.End();
            spriteBatch.End();
        }

        private void DrawStrings()
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, spriteScale);
            Vector3 textColor = defaultTextColor * (1 - frozenColorStrength) + frozenTextColor * frozenColorStrength;

            // draw the player name
            Vector2 playerNameSize = playerNameFont.MeasureString(playerName.ToUpper());
            Vector2 playerNamePos = new Vector2(xStart + 50, yStart) + multiplier * (new Vector2(170, 117) - playerNameSize);
            Vector2 playerNameShadowPos = playerNamePos + textShadowOffset;
            spriteBatch.DrawString(playerNameFont, playerName.ToUpper(), playerNameShadowPos, Color.DimGray);
            spriteBatch.DrawString(playerNameFont, playerName.ToUpper(), playerNamePos, new Color(textColor));

            // draw the current powerup status, if any
            string powerupString = PowerupString();
            Vector2 powerupStringSize = playerNameFont.MeasureString(powerupString);
            Vector2 powerupPos = new Vector2(xStart + 52, yStart + 20) + invMultiplier * (powerupStringSize - (new Vector2(120, 5)));
            //Vector2 powerupPos = new Vector2(xStart + 52, yStart + 20) + invMultiplier * (new Vector2(209, 97) - powerupStringSize);
            Vector2 powerupShadowPos = powerupPos + textShadowOffset;
            spriteBatch.DrawString(powerupFont, powerupString, powerupShadowPos, Color.DimGray);
            spriteBatch.DrawString(powerupFont, powerupString, powerupPos, new Color(textColor));

            // done
            spriteBatch.End();
        }

        private void ComputePositions()
        {
            Viewport viewport = Game.Instance.GraphicsDevice.Viewport;
            float screenScale = (float)viewport.Width / 1280f;
            spriteScale = Matrix.CreateScale(screenScale, screenScale, 1);
            barAreaSize = new Vector2(271, 52);
            Vector2 TotalSize = barAreaSize + new Vector2(0, 50);
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
            barEffect.Parameters["Size"].SetValue(barAreaSize);
            barEffect.Parameters["HealthValue"].SetValue(displayedHealth / maxHealth);
            barEffect.Parameters["EnergyValue"].SetValue(displayedEnergy / maxEnergy);
            barEffect.Parameters["HealthBlink"].SetValue(healthBlink);
            barEffect.Parameters["EnergyBlink"].SetValue(energyBlink);
            barEffect.Parameters["PlayerColor1"].SetValue(color1);
            barEffect.Parameters["PlayerColor2"].SetValue(color2);
            barEffect.Parameters["PlayerMirror"].SetValue(playerMirror);
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
        private int lives;

        private float displayedHealth, displayedEnergy;

        private float frozenColorStrength;

        private Effect barEffect;

        private bool healthBlink, energyBlink;


        private Texture2D barBackgroundTexture;
        private Texture2D barComponentTexture;

        private static readonly Vector3 frozenTextColor = new Vector3(0, 156, 255) / 255;
        private static readonly Vector3 defaultTextColor = new Vector3(255, 255, 255) / 255;

        private int repulsion_seconds;

        private Vector3 color1;
        private Vector3 color2;

        private SpriteFont playerNameFont;
        private SpriteFont powerupFont;

        private Vector2 barAreaSize;
        private Vector2 multiplier, invMultiplier;
        private Matrix spriteScale, playerMirror;
        private int xStart, yStart;

        private static readonly Vector2 textShadowOffset = new Vector2(2, 2);

    }
}
