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
        [XmlArrayItem("camera", typeof(MoveCamera))]
        public List<TimelineAction> actions = new List<TimelineAction>();

        [XmlAttribute]
        public int currentPosition = 0;

        [XmlAttribute]
        public int lastId = 0;

        public int NextId()
        {
            return ++lastId;
        }
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

        public SVector3 position;
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
    public class SVector3
    {
        [XmlAttribute]
        public float x;
        [XmlAttribute]
        public float y;
        [XmlAttribute]
        public float z;


        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
        public static SVector3 FromVector3(Vector3 from)
        {
            return new SVector3() { x = from.x, y = from.y, z = from.z };
        }

    }
    public class MoveCamera : TimelineAction
    {
        [XmlAttribute]
        public float duration;

        public SVector3 oldPos;
        public SVector3 newPos;
        public SVector3 oldRot;
        public SVector3 newRot;
    }
    public class TimelineAction
    {
        [XmlAttribute]
        public float time;
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
                using (StreamWriter sw = new StreamWriter(outputFile, false, Encoding.UTF8))
                {
                    serializer.Serialize(sw, t);
                }
            }
        }
    }
}
