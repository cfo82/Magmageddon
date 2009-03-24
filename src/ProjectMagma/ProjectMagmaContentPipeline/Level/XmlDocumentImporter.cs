using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Xml;
using System.IO;

namespace ProjectMagma.ContentPipeline.Xml.Importer
{
    [ContentImporter(".xml", DisplayName="Magma - Level Importer")]
    public class LevelImporter : ContentImporter<XmlDocument>
    {
        public override XmlDocument Import(string filename, ContentImporterContext context)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filename);
            document.DocumentElement.SetAttribute("filename", filename);
            document.DocumentElement.SetAttribute("identity", Path.GetFileNameWithoutExtension(filename));
            return document;
        }
    }
}
