using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Renderer.ParticleSystem.Stateful
{
    public enum Size
    {
        Max256   = 0,    // 16x16 texture
        Max1024  = 1,   // 32x32 texture
        Max2304  = 2,   // 48x48 texture
        Max4096  = 3,   // 64x64 texture
        Max9308  = 4,   // 96x96 texture
        Max16384 = 5,   // 128x128 texture
        Max65536 = 6,   // 256x256 texture
        SizeCount = 7
    }
}
