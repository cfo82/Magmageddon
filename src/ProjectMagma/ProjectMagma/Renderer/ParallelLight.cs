using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class ParallelLight
    {
        public ParallelLight(
            Vector3 diffuseColor,
            Vector3 specularColor,
            Vector3 direction
        )
        {
            this.diffuseColor = diffuseColor;
            this.specularColor = specularColor;
            this.direction = direction;
        }

        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        }

        public Vector3 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; }
        }

        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        private Vector3 diffuseColor;
        private Vector3 specularColor;
        private Vector3 direction;
    }
}