/*
xWinForms © 2007-2009
Eric Grossinger - ericgrossinger@gmail.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace xWinFormsLib
{
    public static class MouseHelper
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        static MouseState ms, pms;
        static GamePadState gs, pgs;

        static public MouseCursor Cursor { get { return FormCollection.Cursor; } }
        static public MouseState State { get { return ms; } }
        static public MouseState PreviousState { get { return pms; } }
        static public GamePadState GamePadState { get { return gs; } }
        static public GamePadState PreviousGamePadState { get { return pgs; } }

        static public bool IsPressed
        {
            get
            {
                if (ms.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed || gs.Buttons.A == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                    return true;
                else
                    return false;
            }
        }
        static public bool IsReleased
        {
            get
            {
                if (ms.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && gs.Buttons.A == Microsoft.Xna.Framework.Input.ButtonState.Released)
                    return true;
                else
                    return false;
            }
        }

        static public bool HasBeenPressed
        {
            get
            {
                if ((ms.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && pms.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released) ||
                    (gs.Buttons.A == Microsoft.Xna.Framework.Input.ButtonState.Pressed && pgs.Buttons.A == Microsoft.Xna.Framework.Input.ButtonState.Released))
                    return true;
                else
                    return false;
            }
        }
        static public bool HasBeenReleased
        {
            get
            {
                if ((ms.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && pms.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) ||
                    (gs.Buttons.A == Microsoft.Xna.Framework.Input.ButtonState.Released && pgs.Buttons.A == Microsoft.Xna.Framework.Input.ButtonState.Pressed))
                    return true;
                else
                    return false;
            }
        }

        static public void Update()
        {
            pms = ms;
            ms = Mouse.GetState();

            pgs = gs;
//            gs = GamePad.GetState(PlayerIndex.One);

            //if (HasBeenPressed)
            //    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, ms.X, ms.Y, 0, 0);
        }
    }
}
