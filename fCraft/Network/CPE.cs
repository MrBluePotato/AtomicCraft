﻿// Part of FemtoCraft  Copyright 2012-2014 Matvei Stefarov <me@matvei.org>

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
// Part of FemtoCraft | Copyright 2012-2013 Matvei Stefarov <me@matvei.org> | See LICENSE.txt

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
        private const string CustomBlocksExtName = "CustomBlocks";
        private const int CustomBlocksExtVersion = 1;
        private const string BlockPermissionsExtName = "SetBlockPermissions";
        private const int BlockPermissionsExtVersion = 1;
        private const byte CustomBlocksLevel = 1;
        private const string SelectionBoxExtName = "SelectionBoxExt";
        private const int SelectionBoxExtVersion = 1;
        const string HeldBlockExtName = "HeldBlock";
        const int HeldBlockExtVersion = 1;
        private const string MessageTypesExtName = "MessageTypes";
        private const int MessageTypesExtVersion = 1;
        const string ExtPlayerListName = "ExtPlayerList";
        const int ExtPlayerListVersion = 1;
        private const string PlaySoundExtName = "PlaySound";
        private const int PlaySoundExtVersion = 1;


        // Note: if more levels are added, change UsesCustomBlocks from bool to int
        public bool UsesCustomBlocks { get; set; }
        public bool SupportsBlockPermissions { get; set; }
        public bool SelectionBoxExt { get; set; }
        public bool SupportsHeldBlock { get; set; }
        public bool SupportsMessageTypes { get; set; }
        public bool SupportsPlaySound { get; set; }
        private string ClientName { get; set; }
        public bool SupportsExtPlayerList { get; set; }

        private bool NegotiateProtocolExtension()
        {
            this.reader = new PacketReader(this.stream);
            // write our ExtInfo and ExtEntry packets
            writer.Write(Packet.MakeExtInfo(2).Data);
            writer.Write(Packet.MakeExtEntry(CustomBlocksExtName, CustomBlocksExtVersion).Data);
            writer.Write(Packet.MakeExtEntry(BlockPermissionsExtName, BlockPermissionsExtVersion).Data);
            writer.Write(Packet.MakeExtEntry(HeldBlockExtName, HeldBlockExtVersion).Data);
            writer.Write(Packet.MakeExtEntry(MessageTypesExtName, MessageTypesExtVersion).Data);

            Logger.Log(LogType.SystemActivity, "Sent ExtInfo and entry packets");

            // Expect ExtInfo reply from the client
            OpCode extInfoReply = (OpCode) reader.ReadByte();
            Logger.Log(LogType.Debug, "Expected: {0} / Received: {1}", OpCode.ExtInfo, extInfoReply);
            if (extInfoReply != OpCode.ExtInfo)
            {
                Logger.Log(LogType.Warning, "Player {0} from {1}: Unexpected ExtInfo reply ({2})", Name, IP,
                    extInfoReply);
                return false;
            }
            //read EXT_INFO
            ClientName = reader.ReadString();
            int expectedEntries = reader.ReadInt16();

            // wait for client to send its ExtEntries
            bool sendCustomBlockPacket = false;
            List<string> clientExts = new List<string>();
            for (int i = 0; i < expectedEntries; i++)
            {
                // Expect ExtEntry replies (0 or more)
                OpCode extEntryReply = (OpCode) reader.ReadByte();
                Logger.Log(LogType.Debug, "Expected: {0} / Received: {1}", OpCode.ExtEntry, extEntryReply);
                if (extEntryReply != OpCode.ExtEntry)
                {
                    Logger.Log(LogType.Warning, "Player {0} from {1}: Unexpected ExtEntry reply ({2})", Name, IP,
                        extEntryReply);
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
                else if (extName == SelectionBoxExtName && extVersion == SelectionBoxExtVersion)
                {
                    SelectionBoxExt = true;
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
                OpCode customBlockSupportLevelReply = (OpCode) reader.ReadByte();
                Logger.Log(LogType.Debug, "Expected: {0} / Received: {1}", OpCode.CustomBlockSupportLevel,
                    customBlockSupportLevelReply);
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
        public void ProcessOutgoingSetBlock(ref Packet packet)
        {
            if (packet.Data[7] > (byte) Map.MaxLegalBlockType && !UsesCustomBlocks)
            {
                packet.Data[7] = (byte) Map.GetFallbackBlock((Block) packet.Data[7]);
            }
        }


        public void SendBlockPermissions()
        {
            Send(Packet.MakeSetBlockPermission(Block.Bedrock, Can(Permission.PlaceAdmincrete),
                Can(Permission.DeleteAdmincrete)));
        }
    }


    partial struct Packet
    {
        public static Packet MakeExtInfo(short extCount)
        {
            String VersionString = "AtomicCraft " + Updater.LatestStable;
            Logger.Log(LogType.Debug, "Send: ExtInfo({0},{1})", VersionString, extCount);

            Packet packet = new Packet(OpCode.ExtInfo);
            Encoding.ASCII.GetBytes(VersionString.PadRight(64), 0, 64, packet.Data, 1);
            ToNetOrder(extCount, packet.Data, 65);
            return packet;
        }

        public static Packet MakeExtEntry(string name, int version)
        {
            Logger.Log(LogType.Debug, "Send: ExtEntry({0},{1})", name, version);
            Packet packet = new Packet(OpCode.ExtEntry);
            Encoding.ASCII.GetBytes(name.PadRight(64), 0, 64, packet.Data, 1);
            ToNetOrder(version, packet.Data, 65);
            return packet;
        }

        public static Packet MakeAddSelectionBox(byte id, string label, short startX, short startY, short startZ,
            short endX, short endY, short endZ, short r, short g, short b, short a)
        {
            Logger.Log(LogType.Debug, "Send: MakeAddSelectionBox({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11})",
                id, label, startX, startY, startZ, endX, endY, endZ, r, g, b, a);
            Packet packet = new Packet(OpCode.MakeSelection);
            packet.Data[1] = id;
            Encoding.ASCII.GetBytes(label.PadRight(64), 0, 64, packet.Data, 2);
            ToNetOrder(startX, packet.Data, 66);
            ToNetOrder(startY, packet.Data, 68);
            ToNetOrder(startZ, packet.Data, 70);
            ToNetOrder(endX, packet.Data, 72);
            ToNetOrder(endY, packet.Data, 74);
            ToNetOrder(endZ, packet.Data, 76);
            ToNetOrder(r, packet.Data, 78);
            ToNetOrder(g, packet.Data, 80);
            ToNetOrder(b, packet.Data, 82);
            ToNetOrder(a, packet.Data, 84);
            return packet;
        }

        public static Packet MakeRemoveSelectionBox(byte selectionId)
        {
            Packet packet = new Packet(OpCode.RemoveSelection);
            packet.Data[1] = selectionId;
            return packet;
        }

        public static Packet MakeCustomBlockSupportLevel(byte level)
        {
            Logger.Log(LogType.Debug, "Send: CustomBlockSupportLevel({0})", level);
            Packet packet = new Packet(OpCode.CustomBlockSupportLevel);
            packet.Data[1] = level;
            return packet;
        }

        public static Packet MakeSetBlockPermission(Block block, bool canPlace, bool canDelete)
        {
            Packet packet = new Packet(OpCode.SetBlockPermissions);
            packet.Data[1] = (byte) block;
            packet.Data[2] = (byte) (canPlace ? 1 : 0);
            packet.Data[3] = (byte) (canDelete ? 1 : 0);
            return packet;
        }


        public static Packet MakeChangeModel(byte entityId, [NotNull] string modelName)
        {
            if (modelName == null) throw new ArgumentNullException("modelName");
            Packet packet = new Packet(OpCode.ChangeModel);
            packet.Data[1] = entityId;
            Encoding.ASCII.GetBytes(modelName.PadRight(64), 0, 64, packet.Data, 2);
            return packet;
        }

        public static Packet EnvWeatherType(int weatherType)
        {
            Packet packet = new Packet(OpCode.EnvWeatherType);
            packet.Data[1] = (byte) weatherType;
            return packet;
        }

        public static Packet MakeHoldThis(byte blockToHold, byte preventChange)
        {
            Packet packet = new Packet(OpCode.HoldThis);
            packet.Data[1] = blockToHold;
            packet.Data[2] = preventChange;
            return packet;
        }

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

        public static Packet MakeExtRemovePlayerName(short nameId)
        {
            Packet packet = new Packet(OpCode.ExtRemovePlayerName);
            ToNetOrder(nameId, packet.Data, 1);
            return packet;
        }

        public static Packet MakeMessageType(byte messageType, [NotNull] string message)
         {
             if (message == null) throw new ArgumentNullException("message");
 
             Packet packet = new Packet(OpCode.Message);
             packet.Data[1] = messageType;
             Encoding.ASCII.GetBytes(message.PadRight(64), 0, 64, packet.Data, 2);
             return packet;
         }

        public static Packet PlaySound(string sound, short x, short y, short z, byte volume)
        {
            Packet packet = new Packet(fCraft.OpCode.PlaySound);
            Encoding.ASCII.GetBytes(sound.PadRight(64), 0, 64, packet.Data, 1);
            ToNetOrder(x, packet.Data, 66);
            ToNetOrder(y, packet.Data, 68);
            ToNetOrder(z, packet.Data, 70);
            packet.Data[72] = volume;
            return packet;

        }

        private static void ToNetOrder(short number, [NotNull] byte[] arr, int offset)
        {
            if (arr == null)
                throw new Exception("arr");
            arr[offset] = (byte) ((number & 0xff00) >> 8);
            arr[offset + 1] = (byte) (number & 0x00ff);
        }


        private static void ToNetOrder(int number, [NotNull] byte[] arr, int offset)
        {
            if (arr == null)
                throw new ArgumentNullException("arr");
            arr[offset] = (byte) ((number & 0xff000000) >> 24);
            arr[offset + 1] = (byte) ((number & 0x00ff0000) >> 16);
            arr[offset + 2] = (byte) ((number & 0x0000ff00) >> 8);
            arr[offset + 3] = (byte) (number & 0x000000ff);
        }
    }


    public sealed partial class Map
    {
        public const Block MaxCustomBlockType = Block.StoneBrick;
        public const Block MaxLegalBlockType = Block.Obsidian;
        private static readonly Block[] FallbackBlocks = new Block[256];


        private static void DefineFallbackBlocks()
        {
            for (int i = 0; i <= (int) Block.Obsidian; i++)
            {
                FallbackBlocks[i] = (Block) i;
            }
            FallbackBlocks[(int) Block.CobblestoneSlab] = Block.Slab;
            FallbackBlocks[(int) Block.Rope] = Block.BrownMushroom;
            FallbackBlocks[(int) Block.Sandstone] = Block.Sand;
            FallbackBlocks[(int) Block.Snow] = Block.Air;
            FallbackBlocks[(int) Block.Fire] = Block.StillLava;
            FallbackBlocks[(int) Block.LightPinkWool] = Block.PinkWool;
            FallbackBlocks[(int) Block.ForestGreenWool] = Block.GreenWool;
            FallbackBlocks[(int) Block.BrownWool] = Block.Dirt;
            FallbackBlocks[(int) Block.DeepBlueWool] = Block.BlueWool;
            FallbackBlocks[(int) Block.TurquoiseWool] = Block.CyanWool;
            FallbackBlocks[(int) Block.Ice] = Block.Glass;
            FallbackBlocks[(int) Block.Tile] = Block.Iron;
            FallbackBlocks[(int) Block.Magma] = Block.Obsidian;
            FallbackBlocks[(int) Block.Pillar] = Block.WhiteWool;
            FallbackBlocks[(int) Block.Crate] = Block.Plank;
            FallbackBlocks[(int) Block.StoneBrick] = Block.Stone;
        }


        public static Block GetFallbackBlock(Block block)
        {
            return FallbackBlocks[(int) block];
        }

        public unsafe byte[] GetFallbackMap()
        {
            byte[] translatedBlocks = (byte[]) Blocks.Clone();
            int volume = translatedBlocks.Length;
            fixed (byte* ptr = translatedBlocks)
            {
                for (int i = 0; i < volume; i++)
                {
                    byte block = ptr[i];
                    if (block > (byte) MaxLegalBlockType)
                    {
                        ptr[i] = (byte) FallbackBlocks[block];
                    }
                }
            }
            return translatedBlocks;
        }
    }
}