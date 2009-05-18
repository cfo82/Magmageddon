using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.Interface
{
    public class RendererUpdatable
    {
        public virtual void UpdateBool(string id, double timestamp, bool value) { }
        public virtual void UpdateFloat(string id, double timestamp, float value) { }
        public virtual void UpdateInt(string id, double timestamp, int value) { }
        public virtual void UpdateMatrix(string id, double timestamp, Matrix value) { }
        public virtual void UpdateQuaternion(string id, double timestamp, Quaternion value) { }
        public virtual void UpdateString(string id, double timestamp, string value) { }
        public virtual void UpdateVector2(string id, double timestamp, Vector2 value) { }
        public virtual void UpdateVector3(string id, double timestamp, Vector3 value) { }
    }
}
