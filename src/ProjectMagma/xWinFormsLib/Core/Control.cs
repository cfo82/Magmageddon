/*
xWinForms © 2007-2009
Eric Grossinger - ericgrossinger@gmail.com
*/
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace xWinFormsLib
{
    public class Control: iControl
    {
        string name = string.Empty;        
        Vector2 position = Vector2.Zero;
        Vector2 size = Vector2.Zero;
        string text = string.Empty;
        Color backcolor = Color.White;
        Color forecolor = Color.Black;
        bool enabled = true;
        bool visible = true;
        bool isDisposed = true;
        string tooltipText;

        Form owner;
        SpriteFont font;
        string fontName;

        public Rectangle area = Rectangle.Empty;

        public string Name { get { return name; } set { name = value; } }       
        public Vector2 Position { get { return position; } set { position = value; } }
        public Vector2 Size
        {
            get { return size; }
            set
            {
                size = value;
                area.Width = (int)size.X;
                area.Height = (int)size.Y;
            }
        }
        public string Text { get { return text; } set { text = value; } }
        public Color BackColor { get { return backcolor; } set { backcolor = value; } }
        public Color ForeColor { get { return forecolor; } set { forecolor = value; } }
        public bool Enabled { get { return enabled; } set { enabled = value; } }
        public bool Visible { get { return visible; } set { visible = value; } }
        public bool IsDisposed { get { return isDisposed; } set { isDisposed = value; } }
        public Form Owner { get { return owner; } set { owner = value; } }
        public SpriteFont Font
        {
            get { return font; }
            set { font = value; }
        }
        public string FontName
        {
            get { return fontName; }
            set
            {
                fontName = value;
                if (!isDisposed && System.IO.File.Exists(@"content\fonts\" + fontName))
                    font = FormCollection.ContentManager.Load<SpriteFont>(@"content\fonts\" + fontName);
            }
        }
        public string ToolTipText { get { return tooltipText; } set { tooltipText = value; } }

        public float Top { get { return position.Y; } set { position.Y = value; } }
        public float Left { get { return position.X; } set { position.X = value; } }
        public float Width
        {
            get { return size.X; }
            set
            {
                size.X = value;
                area.Width = (int)value;
            }
        }
        public float Height
        {
            get { return size.Y; }
            set
            {
                size.Y = value;
                area.Height = (int)value;
            }
        }
        public float X { get { return position.X; } set { position.X = value; } }
        public float Y { get { return position.Y; } set { position.Y = value; } }

        EventHandler onMouseOver;
        EventHandler onMouseOut;
        EventHandler onPress;
        EventHandler onRelease;

        public EventHandler OnMouseOver { get { return onMouseOver; } set { onMouseOver += value; } }
        public EventHandler OnMouseOut { get { return onMouseOut; } set { onMouseOut += value; } }
        public EventHandler OnPress { get { return onPress; } set { onPress += value; } }
        public EventHandler OnRelease { get { return onRelease; } set { onRelease += value; } }

        public Control() { }
        public Control(string name)
        {
            this.name = name;
        }
        public Control(string name, Vector2 position)
        {
            this.name = name;
            this.position = position;
        }

        virtual public void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            isDisposed = false;
        }

        virtual public void Dispose()
        {
            isDisposed = true;
        }

        virtual public void Update(GameTime gameTime)
        {
            if (owner != null)
            {
                //if (FormCollection.ActiveForm != owner)
                //    return;

                //if (owner.Menu != null && owner.Menu.State != Menu.MenuState.Closed)
                //    return;

                area.X = (int)(Position.X + Owner.Position.X);
                area.Y = (int)(Position.Y + Owner.Position.Y);
            }
            else
            {
                area.X = (int)(Position.X);
                area.Y = (int)(Position.Y);
            }

            /*
            if (area.Contains(MouseHelper.Cursor.Location))
            {
                if (!bMouseOver)
                {
                    bMouseOver = true;
                    if (onMouseOver != null)
                        onMouseOver(this, null);
                }
            }
            else if (bMouseOver)
            {
                bMouseOver = false;
                if (onMouseOut != null)
                    onMouseOut(this, null);
            }
            */

            UpdateToolTip();
        }

        private void UpdateToolTip()
        {
            if (area.Contains(MouseHelper.Cursor.Location))
            {                
            }
        }

        virtual public void Draw(SpriteBatch spriteBatch)
        {
        }
    }
}
