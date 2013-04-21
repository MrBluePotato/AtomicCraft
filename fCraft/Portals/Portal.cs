﻿//Copyright (C) <2011 - 2013>  <Jon Baker, Glenn Mariën and Lao Tszy>

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

//Copyright (C) <2011 - 2013> Glenn Mariën (http://project-vanilla.com)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft.Drawing;
using System.Threading;
using System.Runtime.Serialization;

namespace fCraft.Portals
{
    public class Portal
    {
        public String Name { get; set; }
        public String Creator { get; set; }
        public DateTime Created { get; set; }
        public String World { get; set; }
        public Vector3I[] AffectedBlocks { get; set; }
        public PortalRange Range { get; set; }
        public String Place { get; set; }
        public int DesiredOutputX { get; set; }
        public int DesiredOutputY { get; set; }
        public int DesiredOutputZ { get; set; }
        public byte DesiredOutputR { get; set; }
        public byte DesiredOutputL { get; set; }
        public bool HasDesiredOutput = false;

        public Portal()
        {
            //empty
        }

        public Portal(String world, Vector3I[] affectedBlocks, String Name, String Creator, String Place, bool CustomOutput)
        {
            this.World = world;
            this.AffectedBlocks = affectedBlocks;
            this.Range = Portal.CalculateRange(this);
            this.Name = Name;
            this.Creator = Creator;
            this.Created = DateTime.UtcNow;
            this.Place = Place;
            this.HasDesiredOutput = CustomOutput;
        }

        public static PortalRange CalculateRange(Portal portal)
        {
            PortalRange range = new PortalRange(0, 0, 0, 0, 0, 0);

            foreach (Vector3I block in portal.AffectedBlocks)
            {
                if (range.Xmin == 0)
                {
                    range.Xmin = block.X;
                }
                else
                {
                    if (block.X < range.Xmin)
                    {
                        range.Xmin = block.X;
                    }
                }

                if (range.Xmax == 0)
                {
                    range.Xmax = block.X;
                }
                else
                {
                    if (block.X > range.Xmax)
                    {
                        range.Xmax = block.X;
                    }
                }

                if (range.Ymin == 0)
                {
                    range.Ymin = block.Y;
                }
                else
                {
                    if (block.Y < range.Ymin)
                    {
                        range.Ymin = block.Y;
                    }
                }

                if (range.Ymax == 0)
                {
                    range.Ymax = block.Y;
                }
                else
                {
                    if (block.Y > range.Ymax)
                    {
                        range.Ymax = block.Y;
                    }
                }

                if (range.Zmin == 0)
                {
                    range.Zmin = block.Z;
                }
                else
                {
                    if (block.Z < range.Zmin)
                    {
                        range.Zmin = block.Z;
                    }
                }

                if (range.Zmax == 0)
                {
                    range.Zmax = block.Z;
                }
                else
                {
                    if (block.Z > range.Zmax)
                    {
                        range.Zmax = block.Z;
                    }
                }
            }

            return range;
        }

