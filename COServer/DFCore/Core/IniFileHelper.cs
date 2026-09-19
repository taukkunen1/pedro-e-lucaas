using IniParser.Model;
using System.IO;

namespace Core
{
    public class IniFileHelper
    {
        private IniData IniData;
        private IniParser.FileIniDataParser IniDataParser;
        private string Filename { get; set; }
        public IniFileHelper()
        {
        }
        public IniFileHelper(string Filename)
        {
            this.Filename = Filename;
            this.LoadFile(Filename);
        }
        public void LoadFile(string Filename)
        {
            this.Filename = Filename;
            IniDataParser = new();
            if (File.Exists(Filename))
            {
                this.IniData = IniDataParser.ReadFile(Filename);
            }
        }
        public void SaveFile(string Filename)
        {
            IniDataParser.WriteFile(Filename, this.IniData);
        }
        public byte ReadByte(string Section, string Key, byte Default)
        {
            if (IniData == null) return Default;
            byte value = Default;
            KeyDataCollection kDataCol = IniData[Section];
            if (kDataCol != null)
            {
                if (kDataCol.ContainsKey(Key))
                {
                    value = byte.Parse(kDataCol[Key]);
                }
            }
            return value;
        }
        public sbyte ReadSByte(string Section, string Key, sbyte Default)
        {
            if (IniData == null) return Default;
            sbyte value = Default;
            KeyDataCollection kDataCol = IniData[Section];
            if (kDataCol != null)
            {
                if (kDataCol.ContainsKey(Key))
                {
                    value = sbyte.Parse(kDataCol[Key]);
                }
            }
            return value;
        }
        public bool ReadBool(string Section, string Key, bool Default)
        {
            if (IniData == null) return Default;
            bool value = Default;
            KeyDataCollection kDataCol = IniData[Section];
            if (kDataCol != null)
            {
                if (kDataCol.ContainsKey(Key))
                {
                    value = bool.Parse(kDataCol[Key]);
                }
            }
            return value;
        }
        public int ReadInt32(string Section, string Key, int Default)
        {
            if (IniData == null) return Default;
            int value = Default;
            KeyDataCollection kDataCol = IniData[Section];
            if (kDataCol != null)
            {
                if (kDataCol.ContainsKey(Key)) {
                    value = int.Parse(kDataCol[Key]);
                }
            }
            return value;
        }
        public long ReadInt64(string Section, string Key, long Default)
        {
            if (IniData == null) return Default;
            long value = Default;
            KeyDataCollection kDataCol = IniData[Section];
            if (kDataCol != null)
            {
                if (kDataCol.ContainsKey(Key))
                {
                    value = long.Parse(kDataCol[Key]);
                }
            }
            return value;
        }
        public ushort ReadUInt16(string Section, string Key, ushort Default)
        {
            if (IniData == null) return Default;
            ushort value = Default;
            KeyDataCollection kDataCol = IniData[Section];
            if (kDataCol != null)
            {
                if (kDataCol.ContainsKey(Key))
                {
                    value = ushort.Parse(kDataCol[Key]);
                }
            }
            return value;
        }
        public uint ReadUInt32(string Section, string Key, uint Default)
        {
            if (IniData == null) return Default;
            uint value = Default;
            KeyDataCollection kDataCol = IniData[Section];
            if (kDataCol != null)
            {
                if (kDataCol.ContainsKey(Key))
                {
                    value = uint.Parse(kDataCol[Key]);
                }
            }
            return value;
        }
        public ulong ReadUInt64(string Section, string Key, ulong Default)
        {
            if (IniData == null) return Default;
            ulong value = Default;
            KeyDataCollection kDataCol = IniData[Section];
            if (kDataCol != null)
            {
                if (kDataCol.ContainsKey(Key))
                {
                    value = ulong.Parse(kDataCol[Key]);
                }
            }
            return value;
        }
        public string ReadString(string Section, string Key, string Default)
        {
            if (IniData == null) return Default;
            string value = Default;
            KeyDataCollection kDataCol = IniData[Section];
            if (kDataCol != null)
            {
                if (kDataCol.ContainsKey(Key))
                {
                    value = kDataCol[Key];
                }
            }
            return value;
        }
        public void Write<T>(string Section, string Key, T Value)
        {
            CreateIfNotExists();
            if (IniData == null) return;
            KeyDataCollection kDataCol = IniData[Section];
            if (kDataCol != null)
            {
                kDataCol[Key] = Value.ToString();
                IniDataParser.WriteFile(Filename, IniData);
            }
        }
        public void WriteString(string Section, string Key, string Value)
        {
            CreateIfNotExists();
            if (IniData == null) return;
            KeyDataCollection kDataCol = IniData[Section];
            if (kDataCol != null)
            {
                kDataCol[Key] = Value;
                IniDataParser.WriteFile(Filename, IniData);
            }
        }
        public void CreateIfNotExists()
        {
            if (!File.Exists(Filename))
            {
                File.Create(Filename).Close();
            }
        }
    }
}
