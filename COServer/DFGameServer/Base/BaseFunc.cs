using Core;
using Core.Models.GameServer;
using System;
using System.IO;

namespace GameServer
{
    public static class BaseFunc
    {
        public static int RandGet(int nMax, bool bRealRand)
        {
            if (nMax <= 0)
                nMax = 1;

            if (bRealRand)
                Kernel.srand((uint)DateTime.Now.Value());
            long val = Kernel.rand();
            return (int)(val % nMax);
        }

        public static double RandomRateGet(double dRange)
        {
            double pi = 3.1415926;

            int nRandom = RandGet(999, true) + 1;
            double a = Math.Sin(nRandom * pi / 1000);
            double b;
            if (nRandom >= 90)
                b = (1.0 + dRange) - Math.Sqrt(Math.Sqrt(a)) * dRange;
            else
                b = (1.0 - dRange) + Math.Sqrt(Math.Sqrt(a)) * dRange;

            return b;
        }
        public static bool UserExists(uint UID)
        {
            if (ServerConfig.DbFromFiles)
            {
                string UserPath = Path.Combine(ServerConfig.DbLocation, "Users", UID.ToString() + ".ini");
                return File.Exists(UserPath);
            } else
            {
                Player player = RestApiHelper.GetPlayer(UID);
                return player != null && player.UID > 0;
            }
        }
        public static bool UserIsNormal(string Name)
        {
            return !Name.Contains("[GM]") && !Name.Contains("[PM]");
        }
    }
}
