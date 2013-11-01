// Part of FemtoCraft | Copyright 2012-2013 Matvei Stefarov <me@matvei.org>

using System.IO;
using System.Net;
using System.Text;

namespace fCraft
{
    sealed class PacketReader : BinaryReader
    {
        public PacketReader(Stream stream) :
            base(stream) { }


        public OpCode ReadOpCode()
        {
            return (OpCode)ReadByte();
        }


        public override short ReadInt16()
        {
            return IPAddress.NetworkToHostOrder(base.ReadInt16());
        }


        public override int ReadInt32()
        {
            return IPAddress.NetworkToHostOrder(base.ReadInt32());
        }


        public override string ReadString()
        {
            return Encoding.ASCII.GetString(ReadBytes(64)).Trim();
        }
    }
}
