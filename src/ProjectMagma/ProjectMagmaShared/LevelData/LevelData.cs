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
            entities = new Dictionary<string, EntityData>();
        }

        public Dictionary<string, EntityData> entities;
    }
}
