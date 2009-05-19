using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ProjectMagma
{
    public class DrawTools
    {
        public static int ContourOffset { get { return 2; } }
        public static Color ShadowColor { get { return new Color(0, 0, 0, 90); } }
        public static Vector2 ShadowOffset { get { return new Vector2(2, 2); } }
        public static Color BorderColor { get { return new Color(0, 0, 0, 200); } }

        public static void DrawString(SpriteBatch spriteBatch, SpriteFont font, string str, Vector2 pos, Color color, float scale)
        {
            spriteBatch.DrawString(font, str, pos, color, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 1.0f);
        }

        public static void DrawShadowString(SpriteBatch spriteBatch, SpriteFont font, string str, Vector2 pos, Color color, float scale)
        {
            spriteBatch.DrawString(font, str, pos + ShadowOffset, ShadowColor, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(font, str, pos, color, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 1.0f);
        }

        public static void DrawCenteredShadowString(SpriteBatch spriteBatch, SpriteFont font, string str, Vector2 pos, Color color, float scale)
        {
            pos -= font.MeasureString(str) / 2 * scale;
            DrawShadowString(spriteBatch, font, str, pos, color, scale);
        }

        public static void DrawCenteredBorderedShadowString(SpriteBatch spriteBatch, SpriteFont font, string str, Vector2 pos, Color color, float scale)
        {
            pos -= font.MeasureString(str) / 2 * scale;
            DrawString(spriteBatch, font, str, pos + new Vector2(+ContourOffset, +ContourOffset), BorderColor, scale);
            DrawString(spriteBatch, font, str, pos + new Vector2(-ContourOffset, +ContourOffset), BorderColor, scale);
            DrawString(spriteBatch, font, str, pos + new Vector2(+ContourOffset, -ContourOffset), BorderColor, scale);
            DrawString(spriteBatch, font, str, pos + new Vector2(-ContourOffset, -ContourOffset), BorderColor, scale);
            DrawString(spriteBatch, font, str, pos + new Vector2(+ContourOffset, +ContourOffset) + DrawTools.ShadowOffset, DrawTools.ShadowColor, scale);
            DrawString(spriteBatch, font, str, pos, color, scale);
        }
    }
}