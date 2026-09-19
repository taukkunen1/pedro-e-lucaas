namespace Core.Models.SharedConfig
{
    public class GameServerConfig
    {
        // General properties
        public string ServerMode { get; set; } = "Production";
        public bool GeneralTestMode { get; set; }
        public ushort GeneralPortBacklog { get; set; }
        public ushort GeneralPortReceiveSize { get; set; }
        public ushort GeneralPortSendSize { get; set; }
        private string _generalDatabaseLocation;
        public string GeneralDatabaseLocation
        {
            get => Core.DatabasePaths.Resolve(_generalDatabaseLocation);
            set => _generalDatabaseLocation = value;
        }
        public uint GeneralItemUID { get; set; }
        public uint GeneralClientUID { get; set; }
        public uint GeneralDay { get; set; }
        public uint GeneralPKWarWinnerUID { get; set; }
        // Server properties
        public string ServerIPAddres { get; set; }
        public ushort ServerGamePort { get; set; }
        public string ServerName { get; set; }
        public string ServerWebsite { get; set; }
        public string ServerOwner { get; set; }
        // Database properties
        public string DatabaseHostname { get; set; }
        public ushort DatabasePort { get; set; }
        public string DatabaseUsername { get; set; }
        public string DatabasePassword { get; set; }
        public string DatabaseName { get; set; }
        // Interserver properties
        public string InterServerAddress { get; set; }
        public ushort InterServerPort { get; set; }
        public bool IsInterServer { get; set; }
        // Enable/Disable features
        public bool EnabledChi { get; set; }
        public bool EnabledSubclass { get; set; }
        public bool EnabledMentor { get; set; }
        // Specify type db
        public bool? DbFromFiles { get; set; } = true;

        public GameServerConfig()
        {

        }
    }
}
