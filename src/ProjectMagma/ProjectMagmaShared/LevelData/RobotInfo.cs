using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Shared.LevelData
{
    public class RobotInfo
    {
        private String name;
        private String description;
        private String entity;

        public RobotInfo(String name, String description, String entity)
        {
            this.name = name;
            this.description = description;
            this.entity = entity;
        }

        public String Name
        {
            get { return name; }
        }

        public String Description
        {
            get { return description; }
        }

        public String Entity
        {
            get { return entity; }
        }

    }
}
