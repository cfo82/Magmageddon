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
    public class ControlCollection
    {
        Control activeControl;
        List<Control> controls = new List<Control>();
        
        Form Owner;

        public Control ActiveControl { get { return activeControl; } set { activeControl = value; } }

        public ControlCollection(Form form)
        {
            this.Owner = form;
        }

        public int Count
        {
            get { return controls.Count; }
        }
        public Control this[int index]
        {
            get { return controls[index]; }
            set { controls[index] = value; }
        }
        public Control this[string name]
        {
            get
            {
                for (int i = 0; i < controls.Count; i++)
                    if (controls[i].Name == name)
                        return controls[i];

                return null;
            }
            set
            {
                for (int i = 0; i < controls.Count; i++)
                    if (controls[i].Name == name)
                        controls[i] = value;
            }
        }

        public void Add(Control control)
        {
            control.Owner = Owner;
            control.FontName = Owner.FontName;
            controls.Add(control);
        }
        public void Remove(Control control)
        {
            control.Dispose();
            controls.Remove(control);
        }
        public void Remove(string name)
        {
            for (int i = 0; i < controls.Count; i++)
                if (controls[i].Name == name)
                {
                    controls[i].Dispose();
                    controls.RemoveAt(i);
                    break;
                }
        }
        public void RemoveAt(int index)
        {
            controls[index].Dispose();
            controls.RemoveAt(index);
        }
        public void Clear()
        {
            for (int i = 0; i < controls.Count; i++)
                controls[i].Dispose();

            controls.Clear();
        }
    }
}
