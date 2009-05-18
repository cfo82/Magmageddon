using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Simulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectMagma.Shared.LevelData;

namespace ProjectMagma
{
    class PlayerMenu : MenuScreen
    {
        private readonly Rectangle PlayerIconRect = new Rectangle(30, 30, 265, 350);

        private readonly double[] playerButtonPressedAt = new double[] { 0, 0, 0, 0 };
        private readonly bool[] playerActive = new bool[] { true, false, false, false };
        private readonly int[] robotSelected = new int[] { 0, 1, 2, 3 };

        private readonly Texture2D[] robotSprites;

        private readonly Texture2D playerBackground;
        private readonly Texture2D playerBackgroundInactive;

        private readonly LevelMenu levelMenu;

        private readonly GamePadState[] previousState = new GamePadState[4];

        public PlayerMenu(Menu menu, LevelMenu levelMenu)
            : base(menu, new Vector2(640, 160))
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

            DrawPrevious = false;
        }

        public override void Update(GameTime gameTime)
        {
            double at = gameTime.TotalGameTime.TotalMilliseconds;

            if (at > menu.buttonPressedAt + Menu.ButtonRepeatTimeout
               && ((GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed
                && menu.lastGPState.Buttons.A == ButtonState.Released)
                || GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed
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
                        player.AddIntAttribute("lives", 5);
                        player.AddStringAttribute("robot_entity", Game.Instance.Robots[robotSelected[i]].Entity);
                        player.AddStringAttribute("player_name", Game.Instance.Robots[robotSelected[i]].Name);
                        players.Add(player);
                    }
                }

                Game.Instance.AddPlayers(players.ToArray<Entity>());

                menu.Close();
                return;
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

                /*
                if (at > menu.elementSelectedAt + Menu.StickRepeatTimeout)
                {
                    if (Vector2.Dot(gamePadState.ThumbSticks.Left, Vector2.UnitY) > Menu.StickDirectionSelectionMin
                        || gamePadState.DPad.Up == ButtonState.Pressed)
                    {
                        // select prev robot
                        robotSelected[i]--;
                        if (robotSelected[i] < 0)
                            robotSelected[i] = Game.Instance.Robots.Count - 1;
                        menu.elementSelectedAt = at;
                    }
                    else
                        if (Vector2.Dot(gamePadState.ThumbSticks.Left, Vector2.UnitY) < -Menu.StickDirectionSelectionMin
                            || gamePadState.DPad.Down == ButtonState.Pressed)
                        {
                            // select next robot
                            robotSelected[i]++;
                            if (robotSelected[i] == Game.Instance.Robots.Count)
                                robotSelected[i] = 0;
                            menu.elementSelectedAt = at;
                        }
                }
                */

                previousState[i] = gamePadState;
            }

        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int i = 0; i < 4; i++)
            {
                bool active = playerActive[i];
                //Texture2D sprite = active ? playerBackground : playerBackgroundInactive; // not needed anymore - dpk
                Texture2D sprite = playerBackgroundInactive;
                float scale = 200f / sprite.Width;
                Vector2 pos = Position - new Vector2(210,0) + new Vector2((i & 1) * 210, ((i & 2) >> 1) * (sprite.Height * scale + 10));

                Color backgroundColor = Color.White;

                // load templates
                LevelData levelData = Game.Instance.ContentManager.Load<LevelData>("Level/RobotTemplates");
                EntityData entityData = levelData.entities[Game.Instance.Robots[i].Entity];
                List<AttributeData> attributes = entityData.CollectAttributes(levelData);
                List<PropertyData> properties = entityData.CollectProperties(levelData);

                // create a dummy entity
                Entity entity = new Entity(entityData.name);
                foreach (AttributeData attributeData in attributes)
                {
                    entity.AddAttribute(attributeData.name, attributeData.template, attributeData.value);
                }

                Vector3 color1 = entity.GetVector3("color1");
                Vector3 color2 = entity.GetVector3("color2");

                if (active)
                {
                    // TODO: replace by real colors
                    if (i == 0) backgroundColor = Color.Red;
                    if (i == 1) backgroundColor = Color.Green;
                    if (i == 2) backgroundColor = Color.Blue;
                    if (i == 3) backgroundColor = Color.Yellow;
                }

                spriteBatch.Draw(sprite, pos, null, backgroundColor, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                Vector2 halfSpriteDim = new Vector2(sprite.Width, sprite.Height)/4;              

                if (active)
                {
                    Texture2D robot = robotSprites[robotSelected[i]];
                    float width = PlayerIconRect.Width * scale;
                    float rscale = width / robot.Width;
                    //spriteBatch.Draw(robot, pos + new Vector2(PlayerIconRect.Left, PlayerIconRect.Top) * scale,
                    //    null, Color.White, 0, Vector2.Zero, ((float)sprite.Width )/ robot.Width*scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(robot, pos,
                        null, Color.White, 0, Vector2.Zero, ((float)sprite.Width) / robot.Width * scale, SpriteEffects.None, 0);

                }
                else
                {
                    Menu.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "HIT START", pos+halfSpriteDim-Vector2.UnitY*27*scale, menu.StaticStringColor, 1.2f*scale);
                    Menu.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "TO JOIN", pos + halfSpriteDim + Vector2.UnitY * 27*scale, menu.StaticStringColor, 1.2f*scale);
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