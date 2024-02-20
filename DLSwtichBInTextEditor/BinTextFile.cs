using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DLSwtichBinTextEditor
{
    internal class BinTextFile
    {
        private static Encoding Encoding = Encoding.GetEncoding("shift-jis");
        private byte[] FirstTableData;
        private byte[] Original;
        public FileHeader Header;
        public string FileName { get; set; }
        public List<BinText> Texts { get; set; } = new List<BinText>(){ };

        internal static BinTextFile ReadFromFile(string filename)
        {
            var reader = new BinaryReader(File.OpenRead(filename));

            var file = new BinTextFile
            {
                Header = FileHeader.ReadFromFile(reader),
                FileName = filename
            };
            reader.BaseStream.Position = file.Header.FirstTableOffset;
            file.FirstTableData = reader.ReadBytes(file.Header.TableOffset-file.Header.FirstTableOffset);
            var addresses = new List<int>();
            for(int i = 0;i < file.Header.TextCount;i++)
            {
                addresses.Add(reader.ReadInt32());
            }
            for(int i = 0;i < addresses.Count; i++)
            {
                var address = addresses[i];
                int length = (int)(i != addresses.Count - 1 ? addresses[i + 1] - address : reader.BaseStream.Length - address-file.Header.TextOffset);
                address += file.Header.TextOffset;
                reader.BaseStream.Seek(address, SeekOrigin.Begin);
                var bytes = reader.ReadBytes(length);
                file.Texts.Add(new BinText { Text = Encoding.GetString(bytes, 0, length).Trim(new char[] { '\0' }), Index=i});
            }
            reader.BaseStream.Position = 0;
            file.Original = reader.ReadBytes((int)reader.BaseStream.Length);
            return file;
        }
        internal void Save(string filename)
        {
            var writer = new BinaryWriter(new FileStream(filename, FileMode.Create));
            Header.Write(writer);

            var list = new List<byte[]>();
            writer.Write(FirstTableData);
            var address = 0;
            foreach(var text in Texts)
            {
                writer.Write(address);
                var bytes = Encoding.GetBytes(text.Text + "\0");
                list.Add(bytes);
                address += bytes.Length;
            }
            foreach(var bytes in list)
            {
                writer.Write(bytes);
            }
            writer.Close();
            var reader = new BinaryReader(File.OpenRead(filename));
            var newFile = reader.ReadBytes((int)reader.BaseStream.Length);
            for(int i = 0;i < Original.Length; i++)
            {
                /*if(Original[i] != newFile[i])
                {
                    Console.WriteLine("HUI");
                }*/
            }
        }
    }
}
