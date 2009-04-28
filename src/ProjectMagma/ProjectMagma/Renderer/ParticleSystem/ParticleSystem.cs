using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer.ParticleSystem
{
    interface ParticleSystem
    {
        void AddEmitter(ParticleEmitter emitter);
        void Update(GameTime gameTime);
        void Render(Matrix viewMatrix, Matrix projectionMatrix);
    }
}
