using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.Interface
{
    public class RendererUpdatable
    {
        public virtual void UpdateBool(string id, bool value) { }
        public virtual void UpdateFloat(string id, float value) { }
        public virtual void UpdateInt(string id, int value) { }
        public virtual void UpdateMatrix(string id, Matrix value) { }
        public virtual void UpdateQuaternion(string id, Quaternion value) { }
        public virtual void UpdateString(string id, string value) { }
        public virtual void UpdateVector2(string id, Vector2 value) { }
        public virtual void UpdateVector3(string id, Vector3 value) { }
    }
}
