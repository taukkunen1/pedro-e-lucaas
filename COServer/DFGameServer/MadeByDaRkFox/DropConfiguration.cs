using System.Collections.Generic;
using System.IO;

namespace GameServer.MadeByDaRkFox
{
    public class DropConfiguration
    {
        public ConfigurableDropSystem.DropType Type { get; set; }
        public double Percent { get; set; }
        public bool Enabled { get; set; }
        public MoneyDrop MoneyDrop { get; set; }
        public DropConfiguration()
        {
        }
    }

    public static class ConfigurableDropSystem
    {
        public static List<DropConfiguration> Drops { get; set; }
        public static string ConfigPath { get; set; } = "DropSystemConfig.json";
        public static void Init()
        {
            Drops = new List<DropConfiguration>();
            if (File.Exists(ConfigPath))
            {
                Drops = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DropConfiguration>>(File.ReadAllText(ConfigPath));
                ApplyEra1Policy();
            } else
            {
                Console.WriteLine($"Cannot read the {ConfigPath} Config File. Creating one config default file...");
                Drops.Clear();
                Drops.Add(new DropConfiguration() { Percent = 1.5, Type = DropType.MeteorScroll, Enabled = true });
                Drops.Add(new DropConfiguration() { Percent = 1.2, Type = DropType.Item, Enabled = true });
                Drops.Add(new DropConfiguration() { Percent = 1.3, Type = DropType.Stone, Enabled = false });
                Drops.Add(new DropConfiguration() { Percent = 2, Type = DropType.ExpBall, Enabled = false });
                Drops.Add(new DropConfiguration() { Percent = 2, Type = DropType.Letter, Enabled = false });
                Drops.Add(new DropConfiguration() { Percent = 1.1, Type = DropType.PowerEXPBall, Enabled = false });
                Drops.Add(new DropConfiguration() { Percent = 1.4, Type = DropType.DragonBall, Enabled = true });
                Drops.Add(new DropConfiguration() { Percent = 15, Type = DropType.Money, Enabled = true, MoneyDrop = new MoneyDrop() { Min = 1000, Max = 2000, Type = MoneyType.Money} });
                File.WriteAllText(ConfigPath, Newtonsoft.Json.JsonConvert.SerializeObject(Drops));
                ApplyEra1Policy();
            }
        }
        private static void ApplyEra1Policy()
        {
            if (Drops == null) return;
            foreach (var drop in Drops)
            {
                if (drop == null) continue;
                if (drop.Type == DropType.Stone || drop.Type == DropType.ExpBall
                    || drop.Type == DropType.Letter || drop.Type == DropType.PowerEXPBall)
                    drop.Enabled = false;

                if (drop.Type == DropType.Money && drop.MoneyDrop != null
                    && drop.MoneyDrop.Type != MoneyType.Money)
                    drop.Enabled = false;
            }
        }
        public enum DropType
        {
            MeteorScroll,
            Item,
            Stone,
            ExpBall,
            Letter,
            PowerEXPBall,
            DragonBall,
            Money,
        }
        public enum MoneyType
        {
            Money,
            ConquerPoints,
            BoundConquerPoints,
        }
    }

    public class MoneyDrop
    {
        public uint Min { get; set; }
        public uint Max { get; set; }
        public ConfigurableDropSystem.MoneyType Type { get; set; }
    }

    public class ItemDrop
    {
        public uint Min { get; set; }
        public uint Max { get; set; }
        public uint ItemTypeID { get; set; }
    }
}
