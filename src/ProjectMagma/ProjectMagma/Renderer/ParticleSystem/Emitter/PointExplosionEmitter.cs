using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.ParticleSystem.Emitter
{
    public abstract class PointExplosionEmitter : ParticleEmitter
    {
        public abstract int CalculateParticleCount(double lastFrameTime, double currentFrameTime);
        //NewParticle[] CreateParticles(double lastFrameTime, double currentFrameTime);
        public abstract void CreateParticles(double lastFrameTime, double currentFrameTime, CreateVertex[] array, int start, int length);
        public abstract int EmitterIndex { set; get; }
    }
}
