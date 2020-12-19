using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AmongUsUnlocker
{
    public class BinaryPatch
    {
        public int Offset { get; private set; }
        public byte[] Bytes { get; private set; }
        public int Length => Bytes?.Length ?? 0;

        public BinaryPatch(int pOffset, byte[] pBytes)
        {
            this.Offset = pOffset;
            this.Bytes = pBytes;
        }

        public void Apply(Stream stream, long offset)
        {
            stream.Position = offset + Offset;
            stream.Write(Bytes);
        }
    }
}
