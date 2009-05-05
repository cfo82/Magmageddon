using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.ParticleSystem
{
    public interface ParticleEmitter
    {
        int CalculateParticleCount(double lastFrameTime, double currentFrameTime);
        //NewParticle[] CreateParticles(double lastFrameTime, double currentFrameTime);
        void CreateParticles(double lastFrameTime, double currentFrameTime, CreateVertex[] array, int start, int length);
    }
}
