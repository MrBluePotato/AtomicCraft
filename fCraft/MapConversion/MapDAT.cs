// Copyright 2009-2012 Matvei Stefarov <me@matvei.org>

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using JetBrains.Annotations;

namespace fCraft.MapConversion
{
    public sealed class MapDat : IMapConverter
    {
        private static readonly byte[] Mapping = new byte[256];

        static MapDat()
        {
            Mapping[1] = (byte)Block.Stone;
            Mapping[2] = (byte)Block.Grass;
            Mapping[3] = (byte)Block.Dirt;
            Mapping[4] = (byte)Block.Cobblestone;
            Mapping[5] = (byte)Block.Plank;
            Mapping[6] = (byte)Block.Sapling;
            Mapping[7] = (byte)Block.Bedrock;
            Mapping[8] = (byte)Block.Water;
            Mapping[9] = (byte)Block.StillWater;
            Mapping[10] = (byte)Block.Lava;
            Mapping[11] = (byte)Block.StillLava;
            Mapping[12] = (byte)Block.Sand;
            Mapping[13] = (byte)Block.Gravel;
            Mapping[14] = (byte)Block.GoldOre;
            Mapping[15] = (byte)Block.IronOre;
            Mapping[16] = (byte)Block.Coal;
            Mapping[17] = (byte)Block.Log;
            Mapping[18] = (byte)Block.Leaves;
            Mapping[19] = (byte)Block.Sponge;
            Mapping[20] = (byte)Block.Glass;
            Mapping[21] = (byte)Block.GoldOre;
            Mapping[22] = (byte)Block.Gold;
            Mapping[23] = (byte)Block.Cobblestone;
            Mapping[24] = (byte)Block.Sandstone; // sandstone
            Mapping[25] = (byte)Block.Plank; // Noteblock
            Mapping[27] = (byte)Block.Air; //Powered rail
            Mapping[28] = (byte)Block.Air; // Rail detector
            Mapping[29] = (byte)Block.Plank; //Sticky piston
            Mapping[30] = (byte)Block.Rope; //Spider web
            Mapping[31] = (byte)Block.Sapling; // Grass thing
            Mapping[32] = (byte)Block.Sapling; // Dead shrub
            Mapping[33] = (byte)Block.Plank; // Piston
            Mapping[34] = (byte)Block.Air; // Piston head
            Mapping[35] = (byte)Block.WhiteWool; // Wool
            Mapping[36] = (byte)Block.Air;
            Mapping[37] = (byte)Block.YellowFlower;
            Mapping[38] = (byte)Block.RedFlower;
            Mapping[39] = (byte)Block.BrownMushroom;
            Mapping[40] = (byte)Block.RedFlower;
            Mapping[41] = (byte)Block.Gold;
            Mapping[42] = (byte)Block.Iron;
            Mapping[43] = (byte)Block.DoubleSlab;
            Mapping[44] = (byte)Block.Slab;
            Mapping[45] = (byte)Block.Brick;
            Mapping[46] = (byte)Block.TNT;
            Mapping[47] = (byte)Block.Books;
            Mapping[48] = (byte)Block.MossyCobblestone;
            Mapping[49] = (byte)Block.Obsidian;
            Mapping[50] = (byte) Block.Air; // torch
            Mapping[51] = (byte) Block.Lava; // fire
            Mapping[52] = (byte) Block.Glass; // spawner
            Mapping[53] = (byte) Block.Slab; // wood stairs
            Mapping[54] = (byte) Block.Crate; // chest
            Mapping[55] = (byte) Block.Air; // redstone wire
            Mapping[56] = (byte) Block.IronOre; // diamond ore
            Mapping[57] = (byte) Block.Iron; // diamond block
            Mapping[58] = (byte) Block.Crate; // workbench
            Mapping[59] = (byte) Block.Leaves; // crops
            Mapping[60] = (byte) Block.Dirt; // soil
            Mapping[61] = (byte) Block.Cobblestone; // furnace
            Mapping[62] = (byte) Block.Cobblestone; // burning furnance
            Mapping[63] = (byte) Block.Air; // sign post
            Mapping[64] = (byte) Block.Air; // wooden door
            Mapping[65] = (byte) Block.Rope; // ladder
            Mapping[66] = (byte) Block.Air; // rails
            Mapping[67] = (byte) Block.CobblestoneSlab; // cobblestone stairs
            Mapping[68] = (byte) Block.Air; // wall sign
            Mapping[69] = (byte) Block.Air; // lever
            Mapping[70] = (byte) Block.Air; // pressure plate
            Mapping[71] = (byte) Block.Air; // iron door
            Mapping[72] = (byte) Block.Air; // wooden pressure plate
            Mapping[73] = (byte) Block.IronOre; // redstone ore
            Mapping[74] = (byte) Block.IronOre; // glowing redstone ore
            Mapping[75] = (byte) Block.Air; // redstone torch (off)
            Mapping[76] = (byte) Block.Air; // redstone torch (on)
            Mapping[77] = (byte) Block.Air; // stone button
            Mapping[78] = (byte) Block.Snow; // snow
            Mapping[79] = (byte) Block.Ice; // ice
            Mapping[80] = (byte) Block.WhiteWool; // snow block
            Mapping[81] = (byte) Block.Leaves; // cactus
            Mapping[82] = (byte) Block.GrayWool; // clay
            Mapping[83] = (byte) Block.Leaves; // reed
            Mapping[84] = (byte) Block.Log; // jukebox
            Mapping[85] = (byte) Block.Plank; // fence
            Mapping[86] = (byte) Block.OrangeWool; // pumpkin
            Mapping[87] = (byte) Block.Stone; // netherstone
            Mapping[88] = (byte) Block.Sand; // soul sand
            Mapping[89] = (byte) Block.Glass; // glowstone
            Mapping[90] = (byte) Block.PurpleWool; // portal
            Mapping[91] = (byte) Block.OrangeWool; // jack-o-lantern
            Mapping[92] = (byte)Block.BrownWool; //Cake
            Mapping[93] = (byte)Block.Air; //redstone repeater
            Mapping[94] = (byte)Block.Air; // redstone repeater
            Mapping[95] = (byte)Block.Glass; //stained glass
            Mapping[96] = (byte)Block.Air; //trapdoor
            Mapping[97] = (byte)Block.Stone; //Stone monsteregg
            Mapping[98] = (byte)Block.StoneBrick; //Stone brick
            Mapping[99] = (byte)Block.BrownMushroom; //Brown mushroom block
            Mapping[100] = (byte)Block.RedMushroom; //Red mushroom block
            Mapping[101] = (byte)Block.Glass; //iron bars
            Mapping[102] = (byte)Block.Glass; //glass pane
            Mapping[103] = (byte)Block.Leaves; //Melon block
            Mapping[104] = (byte)Block.Leaves; //Pumpkin vine
            Mapping[105] = (byte)Block.Leaves; //Melon vine
            Mapping[106] = (byte)Block.Leaves; //Vines
            Mapping[107] = (byte)Block.Air; //Fence gate
            Mapping[108] = (byte)Block.Slab; //Brick stairs
            Mapping[109] = (byte)Block.Slab; // Stonebrick stairs
            Mapping[110] = (byte)Block.Dirt; //Mycelium
            Mapping[111] = (byte)Block.Leaves; //Lily pad
            Mapping[112] = (byte)Block.Brick; //Nether brick
            Mapping[113] = (byte)Block.Plank; //Nether brick bence
            Mapping[114] = (byte)Block.Slab; // nether stairs
            Mapping[115] = (byte)Block.RedFlower; //nether wart
            Mapping[116] = (byte)Block.Obsidian; //Enchantment table
            Mapping[117] = (byte)Block.Air;
            Mapping[118] = (byte)Block.Iron;
            Mapping[119] = (byte)Block.Air;
            Mapping[120] = (byte)Block.Air;
            Mapping[121] = (byte)Block.Sand;
            Mapping[122] = (byte)Block.Air;
            Mapping[123] = (byte)Block.Air;
            Mapping[124] = (byte)Block.Air;
            Mapping[125] = (byte)Block.Plank;
            Mapping[126] = (byte)Block.Slab;
            Mapping[127] = (byte)Block.Leaves;
            Mapping[128] = (byte)Block.Slab;
            Mapping[129] = (byte)Block.GoldOre;
            Mapping[130] = (byte)Block.Crate;
            Mapping[131] = (byte)Block.Air;
            Mapping[132] = (byte)Block.Air;
            Mapping[133] = (byte)Block.Gold;
            Mapping[134] = (byte)Block.Slab;
            Mapping[135] = (byte)Block.Slab;
            Mapping[136] = (byte)Block.Slab;
            Mapping[137] = (byte)Block.Plank;
            Mapping[138] = (byte)Block.Glass;
            Mapping[139] = (byte)Block.Cobblestone;
            Mapping[140] = (byte)Block.Brick;
            Mapping[141] = (byte)Block.Leaves;
            Mapping[142] = (byte)Block.Leaves;
            Mapping[143] = (byte)Block.Air;
            Mapping[144] = (byte)Block.Air;
            Mapping[145] = (byte)Block.Air;
            Mapping[146] = (byte)Block.Crate;
            Mapping[147] = (byte)Block.Air;
            Mapping[148] = (byte)Block.Air;
            Mapping[149] = (byte)Block.Air;
            Mapping[150] = (byte)Block.Air;
            Mapping[151] = (byte)Block.Air;
            Mapping[152] = (byte)Block.Iron;
            Mapping[153] = (byte)Block.GoldOre;
            Mapping[154] = (byte)Block.Iron;
            Mapping[155] = (byte)Block.Pillar;
            Mapping[156] = (byte)Block.Slab;
            Mapping[157] = (byte)Block.Air;
            Mapping[158] = (byte)Block.Cobblestone;
            Mapping[159] = (byte)Block.WhiteWool;
            Mapping[160] = (byte)Block.Glass;
            Mapping[162] = (byte)Block.Log;
            Mapping[163] = (byte)Block.Slab;
            Mapping[164] = (byte)Block.Slab;
            Mapping[165] = (byte)Block.GreenWool;
            Mapping[166] = (byte)Block.Air;
            Mapping[167] = (byte)Block.Air;
            Mapping[170] = (byte)Block.YellowWool;
            Mapping[171] = (byte)Block.Air;
            Mapping[172] = (byte)Block.Brick;
            Mapping[173] = (byte)Block.BlackWool;
            Mapping[174] = (byte)Block.Ice;


            // all others default to 0/air
        }


