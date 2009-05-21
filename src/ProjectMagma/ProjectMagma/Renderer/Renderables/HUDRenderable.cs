using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.MathHelpers;
using System.Diagnostics;
using System.Collections.Generic;

namespace ProjectMagma.Renderer
{
    public class HUDRenderable : Renderable
    {
        #region constructor and resource loading/unloading

        public HUDRenderable(
            string playerName, int gamePadIndex,                     // player identification
            int health, int maxHealth, int energy, int maxEnergy,    // displayed in bars
            int lives, int frozen,                                   // displayed as text
            int jumps, float repulsion_seconds,                        // displayed as powerup text          
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
            this.frozen = frozen;
            this.jumps = jumps;
            this.lives = lives;
            this.color1 = color1;
            this.color2 = color2;
            this.repulsion_seconds = repulsion_seconds;
            lastFrameTime = currentFrameTime = 0.0;

            displayedEnergy = energy;
            displayedHealth = health;
            
            frozenColorStrength = new SineFloat(0.0f, 0.85f, 5.0f);
            statusColorStrength = new SineFloat(0.6f, 1.0f, 13.0f);
            powerupPickupDetails = new List<PowerupPickupDetails>();
        }


        public override void LoadResources(Renderer renderer)
        {
            base.LoadResources(renderer);

            barEffect = Game.Instance.ContentManager.Load<Effect>("Effects/Hud/Bars");
            barBackgroundTexture = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Hud/bar_background");
            barComponentTexture = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Hud/bar_highlight");
            playerNameFont = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/hud_playername");
            powerupStatusFont = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/hud_powerup_status");
            livesFont = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/hud_lives");
            powerupCollectFont = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/hud_powerup_collect");
            font = Game.Instance.ContentManager.Load<SpriteFont>("Sprites/HUD/HUDFont");
            
            spriteBatch = new SpriteBatch(Game.Instance.GraphicsDevice);
            ComputePositions();

            statusColorStrength.Start(renderer.Time.At);
        }

        public override void UnloadResources()
        {
            spriteBatch.Dispose();
            base.UnloadResources();
        }


        #endregion constructor

        #region external value updates

        public override void UpdateInt(string id, double timestamp, int value)
        {
            base.UpdateInt(id, timestamp, value);

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

        public override void UpdateString(string id, double timestamp, string value)
        {
            base.UpdateString(id, timestamp, value);

            if (id == "PlayerName")
            {
                playerName = value;
            }
            if (id == "NextPowerupNotification")
            {
                OnPowerupPickup(nextPowerupPickupPosition, value);
            }
        }

        public override void UpdateVector3(string id, double timestamp, Vector3 value)
        {
            base.UpdateVector3(id, timestamp, value);

            if (id == "NextPowerupPickupPosition")
            {
                nextPowerupPickupPosition = value;
            }
        }

        public override void UpdateFloat(string id, double timestamp, float value)
        {
            base.UpdateFloat(id, timestamp, value);

            if (id == "RepulsionSeconds")
            {
                repulsion_seconds = value;
            }
        }

        public override void UpdateBool(string id, double timestamp, bool value)
        {
            base.UpdateBool(id, timestamp, value);

            if (id == "JetpackUsable")
            {
                jetpackUsable = value;
            }
            if (id == "RepulsionUsable")
            {
                repulsionUsable = value;
            }
        }

        #endregion

        public override void Draw(
            Renderer renderer
        )
        {
            // TODO: ADD EVEN MORE BLINKI-BLINKI ON LOW HEALTH?

            // calculate the timestep to make
            lastFrameTime = currentFrameTime;
            currentFrameTime = lastFrameTime + renderer.Time.Dt;

            UpdateDisplayedValues(renderer);
            ApplyBarEffectParameters();
            DrawBars();
            DrawStrings(renderer);
        }

        private void UpdateDisplayedValues(Renderer renderer)
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

            if (frozen > 0 && !frozenColorStrength.Running)
                frozenColorStrength.Start(renderer.Time.At);
            else if (frozen <= 0 && frozenColorStrength.Running)
                frozenColorStrength.StopAfterCurrentPhase();

            frozenColorStrength.Update(renderer.Time.At);
            statusColorStrength.Update(renderer.Time.At);
        }

