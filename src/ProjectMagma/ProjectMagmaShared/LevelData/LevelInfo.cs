using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Shared.LevelData
{
    public class LevelInfo
    {
        public LevelInfo()
        {
        }

        public LevelInfo(string name, string description, string simulationFileName, string rendererFileName)
        {
            this.name = name;
            this.description = description;
            this.simulationFileName = simulationFileName;
            this.rendererFileName = rendererFileName;
        }

        public string Name
        {
            set { name = value; }
            get { return name; }
        }

        public string Description
        {
            set { description = value; }
            get { return description; }
        }

        public string SimulationFileName
        {
            set { simulationFileName = value; }
            get { return simulationFileName; }
        }

        public string RendererFileName
        {
            set { rendererFileName = value; }
            get { return rendererFileName; }
        }

        private string name;
        private string description;
        private string simulationFileName;
        private string rendererFileName;
    }
}
