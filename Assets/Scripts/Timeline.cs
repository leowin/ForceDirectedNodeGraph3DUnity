using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Scripts
{
    [XmlRoot("Timeline")]
    public class Timeline
    {
        [XmlArray("actions")]
        [XmlArrayItem("node", typeof(CreateNode))]
        [XmlArrayItem("delNode", typeof(DeleteNode))]
        [XmlArrayItem("link", typeof(CreateLink))]
        [XmlArrayItem("rename", typeof(RenameAction))]
        public List<TimelineAction> actions = new List<TimelineAction>();

        public int currentPosition = 0;
    }

    public class DeleteNode : TimelineAction
    {
        [XmlAttribute]
        public string nodeId;
    }
    public class CreateNode : TimelineAction
    {
        [XmlAttribute]
        public string nodeId;
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public string type;
        [XmlAttribute]
        public float x;
        [XmlAttribute]
        public float y;
        [XmlAttribute]
        public float z;
    }
    public class CreateLink : TimelineAction
    {
        [XmlAttribute]
        public String SourceId;
        [XmlAttribute]
        public String TargetId;
        [XmlAttribute]
        public String LinkId;

    }
    public class RenameAction : TimelineAction
    {
        [XmlAttribute]
        public string nodeId;
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public string oldName;
    }
    public class TimelineAction
    {
        [XmlAttribute]
        public long time;
    }

    public class TimeLineIO
    {

        public static Timeline Load(string sourceFile)
        {
            if (sourceFile.Length != 0)
            {
                var serializer = new XmlSerializer(typeof(Timeline));
                var stream = new FileStream(sourceFile, FileMode.Open);
                var timeline = serializer.Deserialize(stream) as Timeline;
                stream.Close();
                return timeline;
            }
            return null;
        }

        public static void Save(Timeline t, string outputFile)
        {
            if (outputFile.Length != 0)
            {
                var serializer = new XmlSerializer(typeof(Timeline));
                var stream = new FileStream(outputFile, FileMode.Create);
                serializer.Serialize(stream, t);
                stream.Close();
            }
        }
    }
}
