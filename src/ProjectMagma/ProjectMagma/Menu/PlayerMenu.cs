using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Simulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ProjectMagma
{
    class PlayerMenu : MenuScreen
    {
        private readonly Rectangle PlayerIconRect = new Rectangle(30, 30, 265, 350);

        private readonly double[] playerButtonPressedAt = new double[] { 0, 0, 0, 0 };
        private readonly bool[] playerActive = new bool[] { true, false, false, false };
        private readonly int[] robotSelected = new int[] { 0, 0, 0, 0 };

        private readonly Texture2D[] robotSprites;

        private readonly Texture2D playerBackground;
        private readonly Texture2D playerBackgroundInactive;

        private readonly LevelMenu levelMenu;

        private readonly GamePadState[] previousState = new GamePadState[4];

        public PlayerMenu(Menu menu, LevelMenu levelMenu)
            : base(menu, new Vector2(550, 250))
        {
            this.levelMenu = levelMenu;

            playerBackground = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Menu/robot_selection_background");
            playerBackgroundInactive = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Menu/robot_selection_background_inactive");

            // init robot sprites
            robotSprites = new Texture2D[Game.Instance.Robots.Count];
            for (int i = 0; i < Game.Instance.Robots.Count; i++)
            {
                try
                {
                    robotSprites[i] = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Menu/Robot/" + Game.Instance.Robots[i].Entity);
                }
                catch (Exception)
                {
                    robotSprites[i] = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Menu/Robot/no_image");
                }
            }

            // all active gamepads have a robot
            for (int i = 1; i < 4; i++)
            {
                playerActive[i] = GamePad.GetCapabilities((PlayerIndex)i).IsConnected;
                robotSelected[i] = i;
            }
        }

        public override void Update(GameTime gameTime)
        {
            double at = gameTime.TotalGameTime.TotalMilliseconds;

            if (at > menu.buttonPressedAt + Menu.ButtonRepeatTimeout
               && ((GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed
                && menu.lastGPState.Buttons.A == ButtonState.Released)
                || (Keyboard.GetState().IsKeyDown(Keys.Enter)
                    && menu.lastKBState.IsKeyUp(Keys.Enter))))
            {
                List<Entity> players = new List<Entity>(4);
                for (int i = 0; i < 4; i++)
                {
                    if (playerActive[i])
                    {
                        Entity player = new Entity("player" + (players.Count + 1));
                        player.AddIntAttribute("game_pad_index", i);
                        player.AddStringAttribute("robot_entity", Game.Instance.Robots[robotSelected[i]].Entity);
                        player.AddStringAttribute("player_name", Game.Instance.Robots[robotSelected[i]].Name);
                        players.Add(player);
                    }
                }

                Game.Instance.AddPlayers(players.ToArray<Entity>());

                menu.Close();
            }

            for (int i = 0; i < 4; i++)
            {
                GamePadState gamePadState = GamePad.GetState((PlayerIndex)i);

                if (i > 0 // don't allow player1 do deactivate
                    && gamePadState.Buttons.Start == ButtonState.Pressed
                    && previousState[i].Buttons.Start == ButtonState.Released
                    && at > playerButtonPressedAt[i] + Menu.ButtonRepeatTimeout)
                {
                    playerActive[i] = !playerActive[i];
                    playerButtonPressedAt[i] = at;
                }

                if (at > menu.elementSelectedAt + Menu.StickRepeatTimeout)
                {
                    if (Vector2.Dot(gamePadState.ThumbSticks.Left, Vector2.UnitY) > Menu.StickDirectionSelectionMin
                        || gamePadState.DPad.Up == ButtonState.Pressed)
                    {
                        // select prev robot
                        robotSelected[i]--;
                        if (robotSelected[i] < 0)
                            robotSelected[i] = 0;
                        menu.elementSelectedAt = at;
                    }
                    else
                        if (Vector2.Dot(gamePadState.ThumbSticks.Left, Vector2.UnitY) < -Menu.StickDirectionSelectionMin
                            || gamePadState.DPad.Down == ButtonState.Pressed)
                        {
                            // select next robot
                            robotSelected[i]++;
                            if (robotSelected[i] == Game.Instance.Robots.Count)
                                robotSelected[i] = Game.Instance.Robots.Count - 1;
                            menu.elementSelectedAt = at;
                        }
                }

                previousState[i] = gamePadState;
            }

        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int i = 0; i < 4; i++)
            {
                bool active = playerActive[i];
                Texture2D sprite = active ? playerBackground : playerBackgroundInactive;
                float scale = 200f / sprite.Width;
                Vector2 pos = Position + new Vector2((i & 1) * 220, ((i & 2) >> 1) * (sprite.Height * scale + 20));
                spriteBatch.Draw(sprite, pos, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);

                if (active)
                {
                    Texture2D robot = robotSprites[robotSelected[i]];
                    float width = PlayerIconRect.Width * scale;
                    float rscale = width / robot.Width;
                    spriteBatch.Draw(robot, pos + new Vector2(PlayerIconRect.Left, PlayerIconRect.Top) * scale,
                        null, Color.White, 0, Vector2.Zero, rscale, SpriteEffects.None, 0);
                }
            }
        }

        public override void OnOpen()
        {
            // deactivate players who's gamepad was unplugged
            for (int i = 1; i < 4; i++)
            {
                playerActive[i] = playerActive[i] && GamePad.GetCapabilities((PlayerIndex)i).IsConnected;
            }
        }
    }
}