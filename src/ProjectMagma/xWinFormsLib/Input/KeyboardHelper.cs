/*
xWinForms © 2007-2009
Eric Grossinger - ericgrossinger@gmail.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using System.Timers;

namespace xWinFormsLib
{
    class KeyboardHelper
    {
        KeyboardState ks;

        Keys[] inputKeys;
        List<Keys> pressedKeys;

        public EventHandler OnKeyPress = null;
        public EventHandler OnKeyRelease = null;
        public EventHandler OnPaste = null;

        KeyboardEventsArgs keybArgs = new KeyboardEventsArgs();

        bool bPasting = false;

        Nullable<Keys> currentKey;
        Nullable<Keys> previousKey;
        Timer keyTimer = new Timer(25);

        public KeyboardHelper()
        {
            inputKeys = new Keys[] { 
                Keys.D0,
                Keys.D1,
                Keys.D2,
                Keys.D3,
                Keys.D4,
                Keys.D5,
                Keys.D5,
                Keys.D6,
                Keys.D7,
                Keys.D8,
                Keys.D9,
                Keys.A, 
                Keys.B, 
                Keys.C, 
                Keys.D, 
                Keys.E, 
                Keys.F, 
                Keys.G, 
                Keys.H, 
                Keys.I, 
                Keys.J, 
                Keys.K, 
                Keys.L, 
                Keys.M, 
                Keys.N, 
                Keys.O, 
                Keys.P, 
                Keys.Q, 
                Keys.R, 
                Keys.S, 
                Keys.T, 
                Keys.U, 
                Keys.V, 
                Keys.W, 
                Keys.X, 
                Keys.Y, 
                Keys.Z,
                Keys.Space,
                Keys.Enter,
                Keys.Home,
                Keys.End,
                Keys.Add,
                Keys.Subtract,
                Keys.Multiply,
                Keys.Divide,
                Keys.Left,
                Keys.Right,
                Keys.Up,
                Keys.Down,
                Keys.Back,
                Keys.Delete,
                Keys.OemQuotes,
                Keys.OemTilde,
                Keys.OemComma,
                Keys.OemPeriod,
                Keys.OemSemicolon,
                Keys.OemBackslash,
                Keys.OemCloseBrackets,
                Keys.OemOpenBrackets,
                Keys.OemPlus,
                Keys.OemMinus,
                Keys.OemQuestion,
                Keys.OemPipe,
                Keys.Tab
            };

            pressedKeys = new List<Keys>();

            keyTimer.Elapsed += new ElapsedEventHandler(KeyTimer_Elapsed);
        }

        public void Update()
        {
            ks = Keyboard.GetState();
            
            if (ks.IsKeyDown(Keys.LeftControl) || ks.IsKeyDown(Keys.RightControl))
            {
                if (ks.IsKeyDown(Keys.V))
                {
                    if (!bPasting)
                    {
                        bPasting = true;
                        if (OnPaste != null)
                            OnPaste(System.Windows.Forms.Clipboard.GetDataObject(), null);
                        return;
                    }
                }
                else if (ks.IsKeyUp(Keys.V))
                    bPasting = false;
            }
            else
                bPasting = false;

            //Capslock
            if (System.Windows.Forms.Control.IsKeyLocked(System.Windows.Forms.Keys.CapsLock)) keybArgs.CapsLock = true; else keybArgs.CapsLock = false;
            //Shift
            if (ks.IsKeyDown(Keys.LeftShift) || ks.IsKeyDown(Keys.RightShift)) keybArgs.ShiftDown = true; else keybArgs.ShiftDown = false;
            //Alt
            if (ks.IsKeyDown(Keys.LeftAlt) || ks.IsKeyDown(Keys.RightAlt)) keybArgs.AltDown = true; else keybArgs.AltDown = false;
            //Control
            if (ks.IsKeyDown(Keys.LeftControl) || ks.IsKeyDown(Keys.RightControl)) keybArgs.ControlDown = true; else keybArgs.ControlDown = false;

            foreach (Keys key in inputKeys)
            {
                if (ks.IsKeyDown(key))
                {
                    if (!pressedKeys.Contains(key))
                    {
                        pressedKeys.Add(key);
                        if (key == Keys.V && bPasting)
                            return;
                        if (OnKeyPress != null)
                            OnKeyPress(key, keybArgs);

                        previousKey = currentKey;
                        currentKey = pressedKeys[pressedKeys.Count - 1];
                        if (previousKey != currentKey)
                        {
                            keyTime = 0;
                            keyTimer.Start();
                        }
                    }
                }
                else if (pressedKeys.Contains(key) && ks.IsKeyUp(key))
                {
                    pressedKeys.Remove(key);

                    if (key == currentKey)
                    {
                        currentKey = null;
                        keyTimer.Stop();
                    }

                    if (OnKeyRelease != null)
                        OnKeyRelease(key, keybArgs);
                }
            }            
        }

        int keyTime = 0;
        private void KeyTimer_Elapsed(Object obj, ElapsedEventArgs args)
        {
            if (currentKey.HasValue)
            {
                if (previousKey.HasValue && currentKey.Value != previousKey && previousKey.Value != Keys.LeftControl && previousKey.Value != Keys.RightControl)
                    keyTime = 0;
                else
                    keyTime++;

                if (keyTime >= 15 && OnKeyPress != null)
                    OnKeyPress(currentKey, keybArgs);
            }
        }
    }

    class KeyboardEventsArgs : EventArgs
    {
        bool bShiftDown = false;
        bool bControlDown = false;
        bool bAltDown = false;
        bool bCapsLock = false;

        public bool ShiftDown { get { return bShiftDown; } set { bShiftDown = value; } }
        public bool CapsLock { get { return bCapsLock; } set { bCapsLock = value; } }
        public bool ControlDown { get { return bControlDown; } set { bControlDown = value; } }
        public bool AltDown { get { return bAltDown; } set { bAltDown = value; } }

        public KeyboardEventsArgs() { }
    }
}
