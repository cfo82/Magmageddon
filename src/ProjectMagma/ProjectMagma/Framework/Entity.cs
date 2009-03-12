﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ProjectMagma.Shared.Serialization.LevelData;

namespace ProjectMagma.Framework
{
    public class Entity
    {
        public Entity(EntityManager entityManager, string name)
        {
            this.entityManager = entityManager;
            this.name = name;
            this.attributes = new Dictionary<string, Attribute>();
            this.properties = new Dictionary<string, Property>();
        }

        public event UpdateHandler Update;

        public void OnUpdate(GameTime gameTime)
        {
            if (Update != null)
            {
                Update(this, gameTime);
            }
        }

        #region Attribute Handling 

        public void AddAttribute(ContentManager content, AttributeData attributeData)
        {
            AddAttribute(content, attributeData.name, attributeData.template, attributeData.value);
        }

        public void AddAttribute(ContentManager content, string name, string template, string value)
        {
            AttributeTemplateManager attributeTemplateManager = Game.GetInstance().AttributeTemplateManager;
            AttributeTemplate attributeTemplate = attributeTemplateManager.GetAttributeTemplate(template);
            Attribute attribute = attributeTemplate.CreateAttribute(name);
            attribute.Initialize(content, value);
            this.attributes.Add(attribute.Name, attribute);
        }

        public bool HasAttribute(string attribute)
        {
            return Attributes.ContainsKey(attribute);
        }

        public bool IsInt(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return (Attributes[attribute] as IntAttribute) != null;
        }

        public bool IsVector2(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return (Attributes[attribute] as Vector2Attribute) != null;
        }

        public bool IsVector3(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return (Attributes[attribute] as Vector3Attribute) != null;
        }

        public int GetInt(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsInt(attribute));
            return (Attributes[attribute] as IntAttribute).Value;
        }

        public Vector2 GetVector2(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsVector2(attribute));
            return (Attributes[attribute] as Vector2Attribute).Value;
        }

        public Vector3 GetVector3(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsVector3(attribute));
            return (Attributes[attribute] as Vector3Attribute).Value;
        }

        public void SetInt(string attribute, int value)
        {
            (Attributes[attribute] as IntAttribute).Value = value;
        }

        public void SetVector2(string attribute, Vector2 value)
        {
            (Attributes[attribute] as Vector2Attribute).Value = value;
        }

        public void SetVector3(string attribute, Vector3 value)
        {
            (Attributes[attribute] as Vector3Attribute).Value = value;
        }

        #endregion

        #region Property Handling

        public void AddProperty(string name, Property property)
        {
            if (properties.ContainsKey(name))
            {
                // TODO: duplicate property exception
                properties.Add(name, null); // throws exception
            }
            else
            {
                properties.Add(name, property);
                property.OnAttached(this);
            }
        }

        public void RemoveProperty(string name)
        {
            if (properties.ContainsKey(name))
            {
                Property property = properties[name];
                property.OnDetached(this);
                properties.Remove(name);
            }
        }

        #endregion

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public Dictionary<string, Attribute> Attributes
        {
            get
            {
                return attributes;
            }
        }

        private EntityManager entityManager;
        private string name;
        private Dictionary<string, Attribute> attributes;
        private Dictionary<string, Property> properties;
    }
}
