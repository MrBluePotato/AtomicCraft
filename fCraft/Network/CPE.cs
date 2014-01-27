// Part of FemtoCraft  Copyright 2012-2014 Matvei Stefarov <me@matvei.org>

// Modifications Copyright (c) 2013 Michael Cummings <michael.cummings.97@outlook.com>
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright
//      notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright
//      notice, this list of conditions and the following disclaimer in the
//      documentation and/or other materials provided with the distribution.
//    * Neither the name of 800Craft or the names of its
//      contributors may be used to endorse or promote products derived from this
//      software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace fCraft
{
    public sealed partial class Player
    {
        const string CustomBlocksExtName = "CustomBlocks";
        const int CustomBlocksExtVersion = 1;
        const string BlockPermissionsExtName = "BlockPermissions";
        const int BlockPermissionsExtVersion = 1;
        const string ClickDistanceExtName = "ClickDistance";
        const int ClickDistanceExtVersion = 1;
        const string HeldBlockExtName = "HeldBlock";
        const int HeldBlockExtVersion = 1;
        const string EmoteFixExtName = "EmoteFix";
        const int EmoteFixExtVersion = 1;
        const string TextHotKeyExtName = "TextHotKey";
        const int TextHotKeyExtVersion = 1;
        const string ExtPlayerListName = "ExtPlayerList";
        const int ExtPlayerListVersion = 1;
        const string EnvColorsExtName = "EnvColors";
        const int EnvColorsExtVersion = 1;
        const string SelectionCuboidExtName = "SelectionCuboid";
        const int SelectionCuboidExtVersion = 1;
        const string ChangeModelExtName = "ChangeModel";
        const int ChangeModelExtVersion = 1;
        const string EnvMapAppearanceExtName = "EnvMapAppearance";
        const int EnvMapAppearanceExtVersion = 1;
        const string EnvWeatherTypeExtName = "EnvWeatherType";
        const int EnvWeatherTypeExtVersion = 1;
        const string HackControlExtName = "HackControl";
        const int HackControlExtVersion = 1;
        const byte CustomBlocksLevel = 1;

        // Note: if more levels are added, change UsesCustomBlocks from bool to int
        bool UsesCustomBlocks { get; set; }
        public bool SupportsBlockPermissions { get; set; }
        public bool SupportsClickDistance { get; set; }
        public bool SupportsHeldBlock { get; set; }
        public bool SupportsEmoteFix { get; set; }
        public bool SupportsTextHotKey { get; set; }
        public bool SupportsExtPlayerList { get; set; }
        public bool SupportsEnvColors { get; set; }
        public bool SupportsSelectionCuboid { get; set; }
        public bool SupportsChangeModel { get; set; }
        public bool SupportsEnvMapAppearance { get; set; }
        public bool SupportsEnvWeatherType { get; set; }
        public bool SupportsHackControl { get; set; }
        string ClientName { get; set; }

        bool NegotiateProtocolExtension()
        {
            // write our ExtInfo and ExtEntry packets
            this.reader = new PacketReader(this.stream);
            writer.Write(Packet.MakeExtInfo(2).Data);
            writer.Write(Packet.MakeExtEntry(CustomBlocksExtName, CustomBlocksExtVersion).Data);
            writer.Write(Packet.MakeExtEntry(BlockPermissionsExtName, BlockPermissionsExtVersion).Data);
            writer.Write(Packet.MakeExtEntry(ClickDistanceExtName, ClickDistanceExtVersion).Data);
            writer.Write(Packet.MakeExtEntry(HeldBlockExtName, HeldBlockExtVersion).Data);
            writer.Write(Packet.MakeExtEntry(EmoteFixExtName, EmoteFixExtVersion).Data);
            writer.Write(Packet.MakeExtEntry(TextHotKeyExtName, TextHotKeyExtVersion).Data);
            writer.Write(Packet.MakeExtEntry(EnvColorsExtName, EnvColorsExtVersion).Data);
            writer.Write(Packet.MakeExtEntry(SelectionCuboidExtName, SelectionCuboidExtVersion).Data);
            writer.Write(Packet.MakeExtEntry(ChangeModelExtName, ChangeModelExtVersion).Data);
            writer.Write(Packet.MakeExtEntry(EnvMapAppearanceExtName, EnvMapAppearanceExtVersion).Data);
            writer.Write(Packet.MakeExtEntry(EnvWeatherTypeExtName, EnvWeatherTypeExtVersion).Data);
            writer.Write(Packet.MakeExtEntry(HackControlExtName, HackControlExtVersion).Data);

            // Expect ExtInfo reply from the client
            OpCode extInfoReply = (OpCode)reader.ReadByte();
            //Logger.Log( "Expected: {0} / Received: {1}", OpCode.ExtInfo, extInfoReply );
            if (extInfoReply != OpCode.ExtInfo)
            {
                Logger.Log(LogType.Warning, "Player {0} from {1}: Unexpected ExtInfo reply ({2})", Name, IP, extInfoReply);
                return false;
            }
            ClientName = reader.ReadString();
            int expectedEntries = reader.ReadInt16();

            // wait for client to send its ExtEntries
            bool sendCustomBlockPacket = false;
            List<string> clientExts = new List<string>();
            for (int i = 0; i < expectedEntries; i++)
            {
                // Expect ExtEntry replies (0 or more)
                OpCode extEntryReply = (OpCode)reader.ReadByte();
                //Logger.Log( "Expected: {0} / Received: {1}", OpCode.ExtEntry, extEntryReply );
                if (extEntryReply != OpCode.ExtEntry)
                {
                    Logger.Log(LogType.Warning, "Player {0} from {1}: Unexpected ExtEntry reply ({2})", Name, IP, extInfoReply);
                    return false;
                }
                string extName = reader.ReadString();
                int extVersion = reader.ReadInt32();
                if (extName == CustomBlocksExtName && extVersion == CustomBlocksExtVersion)
                {
                    // Hooray, client supports custom blocks! We still need to check support level.
                    sendCustomBlockPacket = true;
                    clientExts.Add(extName + " " + extVersion);
                }
                else if (extName == BlockPermissionsExtName && extVersion == BlockPermissionsExtVersion)
                {
                    SupportsBlockPermissions = true;
                    clientExts.Add(extName + " " + extVersion);
                }
            }

            // log client's capabilities
            if (clientExts.Count > 0)
            {
                Logger.Log(LogType.SystemActivity, "Player {0} is using \"{1}\", supporting: {2}",
                            Name,
                            ClientName,
                            clientExts.JoinToString(", "));
            }

            if (sendCustomBlockPacket)
            {
                // if client also supports CustomBlockSupportLevel, figure out what level to use

                // Send CustomBlockSupportLevel
                writer.Write(Packet.MakeCustomBlockSupportLevel(CustomBlocksLevel).Data);

                // Expect CustomBlockSupportLevel reply
                OpCode customBlockSupportLevelReply = (OpCode)reader.ReadByte();
                Logger.Log(LogType.Debug, "Expected: {0} / Received: {1}", OpCode.CustomBlockSupportLevel, customBlockSupportLevelReply);
                if (customBlockSupportLevelReply != OpCode.CustomBlockSupportLevel)
                {
                    Logger.Log(LogType.Warning, "Player {0} from {1}: Unexpected CustomBlockSupportLevel reply ({2})",
                                       Name,
                                       IP,
                                       customBlockSupportLevelReply);
                    return false;
                }
                byte clientLevel = reader.ReadByte();
                UsesCustomBlocks = (clientLevel >= CustomBlocksLevel);
            }
            this.reader = new BinaryReader(this.stream);
            return true;
        }


        // For non-extended players, use appropriate substitution
        void ProcessOutgoingSetBlock(ref Packet packet)
        {
            if (packet.Data[7] > (byte)Map.MaxLegalBlockType && !UsesCustomBlocks)
            {
                packet.Data[7] = (byte)Map.GetFallbackBlock((Block)packet.Data[7]);
            }
        }


        void SendBlockPermissions()
        {
            Send(Packet.MakeSetBlockPermission(Block.Bedrock, Can(Permission.PlaceAdmincrete), Can(Permission.DeleteAdmincrete)));
        }
    }


    partial struct Packet
    {
        [Pure]
        public static Packet MakeExtInfo(short extCount)
        {
            String VersionString = "AtomicCraft " + Updater.LatestStable;
            // Logger.Log( "Send: ExtInfo({0},{1})", Server.VersionString, extCount );
            Packet packet = new Packet(OpCode.ExtInfo);
            Encoding.ASCII.GetBytes(VersionString.PadRight(64), 0, 64, packet.Data, 1);
            ToNetOrder(extCount, packet.Data, 65);
            return packet;
        }

        [Pure]
        public static Packet MakeExtEntry([NotNull] string name, int version)
        {
            if (name == null) throw new ArgumentNullException("name");
            // Logger.Log( "Send: ExtEntry({0},{1})", name, version );
            Packet packet = new Packet(OpCode.ExtEntry);
            Encoding.ASCII.GetBytes(name.PadRight(64), 0, 64, packet.Data, 1);
            ToNetOrder(version, packet.Data, 65);
            return packet;
        }

        [Pure]
        public static Packet MakeCustomBlockSupportLevel(byte level)
        {
            Logger.Log(LogType.Debug, "Send: CustomBlockSupportLevel({0})", level);
            Packet packet = new Packet(OpCode.CustomBlockSupportLevel);
            packet.Data[1] = level;
            return packet;
        }

        [Pure]
        public static Packet MakeSetBlockPermission(Block block, bool canPlace, bool canDelete)
        {
            Packet packet = new Packet(OpCode.SetBlockPermissions);
            packet.Data[1] = (byte)block;
            packet.Data[2] = (byte)(canPlace ? 1 : 0);
            packet.Data[3] = (byte)(canDelete ? 1 : 0);
            return packet;
        }


        [Pure]
        public static Packet MakeSetClickDistance(short distance)
        {
            if (distance < 0) throw new ArgumentOutOfRangeException("distance");
            Packet packet = new Packet(OpCode.SetClickDistance);
            ToNetOrder(distance, packet.Data, 1);
            return packet;
        }

        [Pure]
        public static Packet MakeHoldThis(Block block, bool preventChange)
        {
            Packet packet = new Packet(OpCode.HoldThis);
            packet.Data[1] = (byte)block;
            packet.Data[2] = (byte)(preventChange ? 1 : 0);
            return packet;
        }

        [Pure]
        public static Packet MakeSetTextHotKey([NotNull] string label, [NotNull] string action, int keyCode,
                                               byte keyMods)
        {
            if (label == null) throw new ArgumentNullException("label");
            if (action == null) throw new ArgumentNullException("action");
            Packet packet = new Packet(OpCode.SetTextHotKey);
            Encoding.ASCII.GetBytes(label.PadRight(64), 0, 64, packet.Data, 1);
            Encoding.ASCII.GetBytes(action.PadRight(64), 0, 64, packet.Data, 65);
            ToNetOrder(keyCode, packet.Data, 129);
            packet.Data[133] = keyMods;
            return packet;
        }


        [Pure]
        public static Packet MakeExtAddPlayerName(short nameId, string playerName, string listName, string groupName,
                                                   byte groupRank)
        {
            if (playerName == null) throw new ArgumentNullException("playerName");
            if (listName == null) throw new ArgumentNullException("listName");
            if (groupName == null) throw new ArgumentNullException("groupName");
            Packet packet = new Packet(OpCode.ExtAddPlayerName);
            ToNetOrder(nameId, packet.Data, 1);
            Encoding.ASCII.GetBytes(playerName.PadRight(64), 0, 64, packet.Data, 3);
            Encoding.ASCII.GetBytes(listName.PadRight(64), 0, 64, packet.Data, 67);
            Encoding.ASCII.GetBytes(groupName.PadRight(64), 0, 64, packet.Data, 131);
            packet.Data[195] = groupRank;
            return packet;
        }


        [Pure]
        public static Packet MakeExtAddEntity(byte entityId, [NotNull] string inGameName, [NotNull] string skinName)
        {
            if (inGameName == null) throw new ArgumentNullException("inGameName");
            if (skinName == null) throw new ArgumentNullException("skinName");
            Packet packet = new Packet(OpCode.ExtAddEntity);
            packet.Data[1] = entityId;
            Encoding.ASCII.GetBytes(inGameName.PadRight(64), 0, 64, packet.Data, 2);
            Encoding.ASCII.GetBytes(skinName.PadRight(64), 0, 64, packet.Data, 66);
            return packet;
        }


        [Pure]
        public static Packet MakeExtRemovePlayerName(short nameId)
        {
            Packet packet = new Packet(OpCode.ExtRemovePlayerName);
            ToNetOrder(nameId, packet.Data, 1);
            return packet;
        }


        /*[Pure]
        public static Packet MakeEnvSetColor(EnvVariable variable, int color)
        {
            Packet packet = new Packet(OpCode.EnvSetColor);
            packet.Data[1] = (byte)variable;
            ToNetOrder((short)((color >> 16) & 0xFF), packet.Data, 2);
            ToNetOrder((short)((color >> 8) & 0xFF), packet.Data, 4);
            ToNetOrder((short)(color & 0xFF), packet.Data, 6);
            return packet;
        }*/


        [Pure]
        public static Packet MakeMakeSelection(byte selectionId, [NotNull] string label, [NotNull] BoundingBox bounds,
                                               int color, byte opacity)
        {
            if (label == null) throw new ArgumentNullException("label");
            if (bounds == null) throw new ArgumentNullException("bounds");
            Packet packet = new Packet(OpCode.MakeSelection);
            packet.Data[1] = selectionId;
            Encoding.ASCII.GetBytes(label.PadRight(64), 0, 64, packet.Data, 2);
            ToNetOrder(bounds.XMin, packet.Data, 66);
            ToNetOrder(bounds.ZMin, packet.Data, 68);
            ToNetOrder(bounds.YMin, packet.Data, 70);
            ToNetOrder(bounds.XMax, packet.Data, 72);
            ToNetOrder(bounds.ZMax, packet.Data, 74);
            ToNetOrder(bounds.YMax, packet.Data, 76);
            packet.Data[78] = (byte)((color >> 16) & 0xFF);
            packet.Data[79] = (byte)((color >> 8) & 0xFF);
            packet.Data[81] = (byte)(color & 0xFF);
            packet.Data[82] = opacity;
            return packet;
        }


        [Pure]
        public static Packet MakeRemoveSelection(byte selectionId)
        {
            Packet packet = new Packet(OpCode.RemoveSelection);
            packet.Data[1] = selectionId;
            return packet;
        }

        [Pure]
        public static Packet MakeChangeModel(byte entityId, [NotNull] string modelName)
        {
            if (modelName == null) throw new ArgumentNullException("modelName");
            Packet packet = new Packet(OpCode.ChangeModel);
            packet.Data[1] = entityId;
            Encoding.ASCII.GetBytes(modelName.PadRight(64), 0, 64, packet.Data, 2);
            return packet;
        }

        [Pure]
        public static Packet MakeEnvSetMapAppearance([NotNull] string textureUrl, Block sideBlock, Block edgeBlock,
                                                     short sideLevel)
        {
            if (textureUrl == null) throw new ArgumentNullException("textureUrl");
            Packet packet = new Packet(OpCode.EnvMapAppearance);
            Encoding.ASCII.GetBytes(textureUrl.PadRight(64), 0, 64, packet.Data, 1);
            packet.Data[65] = (byte)sideBlock;
            packet.Data[66] = (byte)edgeBlock;
            ToNetOrder(sideLevel, packet.Data, 67);
            return packet;
        }

        [Pure]
        public static Packet EnvWeatherType(int weatherType)
        {
            Packet packet = new Packet(OpCode.EnvWeatherType);
            packet.Data[1] = (byte)weatherType;
            return packet;
        }

        static void ToNetOrder(short number, [NotNull] byte[] arr, int offset)
        {
            if (arr == null) throw new ArgumentNullException("arr");
            arr[offset] = (byte)((number & 0xff00) >> 8);
            arr[offset + 1] = (byte)(number & 0x00ff);
        }


        static void ToNetOrder(int number, [NotNull] byte[] arr, int offset)
        {
            if (arr == null) throw new ArgumentNullException("arr");
            arr[offset] = (byte)((number & 0xff000000) >> 24);
            arr[offset + 1] = (byte)((number & 0x00ff0000) >> 16);
            arr[offset + 2] = (byte)((number & 0x0000ff00) >> 8);
            arr[offset + 3] = (byte)(number & 0x000000ff);
        }
    }


    sealed partial class Map
    {
        public const Block MaxCustomBlockType = Block.StoneBrick;
        readonly static Block[] FallbackBlocks = new Block[256];


        static void DefineFallbackBlocks()
        {
            for (int i = 0; i <= (int)Block.Obsidian; i++)
            {
                FallbackBlocks[i] = (Block)i;
            }
            FallbackBlocks[(int)Block.CobblestoneSlab] = Block.Slab;
            FallbackBlocks[(int)Block.Rope] = Block.BrownMushroom;
            FallbackBlocks[(int)Block.Sandstone] = Block.Sand;
            FallbackBlocks[(int)Block.Snow] = Block.Air;
            FallbackBlocks[(int)Block.Fire] = Block.StillLava;
            FallbackBlocks[(int)Block.LightPinkWool] = Block.PinkWool;
            FallbackBlocks[(int)Block.ForestGreenWool] = Block.GreenWool;
            FallbackBlocks[(int)Block.BrownWool] = Block.Dirt;
            FallbackBlocks[(int)Block.DeepBlueWool] = Block.BlueWool;
            FallbackBlocks[(int)Block.TurquoiseWool] = Block.CyanWool;
            FallbackBlocks[(int)Block.Ice] = Block.Glass;
            FallbackBlocks[(int)Block.Tile] = Block.Iron;
            FallbackBlocks[(int)Block.Magma] = Block.Obsidian;
            FallbackBlocks[(int)Block.Pillar] = Block.WhiteWool;
            FallbackBlocks[(int)Block.Crate] = Block.Plank;
            FallbackBlocks[(int)Block.StoneBrick] = Block.Stone;
        }


        public static Block GetFallbackBlock(Block block)
        {
            return FallbackBlocks[(int)block];
        }


        public const Block MaxLegalBlockType = Block.Obsidian;
        public unsafe byte[] GetFallbackMap()
        {
            byte[] translatedBlocks = (byte[])Blocks.Clone();
            int volume = translatedBlocks.Length;
            fixed (byte* ptr = translatedBlocks)
            {
                for (int i = 0; i < volume; i++)
                {
                    byte block = ptr[i];
                    if (block > (byte)MaxLegalBlockType)
                    {
                        ptr[i] = (byte)FallbackBlocks[block];
                    }
                }
            }
            return translatedBlocks;
        }
    }
}