using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer
{
    public class ParallelLight
    {
        public ParallelLight(
            Vector3 diffuseColor,
            //Vector3 specularColor,
            Vector3 direction
        )
        {
            DiffuseColor = diffuseColor;
            SpecularColor = diffuseColor;
            Direction = direction;
        }

        public Vector3 DiffuseColor { get; set; }
        public Vector3 SpecularColor { get; set; }

        private Vector3 direction;
        public Vector3 Direction
        {
            get { return direction; }
            set { direction = Vector3.Normalize(value); }
        }
    }
}