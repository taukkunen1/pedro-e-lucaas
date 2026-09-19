namespace Core.Models.SharedConfig
{
    public class AccountServerConfig
    {
        public string DatabaseHostname { get; set; }
        public uint DatabasePort { get; set; }
        public string DatabaseUsername { get; set; }
        public string DatabasePassword { get; set; }
        public string DatabaseName { get; set; }
        /// <summary>Banco onde o usuario do MongoDB foi criado (padrao "admin").</summary>
        public string DatabaseAuthSource { get; set; } = "admin";
        public string ServerName { get; set; } = "TrinityConquer";

        public AccountServerConfig()
        {

        }
    }
}
