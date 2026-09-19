using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Core
{
    public class BinaryFileHelper
    {
        private string Filename { get; set; }
        private FileStream FileStream { get; set; }
        public BinaryFileHelper()
        {
        }
        public BinaryFileHelper(string Filename, FileMode FileMode = FileMode.Open)
        {
            this.Filename = Filename;
            this.FileStream = File.Open(Filename, FileMode);
        }
        public bool LoadFile(string Filename, FileMode FileMode = FileMode.Open)
        {
            this.Filename = Filename;
            if (File.Exists(Filename))
            {
                while (!CanSetFileStream(FileMode))
                {
                    // Wait for set :)
                }
            } else if (FileMode != FileMode.Open)
            {
                this.FileStream = File.Create(Filename);
            }
            return File.Exists(Filename) || Directory.Exists(Filename);
        }
        private bool CanSetFileStream(FileMode FileMode = FileMode.Open)
        {
            bool canSetFileStream;
            try
            {
                this.FileStream = File.Open(Filename, FileMode);
                canSetFileStream = true;
            }
            catch (Exception)
            {
                canSetFileStream = false;
            }
            return canSetFileStream;
        }
        public byte ReadByte()
        {
            using (var reader = new BinaryReader(this.FileStream, Utils.Encoding, true))
            {
                return reader.ReadByte();
            }
        }
        public uint ReadUint()
        {
            using (var reader = new BinaryReader(this.FileStream, Utils.Encoding, true))
            {
                return reader.ReadUInt32();
            }
        }
        public int ReadInt()
        {
            using (var reader = new BinaryReader(this.FileStream, Utils.Encoding, true))
            {
                return reader.ReadInt32();
            }
        }
        public T Read<T>()
        {
            using (var reader = new BinaryReader(this.FileStream, Utils.Encoding, true))
            {
                byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

                GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                handle.Free();

                return theStructure;
            }
        }
        public void WriteByte(byte Byte)
        {
            using (var writer = new BinaryWriter(this.FileStream, Utils.Encoding, true))
            {
                writer.Write(Byte);
            }
        }
        public void WriteInt(int Integer)
        {
            using (var writer = new BinaryWriter(this.FileStream, Utils.Encoding, true))
            {
                writer.Write(Integer);
            }
        }
        public void WriteUInt(uint UnsignedInteger)
        {
            using (var writer = new BinaryWriter(this.FileStream, Utils.Encoding, true))
            {
                writer.Write(UnsignedInteger);
            }
        }
        public void Write<T>(T generic)
        {
            int length = Marshal.SizeOf(generic);
            IntPtr ptr = Marshal.AllocHGlobal(length);
            byte[] myBuffer = new byte[length];

            Marshal.StructureToPtr(generic, ptr, true);
            Marshal.Copy(ptr, myBuffer, 0, length);
            Marshal.FreeHGlobal(ptr);

            this.FileStream.Write(myBuffer, 0, myBuffer.Length);
        }
        public void Close()
        {
            this.FileStream.Close();
        }
        public string GetFilename { get { return this.Filename; } }
    }
}