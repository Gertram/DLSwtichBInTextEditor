using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace DLSwtichBinTextEditor
{
    internal class FileHeader
    {
        private byte[] data;
        public int TextCount { get;private set; }
        public int TextOffset { get; private set; }
        public int TableOffset { get; private set; }
        public int FirstTableOffset { get;private set; }
        public int FirstCount { get; private set; }
        public int SecondCount { get; private set; }
        public int Length => data.Length;
        public static FileHeader ReadFromFile(BinaryReader reader)
        {
            var header = new FileHeader();
            reader.BaseStream.Seek(0x6, SeekOrigin.Begin);
            header.FirstCount = reader.ReadInt16();
            header.SecondCount = reader.ReadInt16();
            reader.BaseStream.Seek(0xC, SeekOrigin.Begin);
            header.TextCount = reader.ReadInt32();
            reader.BaseStream.Seek(0x14, SeekOrigin.Begin);
            header.TableOffset = reader.ReadInt32();
            reader.BaseStream.Seek(0x18,SeekOrigin.Begin);
            header.TextOffset = reader.ReadInt32();
            reader.BaseStream.Seek(0x10,SeekOrigin.Begin);
            header.FirstTableOffset = reader.ReadInt32();
            reader.BaseStream.Position = 0;
            header.data = reader.ReadBytes(header.FirstTableOffset);
            return header;
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(data);
            writer.Seek(0xC, SeekOrigin.Begin);
            writer.Write(TextCount);
            writer.Seek(0x14, SeekOrigin.Begin);
            writer.Write(TableOffset);
            writer.Seek(0x18, SeekOrigin.Begin);
            writer.Write(TextOffset);
        }
    }
}