        public string ServerName
        {
            get { return "Creative/Vanilla"; }
        }


        public MapStorageType StorageType
        {
            get { return MapStorageType.SingleFile; }
        }


        public MapFormat Format
        {
            get { return MapFormat.Creative; }
        }


        public bool ClaimsName([NotNull] string fileName)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            return fileName.EndsWith(".dat", StringComparison.OrdinalIgnoreCase) ||
                   fileName.EndsWith(".mine", StringComparison.OrdinalIgnoreCase);
        }


        public bool Claims([NotNull] string fileName)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            try
            {
                using (FileStream mapStream = File.OpenRead(fileName))
                {
                    byte[] temp = new byte[8];
                    mapStream.Seek(-4, SeekOrigin.End);
                    mapStream.Read(temp, 0, 4);
                    mapStream.Seek(0, SeekOrigin.Begin);
                    int length = BitConverter.ToInt32(temp, 0);
                    byte[] data = new byte[length];
                    using (GZipStream reader = new GZipStream(mapStream, CompressionMode.Decompress, true))
                    {
                        reader.Read(data, 0, length);
                    }

                    for (int i = 0; i < length - 1; i++)
                    {
                        if (data[i] == 0xAC && data[i + 1] == 0xED)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }


        public Map LoadHeader([NotNull] string fileName)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            Map map = Load(fileName);
            map.Blocks = null;
            return map;
        }


        public Map Load([NotNull] string fileName)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            using (FileStream mapStream = File.OpenRead(fileName))
            {
                byte[] temp = new byte[8];
                Map map = null;

                mapStream.Seek(-4, SeekOrigin.End);
                mapStream.Read(temp, 0, 4);
                mapStream.Seek(0, SeekOrigin.Begin);
                int uncompressedLength = BitConverter.ToInt32(temp, 0);
                byte[] data = new byte[uncompressedLength];
                using (GZipStream reader = new GZipStream(mapStream, CompressionMode.Decompress, true))
                {
                    reader.Read(data, 0, uncompressedLength);
                }

                for (int i = 0; i < uncompressedLength - 1; i++)
                {
                    if (data[i] != 0xAC || data[i + 1] != 0xED) continue;

                    // bypassing the header crap
                    int pointer = i + 6;
                    Array.Copy(data, pointer, temp, 0, 2);
                    pointer += IPAddress.HostToNetworkOrder(BitConverter.ToInt16(temp, 0));
                    pointer += 13;

                    int headerEnd;
                    // find the end of serialization listing
                    for (headerEnd = pointer; headerEnd < data.Length - 1; headerEnd++)
                    {
                        if (data[headerEnd] == 0x78 && data[headerEnd + 1] == 0x70)
                        {
                            headerEnd += 2;
                            break;
                        }
                    }

                    // start parsing serialization listing
                    int offset = 0;
                    int width = 0, length = 0, height = 0;
                    Position spawn = new Position();
                    while (pointer < headerEnd)
                    {
                        switch ((char) data[pointer])
                        {
                            case 'Z':
                                offset++;
                                break;
                            case 'F':
                            case 'I':
                                offset += 4;
                                break;
                            case 'J':
                                offset += 8;
                                break;
                        }

                        pointer += 1;
                        Array.Copy(data, pointer, temp, 0, 2);
                        short skip = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(temp, 0));
                        pointer += 2;

                        // look for relevant variables
                        Array.Copy(data, headerEnd + offset - 4, temp, 0, 4);
                        if (MemCmp(data, pointer, "width"))
                        {
                            width = (ushort) IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0));
                        }
                        else if (MemCmp(data, pointer, "depth"))
                        {
                            height = (ushort) IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0));
                        }
                        else if (MemCmp(data, pointer, "height"))
                        {
                            length = (ushort) IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0));
                        }
                        else if (MemCmp(data, pointer, "xSpawn"))
                        {
                            spawn.X = (short) (IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0))*32 + 16);
                        }
                        else if (MemCmp(data, pointer, "ySpawn"))
                        {
                            spawn.Z = (short) (IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0))*32 + 16);
                        }
                        else if (MemCmp(data, pointer, "zSpawn"))
                        {
                            spawn.Y = (short) (IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0))*32 + 16);
                        }

                        pointer += skip;
                    }

                    map = new Map(null, width, length, height, false) {Spawn = spawn};

                    if (!map.ValidateHeader())
                    {
                        throw new MapFormatException("One or more of the map dimensions are invalid.");
                    }

                    // find the start of the block array
                    bool foundBlockArray = false;
                    offset = Array.IndexOf<byte>(data, 0x00, headerEnd);
                    while (offset != -1 && offset < data.Length - 2)
                    {
                        if (data[offset] == 0x00 && data[offset + 1] == 0x78 && data[offset + 2] == 0x70)
                        {
                            foundBlockArray = true;
                            pointer = offset + 7;
                        }
                        offset = Array.IndexOf<byte>(data, 0x00, offset + 1);
                    }

                    // copy the block array... or fail
                    if (foundBlockArray)
                    {
                        map.Blocks = new byte[map.Volume];
                        Array.Copy(data, pointer, map.Blocks, 0, map.Blocks.Length);
                        map.ConvertBlockTypes(Mapping);
                    }
                    else
                    {
                        throw new MapFormatException("Could not locate block array.");
                    }
                    break;
                }
                return map;
            }
        }


        public bool Save([NotNull] Map mapToSave, [NotNull] string fileName)
        {
            if (mapToSave == null) throw new ArgumentNullException("mapToSave");
            if (fileName == null) throw new ArgumentNullException("fileName");
            throw new NotImplementedException();
        }

        public static byte MapBlock(byte block)
        {
            return Mapping[block];
        }

        public static Block MapBlock(Block block)
        {
            return (Block) Mapping[(byte) block];
        }


        private static bool MemCmp([NotNull] IList<byte> data, int offset, [NotNull] string value)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (value == null) throw new ArgumentNullException("value");
            // ReSharper disable LoopCanBeConvertedToQuery
            for (int i = 0; i < value.Length; i++)
            {
                if (offset + i >= data.Count || data[offset + i] != value[i]) return false;
            }
            // ReSharper restore LoopCanBeConvertedToQuery
            return true;
        }
    }
}