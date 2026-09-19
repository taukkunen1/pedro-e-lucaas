using Core.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class Utils
    {
        private static readonly Random Random = new();
        public static Encoding Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1252) ?? Encoding.UTF8;
        public static bool Rate(double Percentage)
        {
            double randomNumber = Random.NextDouble() * (100 - 1) + 1;
            return randomNumber <= Percentage;
        }
        public static bool CanConnect()
        {
            bool canConnect;
            try
            {
                canConnect = RestApiHelper.GetRequest<InitialConnectionStatus>("CanConnect").IsConnected;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                canConnect = false;
            }
            return canConnect;
        }
        public static bool CanConnectGS()
        {
            bool canConnect;
            try
            {
                canConnect = RestApiHelper.GetRequest<InitialConnectionStatus>("CanConnectGS").IsConnected;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                canConnect = false;
            }
            return canConnect;
        }
        public static void ASMigrationInit()
        {
            RestApiHelper.GetRequest<Task>("ASMigrationInit");
        }
        public static void GSMigrationInit()
        {
            RestApiHelper.GetRequest<Task>("GSMigrationInit");
        }

        // Decrypt itemtype.dat
        public static bool DecryptCommonDat(string SourceFile)
        {
            bool decryptedSuccess = true;
            try
            {
                byte[] key = new byte[0x80];
                int seed;
                if (!int.TryParse("2537", NumberStyles.HexNumber, null, out seed))
                {
                    return false;
                }
                RandomDat r = new RandomDat(seed);
                for (int i = 0; i < key.Length; i++)
                {
                    key[i] = (byte)(r.Next() % 0x100);
                }
                byte[] b = File.ReadAllBytes(SourceFile);
                for (int i = 0; i < b.Length; i++)
                {
                    int num = b[i] ^ key[i % 0x80];
                    int bits = i % 8;
                    b[i] = (byte)((num << (8 - bits)) + (num >> bits));
                }
                string TargetFile = Path.Combine(Path.GetDirectoryName(SourceFile), Path.GetFileNameWithoutExtension(SourceFile) + ".txt");
                File.WriteAllBytes(TargetFile, b);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                decryptedSuccess = false;
            }
            return decryptedSuccess;
        }
        private class RandomDat
        {
            public long Seed;
            public RandomDat(int seed)
            {
                Seed = seed;
            }
            public int Next()
            {
                return (int)(((Seed = Seed * 214013L + 2531011L) >> 16) & 0x7fff);
            }
        }

        public static string GetPrivateLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipPrivada = host
                .AddressList
                .FirstOrDefault(ip =>
                    ip.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(ip) &&
                    IsPrivateAddress(ip));

            return ipPrivada?.ToString() ?? "No se encontró IP privada.";
        }

        private static bool IsPrivateAddress(IPAddress ip)
        {
            byte[] bytes = ip.GetAddressBytes();

            return (bytes[0] == 10)
                || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                || (bytes[0] == 192 && bytes[1] == 168);
        }
    }
}
