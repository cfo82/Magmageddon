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
    public class MessageBox : Form
    {
        Type type;
        public enum Type
        {
            MB_OK,
            MB_OKCANCEL,
            MB_YESNO,
            MB_YESNOCANCEL,
            MB_INPUTOK,
            MB_INPUTOKCANCEL
        }

        Label lblText;
        Textbox txtInput;
        Button btOk, btYes, btNo, btCancel;

        public EventHandler OnOk;
        public EventHandler OnYes;
        public EventHandler OnNo;
        public EventHandler OnCancel;

        public MessageBox(Vector2 size, string title, string text, Type type)
            : base("msgbox", title, size, BorderStyle.Fixed)
        {
            this.Text = text;
            this.type = type;
            Initialize(FormCollection.ContentManager, FormCollection.Graphics.GraphicsDevice);
        }

        public MessageBox(Vector2 size, Vector2 position, string title, string text, Type type)
            : base("msgbox", title, size, BorderStyle.Fixed)
        {
            this.Text = text;
            this.type = type;
            this.Position = position;
            Initialize(FormCollection.ContentManager, FormCollection.Graphics.GraphicsDevice);
        }

        public override void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            if(Position == Vector2.Zero)
                Position = new Vector2(FormCollection.Graphics.GraphicsDevice.Viewport.Width / 2f - Size.X / 2f,
                FormCollection.Graphics.GraphicsDevice.Viewport.Height / 2f - Size.Y / 2f);

            lblText = new Label("lblText", new Vector2(20, 35), Text, Color.TransparentBlack, Color.Black, (int)Size.X - 40, Label.Align.Center);
            this.Controls.Add(lblText);

            InitButtons();

            if (txtInput != null)
                txtInput.HasFocus = true;

            FormCollection.Forms.Add(this);

            this.Show();
            this.Focus();

            base.Initialize(content, graphics);
        }

        private void InitButtons()
        {
            switch (type)
            {
                case Type.MB_OK:
                    btOk = new Button("btOk", new Vector2(Size.X / 2 - 50, Size.Y - 30), 100, "OK", Color.White, Color.Black);
                    btOk.OnRelease += btOk_OnPress;
                    this.Controls.Add(btOk);
                    break;
                case Type.MB_OKCANCEL:
                    btOk = new Button("btOk", new Vector2(Size.X / 2 - 83, Size.Y - 30), 80, "OK", Color.White, Color.Black);
                    btOk.OnRelease += btOk_OnPress;
                    this.Controls.Add(btOk);
                    btCancel = new Button("btCancel", new Vector2(Size.X / 2 + 3, Size.Y - 30), 80, "Cancel", Color.White, Color.Black);
                    btCancel.OnRelease += btCancel_OnPress;
                    this.Controls.Add(btCancel);
                    break;
                case Type.MB_YESNO:
                    btYes = new Button("btYes", new Vector2(Size.X / 2 - 83, Size.Y - 30), 80, "Yes", Color.White, Color.Black);
                    btYes.OnRelease += btYes_OnPress;
                    this.Controls.Add(btYes);
                    btNo = new Button("btNo", new Vector2(Size.X / 2 + 3, Size.Y - 30), 80, "No", Color.White, Color.Black);
                    btNo.OnRelease += btNo_OnPress;
                    this.Controls.Add(btNo);
                    break;
                case Type.MB_YESNOCANCEL:
                    btYes = new Button("btYes", new Vector2(Size.X / 2 - 125, Size.Y - 30), 80, "Yes", Color.White, Color.Black);
                    btYes.OnRelease += btYes_OnPress;
                    this.Controls.Add(btYes);
                    btNo = new Button("btNo", new Vector2(Size.X / 2 - 40, Size.Y - 30), 80, "No", Color.White, Color.Black);
                    btNo.OnRelease += btNo_OnPress;
                    this.Controls.Add(btNo);
                    btCancel = new Button("btCancel", new Vector2(Size.X / 2 + 45, Size.Y - 30), 80, "Cancel", Color.White, Color.Black);
                    btCancel.OnRelease += btCancel_OnPress;
                    this.Controls.Add(btCancel);
                    break;
                case Type.MB_INPUTOK:
                    lblText.Y = 25;
                    txtInput = new Textbox("inputBox", new Vector2(Size.X / 2 - (Size.X - 40) / 2, 43), (int)Size.X - 40);
                    this.Controls.Add(txtInput);
                    btOk = new Button("btOk", new Vector2(Size.X / 2 - 50, Size.Y - 30), 100, "OK", Color.White, Color.Black);
                    btOk.OnRelease += btOk_OnPress ;
                    this.Controls.Add(btOk);
                    break;
                case Type.MB_INPUTOKCANCEL:                    
                    lblText.Y = 25;
                    txtInput = new Textbox("inputBox", new Vector2(Size.X / 2 - (Size.X - 40) / 2, 43), (int)Size.X - 40);
                    this.Controls.Add(txtInput);
                    btOk = new Button("btOk", new Vector2(Size.X / 2 - 83, Size.Y - 30), 80, "OK", Color.White, Color.Black);
                    btOk.OnRelease += btOk_OnPress;
                    this.Controls.Add(btOk);
                    btCancel = new Button("btCancel", new Vector2(Size.X / 2 + 3, Size.Y - 30), 80, "Cancel", Color.White, Color.Black);
                    btCancel.OnRelease += btCancel_OnPress;
                    this.Controls.Add(btCancel);
                    break;
            }            
        }

        private void btOk_OnPress(object obj, EventArgs e)
        {
            if (type == Type.MB_INPUTOK || type == Type.MB_INPUTOKCANCEL)
            {
                if (OnOk != null)
                    OnOk(txtInput.Text, null);
            }
            else if (OnOk != null)
                OnOk(null, null);

            this.Close();
        }
        private void btCancel_OnPress(object obj, EventArgs e)
        {
            if (type == Type.MB_INPUTOK || type == Type.MB_INPUTOKCANCEL)
            {
                if (OnCancel != null)
                    OnCancel(txtInput.Text, null);
            }
            else if (OnCancel != null)
                OnCancel(null, null);

            this.Close();
        }
        private void btYes_OnPress(object obj, EventArgs e)
        {
            if (OnYes != null)
                OnYes(null, null);

            this.Close();
        }
        private void btNo_OnPress(object obj, EventArgs e)
        {
            if (OnNo != null)
                OnNo(null, null);

            this.Close();
        }

        public override void Dispose()
        {            
            base.Dispose();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
