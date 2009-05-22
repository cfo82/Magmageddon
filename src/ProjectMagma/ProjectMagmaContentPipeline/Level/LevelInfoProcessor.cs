using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Xml;
using ProjectMagma.Shared.LevelData;
using ProjectMagma.Shared.LevelData.Serialization;
using System.IO;

namespace ProjectMagma.ContentPipeline.Level
{
    [ContentProcessor(DisplayName = "Magma - LevelInfo Processor")]
    public class LevelInfoProcessor : ContentProcessor<XmlDocument, List<LevelInfo>>
    {
        private static LevelInfo ProcessLevel(XmlElement level)
        {
            string name = level["Name"].InnerText;
            string description = level["Description"].InnerText;
            string simulationFileName = level["SimulationFileName"].InnerText;
            string rendererFileName = level["RendererFileName"].InnerText;

            return new LevelInfo(name, description, simulationFileName, rendererFileName);
        }

        private List<LevelInfo> ProcessLevelInfo(XmlDocument input, ContentProcessorContext context)
        {
            XmlElement documentRoot = input.DocumentElement;

            XmlNodeList levelNodes = documentRoot.GetElementsByTagName("Level");

            List<LevelInfo> levels = new List<LevelInfo>(levelNodes.Count);

            foreach(XmlElement levelEl in levelNodes)
            {
                levels.Add(ProcessLevel(levelEl));
            }

            return levels;
        }

        public override List<LevelInfo> Process(XmlDocument input, ContentProcessorContext context)
        {
            return ProcessLevelInfo(input, context);
        }

    }
}
