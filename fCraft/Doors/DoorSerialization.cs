using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using fCraft.MapConversion;

namespace fCraft.Doors
{
    public class DoorSerialization : IConverterExtension
    {
        private static readonly object SaveLoadLock = new object();
        private static List<string> _group = new List<string> {"door"};

        public IEnumerable<string> AcceptedGroups
        {
            get { return _group; }
        }

        public int Serialize(Map map, Stream stream, IMapConverterEx converter)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            int count = 0;
            if (map.Doors != null)
            {
                if (map.Doors.Count >= 1)
                {
                    foreach (Door Door in map.Doors)
                    {
                        converter.WriteMetadataEntry(_group[0], Door.Name, Door.Serialize(), writer);
                        ++count;
                    }
                }
            }
            return count;
        }

        public void Deserialize(string group, string key, string value, Map map)
        {
            try
            {
                Door Door = Door.Deserialize(key, value, map);
                if (map.Doors == null) map.Doors = new ArrayList();
                if (map.Doors.Count >= 1)
                {
                    if (map.Doors.Contains(key))
                    {
                        Logger.Log(LogType.Error, "Map loading warning: duplicate Door name found: " + key + ", ignored");
                        return;
                    }
                }
                map.Doors.Add(Door);
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, "Door.Deserialize: Error deserializing Door {0}: {1}", key, ex);
            }
        }
    }
}