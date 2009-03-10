using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Framework
{
    class MeshAttribute : Attribute
    {
        public MeshAttribute(string name, AttributeTemplate template)
        :   base(name, template)
        {
        }
            
        public override void Initialize(ContentManager content, string value)
        {
            this.meshName = value;
            model = content.Load<Model>(meshName);
        }

        public string MeshName
        {
            get
            {
                return this.meshName;
            }
        }

        public Model Model
        {
            get
            {
                return this.model;
            }
        }

        private string meshName;
        private Model model;
    }
}
