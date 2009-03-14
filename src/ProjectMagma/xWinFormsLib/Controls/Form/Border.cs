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
    class Border
    {
        Texture2D[] texture = new Texture2D[9];
        Rectangle[] destRect = new Rectangle[9];
        string prefix = "default";

        Color shadowColor = new Color(0, 0, 0, 0.1f);
        Vector4 shadowVect = Vector4.Zero;
        Rectangle shadowRect = new Rectangle();
        Point shadowOffset = new Point(6, 4);

        Color backColor;

        bool resizable = false;

        public Border(string texture_prefix, Color color, bool resizable)            
        {
            this.prefix = texture_prefix;
            backColor = color;
            this.resizable = resizable;
        }

        public void Initialize(ContentManager content, GraphicsDevice graphics)
        {
            texture[0] = Texture2D.FromFile(graphics, @"content\textures\controls\form\" + prefix + "_upperleft.png");
            texture[1] = Texture2D.FromFile(graphics, @"content\textures\controls\form\" + prefix + "_uppercenter.png");
            texture[2] = Texture2D.FromFile(graphics, @"content\textures\controls\form\" + prefix + "_upperright.png");
            texture[3] = Texture2D.FromFile(graphics, @"content\textures\controls\form\" + prefix + "_midleft.png");
            texture[4] = Texture2D.FromFile(graphics, @"content\textures\controls\form\" + prefix + "_midcenter.png");
            texture[5] = Texture2D.FromFile(graphics, @"content\textures\controls\form\" + prefix + "_midright.png");
            texture[6] = Texture2D.FromFile(graphics, @"content\textures\controls\form\" + prefix + "_lowerleft.png");
            texture[7] = Texture2D.FromFile(graphics, @"content\textures\controls\form\" + prefix + "_lowercenter.png");
            if (resizable)
                texture[8] = Texture2D.FromFile(graphics, @"content\textures\controls\form\" + prefix + "_lowerright_resize.png");
            else
                texture[8] = Texture2D.FromFile(graphics, @"content\textures\controls\form\" + prefix + "_lowerright.png");

            for (int i = 0; i < texture.Length; i++)
                destRect[i] = new Rectangle(0, 0, texture[i].Width, texture[i].Height);
        }

        public void Dispose()
        {
            for (int i = 0; i < texture.Length; i++)
                texture[i].Dispose();
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Vector2 size, float shadowAlpha)
        {
            UpdateRects(position, size);

            if (size.Y > texture[0].Height)
                DrawShadow(spriteBatch, position + Vector2.One * 15f, shadowAlpha);

            DrawUpper(spriteBatch);
            if (size.Y > texture[0].Height)
                DrawMiddle(spriteBatch);
            if (size.Y > texture[0].Height)
                DrawLower(spriteBatch);            
        }

        private void UpdateRects(Vector2 position, Vector2 size)
        {
            #region Upper
            destRect[0].X = (int)position.X;
            destRect[0].Y = (int)position.Y;

            destRect[1].X = destRect[0].X + destRect[0].Width;
            destRect[1].Y = destRect[0].Y;
            destRect[1].Width = (int)size.X - (texture[0].Width + texture[2].Width);

            destRect[2].X = destRect[1].X + destRect[1].Width;
            destRect[2].Y = destRect[1].Y;
            #endregion

            #region Middle
            destRect[3].X = destRect[0].X;
            destRect[3].Y = destRect[0].Y + destRect[0].Height;
            destRect[3].Height = (int)size.Y - (texture[0].Height + texture[6].Height);

            destRect[4].X = destRect[1].X;
            destRect[4].Y = destRect[3].Y;
            destRect[4].Width = destRect[1].Width;
            destRect[4].Height = destRect[3].Height;

            destRect[5].X = destRect[2].X;
            destRect[5].Y = destRect[3].Y;
            destRect[5].Height = destRect[3].Height;

            #endregion

            #region Lower
            destRect[6].X = destRect[0].X;
            destRect[6].Y = destRect[3].Y + destRect[3].Height;
            if ((int)size.Y > texture[0].Height + texture[6].Height)
                destRect[6].Height = texture[6].Height;
            else
            {
                destRect[6].Y = destRect[0].Y + destRect[0].Height;
                destRect[6].Height = (int)size.Y - texture[6].Height;
            }

            destRect[7].X = destRect[1].X;
            destRect[7].Y = destRect[6].Y;
            destRect[7].Width = destRect[1].Width;
            destRect[7].Height = destRect[6].Height;

            destRect[8].X = destRect[2].X;
            destRect[8].Y = destRect[6].Y;
            destRect[8].Height = destRect[6].Height;
            #endregion
        }

        private void DrawUpper(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture[0], destRect[0], backColor);
            spriteBatch.Draw(texture[1], destRect[1], backColor);
            spriteBatch.Draw(texture[2], destRect[2], backColor);
        }
        private void DrawMiddle(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture[3], destRect[3], backColor);
            spriteBatch.Draw(texture[4], destRect[4], backColor);
            spriteBatch.Draw(texture[5], destRect[5], backColor);
        }
        private void DrawLower(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture[6], destRect[6], backColor);
            spriteBatch.Draw(texture[7], destRect[7], backColor);
            spriteBatch.Draw(texture[8], destRect[8], backColor);
        }

        private void DrawShadow(SpriteBatch spriteBatch, Vector2 position, float alpha)
        {
            shadowVect = shadowColor.ToVector4();
            shadowVect.W = alpha;
            shadowColor = new Color(shadowVect);

            shadowRect = destRect[2];
            shadowRect.X += shadowOffset.X;
            shadowRect.Y += shadowOffset.Y;
            spriteBatch.Draw(texture[2], shadowRect, shadowColor);

            shadowRect = destRect[5];
            shadowRect.X += shadowOffset.X;
            shadowRect.Y += shadowOffset.Y;
            spriteBatch.Draw(texture[5], shadowRect, shadowColor);

            shadowRect = destRect[6];
            shadowRect.X += shadowOffset.X;
            shadowRect.Y += shadowOffset.Y;
            spriteBatch.Draw(texture[6], shadowRect, shadowColor);

            shadowRect = destRect[7];
            shadowRect.X += shadowOffset.X;
            shadowRect.Y += shadowOffset.Y;
            spriteBatch.Draw(texture[7], shadowRect, shadowColor);

            shadowRect = destRect[8];
            shadowRect.X += shadowOffset.X;
            shadowRect.Y += shadowOffset.Y;
            spriteBatch.Draw(texture[8], shadowRect, shadowColor);
        }
    }
}
