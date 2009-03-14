/*
xWinForms © 2007-2009
Eric Grossinger - ericgrossinger@gmail.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace xWinFormsLib
{
    public class MouseCursor: iMouseCursor
    {
        Vector2 position = Vector2.Zero;
        Point location = Point.Zero;
        Vector2 previousPos = Vector2.Zero;
        Vector2 speed = Vector2.Zero;
        
        float rotation = 0f;
        float rotationSpeed = 0f;
        float scale = 1f;

        bool hasShadow = true;

        Texture2D texture;        
        Rectangle sourceRect;
        Vector2 center;
        Color color = Color.White;
        SpriteEffects effect = SpriteEffects.None;

        Color shadowColor = new Color(0, 0, 0, 0.1f);
        Vector2 shadowOrigin = new Vector2(6f, 2f);
        Vector2 shadowOffset = Vector2.Zero;

        CursorType type = CursorType.Default;
        public enum CursorType
        {
            Default,
            Resize
        }

        bool limitToWorkingAreaOnly = false;

        SpriteBatch spriteBatch;

        public Vector2 Position { get { return position; } set { position = value; } }
        public Point Location { get { return location; } set { location = value; } }
        public Vector2 Speed { get { return speed; } set { speed = value; } }
        public float Rotation { get { return rotation; } set { rotation = value; } }
        public float RotationSpeed { get { return rotationSpeed; } set { rotationSpeed = value; } }
        public float Scale { get { return scale; } set { scale = value; } }
        public Rectangle SourceRect { get { return sourceRect; } set { sourceRect = value; } }
        public Vector2 Center { get { return center; } set { center = value; } }
        public Texture2D Texture { get { return texture; } set { texture = value; } }
        public Color Color { get { return color; } set { color = value; } }
        public SpriteEffects Effect { get { return effect; } set { effect = value; } }
        public bool HasShadow { get { return hasShadow; } set { hasShadow = value; } }
        public CursorType Type
        {
            get { return type; }
            set
            {
                type = value;
                Initialize();
            }
        }

        public int X { get { return location.X; } }
        public int Y { get { return location.Y; } }

        MouseTracker tracker;

        public MouseCursor(bool bUseTracker, bool limitToWorkingAreaOnly)
        {
            this.limitToWorkingAreaOnly = limitToWorkingAreaOnly;
            spriteBatch = new SpriteBatch(FormCollection.Graphics.GraphicsDevice);
            Initialize();

            if (bUseTracker)
                tracker = new MouseTracker();
        }

        private void Initialize()
        {
            this.texture = Texture2D.FromFile(FormCollection.Graphics.GraphicsDevice, @"content\textures\cursors\" + type.ToString() + ".png");
            sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
            center = Vector2.Zero;
        }

        public void Dispose()
        {
            if (tracker != null)
                tracker.Dispose();

            texture.Dispose();
            spriteBatch.Dispose();
        }

        public void Update(GameTime gameTime)
        {
            previousPos = position;
           
            UpdateMouseState();
            UpdateGamePadState();

            speed = Vector2.Normalize(position - previousPos) * Vector2.Distance(position, previousPos);

            //rotation += rotationSpeed;

            if (limitToWorkingAreaOnly)
            {
                if (position.X < 0)
                    position.X = 0;
                else if (position.X > FormCollection.Graphics.GraphicsDevice.Viewport.Width)
                    position.X = FormCollection.Graphics.GraphicsDevice.Viewport.Width;

                if (position.Y < 0)
                    position.Y = 0;
                else if (position.Y > FormCollection.Graphics.GraphicsDevice.Viewport.Height)
                    position.Y = FormCollection.Graphics.GraphicsDevice.Viewport.Height;
            }

            location.X = (int)position.X;
            location.Y = (int)position.Y;

            UpdateShadow();

            if (tracker != null)
                tracker.Update();
        }

        private void UpdateShadow()
        {
            if (MouseHelper.IsReleased)
                shadowOffset += Vector2.Normalize(shadowOrigin - shadowOffset);
            else
                shadowOffset *= 0.5f;
        }

        private void UpdateMouseState()
        {
            position.X = MouseHelper.State.X;
            position.Y = MouseHelper.State.Y;
        }

        private void UpdateGamePadState()
        {
            if (MouseHelper.GamePadState.Triggers.Left == 0f)
            {
                position.X += MouseHelper.GamePadState.ThumbSticks.Left.X * 10f;
                position.Y -= MouseHelper.GamePadState.ThumbSticks.Left.Y * 10f;
            }
            else
            {
                position.X += MouseHelper.GamePadState.ThumbSticks.Left.X * 4f;
                position.Y -= MouseHelper.GamePadState.ThumbSticks.Left.Y * 4f;
            }

            location.X = (int)position.X;
            location.Y = (int)position.Y;

            if (MouseHelper.State.X != position.X)
                Mouse.SetPosition((int)position.X, (int)position.Y);
            if (MouseHelper.State.Y != position.Y)
                Mouse.SetPosition((int)position.X, (int)position.Y);
        }

        public void Draw()
        {
            if (tracker != null)
                tracker.Draw();

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            if (hasShadow)
                spriteBatch.Draw(texture, position + shadowOffset, sourceRect, shadowColor, rotation, center, scale, effect, 0f);
            
            spriteBatch.Draw(texture, position, sourceRect, color, rotation, center, scale, effect, 0f);
            
            spriteBatch.End();
        }
    }
}
