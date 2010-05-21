using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Simulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ProjectMagma.Shared.Model;
using ProjectMagma.Shared.LevelData;
using Xclna.Xna.Animation;
using ProjectMagma.MathHelpers;

namespace ProjectMagma
{
    public class PlayerMenu : MenuScreen
    {
        private const int MaxPlayers = 4;

        private readonly Rectangle PlayerIconRect = new Rectangle(30, 30, 265, 350);

        private readonly double[] playerButtonPressedAt = new double[] { 0, 0, 0, 0 };
        private readonly bool[] playerActive = new bool[] { true, false, false, false };
        private readonly int[] robotSelected = new int[] { 0, 1, 2, 3 };

        private readonly Texture2D playerBackgroundInactive;

        private readonly LevelMenu levelMenu;

        private readonly GamePadState[] previousState = new GamePadState[MaxPlayers];

        private SineFloat playerBoxSize;

        public PlayerMenu(Menu menu, LevelMenu levelMenu)
            : base(menu, new Vector2(640, 160))
        {
            this.levelMenu = levelMenu;

            playerBackgroundInactive = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Menu/robot_selection_background_inactive");

            DrawPrevious = false;

            // initialize walking player hack
            Effect clonedEffect = Game.Instance.ContentManager.Load<Effect>("Effects/Basic/Basic").Clone(Game.Instance.GraphicsDevice);
            playerModel = Game.Instance.ContentManager.Load<MagmaModel>("Models/Player/robot_grp").XnaModel;
            playerMesh = playerModel.Meshes[0];
            foreach (ModelMeshPart meshPart in playerMesh.MeshParts)
            {
                Effect oldEffect = meshPart.Effect;
                meshPart.Effect = clonedEffect;
                oldEffect.Dispose();
            }
            animator = new ModelAnimator(playerModel);
            walkController = new AnimationController(Game.Instance, animator.Animations["walk"]);
            Game.Instance.Components.RemoveAt(Game.Instance.Components.Count - 1);
            foreach (BonePose p in animator.BonePoses)
            {
                p.CurrentController = walkController;
                p.CurrentBlendController = null;
                p.BlendFactor = 0.0f;
            }

            playerPreview = new RenderTarget2D[MaxPlayers];
            for (int i = 0; i < MaxPlayers; ++i)
            {
                playerPreview[i] = new RenderTarget2D(Game.Instance.GraphicsDevice, 445, 445, 1, 
                    Game.Instance.GraphicsDevice.PresentationParameters.BackBufferFormat);
            }

            playerTexture = Game.Instance.ContentManager.Load<Texture2D>("Textures/Player/Robot_texture10");
            specularTexture = Game.Instance.ContentManager.Load<Texture2D>("Textures/Player/robot_spec");

            playerBoxSize = new SineFloat(0.96f, 1.0f, 8.0f);
            playerBoxSize.Start(0.001f);
        }

        public override void Update(GameTime gameTime)
        {
            double at = gameTime.TotalGameTime.TotalMilliseconds;
            playerBoxSize.Update(at);

            if (at > menu.buttonPressedAt + Menu.ButtonRepeatTimeout
               && ((GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed
                && menu.lastGPState.Buttons.A == ButtonState.Released)
                || GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed
                || (Keyboard.GetState().IsKeyDown(Keys.Enter)
                    && menu.lastKBState.IsKeyUp(Keys.Enter))))
            {
                List<Entity> players = new List<Entity>(MaxPlayers);
                for (int i = 0; i < MaxPlayers; i++)
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
                return;
            }

            for (int i = 0; i < MaxPlayers; i++)
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
                    int oldRobotSelected = robotSelected[i];
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
                    for (int j = 0; j < MaxPlayers; ++j)
                    {
                        if(robotSelected[j] == robotSelected[i] && i != j)
                            if(playerActive[j])
                                robotSelected[i] = oldRobotSelected;
                            else
                                robotSelected[j] = oldRobotSelected;
                    }
                }

                previousState[i] = gamePadState;
            }

