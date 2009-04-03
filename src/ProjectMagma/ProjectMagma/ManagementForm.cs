using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using xWinFormsLib;
using ProjectMagma.Simulation;

namespace ProjectMagma
{
    public delegate void EntitySelectionChangeHandler(ManagementForm form, Entity oldSelection, Entity newSelection);
 
    public class ManagementForm
    {
        public ManagementForm(FormCollection formCollection)
        {
            this.formCollection = formCollection;
        }

        public void BuildForm()
        {
            isMinimized = false;

            //Create a new form
            formCollection.Add(new Form(formName, "Game Control", new Vector2(640, 300), new Vector2(10, 
                Game.Instance.Window.ClientBounds.Height-310), Form.BorderStyle.Sizable));
            formCollection[formName].Style = Form.BorderStyle.Sizable;

            List<string> entityNames = new List<string>();
            foreach (Entity e in Game.Instance.Simulation.EntityManager)
            {
                entityNames.Add(e.Name);
            }
            string[] entityNameArray = new string[entityNames.Count];
            entityNames.CopyTo(entityNameArray, 0);

            formCollection[formName].Controls.Add(new Listbox(
                entityListName, new Vector2(10, 30), 120, 260,
                entityNameArray));
            ((Listbox)formCollection[formName][entityListName]).HorizontalScrollbar = true;

            formCollection[formName].Controls.Add(new Listbox(
                attributeListName, new Vector2(135, 30), 190, 260,
                new string[0]));
            ((Listbox)formCollection[formName][attributeListName]).HorizontalScrollbar = true;

            formCollection[formName].Controls.Add(new Label(attributeValueLabelName, new Vector2(330, 30), "Attribute Value: ", Color.TransparentBlack, Color.Black, 100, Label.Align.Left));
            formCollection[formName].Controls.Add(new Textbox(attributeValueTextName, new Vector2(330, 50), 300));
            formCollection[formName].Controls.Add(new Button(changeAttributeName, new Vector2(330, 80), "Change Attribute", formCollection[formName].BackColor, Color.Black));

            formCollection[formName].Show();
            formCollection[formName].Minimize();

            // register events
            Game.Instance.Simulation.EntityManager.EntityAdded += OnEntityAdded;
            Game.Instance.Simulation.EntityManager.EntityRemoved += OnEntityRemoved;
            formCollection[formName].OnResized += OnResized;
            ((Listbox)formCollection[formName][entityListName]).OnChangeSelection += OnEntityListSelectionChanged;
            ((Listbox)formCollection[formName][attributeListName]).OnChangeSelection += OnAttributeListSelectionChanged;
            ((Button)formCollection[formName][changeAttributeName]).OnPress += OnChangeAttribute;
        }

        private void OnEntityAdded(EntityManager manager, Entity entity)
        {
            ((Listbox)formCollection[formName][entityListName]).Add(entity.Name);
        }

        private void OnEntityRemoved(EntityManager manager, Entity entity)
        {
            ((Listbox)formCollection[formName][entityListName]).Remove(entity.Name);
        }

        private void OnResized(object obj, System.EventArgs e)
        {
            if (formCollection[formName].IsMinimized)
            {
                isMinimized = true;

                if (this.EntitySelectionChanged != null && currentSelectedEntity != null)
                {
                    this.EntitySelectionChanged(this, currentSelectedEntity, null);
                }
            }
            else if (isMinimized)
            {
                if (this.EntitySelectionChanged != null && currentSelectedEntity != null)
                {
                    this.EntitySelectionChanged(this, null, currentSelectedEntity);
                }
            }
        }

        private void OnEntityListSelectionChanged(object obj, System.EventArgs e)
        {
            Entity selectedEntity = null;

            if (((Listbox)formCollection[formName][entityListName]).SelectedIndex >= 0)
            {
                string selectedEntityName = ((Listbox)formCollection[formName][entityListName]).SelectedItem;
                selectedEntity = Game.Instance.Simulation.EntityManager[selectedEntityName];

                ((Listbox)formCollection[formName][attributeListName]).Clear();
                foreach (Attribute attribute in selectedEntity.Attributes.Values)
                {
                    ((Listbox)formCollection[formName][attributeListName]).Add(attribute.Name);
                }
            }
            else
            {
                ((Listbox)formCollection[formName][attributeListName]).Clear();
            }

            if (this.EntitySelectionChanged != null)
            {
                this.EntitySelectionChanged(this, currentSelectedEntity, selectedEntity);
            }
            this.currentSelectedEntity = selectedEntity;
        }

        private void OnAttributeListSelectionChanged(object obj, System.EventArgs e)
        {
            if (((Listbox)formCollection[formName][entityListName]).SelectedIndex >= 0 &&
                ((Listbox)formCollection[formName][attributeListName]).SelectedIndex >= 0)
            {
                string selectedEntityName = ((Listbox)formCollection[formName][entityListName]).SelectedItem;
                string selectedAttributeName = ((Listbox)formCollection[formName][attributeListName]).SelectedItem;
                Entity selectedEntity = Game.Instance.Simulation.EntityManager[selectedEntityName];
                if (selectedEntity.HasAttribute(selectedAttributeName))
                {
                    Attribute attribute = selectedEntity.GetAttribute(selectedAttributeName);
                    ((Textbox)formCollection[formName][attributeValueTextName]).Text = attribute.StringValue;
                }
            }
        }

        private void OnChangeAttribute(object obj, System.EventArgs e)
        {
            if (((Listbox)formCollection[formName][entityListName]).SelectedIndex >= 0 &&
                ((Listbox)formCollection[formName][attributeListName]).SelectedIndex >= 0)
            {
                string selectedEntityName = ((Listbox)formCollection[formName][entityListName]).SelectedItem;
                string selectedAttributeName = ((Listbox)formCollection[formName][attributeListName]).SelectedItem;
                Entity selectedEntity = Game.Instance.Simulation.EntityManager[selectedEntityName];
                if (selectedEntity.HasAttribute(selectedAttributeName))
                {
                    selectedEntity.GetAttribute(selectedAttributeName).Initialize(
                        ((Textbox)formCollection[formName][attributeValueTextName]).Text);
                }
            }
        }

        public event EntitySelectionChangeHandler EntitySelectionChanged;

        private FormCollection formCollection;
        private Entity currentSelectedEntity;
        private bool isMinimized;
        private static readonly string formName = "managementForm";
        private static readonly string entityListName = "entityList";
        private static readonly string attributeListName = "attributeList";
        private static readonly string attributeValueLabelName = "attributeValueLabel";
        private static readonly string attributeValueTextName = "attributeValueText";
        private static readonly string changeAttributeName = "changeAttributeText";
    }
}