        private string PowerupString()
        {
            if (jetpackUsable)
            {
                return "HOLD A TO USE JETPACK";
            }
            else if (repulsionUsable)
            {
                return "USE STICK TO MOVE ISLAND";
            }

            // TODO: the stuff below is actually deprecated, could be killed.
            else if (jumps > 0)
            {
                return String.Format("FAR JUMPS: {0}", jumps);
            }
            else if (repulsion_seconds > 0)
            {
                return String.Format("REPULSION: {0:0.0}S", repulsion_seconds);
            }
            else
            {
                //return "TEMP :P";
                return "";
            }
        }

        private void DrawBars()
        {
            Rectangle barRect = new Rectangle(xStart, (int) (yStart + 30), (int)barAreaSize.X, (int)barAreaSize.Y);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, spriteScale);
            barEffect.Begin();
            barEffect.CurrentTechnique.Passes[0].Begin();
            spriteBatch.Draw(barBackgroundTexture, barRect, Color.White);
            barEffect.CurrentTechnique.Passes[0].End();
            barEffect.End();
            spriteBatch.End();
        }

        private void DrawStrings(Renderer renderer)
        {
            double dt = currentFrameTime - lastFrameTime;

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, spriteScale);
            Vector3 textColor = defaultTextColor * (1 - frozenColorStrength.Value) + frozenTextColor * frozenColorStrength.Value;


            // draw the player name
            Vector2 playerNameSize = playerNameFont.MeasureString(playerName.ToUpper());
            Vector2 playerNamePos = new Vector2(xStart + 50, yStart) + multiplier * (new Vector2(170, 117) - playerNameSize);
            Vector2 playerNameShadowPos = playerNamePos + textShadowOffset;
            spriteBatch.DrawString(playerNameFont, playerName.ToUpper(), playerNameShadowPos, Color.DimGray);
            spriteBatch.DrawString(playerNameFont, playerName.ToUpper(), playerNamePos, new Color(textColor));

            // draw the current powerup status, if any            
            string powerupString = PowerupString();
            Vector2 powerupStringSize = powerupStatusFont.MeasureString(powerupString);
            Vector2 powerupPos = new Vector2(xStart + 52, yStart + 21) + invMultiplier * (new Vector2(165, 71) - powerupStringSize);
            Vector2 powerupShadowPos = powerupPos + textShadowOffset;
            spriteBatch.DrawString(powerupStatusFont, powerupString, powerupShadowPos, Color.DimGray);
            spriteBatch.DrawString(powerupStatusFont, powerupString, powerupPos, new Color(Vector3.One * statusColorStrength.Value));

            // draw the number of lives
            string livesString = lives.ToString();
            Vector2 livesStringSize = powerupStatusFont.MeasureString(livesString);
            Vector2 livesCenterOffset = new Vector2((int) livesStringSize.X / 2, (int) livesStringSize.Y / 2);
            //Viewport v = Game.Instance.GraphicsDevice.Viewport
            //            Vector2 livesPos = new Vector2(xStart + 19, yStart + 50) + multiplier * (new Vector2(236*v.Width/1280, 21*v.Height/720) - livesStringSize * 0.25f) - livesCenterOffset;
            Vector2 livesPos = new Vector2(xStart + 19, yStart + 50) + multiplier * (new Vector2(236, 21) - livesStringSize * 0.25f) - livesCenterOffset;
            spriteBatch.DrawString(livesFont, livesString, livesPos, new Color(Color.White, 0.85f));

            for(int i = powerupPickupDetails.Count-1; i >= 0; i--)
            {
                PowerupPickupDetails details = powerupPickupDetails[i];
                string detailsString = details.Notification;
                Vector2 detailsStringSize = powerupStatusFont.MeasureString(detailsString);
                Vector2 detailsCenterOffset = new Vector2((int)detailsStringSize.X / 2, (int)detailsStringSize.Y / 2);

                // compute projected position
                Vector3 projection = Vector3.Transform(details.Position, renderer.Camera.CenterView * renderer.Camera.Projection);
                Vector2 detailsPos = new Vector2(projection.X / projection.Z, projection.Y / projection.Z);
                detailsPos = detailsPos / 2 + new Vector2(0.5f, 0.5f / renderer.Camera.AspectRatio);
                detailsPos.Y = 1.0f / renderer.Camera.AspectRatio - detailsPos.Y;
                detailsPos *= Game.Instance.GraphicsDevice.Viewport.Width;
                
                // apply age effect
                //detailsPos.Y -= (float)dt * PowerupNotificationFadeoutVerticalSpeed * details.Age;
                //details.Age += (float)dt * PowerupNotificationAgeStep;


                detailsPos.Y -= PowerupNotificationFadeoutVerticalSpeed * details.Age;
                details.Age += PowerupNotificationAgeStep;

                // draw it 
                Vector2 detailsShadowPos = detailsPos + textShadowOffset;
                spriteBatch.DrawString(powerupCollectFont, detailsString, detailsShadowPos, new Color(Color.DimGray, 1.0f - details.Age));
                spriteBatch.DrawString(powerupCollectFont, detailsString, detailsPos, new Color(Color.White, 1.0f - details.Age));

                if(details.Age >= 1.0f)
                {
                    powerupPickupDetails.RemoveAt(i);
                }
            }

