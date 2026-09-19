using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.AccountServer
{
    public class ServerInfo
    {
        public string Name;
        public string IP;
        public ushort Port;
        public string TransferKey;
        public string TransferSalt;
    }
    [Table("servers")]
    public class Server
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string IP { get; set; }
        public uint Port { get; set; }
        public string TransferKey { get; set; }
        public string TransferSalt { get; set; }
        public static Dictionary<string, ServerInfo> Servers = new Dictionary<string, ServerInfo>();
        public Server ()
        {
        }
        public static void Load(Server ServerToLoad)
        {
            ServerInfo serverinfo = new ServerInfo();
            serverinfo.Name = ServerToLoad.Name;
            serverinfo.IP = ServerToLoad.IP;
            serverinfo.Port = (ushort)ServerToLoad.Port;
            serverinfo.TransferKey = ServerToLoad.TransferKey;
            serverinfo.TransferSalt = ServerToLoad.TransferSalt;
            Servers.Add(serverinfo.Name, serverinfo);
            string format = "{0} [{1}:{2}]";
            Console.WriteLine(string.Format(format, serverinfo.Name, serverinfo.IP, serverinfo.Port), ConsoleColor.Green);
        }
    }
}
