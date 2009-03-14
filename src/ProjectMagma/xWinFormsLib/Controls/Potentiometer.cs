/*
xWinForms © 2007-2009
Eric Grossinger - ericgrossinger@gmail.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace xWinFormsLib
{
    public class Potentiometer : Control
    {
        Texture2D texture;
        Rectangle srcRect;
        Vector2 center;
        float scale = 1f;
        float rotation = 0f;

        bool isInUse = false;
        Vector2 mouseStartPos = Vector2.Zero;

        int value = 0;

        public EventHandler OnChangeValue;
        public int Value
        {
            get { return value; }
            set
            {
                if (value != this.value)
                {
                    this.value = value;
                    OnChangeValue(value, null);
                }
            }
        }

        public Potentiometer(string name, Vector2 position)
            : base(name, position)
        {
        }

        public override void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            // TODO: load your content here
            texture = Texture2D.FromFile(graphics, @"content\textures\controls\potentiometer.png");
            srcRect = new Rectangle(0, 0, texture.Width, texture.Height);
            center = new Vector2(texture.Width / 2, texture.Height / 2);
            
            area.Width = texture.Width;
            area.Height = texture.Height;

            UpdateRotation();

            base.Initialize(content, graphics);
        }

        public override void Dispose()
        {
            // TODO: dispose of your content here
            texture.Dispose();

            base.Dispose();
        }

        private void UpdateRotation()
        {
            if (value < 0)
                value = 0;
            else if (value > 100)
                value = 100;

            if (value > 50)
            {
                float angle = (value - 50) * 2 * 140f * 0.01f;
                rotation = MathHelper.ToRadians(angle);
            }
            else if (value <= 50)
            {
                float angle = value * 2 * 140f * 0.01f + 220f;
                rotation = MathHelper.ToRadians(angle);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // TODO: Add your update logic here
            area.X = (int)(Position.X + Owner.Position.X - center.X);
            area.Y = (int)(Position.Y + Owner.Position.Y - center.Y);

            if (area.Contains(MouseHelper.Cursor.Location) && Owner.area.Contains(MouseHelper.Cursor.Location))
            {
                if (MouseHelper.HasBeenPressed)
                    isInUse = true;
            }
            else if(MouseHelper.IsReleased)
                isInUse = false;

            if (isInUse && MouseHelper.IsPressed)
            {
                rotation = Math.GetAngleFrom2DVectors(Position + Owner.Position, MouseHelper.Cursor.Position, true);

                float angle = MathHelper.ToDegrees(rotation) % 360f;

                if (angle > 140f && angle <= 180f)
                    angle = 140f;
                else if (angle < 220f && angle > 180f)
                    angle = 220f;

                rotation = MathHelper.ToRadians(angle);

                int newValue = 0;
                if (angle < 180f)
                    newValue = (int)((float)(angle / 140f) * 50f) + 50;
                else if (angle > 180f)
                    newValue = -(int)(((float)(220 - angle) / 140f) * 50f);

                if (value != newValue)
                {
                    value = newValue;
                    if (OnChangeValue != null)
                        OnChangeValue(value, null);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // TODO: Add your drawing code here     
            spriteBatch.Draw(texture, Position, srcRect, BackColor, rotation, center, scale, SpriteEffects.None, 0f);
        }
    }
}