            for (int i = 0; i < MaxPlayers; ++i)
            {
                // load templates
                LevelData levelData = Game.Instance.ContentManager.Load<LevelData>("Level/Common/RobotTemplates");
                EntityData entityData = levelData.entities[Game.Instance.Robots[robotSelected[i]].Entity];
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

                RenderTarget2D oldRenderTarget = (RenderTarget2D)Game.Instance.GraphicsDevice.GetRenderTarget(0);
                Game.Instance.GraphicsDevice.SetRenderTarget(0, playerPreview[i]);
                Game.Instance.GraphicsDevice.Clear(new Color(0,0,0,0));

                ////////playerModel.Meshes[0].Draw();
                playerMesh.Effects[0].CurrentTechnique = playerMesh.Effects[0].Techniques["AnimatedPlayer"];
                playerMesh.Effects[0].Parameters["Local"].SetValue(Matrix.Identity);
                playerMesh.Effects[0].Parameters["World"].SetValue(Matrix.Identity*0.8f);
                playerMesh.Effects[0].Parameters["View"].SetValue(Matrix.CreateLookAt(new Vector3(3, 3, 3), new Vector3(0,2.0f,-3), Vector3.Up));
                ////////playerMesh.Effects[0].Parameters["View"].SetValue(Matrix.CreateLookAt(new);
                playerMesh.Effects[0].Parameters["Projection"].SetValue(Matrix.CreateOrthographic(8.5f, 8.5f, 0.001f, 1000) * Matrix.CreateTranslation(Vector3.UnitX * 0.36f));
                playerMesh.Effects[0].Parameters["DiffuseTexture"].SetValue(playerTexture);
                playerMesh.Effects[0].Parameters["SpecularTexture"].SetValue(specularTexture);
                playerMesh.Effects[0].Parameters["DiffuseColor"].SetValue(Vector3.One * 1.0f);
                playerMesh.Effects[0].Parameters["SpecularColor"].SetValue(Vector3.One);
                playerMesh.Effects[0].Parameters["SpecularPower"].SetValue(16f);
                playerMesh.Effects[0].Parameters["EmissiveColor"].SetValue(Vector3.One * 0.2f);
                playerMesh.Effects[0].Parameters["ToneColor"].SetValue(entity.GetVector3("color1"));
                playerMesh.Effects[0].Parameters["DirLight0Direction"].SetValue(-Vector3.One);
                playerMesh.Effects[0].Parameters["DirLight0DiffuseColor"].SetValue(entity.GetVector3("color1") * 0.5f);
                playerMesh.Effects[0].Parameters["DirLight0SpecularColor"].SetValue(Vector3.One/3);
                playerMesh.Effects[0].Parameters["DirLight1Direction"].SetValue(new Vector3(-1,1,-1));
                playerMesh.Effects[0].Parameters["DirLight1DiffuseColor"].SetValue(new Vector3(1,1,1)*0.75f);
                playerMesh.Effects[0].Parameters["DirLight1SpecularColor"].SetValue(Vector3.One/3);
                playerMesh.Effects[0].Parameters["DirLight2Direction"].SetValue(new Vector3(-1, -1, 1));
                playerMesh.Effects[0].Parameters["DirLight2DiffuseColor"].SetValue(entity.GetVector3("color2") * 0.75f);
                playerMesh.Effects[0].Parameters["DirLight2SpecularColor"].SetValue(Vector3.One/3);

                Viewport oldViewport = Game.Instance.GraphicsDevice.Viewport;

                double now = Game.Instance.GlobalClock.ContinuousMilliseconds;

                GameTime myGameTime = new GameTime(
                        new TimeSpan((long)(now * 10000d)),
                        new TimeSpan((long)((now - last) * 10000d)),
                        new TimeSpan((long)(now * 10000d)),
                        new TimeSpan((long)((now - last) * 10000d)));
                last = now;

                animator.World = Matrix.Identity;
                walkController.Update(myGameTime);
                animator.Update();
                animator.Draw();

                playerMesh.Draw();

                Game.Instance.GraphicsDevice.SetRenderTarget(0, oldRenderTarget);
            }
        }

        //private static readonly float blendIncrement = 0.2f;
        //private enum AnimationMode
        //{
        //    Permanent, PermanentToPermanent, PermanentToOnce, Once, OnceToOnce, OnceToPermanent
        //}
        private ModelAnimator animator;
        //private AnimationMode animationMode;
        private AnimationController walkController;
        //private Effect clonedEffect;
        //private Dictionary<string, AnimationController> controllers;
        //float blendFactor;
        //string state, destState, permanentState;

        private Model playerModel;
        ModelMesh playerMesh;
        private RenderTarget2D[] playerPreview;
        Texture2D playerTexture;
        Texture2D specularTexture;

        double last = 0.0f;

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            for (int i = 0; i < MaxPlayers; i++)
            {
                bool active = playerActive[i];
                Texture2D sprite = playerBackgroundInactive;
                float scale = 200f / sprite.Width;
                Vector2 pos = Position - new Vector2(210,0) + new Vector2((i & 1) * 210, ((i & 2) >> 1) * (sprite.Height * scale + 10));

                Color backgroundColor = Color.White;

                // load templates
                LevelData levelData = Game.Instance.ContentManager.Load<LevelData>("Level/Common/RobotTemplates");
                EntityData entityData = levelData.entities[Game.Instance.Robots[robotSelected[i]].Entity];
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
                    backgroundColor = new Color(color2 * 0.8f);

                Vector2 halfSpriteDim = new Vector2(sprite.Width, sprite.Height) / 4;
                spriteBatch.Draw(sprite, pos - halfSpriteDim * (playerBoxSize.Value-1f), 
                    null, backgroundColor, 0, Vector2.Zero, scale * playerBoxSize.Value, SpriteEffects.None, 0);

                if (active)
                {
                    Texture2D robot = playerPreview[i].GetTexture();
                    float width = PlayerIconRect.Width * scale;
                    float rscale = width / robot.Width;
                    //spriteBatch.Draw(robot, pos + new Vector2(PlayerIconRect.Left, PlayerIconRect.Top) * scale,
                    //    null, Color.White, 0, Vector2.Zero, ((float)sprite.Width )/ robot.Width*scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(robot, pos,
                        null, Color.White, 0, Vector2.Zero, ((float)sprite.Width) / robot.Width * scale, SpriteEffects.None, 0);

                }
                else
                {
                    DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "HIT START", pos+halfSpriteDim-Vector2.UnitY*27*scale, menu.StaticStringColor, 1.2f*scale);
                    DrawTools.DrawCenteredShadowString(spriteBatch, menu.StaticStringFont, "TO JOIN", pos + halfSpriteDim + Vector2.UnitY * 27 * scale, menu.StaticStringColor, 1.2f * scale);
                }
            }
        }

        public override void OnOpen()
        {
            // deactivate players who's gamepad was unplugged
            for (int i = 1; i < MaxPlayers; i++)
            {
                playerActive[i] = playerActive[i] && GamePad.GetCapabilities((PlayerIndex)i).IsConnected;
            }
        }
    }
}
