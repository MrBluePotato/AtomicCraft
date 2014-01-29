﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft.Drawing;
using System.Threading;
using System.Runtime.Serialization;

namespace fCraft
{
    public class MessageBlock
    {
        public MessageBlock()
        {
            //empty
        }

        public MessageBlock(String world, Vector3I affectedBlock, String Name, String Creator, String Message)
        {
            this.World = world;
            this.AffectedBlock = affectedBlock;
            this.Range = MessageBlock.CalculateRange(this);
            this.Name = Name;
            this.Creator = Creator;
            this.Created = DateTime.UtcNow;
            this.Message = Message;
        }

        public String Name { get; set; }
        public String Creator { get; set; }
        public DateTime Created { get; set; }
        public String World { get; set; }
        public Vector3I AffectedBlock { get; set; }
        public MessageBlockRange Range { get; set; }
        public String Message { get; set; }

        public static MessageBlockRange CalculateRange(MessageBlock MessageBlock)
        {
            MessageBlockRange range = new MessageBlockRange(0, 0, 0, 0, 0, 0);
            range.Xmin = MessageBlock.AffectedBlock.X;
            range.Xmax = MessageBlock.AffectedBlock.X;
            range.Ymin = MessageBlock.AffectedBlock.Y;
            range.Ymax = MessageBlock.AffectedBlock.Y;
            range.Zmin = MessageBlock.AffectedBlock.Z;
            range.Zmax = MessageBlock.AffectedBlock.Z;
            return range;
        }

        public bool IsInRange(Player player)
        {
            if ((player.Position.X/32) <= Range.Xmax + 1 && (player.Position.X/32) >= Range.Xmin - 1)
            {
                if ((player.Position.Y/32) <= Range.Ymax + 1 && (player.Position.Y/32) >= Range.Ymin - 1)
                {
                    if (((player.Position.Z/32) - 1) <= Range.Zmax + 1 && ((player.Position.Z/32) - 1) >= Range.Zmin - 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsInRange(Vector3I vector)
        {
            if (vector.X <= Range.Xmax && vector.X >= Range.Xmin)
            {
                if (vector.Y <= Range.Ymax && vector.Y >= Range.Ymin)
                {
                    if (vector.Z <= Range.Zmax && vector.Z >= Range.Zmin)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public String GetMessage()
        {
            if (this.Message == null) return "";
            if (this.Message.Length < 1) return "";
            string SortedMessage = Color.ReplacePercentCodes(Message);
            SortedMessage = Chat.ReplaceEmoteKeywords(SortedMessage);
            return String.Format("MessageBlock: {0}{1}", Color.Green, SortedMessage);
        }

        public static String GenerateName(World world)
        {
            if (world.Map.MessageBlocks != null)
            {
                if (world.Map.MessageBlocks.Count > 0)
                {
                    bool found = false;

                    while (!found)
                    {
                        bool taken = false;

                        foreach (MessageBlock MessageBlock in world.Map.MessageBlocks)
                        {
                            if (MessageBlock.Name.Equals("MB" + world.Map.MessageBlockID))
                            {
                                taken = true;
                                break;
                            }
                        }

                        if (!taken)
                        {
                            found = true;
                        }
                        else
                        {
                            world.Map.MessageBlockID++;
                        }
                    }

                    return "MB" + world.Map.MessageBlockID;
                }
            }

            return "MB1";
        }

        public static bool DoesNameExist(World world, String name)
        {
            if (world.Map.MessageBlocks != null)
            {
                if (world.Map.MessageBlocks.Count > 0)
                {
                    foreach (MessageBlock MessageBlock in world.Map.MessageBlocks)
                    {
                        if (MessageBlock.Name.Equals(name))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void Remove(Player requester)
        {
            lock (requester.World.Map.MessageBlocks.SyncRoot)
            {
                requester.World.Map.MessageBlocks.Remove(this);
            }
        }

        public string Serialize()
        {
            SerializedData data = new SerializedData(this);
            DataContractSerializer serializer = new DataContractSerializer(typeof (SerializedData));
            System.IO.MemoryStream s = new System.IO.MemoryStream();
            serializer.WriteObject(s, data);
            return Convert.ToBase64String(s.ToArray());
        }

        public static MessageBlock Deserialize(string name, string sdata, Map map)
        {
            byte[] bdata = Convert.FromBase64String(sdata);
            MessageBlock MessageBlock = new MessageBlock();
            DataContractSerializer serializer = new DataContractSerializer(typeof (SerializedData));
            System.IO.MemoryStream s = new System.IO.MemoryStream(bdata);
            SerializedData data = (SerializedData) serializer.ReadObject(s);

            data.UpdateMessageBlock(MessageBlock);
            return MessageBlock;
        }

        [DataContract]
        private class SerializedData
        {
            [DataMember] public int AffectedBlockX;
            [DataMember] public int AffectedBlockY;
            [DataMember] public int AffectedBlockZ;
            [DataMember] public DateTime Created;
            [DataMember] public String Creator;
            [DataMember] public String Message;
            [DataMember] public String Name;
            [DataMember] public String World;
            [DataMember] public int XMax;
            [DataMember] public int XMin;
            [DataMember] public int YMax;
            [DataMember] public int YMin;
            [DataMember] public int ZMax;
            [DataMember] public int ZMin;

            public SerializedData(MessageBlock MessageBlock)
            {
                lock (MessageBlock)
                {
                    Name = MessageBlock.Name;
                    Creator = MessageBlock.Creator;
                    Created = MessageBlock.Created;
                    World = MessageBlock.World;
                    AffectedBlockX = MessageBlock.AffectedBlock.X;
                    AffectedBlockY = MessageBlock.AffectedBlock.Y;
                    AffectedBlockZ = MessageBlock.AffectedBlock.Z;
                    XMin = MessageBlock.Range.Xmin;
                    XMax = MessageBlock.Range.Xmax;
                    YMin = MessageBlock.Range.Ymin;
                    YMax = MessageBlock.Range.Ymax;
                    ZMin = MessageBlock.Range.Zmin;
                    ZMax = MessageBlock.Range.Zmax;
                    Message = MessageBlock.Message;
                }
            }

            public void UpdateMessageBlock(MessageBlock MessageBlock)
            {
                MessageBlock.Name = Name;
                MessageBlock.Creator = Creator;
                MessageBlock.Created = Created;
                MessageBlock.World = World;
                MessageBlock.AffectedBlock = new Vector3I(AffectedBlockX, AffectedBlockY, AffectedBlockZ);
                MessageBlock.Range = new MessageBlockRange(XMin, XMax, YMin, YMax, ZMin, ZMax);
                MessageBlock.Message = Message;
            }
        }
    }
}