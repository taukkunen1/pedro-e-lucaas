using System;
using System.Collections.Generic;
using System.Text;

namespace Poker.Packets
{
    public class Packet
    {
        public static void WriteStringWithLength(string arg, int offset, byte[] buffer, byte max)
        {
            if (buffer == null)
                return;
            if (offset > buffer.Length - 1)
                return;
            var till = buffer.Length - offset;
            till = Math.Min(arg.Length, till);
            if (arg.Length < max)
                buffer[offset] = (byte) arg.Length;
            else
                buffer[offset] = max;
            offset++;
            ushort i = 0;
            var bytes = Encoding.Default.GetBytes(arg);
            while ((i < till) && (i <= max))
            {
                buffer[(ushort) (i + offset)] = bytes[i];
                i = (ushort) (i + 1);
            }
        }

        public static void WriteStringWithLength(string arg, int offset, byte[] buffer)
        {
            if (buffer == null)
                return;
            if (offset > buffer.Length - 1)
                return;
            var till = buffer.Length - offset;
            till = Math.Min(arg.Length, till);
            buffer[offset] = (byte) arg.Length;
            offset++;
            ushort i = 0;
            var bytes = Encoding.Default.GetBytes(arg);
            while (i < till)
            {
                buffer[(ushort) (i + offset)] = bytes[i];
                i = (ushort) (i + 1);
            }
        }

        public static void WriteString(string arg, int offset, byte[] buffer)
        {
            if (buffer == null)
                return;
            if (offset > buffer.Length - 1)
                return;
            if (buffer.Length >= offset + arg.Length)
            {
#if UNSAFE
                    fixed (byte* Buffer = buffer)
                    {
                        ushort i = 0;
                        while (i < arg.Length)
                        {
                            *((byte*) (Buffer + offset + i)) = (byte) arg[i];
                            i++;
                        }
                    }
#else
                ushort i = 0;
                while (i < arg.Length)
                {
                    buffer[(ushort) (i + offset)] = (byte) arg[i];
                    i = (ushort) (i + 1);
                }
#endif
            }
        }

        public static void WriteByte(byte arg, int offset, byte[] buffer)
        {
            if (buffer == null)
                return;
            if (offset > buffer.Length - 1)
                return;
            buffer[offset] = arg;
        }

        public static void WriteBoolean(bool arg, int offset, byte[] buffer)
        {
            if (buffer == null)
                return;
            if (offset > buffer.Length - 1)
                return;
            WriteByte(arg ? (byte) 1 : (byte) 0, offset, buffer);
        }

        public static void WriteUInt16(ushort arg, int offset, byte[] buffer)
        {
            if (buffer == null)
                return;
            if (offset > buffer.Length - 1)
                return;
            if (buffer.Length >= offset + sizeof(ushort))
            {
#if UNSAFE
                    fixed (byte* Buffer = buffer)
                    {
                        *((ushort*) (Buffer + offset)) = arg;
                    }
#else
                buffer[offset] = (byte) arg;
                buffer[offset + 1] = (byte) (arg >> 8);
#endif
            }
        }

        public static void WriteUInt32(uint arg, int offset, byte[] buffer)
        {
            if (buffer == null)
                return;
            if (offset > buffer.Length - 1)
                return;
            if (buffer.Length >= offset + sizeof(uint))
            {
#if UNSAFE
                    fixed (byte* Buffer = buffer)
                    {
                        *((uint*) (Buffer + offset)) = arg;
                    }
#else
                buffer[offset] = (byte) arg;
                buffer[offset + 1] = (byte) (arg >> 8);
                buffer[offset + 2] = (byte) (arg >> 16);
                buffer[offset + 3] = (byte) (arg >> 24);
#endif
            }
        }

        public static void WriteUInt64(ulong arg, int offset, byte[] buffer)
        {
            if (buffer == null)
                return;
            if (offset > buffer.Length - 1)
                return;
            if (buffer.Length >= offset + sizeof(ulong))
            {
#if UNSAFE
                    fixed (byte* Buffer = buffer)
                    {
                        *((ulong*) (Buffer + offset)) = arg;
                    }
#else
                buffer[offset] = (byte) arg;
                buffer[offset + 1] = (byte) (arg >> 8);
                buffer[offset + 2] = (byte) (arg >> 16);
                buffer[offset + 3] = (byte) (arg >> 24);
                buffer[offset + 4] = (byte) (arg >> 32);
                buffer[offset + 5] = (byte) (arg >> 40);
                buffer[offset + 6] = (byte) (arg >> 48);
                buffer[offset + 7] = (byte) (arg >> 56);
#endif
            }
        }
        public static void WriteInt16(short arg, int offset, byte[] buffer)
        {
            if (buffer == null)
            {
                return;
            }
            if (offset > buffer.Length - 1)
            {
                return;
            }
            if (buffer.Length >= offset + sizeof(short))
            {
                unsafe
                {
                    fixed (byte* Buffer = buffer)
                    {
                        *((short*)(Buffer + offset)) = arg;
                    }
                }
            }
        }
        public static void WriteInt32(int arg, int offset, byte[] buffer)
        {
            if (buffer == null)
            {
                return;
            }
            if (offset > buffer.Length - 1)
            {
                return;
            }
            if (buffer.Length >= offset + sizeof(int))
            {
                unsafe
                {
                    fixed (byte* Buffer = buffer)
                    {
                        *((int*) (Buffer + offset)) = arg;
                    }
                }
            }
        }
        public static void WriteInt64(long arg, int offset, byte[] buffer)
        {
            if (buffer == null)
            {
                return;
            }
            if (offset > buffer.Length - 1)
            {
                return;
            }
            if (buffer.Length >= offset + sizeof(long))
            {
                unsafe
                {
                    fixed (byte* Buffer = buffer)
                    {
                        *((long*)(Buffer + offset)) = arg;
                    }
                }
            }
        }

        public static void WriteStringList(List<string> arg, int offset, byte[] buffer)
        {
            if (arg == null)
                return;
            if (buffer == null)
                return;
            if (offset > buffer.Length - 1)
                return;
            buffer[offset] = (byte) arg.Count;
            offset++;
            foreach (var str in arg)
            {
                buffer[offset] = (byte) str.Length;
                WriteString(str, offset + 1, buffer);
                offset += str.Length + 1;
            }
        }

        public static void WriteStringList(string[] arg, int offset, byte[] buffer)
        {
            if (arg == null)
                return;
            if (buffer == null)
                return;
            if (offset > buffer.Length - 1)
                return;
            buffer[offset] = (byte) arg.Length;
            offset++;
            foreach (var str in arg)
            {
                buffer[offset] = (byte) str.Length;
                WriteString(str, offset + 1, buffer);
                offset += str.Length + 1;
            }
        }
    }
}