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
    interface iMouseCursor
    {
        Point Location { get; set; }
        Vector2 Position { get; set; }
        Vector2 Speed { get; set; }
        float RotationSpeed { get; set; }
        float Rotation { get; set; }
        float Scale { get; set; }

        bool HasShadow { get; set; }

        Texture2D Texture { get; set; }
        Rectangle SourceRect { get; set; }
        Vector2 Center { get; set; }        
        Color Color { get; set; }
        SpriteEffects Effect { get; set; }

        //EventHandler OnMousePress { get; set; }
        //EventHandler OnMouseRelease { get; set; }
    }
}
