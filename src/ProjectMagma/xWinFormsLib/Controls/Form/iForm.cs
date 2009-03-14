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
    interface iForm
    {
        /// <summary>
        /// Form Title displayed in the titlebar (if the border is visible)
        /// </summary>
        string Title { get; set; }
        /// <summary>
        /// SpriteBatch used to draw the form and its controls
        /// </summary>
        SpriteBatch Spritebatch { get; set; }
        /// <summary>
        /// Form font
        /// </summary>
        SpriteFont Font { get; set; }
        /// <summary>
        /// Form File Title
        /// </summary>
        String FontName { get; set; }
        /// <summary>
        /// Form Controls
        /// </summary>
        ControlCollection Controls { get; set; }
        /// <summary>
        /// Minimum Size the form can be resized to
        /// </summary>
        Vector2 MinimumSize { get; set; }

        /// <summary>
        /// Border Style (None = will remove the title, buttons and dragArea)
        /// </summary>
        Form.BorderStyle Style { get; set; }

        /// <summary>
        /// Determines if the form has a Minimize button
        /// </summary>
        bool HasMinimizeButton { get; set; }
        /// <summary>
        /// Determines if the form has a Maximize button
        /// </summary>
        bool HasMaximizeButton { get; set; }
    }
}