        public bool IsInRange(Player player)
        {
            if ((player.Position.X / 32) <= Range.Xmax && (player.Position.X / 32) >= Range.Xmin)
            {
                if ((player.Position.Y / 32) <= Range.Ymax && (player.Position.Y / 32) >= Range.Ymin)
                {
                    if (((player.Position.Z / 32) - 1) <= Range.Zmax && ((player.Position.Z / 32) - 1) >= Range.Zmin)
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

        public static String GenerateName(string World, bool Custom)
        {
            World world = WorldManager.FindWorldExact(World);
            if (Custom)
            {
                if (!world.IsLoaded)
                {
                    world.LoadMap();
                }
            }
            if (world.Map.Portals != null)
            {
                if (world.Map.Portals.Count > 0)
                {
                    bool found = false;

                    while (!found)
                    {
                        bool taken = false;

                        foreach (Portal portal in world.Map.Portals)
                        {
                            if (portal.Name.Equals("portal" + world.Map.portalID))
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
                            world.Map.portalID++;
                        }
                    }
                    if (Custom)
                    {
                        if (world.IsLoaded)
                        {
                            world.UnloadMap(true);
                        }
                    }
                    return "portal" + world.Map.portalID;
                }
            }
            if (world.IsLoaded)
            {
                world.UnloadMap(true);
            }
            return "portal1";
        }

        public static bool DoesNameExist(World world, String name)
        {
            if (world.Map.Portals != null)
            {
                if (world.Map.Portals.Count > 0)
                {
                    foreach (Portal portal in world.Map.Portals)
                    {
                        if (portal.Name.Equals(name))
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
            NormalBrush brush = new NormalBrush(Block.Air, Block.Air);
            DrawOperation removeOperation = new CuboidDrawOperation(requester);
            removeOperation.AnnounceCompletion = false;
            removeOperation.Brush = brush;
            removeOperation.Context = BlockChangeContext.Portal;

            if (this.AffectedBlocks == null)
            {
                this.AffectedBlocks = new Vector3I[2];
                this.AffectedBlocks[0] = new Vector3I(Range.Xmin, Range.Ymin, Range.Zmin);
                this.AffectedBlocks[1] = new Vector3I(Range.Xmax, Range.Ymax, Range.Zmax);
            }

            if (!removeOperation.Prepare(this.AffectedBlocks))
            {
                throw new PortalException("Unable to remove portal.");
            }

            removeOperation.Begin();

            lock (requester.World.Map.Portals.SyncRoot)
            {
                requester.World.Map.Portals.Remove(this);
            }
        }

        public string Serialize()
        {
            SerializedData data = new SerializedData(this);
            DataContractSerializer serializer = new DataContractSerializer(typeof(SerializedData));
            System.IO.MemoryStream s = new System.IO.MemoryStream();
            serializer.WriteObject(s, data);
            return Convert.ToBase64String(s.ToArray());
        }

        public static Portal Deserialize(string name, string sdata, Map map)
        {
            byte[] bdata = Convert.FromBase64String(sdata);
            Portal portal = new Portal();
            DataContractSerializer serializer = new DataContractSerializer(typeof(SerializedData));
            System.IO.MemoryStream s = new System.IO.MemoryStream(bdata);
            SerializedData data = (SerializedData)serializer.ReadObject(s);

            data.UpdatePortal(portal);
            return portal;
        }
        [DataContract]
        private class SerializedData
        {
            [DataMember]
            public String Name;
            [DataMember]
            public String Creator;
            [DataMember]
            public DateTime Created;
            [DataMember]
            public String World;
            [DataMember]
            public int XMin;
            [DataMember]
            public int XMax;
            [DataMember]
            public int YMin;
            [DataMember]
            public int YMax;
            [DataMember]
            public int ZMin;
            [DataMember]
            public int ZMax;
            [DataMember]
            public String Place;
            [DataMember]
            public int DesiredOutputX;
            [DataMember]
            public int DesiredOutputY;
            [DataMember]
            public int DesiredOutputZ;
            [DataMember]
            public byte DesiredOutputR;
            [DataMember]
            public byte DesiredOutputL;
            [DataMember]
            public bool HasDesiredOutput;

            public SerializedData(Portal portal)
            {
                lock (portal)
                {
                    Name = portal.Name;
                    Creator = portal.Creator;
                    Created = portal.Created;
                    World = portal.World;
                    XMin = portal.Range.Xmin;
                    XMax = portal.Range.Xmax;
                    YMin = portal.Range.Ymin;
                    YMax = portal.Range.Ymax;
                    ZMin = portal.Range.Zmin;
                    ZMax = portal.Range.Zmax;
                    Place = portal.Place;
                    DesiredOutputX = portal.DesiredOutputX;
                    DesiredOutputY = portal.DesiredOutputY;
                    DesiredOutputZ = portal.DesiredOutputZ;
                    DesiredOutputR = portal.DesiredOutputR;
                    DesiredOutputL = portal.DesiredOutputL;
                    HasDesiredOutput = portal.HasDesiredOutput;
                }
            }

            public void UpdatePortal(Portal portal)
            {
                portal.Name = Name;
                portal.Creator = Creator;
                portal.Created = Created;
                portal.World = World;
                portal.Range = new PortalRange(XMin, XMax, YMin, YMax, ZMin, ZMax);
                portal.Place = Place;
                portal.DesiredOutputX = DesiredOutputX;
                portal.DesiredOutputY = DesiredOutputY;
                portal.DesiredOutputZ = DesiredOutputZ;
                portal.DesiredOutputR = DesiredOutputR;
                portal.DesiredOutputL = DesiredOutputL;
                portal.HasDesiredOutput = HasDesiredOutput;
            }
        }
    }
}