/*
xWinForms © 2007-2009
Eric Grossinger - ericgrossinger@gmail.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace xWinFormsLib
{
    class Scrollbar: Control
    {
        Type type = Type.Horizontal;
        public enum Type
        {
            Horizontal,
            Vertical
        }

        HScrollbar hscrollbar;
        VScrollbar vscrollbar;

        public int Value
        {
            get
            {
                if (type == Type.Horizontal)
                    return hscrollbar.Value;
                else
                    return vscrollbar.Value;
            }
            set
            {
                if (type == Type.Horizontal)
                    hscrollbar.Value = value;
                else
                    vscrollbar.Value = value;
            }
        }
        public int Max
        {
            get
            {
                if (type == Type.Horizontal)
                    return hscrollbar.Max;
                else
                    return vscrollbar.Max;
            }
            set
            {
                if (type == Type.Horizontal)
                    hscrollbar.Max = value;
                else
                    vscrollbar.Max = value;
            }
        }
        public int Step
        {
            set
            {
                if (type == Type.Horizontal)
                    hscrollbar.Step = value;
                else
                    vscrollbar.Step = value;
            }
        }
        public EventHandler OnChangeValue
        {
            get
            {
                if (type == Type.Horizontal)
                    return hscrollbar.OnChangeValue;
                else
                    return vscrollbar.OnChangeValue;
            }
            set
            {
                if(type == Type.Horizontal)
                    hscrollbar.OnChangeValue += value;
                else
                    vscrollbar.OnChangeValue += value;
            }
        }
        public bool Inverted
        {
            get
            {
                if (type == Type.Horizontal)
                    return hscrollbar.Inverted;
                else
                    return vscrollbar.Inverted;
            }
            set
            {
                if (type == Type.Horizontal)
                    hscrollbar.Inverted = value;
                else
                    vscrollbar.Inverted = value;
            }
        }

        new public float Width
        {
            get { return base.Width; }
            set
            {
                base.Width = value;
                if (hscrollbar != null)
                    hscrollbar.Width = value;
            }
        }
        new public float Height
        {
            get { return base.Height; }
            set
            {
                base.Height = value;
                if (vscrollbar != null)
                    vscrollbar.Height = value;
            }
        }

        public Scrollbar(string name, Vector2 position, Type type, float size)
        {
            this.Name = name;
            this.type = type;

            if (type == Type.Horizontal)           
                hscrollbar = new HScrollbar(position, size);            
            else if (type == Type.Vertical)
                vscrollbar = new VScrollbar(position, size);
        }

        public override void Initialize(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphics)
        {
            if (hscrollbar != null)
            {
                hscrollbar.Owner = this.Owner;
                hscrollbar.Initialize(content, graphics);
            }
            else if (vscrollbar != null)
            {
                vscrollbar.Owner = this.Owner;
                vscrollbar.Initialize(content, graphics);
            }

            base.Initialize(content, graphics);
        }

        public override void Dispose()
        {
            if (hscrollbar != null)
                hscrollbar.Dispose();
            else if (vscrollbar != null)
                vscrollbar.Dispose();

            base.Dispose();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Visible)
            {
                if (hscrollbar != null)
                    hscrollbar.Update(gameTime);
                else if (vscrollbar != null)
                    vscrollbar.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                if (hscrollbar != null)
                    hscrollbar.Draw(spriteBatch);
                else if (vscrollbar != null)
                    vscrollbar.Draw(spriteBatch);
                    
            }
        }
    }
}
