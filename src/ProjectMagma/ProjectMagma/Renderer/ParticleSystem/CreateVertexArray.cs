using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Renderer.ParticleSystem
{
    public class CreateVertexArray
    {
        public CreateVertexArray(int arraySize)
        {
            Array = new CreateVertex[arraySize];
        }

        public CreateVertexArray(int arraySize, int occupiedSize)
        {
            Array = new CreateVertex[arraySize];
            OccupiedSize = occupiedSize;
        }

        public int OccupiedSize { get; set; }

        public readonly CreateVertex[] Array;
    }
}