            spriteBatch.End();
        }

        void OnPowerupPickup(Vector3 nextPowerupPickupPosition, String notification)
        {
            PowerupPickupDetails details = new PowerupPickupDetails();
            details.Position = nextPowerupPickupPosition;
            details.Notification = notification;
            details.Age = 0;
            powerupPickupDetails.Add(details);
        }
        
        private void ComputePositions()
        {
            Viewport viewport = Game.Instance.GraphicsDevice.Viewport;
            float screenScale = (float)viewport.Width / 1280f;
            spriteScale = Matrix.CreateScale(screenScale, screenScale, 1);

            barAreaSize = new Vector2(271, 52);
            Vector2 TotalSize = barAreaSize + new Vector2(0, 50);
            int horOff = (int)(100);
            int vertOff = (int)(50);

            playerMirror = Matrix.Identity;
            multiplier = new Vector2(0, 0);
            if (gamePadIndex == 0 || gamePadIndex == 2)
            {
                xStart = horOff;
            }
            else
            {
                // everything is computed in 1280x720 space and then scaled later, thus 1280 here
                xStart = 1280 - (int)TotalSize.X - horOff;
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
                // everything is computed in 1280x720 space and then scaled later, thus 720 here
                yStart = 720 - (int)TotalSize.Y - vertOff;
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
        }

        public string PlayerName
        {
            set { playerName = value; }
        }

        private SpriteFont font, powerupCollectFont;
        private SpriteBatch spriteBatch;

        private string playerName;
        private int gamePadIndex;
        private int health;
        private int maxHealth;
        private int energy;
        private int maxEnergy;
        private int frozen; // remaining time in milliseconds
        private int jumps;
        private int lives;

        private bool jetpackUsable, repulsionUsable;

        private float displayedHealth, displayedEnergy;

        private SineFloat frozenColorStrength;
        private SineFloat statusColorStrength;

        private Effect barEffect;

        private bool healthBlink, energyBlink;


        private Texture2D barBackgroundTexture;
        private Texture2D barComponentTexture;

        private static readonly Vector3 frozenTextColor = new Vector3(0, 156, 255) / 255;
        //private static readonly Vector3 frozenTextColor = new Vector3(128, 200, 255) / 255;
        private static readonly Vector3 defaultTextColor = new Vector3(255, 255, 255) / 255;

        private float repulsion_seconds;

        private Vector3 color1;
        private Vector3 color2;

        private SpriteFont playerNameFont;
        private SpriteFont powerupStatusFont;
        private SpriteFont livesFont;

        private Vector2 barAreaSize;
        private Vector2 multiplier, invMultiplier;
        private Matrix spriteScale, playerMirror;
        private int xStart, yStart;

        private static readonly Vector2 textShadowOffset = new Vector2(2, 2);

        private Vector3 nextPowerupPickupPosition;

        class PowerupPickupDetails
        {
            public Vector3 Position { get; set; }
            public String Notification { get; set; }
            public float Age { get; set; }
        }


        private double lastFrameTime;
        private double currentFrameTime;
        //private static readonly float PowerupNotificationAgeStep = 1.0f;
        //private static readonly float PowerupNotificationFadeoutVerticalSpeed = 2000.0f;

        private static readonly float PowerupNotificationAgeStep = 0.02f;
        private static readonly float PowerupNotificationFadeoutVerticalSpeed = 20.0f;

        private List<PowerupPickupDetails> powerupPickupDetails;

    }
}
