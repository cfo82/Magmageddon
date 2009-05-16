﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.Interface
{
    public interface RendererUpdate
    {
        void Apply();
    }

    public abstract class TargetedRendererUpdate : RendererUpdate
    {
        public TargetedRendererUpdate(RendererUpdatable updatable)
        {
            this.updatable = updatable;
        }

        public abstract void Apply();

        protected RendererUpdatable updatable;
    }

    public abstract class ValueRendererUpdate : TargetedRendererUpdate
    {
        public ValueRendererUpdate(RendererUpdatable updatable, string id)
        :   base(updatable)
        {
            this.id = id;
        }

        protected string id;
    }

    public class BoolRendererUpdate : ValueRendererUpdate
    {
        public BoolRendererUpdate(RendererUpdatable updatable, string id, bool value)
        :   base(updatable, id)
        {
            this.value = value;
        }

        public override void Apply()
        {
            updatable.UpdateBool(id, value);
        }

        protected bool value;
    }

    public class FloatRendererUpdate : ValueRendererUpdate
    {
        public FloatRendererUpdate(RendererUpdatable updatable, string id, float value)
        :   base(updatable, id)
        {
            this.value = value;
        }

        public override void Apply()
        {
            updatable.UpdateFloat(id, value);
        }

        protected float value;
    }

    public class IntRendererUpdate : ValueRendererUpdate
    {
        public IntRendererUpdate(RendererUpdatable updatable, string id, int value)
        :   base(updatable, id)
        {
            this.value = value;
        }

        public override void Apply()
        {
            updatable.UpdateInt(id, value);
        }

        protected int value;
    }

    public class MatrixRendererUpdate : ValueRendererUpdate
    {
        public MatrixRendererUpdate(RendererUpdatable updatable, string id, Matrix value)
        :   base(updatable, id)
        {
            this.value = value;
        }

        public override void Apply()
        {
            updatable.UpdateMatrix(id, value);
        }

        protected Matrix value;
    }

    public class QuaternionRendererUpdate : ValueRendererUpdate
    {
        public QuaternionRendererUpdate(RendererUpdatable updatable, string id, Quaternion value)
        :   base(updatable, id)
        {
            this.value = value;
        }

        public override void Apply()
        {
            updatable.UpdateQuaternion(id, value);
        }

        protected Quaternion value;
    }

    public class StringRendererUpdate : ValueRendererUpdate
    {
        public StringRendererUpdate(RendererUpdatable updatable, string id, string value)
        :   base(updatable, id)
        {
            this.value = value;
        }

        public override void Apply()
        {
            updatable.UpdateString(id, value);
        }

        protected string value;
    }

    public class Vector2RendererUpdate : ValueRendererUpdate
    {
        public Vector2RendererUpdate(RendererUpdatable updatable, string id, Vector2 value)
        :   base(updatable, id)
        {
            this.value = value;
        }

        public override void Apply()
        {
            updatable.UpdateVector2(id, value);
        }

        protected Vector2 value;
    }

    public class Vector3RendererUpdate : ValueRendererUpdate
    {
        public Vector3RendererUpdate(RendererUpdatable updatable, string id, Vector3 value)
        :   base(updatable, id)
        {
            this.value = value;
        }

        public override void Apply()
        {
            updatable.UpdateVector3(id, value);
        }

        protected Vector3 value;
    }

    public class AddRenderableUpdate : TargetedRendererUpdate
    {
        public AddRenderableUpdate(Renderable updatable)
        :   base(updatable)
        {
        }

        public override void Apply()
        {
            Game.Instance.Renderer.AddRenderable((Renderable)updatable);
        }
    }

    public class RemoveRenderableUpdate : TargetedRendererUpdate
    {
        public RemoveRenderableUpdate(Renderable updatable)
        :   base(updatable)
        {
        }

        public override void Apply()
        {
            Game.Instance.Renderer.RemoveRenderable((Renderable)updatable);
        }
    }
}
