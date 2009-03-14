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
    interface iControl
    {
        /// <summary>
        /// Control name
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Position of the control
        /// </summary>
        Vector2 Position { get; set; }
        /// <summary>
        /// Size of the control
        /// </summary>
        Vector2 Size { get; set; }
        /// <summary>
        /// Control Text
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// BackColor of the control
        /// </summary>
        Color BackColor { get; set; }
        /// <summary>
        /// ForeColor of the control
        /// </summary>
        Color ForeColor { get; set; }

        /// <summary>
        /// Determines if the control is enabled
        /// </summary>
        bool Enabled { get; set; }
        /// <summary>
        /// Determines if the control is visible
        /// </summary>
        bool Visible { get; set; }
        /// <summary>
        /// Determines if the control has been disposed
        /// </summary>
        bool IsDisposed { get; set; }        
    }
}
