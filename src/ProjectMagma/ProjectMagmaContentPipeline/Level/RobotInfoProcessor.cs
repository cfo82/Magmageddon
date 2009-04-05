using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Xml;
using ProjectMagma.Shared.LevelData;

namespace ProjectMagma.ContentPipeline.Robot
{
    [ContentProcessor(DisplayName = "Magma - RobotInfo Processor")]
    public class RobotInfoProcessor : ContentProcessor<XmlDocument, List<RobotInfo>>
    {
        private static RobotInfo ProcessRobot(XmlElement Robot)
        {
            XmlNode el = Robot.FirstChild;
            String name = el.InnerText;
            el = el.NextSibling;
            String description = el.InnerText;
            el = el.NextSibling;
            String entityName = el.InnerText;

            return new RobotInfo(name, description, entityName);
        }

        private List<RobotInfo> ProcessRobotInfo(XmlDocument input, ContentProcessorContext context)
        {
            XmlElement documentRoot = input.DocumentElement;

            XmlNodeList RobotNodes = documentRoot.GetElementsByTagName("Robot");

            List<RobotInfo> Robots = new List<RobotInfo>(RobotNodes.Count);

            foreach(XmlElement RobotEl in RobotNodes)
            {
                Robots.Add(ProcessRobot(RobotEl));
            }

            return Robots;
        }

        public override List<RobotInfo> Process(XmlDocument input, ContentProcessorContext context)
        {
            return ProcessRobotInfo(input, context);
        }

    }
}
