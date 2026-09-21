using Core;
using Core.Models.SharedConfig;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Cryptography;

namespace API.Models
{
    public static class Utils
    {
        public static string StartupPath { get; set; }
        public static string XmlCommentsFilePath
        {
            get
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                return xmlPath;
            }
        }
        public static AccountServerConfig ReadAccountServerConfig(bool OnlyRead = true)
        {
            StartupPath = AppContext.BaseDirectory;
            AccountServerConfig aServConf = new();
            string jsonConfigPath = Path.Combine(StartupPath, "AccountServerConfig.json");
            if (File.Exists(jsonConfigPath))
            {
                aServConf = Newtonsoft.Json.JsonConvert.DeserializeObject<AccountServerConfig>(File.ReadAllText(jsonConfigPath));
            }
            else
            {
                if (OnlyRead) { return aServConf; }
                Console.WriteLine("Cannot read AccountServerConfig.json.");
                Console.WriteLine("Creating a new configuration for AccountServer. Follow the instructions.");
                bool configValid = false;
                aServConf.ServerName = "TrinityConquer";
                // Specify servername
                Console.Write("Input your ServerName[TrinityConquer]: ");
                string ServerNameInput = Console.ReadLine();
                if (ServerNameInput.Length > 0)
                {
                    aServConf.ServerName = ServerNameInput;
                }
                aServConf.DatabaseHostname = "";
                aServConf.DatabasePort = 0;
                aServConf.DatabaseName = "json";
                aServConf.DatabaseUsername = "";
                aServConf.DatabasePassword = "";
                aServConf.DatabaseAuthSource = "";
                try
                {
                    var authStore = new JsonAuthStore();
                    if (!authStore.PingAsync().GetAwaiter().GetResult())
                        throw new Exception("Cannot initialize local JSON auth store.");
                    authStore.EnsureIndexesAsync().GetAwaiter().GetResult();
                    configValid = true;
                } catch(Exception ex)
                {
                    Console.WriteLine("Invalid configuration detected. " + ex.Message);
                    RemoveLockedConfigCreator();
                }
                if (configValid)
                {
                    File.WriteAllText(jsonConfigPath, Newtonsoft.Json.JsonConvert.SerializeObject(aServConf));
                }
            }
            return aServConf;
        }
        public static GameServerConfig ReadGameServerConfig()
        {
            StartupPath = AppContext.BaseDirectory;
            GameServerConfig gameServConf = new();
            string jsonConfigPath = Path.Combine(StartupPath, "GameServerConfig.json");
            if (File.Exists(jsonConfigPath))
            {
                gameServConf = Newtonsoft.Json.JsonConvert.DeserializeObject<GameServerConfig>(File.ReadAllText(jsonConfigPath));
            }
            else
            {
                Console.WriteLine("Cannot read GameServerConfig.json.");
                Console.WriteLine("Creating a new configuration for GameServer. Follow the instructions.");
                bool configValid = false;
                gameServConf.GeneralPortBacklog = 100;
                gameServConf.GeneralPortReceiveSize = 8194;
                gameServConf.GeneralPortSendSize = 1024;
                gameServConf.GeneralTestMode = false;
                gameServConf.InterServerAddress = "";
                gameServConf.InterServerPort = 9915;
                gameServConf.IsInterServer = false;
                gameServConf.GeneralItemUID = 0;
                gameServConf.GeneralClientUID = 1000000;
                gameServConf.GeneralDay = 0;
                gameServConf.GeneralPKWarWinnerUID = 0;
                gameServConf.ServerGamePort = 5816;
                gameServConf.ServerName = "TrinityConquer";
                // Specify servername
                Console.Write("Input your ServerName [TrinityConquer]: ");
                string ServerNameInput = Console.ReadLine();
                if (ServerNameInput.Length > 0)
                {
                    gameServConf.ServerName = ServerNameInput;
                }
                // Specify server ip
                Console.Write("Input your ServerIP [Use PrivateIP or PublicIP, 127.0.0.1 default (Only Local Connections)]: ");
                string ServerIPInput = Console.ReadLine();
                if (ServerIPInput.Length > 0)
                {
                    gameServConf.ServerIPAddres = ServerIPInput;
                } else
                {
                    gameServConf.ServerIPAddres = "127.0.0.1"; // Only local
                }
                // Specify website
                Console.Write("Input your Website [https://trinityconquer.eu]: ");
                string WebsiteInput = Console.ReadLine();
                if (WebsiteInput.Length > 0)
                {
                    if (!WebsiteInput.StartsWith("http://") && !WebsiteInput.StartsWith("https://"))
                    {
                        WebsiteInput = "http://" + WebsiteInput;
                    }
                    gameServConf.ServerWebsite = WebsiteInput;
                }
                // Specify server owner
                Console.Write("Input the name of the Server Owner [DaRkFoxDev[PM]]: ");
                string ServerOwnerInput = Console.ReadLine();
                if (ServerOwnerInput.Length > 0)
                {
                    gameServConf.ServerOwner = ServerOwnerInput;
                }
                // Question enable chi
                Console.WriteLine("Do you want Chi enabled?[Y/N]: ");
                string EnabledChi = Console.ReadLine();
                gameServConf.EnabledChi = EnabledChi.ToLower() == "y";
                Console.WriteLine("Do you want Subclass enabled?[Y/N]: ");
                string EnabledSubclass = Console.ReadLine();
                gameServConf.EnabledSubclass = EnabledSubclass.ToLower() == "y";
                Console.WriteLine("Do you want Mentor enabled?[Y/N]: ");
                string EnabledMentor = Console.ReadLine();
                gameServConf.EnabledMentor = EnabledMentor.ToLower() == "y";
                // Specify db folder
                string defaultDb = Core.DatabasePaths.BuiltInDir ?? @"C:\Database5700\";
                Console.Write($"Input your Database Folder [{defaultDb}]: ");
                string dbFolder = Console.ReadLine();
                gameServConf.GeneralDatabaseLocation = dbFolder.Length > 0 ? dbFolder : defaultDb;
                AdvancedConsole.WriteLine("Changing the ServerName in client_config.ini...", ConsoleColor.DarkGreen);
                string[] clientConfigLines = File.ReadAllLines(Path.Combine(gameServConf.GeneralDatabaseLocation, "client_config.ini"));
                List<string> fixedClientConfigLines = new List<string>();
                foreach(string lineConfig in clientConfigLines)
                {
                    string[] l = lineConfig.Split(' ');
                    if (l[0] == "1")
                    {
                        l[1] = gameServConf.ServerName;
                    }
                    fixedClientConfigLines.Add(string.Join(' ', l));
                }
                File.WriteAllLines(Path.Combine(gameServConf.GeneralDatabaseLocation, "client_config.ini"), fixedClientConfigLines.ToArray());
                gameServConf.DatabaseHostname = "";
                gameServConf.DatabasePort = 0;
                gameServConf.DatabaseName = "";
                gameServConf.DatabaseUsername = "";
                gameServConf.DatabasePassword = "";
                try
                {
                    configValid = true;
                    var authStore = new JsonAuthStore();
                    var serverExisting = authStore.GetServerByNameAsync(gameServConf.ServerName).GetAwaiter().GetResult();
                    authStore.SaveServerAsync(new AccountServer.Server()
                    {
                        Id = serverExisting?.Id ?? 0,
                        Name = gameServConf.ServerName,
                        IP = gameServConf.ServerIPAddres,
                        Port = gameServConf.ServerGamePort,
                        TransferKey = GenerateRandomTransferKey(),
                        TransferSalt = GenerateRandomTransferKey()
                    }).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Invalid configuration detected. " + ex.Message);
                    RemoveLockedConfigCreator();
                }
                if (configValid)
                {
                    File.WriteAllText(jsonConfigPath, Newtonsoft.Json.JsonConvert.SerializeObject(gameServConf));
                    Console.WriteLine("You need restart now for get AccountServer working. All configuration ready :)");
                }
            }
            return gameServConf;
        }
        public static bool SaveGameServerConfig(GameServerConfig GSConfig)
        {
            StartupPath = AppContext.BaseDirectory;
            string jsonConfigPath = Path.Combine(StartupPath, "GameServerConfig.json");
            File.WriteAllText(jsonConfigPath, Newtonsoft.Json.JsonConvert.SerializeObject(GSConfig));
            return true;
        }
        public static bool ExistAccountServerConfig()
        {
            return File.Exists(Path.Combine(AppContext.BaseDirectory, "AccountServerConfig.json"));
        }
        public static bool ExistGameServerConfig()
        {
            return File.Exists(Path.Combine(AppContext.BaseDirectory, "GameServerConfig.json"));
        }
        public static void RemoveLockedConfigCreator()
        {
            string lockedConfigCreatorPath = Path.Combine(AppContext.BaseDirectory, "configCreator.locked");
            bool lockedConfigCreator = File.Exists(lockedConfigCreatorPath);
            if (lockedConfigCreator)
            {
                File.Delete(lockedConfigCreatorPath);
            }
        }
        public static bool LockedConfigCreator()
        {
            string lockedConfigCreatorPath = Path.Combine(AppContext.BaseDirectory, "configCreator.locked");
            bool lockedConfigCreator = File.Exists(lockedConfigCreatorPath);
            LockConfigCreator();
            return lockedConfigCreator;
        }
        public static void LockConfigCreator()
        {
            try
            {
                string lockedConfigCreator = Path.Combine(AppContext.BaseDirectory, "configCreator.locked");
                if (!File.Exists(lockedConfigCreator))
                {
                    File.WriteAllText(lockedConfigCreator, "LOCKED");
                    ReadAccountServerConfig(false);
                    AdvancedConsole.WriteLine("The console needs to be restarted. The GameServer now needs to be configured after the restart.", ConsoleColor.DarkRed);
                }
            } catch(Exception)
            {
            }
        }
        public static string GenerateRandomTransferKey(int length = 32)
        {
            byte[] bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}

