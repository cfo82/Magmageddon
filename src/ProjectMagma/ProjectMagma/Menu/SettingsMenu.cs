using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ProjectMagma
{
    class SettingsMenu : ItemizedMenuScreen
    {
        readonly MenuItem[] menuItems;
        private readonly Texture2D volumeIndicator;
        private readonly Texture2D volumeIndicatorSelected;

        public SettingsMenu(Menu menu)
            : base(menu, new Vector2(280, 600), 250)
        {
            this.menuItems = new MenuItem[] { 
                new MenuItem("sound_volume", "sound_volume", new ItemSelectionHandler(DoNothing)),
                new MenuItem("music_volume", "music_volume", new ItemSelectionHandler(DoNothing))
            };

            volumeIndicator = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Menu/volume_indicator");
            volumeIndicatorSelected = Game.Instance.ContentManager.Load<Texture2D>("Sprites/Menu/volume_indicator_selected");
        }

        public override MenuItem[] MenuItems
        {
            get { return menuItems; }
        }

        private void DoNothing(MenuItem sender)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float dt = gameTime.ElapsedGameTime.Milliseconds / 1000f;
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            if (Vector2.Dot(gamePadState.ThumbSticks.Left, Vector2.UnitX) > Menu.StickDirectionSelectionMin
                || gamePadState.DPad.Right == ButtonState.Pressed)
            {
                if (SelectedItem.Name == "music_volume")
                    Game.Instance.MusicVolume += dt;
                else
                    if (SelectedItem.Name == "sound_volume")
                        Game.Instance.EffectsVolume += dt;
            }
            else
                if (Vector2.Dot(gamePadState.ThumbSticks.Left, Vector2.UnitX) < -Menu.StickDirectionSelectionMin
                    || gamePadState.DPad.Left == ButtonState.Pressed)
                {
                    if (SelectedItem.Name == "music_volume")
                        Game.Instance.MusicVolume -= dt;
                    else
                        if (SelectedItem.Name == "sound_volume")
                            Game.Instance.EffectsVolume -= dt;
                }

            if (Game.Instance.MusicVolume > 1)
                Game.Instance.MusicVolume = 1;
            if (Game.Instance.MusicVolume < 0)
                Game.Instance.MusicVolume = 0;
            if (Game.Instance.EffectsVolume > 1)
                Game.Instance.EffectsVolume = 1;
            if (Game.Instance.EffectsVolume < 0)
                Game.Instance.EffectsVolume = 0;
        }

        public override void DrawWithItem(GameTime gameTime, SpriteBatch spriteBatch, MenuItem item, Vector2 pos, float scale)
        {
            if (item.Name == "music_volume")
            {
                pos.X += 550 * scale;
                Vector2 nuScale = new Vector2(scale * Game.Instance.MusicVolume, scale);
                if (SelectedItem == item)
                    spriteBatch.Draw(volumeIndicatorSelected, pos, null, Color.White, 0, Vector2.Zero, nuScale, SpriteEffects.None, 0);
                else
                    spriteBatch.Draw(volumeIndicator, pos, null, Color.White, 0, Vector2.Zero, nuScale, SpriteEffects.None, 0);
            }
            else
                if (item.Name == "sound_volume")
                {
                    pos.X += 550 * scale;
                    Vector2 nuScale = new Vector2(scale * Game.Instance.EffectsVolume, scale);
                    if (SelectedItem == item)
                        spriteBatch.Draw(volumeIndicatorSelected, pos, null, Color.White, 0, Vector2.Zero, nuScale, SpriteEffects.None, 0);
                    else
                        spriteBatch.Draw(volumeIndicator, pos, null, Color.White, 0, Vector2.Zero, nuScale, SpriteEffects.None, 0);
                }
        }

        public override void OnClose()
        {
            Game.Instance.SaveSettings();
            base.OnClose();
        }
    }
}