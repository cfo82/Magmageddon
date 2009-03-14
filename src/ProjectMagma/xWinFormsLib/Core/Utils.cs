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
    static class Utils
    {
        static public Color InvertColor(Color color, Vector3 modifier)
        {
            Color invertedColor = new Color(255 - color.R + (int)modifier.X, 255 - color.G + (int)modifier.Y, 255 - color.B + (int)modifier.Z, color.A);
            return invertedColor;
        }

        static public Keys GetKey(char letter)
        {
            switch (letter.ToString().ToLower())
            {
                case "a": return Keys.A;
                case "b": return Keys.B;
                case "c": return Keys.C;
                case "d": return Keys.D;
                case "e": return Keys.E;
                case "f": return Keys.F;
                case "g": return Keys.G;
                case "h": return Keys.H;
                case "i": return Keys.I;
                case "j": return Keys.J;
                case "k": return Keys.K;
                case "l": return Keys.L;
                case "m": return Keys.M;
                case "n": return Keys.N;
                case "o": return Keys.O;
                case "p": return Keys.P;
                case "q": return Keys.Q;
                case "r": return Keys.R;
                case "s": return Keys.S;
                case "t": return Keys.T;
                case "u": return Keys.U;
                case "v": return Keys.V;
                case "w": return Keys.W;
                case "x": return Keys.X;
                case "y": return Keys.Y;
                case "z": return Keys.Z;
            }

            return Keys.None;
        }
    }
}
