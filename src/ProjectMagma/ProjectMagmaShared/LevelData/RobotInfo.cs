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
        private String fileName;

        public RobotInfo(String name, String description, String fileName)
        {
            this.name = name;
            this.description = description;
            this.fileName = fileName;
        }

        public String Name
        {
            get { return name; }
        }

        public String Description
        {
            get { return description; }
        }

        public String FileName
        {
            get { return fileName; }
        }

    }
}
