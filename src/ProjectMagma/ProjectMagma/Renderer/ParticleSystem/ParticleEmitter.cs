using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.ParticleSystem
{
    public interface ParticleEmitter
    {
        NewParticle[] CreateParticles(double lastFrameTime, double currentFrameTime);
    }
}
