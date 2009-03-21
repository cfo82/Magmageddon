using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Shared.LevelData
{
    public class LevelData
    {
        public LevelData()
        {
            entities = new List<EntityData>();
        }

        public List<EntityData> entities;
    }
}
