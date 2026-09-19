using System.Collections.Generic;
using System.IO;

namespace GameServer.MadeByDaRkFox
{
    public static class BulletinManager
    {
        public static string ConfigPath { get; set; } = "BulletinConfiguration.json";
        public static List<BulletinConfiguration> BulletinConfigurations { get; set; }
        public static void Init()
        {
            BulletinConfigurations = new List<BulletinConfiguration>();
            if (File.Exists(ConfigPath))
            {
                BulletinConfigurations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BulletinConfiguration>>(File.ReadAllText(ConfigPath));
            } else
            {
                Console.WriteLine($"Cannot read the {ConfigPath} Config File. Creating one config default file...");
                BulletinConfigurations.Clear();
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 1, MapId = 1020, MapX = 576, MapY = 623 }); //Pole Domination AP
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 2, MapId = 1015, MapX = 718, MapY = 573 }); //Pole Domination BI
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 3, MapId = 1000, MapX = 469, MapY = 657 }); //Pole Domination DC
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 4, MapId = 1011, MapX = 275, MapY = 288 }); //Pole Domination PC
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 5, MapId = 1002, MapX = 454, MapY = 387 }); //Fortress War
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 6, MapId = 1002, MapX = 419, MapY = 243 }); //TeamPK Tournament
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 7, MapId = 1002, MapX = 441, MapY = 243 }); //SkillPK Tournament
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 8, MapId = 1002, MapX = 446, MapY = 249 }); //TreasureThief
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 9, MapId = 1002, MapX = 425, MapY = 368 }); //CityWar
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 10, MapId = 1002, MapX = 447, MapY = 352 }); //Nobility Tournament
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 11, MapId = 1002, MapX = 354, MapY = 338 }); //CaptureTheFlag
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 12, MapId = 1002, MapX = 430, MapY = 249 }); //ElitePKTournament
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 13, MapId = 1038, MapX = 200, MapY = 254 }); //GuildWar
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 14, MapId = 1002, MapX = 404, MapY = 292 }); //KnightGame
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 15, MapId = 1002, MapX = 424, MapY = 249 }); //ClassicClanWar
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 16, MapId = 1002, MapX = 436, MapY = 243 }); //ClassPKWar
                BulletinConfigurations.Add(new BulletinConfiguration() { Id = 17, MapId = 1002, MapX = 437, MapY = 250 }); //EliteGuildWar
                File.WriteAllText(ConfigPath, Newtonsoft.Json.JsonConvert.SerializeObject(BulletinConfigurations));
            }
        }
    }

    public class BulletinConfiguration
    {
        public uint Id { get; set; }
        public uint MapId { get; set; }
        public ushort MapX { get; set; }
        public ushort MapY { get; set; }
        public BulletinConfiguration()
        {
        }
    }
}
