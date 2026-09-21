namespace Core.Models.SharedConfig
{
    public class AccountServerConfig
    {
        public string DatabaseHostname { get; set; }
        public uint DatabasePort { get; set; }
        public string DatabaseUsername { get; set; }
        public string DatabasePassword { get; set; }
        public string DatabaseName { get; set; }
        /// <summary>Origem de autenticacao legada. No fluxo atual, a API usa JSON local.</summary>
        public string DatabaseAuthSource { get; set; } = "admin";
        public string ServerName { get; set; } = "Placebo";

        public AccountServerConfig()
        {

        }
    }
}
