namespace GameServer
{
    public static class ServerConfig
    {
        public static string CO2Folder = "";
        public static string FacebookLink = "";
        public static uint ServerID = 0;
        public static string IPAddres = "";
        public static ushort GamePort = 5817;
        public static string ServerName = "TrinityConquer";
        public static string ServerOwner = "DaRkFoxDev[PM]";
        public static string OfficialWebSite = "https://trinityconquer.eu";
        public static ushort Port_BackLog;
        public static ushort Port_ReceiveSize = 8191;
        public static ushort Port_SendSize = 8191;

        //Database
        public static string DbLocation = "";
        public static uint ExpRateSpell = 100;
        public static uint ExpRateProf = 2000;
        public static uint UserExpRate = 1*10; // *10 EXP (Normal Rate is 1)
        public static uint PhysicalDamage = 100; // 100 is the recommended value for a normal physical damage

        //Database Auth
        public static string DatabaseAuthHostname = "localhost";
        public static uint DatabaseAuthPort = 3306;
        public static string DatabaseAuthUsername = "root";
        public static string DatabaseAuthPassword = "";
        public static string DatabaseAuthName = "cq.auth";

        //interServer
        public static string InterServerAddress = "";
        public static ushort InterServerPort = 0;
        public static bool IsInterServer = false;

        // Enable/Disable Features
        public static bool EnabledChi { get; set; }
        public static bool EnabledSubclass { get; set; }
        public static bool EnabledMentor { get; set; }
        // Specify type db
        public static bool DbFromFiles { get; set; } = true;
    }
}
