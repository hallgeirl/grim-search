using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GDItemSearchUtil
{
    public class GDFileReader
    {
        UInt32 key;
        UInt32[] table = new UInt32[256];
        long end;


        public GDFileReader(Stream s)
        {
            file = new BinaryReader(s);
            end = s.Length;
        }

        public void BeginRead()
        {
            file.BaseStream.Seek(0, SeekOrigin.Begin);
            ReadKey();
        }

        public void EndRead()
        {
            if (file.BaseStream.Position != end)
                throw new Exception();
        }


        BinaryReader file;

        void ReadKey()
        {
            UInt32 k = file.ReadUInt32();

            k ^= 0x55555555;

            key = k;

            for (int i = 0; i < 256; i++)
            {
                k = (k >> 1) | (k << 31);
                k *= 39916801;
                table[i] = k;
            }
        }

        public UInt32 NextInt()
        {

            UInt32 ret = file.ReadUInt32();

            ret ^= key;

            return ret;
        }

        void UpdateKey(byte[] ptr, UInt32 len)
        {
            for (UInt32 i = 0; i < len; i++)
            {
                key ^= table[ptr[i]];
            }
        }

        public UInt32 ReadInt()
        {

            UInt32 val = file.ReadUInt32();
            UInt32 ret = val ^ key;

            UpdateKey(BitConverter.GetBytes(val), 4);

            return ret;
        }

        public UInt16 ReadShort()
        {
            UInt16 val = file.ReadUInt16();

            UInt16 smallKey = (UInt16)(key & 0xFFFF);
            UInt16 ret = (UInt16)(val ^ smallKey); //TODO: Check

            UpdateKey(BitConverter.GetBytes(val), 2);

            return ret;
        }


        public byte ReadByte()
        {
            byte val = file.ReadByte();
            byte smallKey = (byte)(key & 0xFF);
            byte ret = (byte)(val ^ smallKey); //TODO: Check
            UpdateKey(BitConverter.GetBytes(val), 1);

            return ret;
        }

        public float ReadFloat()
        {
            UInt32 i = ReadInt();
            float val = BitConverter.ToSingle(BitConverter.GetBytes(i), 0);

            return val;
        }

        public UInt32 ReadBlockStart(Block b)
        {
            UInt32 ret = ReadInt();

            b.len = NextInt();

            b.end = file.BaseStream.Position + b.len;

            return ret;
        }

        public void ReadBlockEnd(Block b)
        {
            if (file.BaseStream.Position != b.end)
                throw new Exception();

            if (NextInt() != 0)
                throw new Exception();
        }
    }
}
