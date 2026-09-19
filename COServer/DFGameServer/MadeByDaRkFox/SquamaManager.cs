using System.Collections.Generic;
using System.IO;

namespace GameServer.MadeByDaRkFox
{
    public static class SquamaManager
    {
        private static List<SquamaReward> DefaultSquamaRewards()
        {
            return new List<SquamaReward>() {
                new SquamaReward() {
                    RewardPercentage = 40,
                    RewardType = SquamaLocationRewardType.Gold,
                    RewardValue = 100000,
                    IsDefaultReward = true
                },
                new SquamaReward() {
                    RewardPercentage = 40,
                    RewardType = SquamaLocationRewardType.ConquerPoints,
                    RewardValue = 215,
                    IsDefaultReward = false
                },
                new SquamaReward() {
                    RewardPercentage = 20,
                    RewardType = SquamaLocationRewardType.Item,
                    RewardValue = Database.ItemType.DragonBall,
                    IsDefaultReward = false
                }
            };
        }
        private static List<SquamaMapCord> DefaultCords()
        {
            return new List<SquamaMapCord>()
            {
                // Twin City (MapID 1002)
                new SquamaMapCord() { MapName = "TwinCity", MapId = 1002, MapX = 850, MapY = 651 },
                new SquamaMapCord() { MapName = "TwinCity", MapId = 1002, MapX = 939, MapY = 570 },
                new SquamaMapCord() { MapName = "TwinCity", MapId = 1002, MapX = 387, MapY = 369 },
                new SquamaMapCord() { MapName = "TwinCity", MapId = 1002, MapX = 417, MapY = 276 },
                new SquamaMapCord() { MapName = "TwinCity", MapId = 1002, MapX = 398, MapY = 324 },
                new SquamaMapCord() { MapName = "TwinCity", MapId = 1002, MapX = 491, MapY = 673 },

                // Phoenix Castle (MapID 1011)
                new SquamaMapCord() { MapName = "PhoenixCastle", MapId = 1011, MapX = 248, MapY = 376 },
                new SquamaMapCord() { MapName = "PhoenixCastle", MapId = 1011, MapX = 413, MapY = 288 },
                new SquamaMapCord() { MapName = "PhoenixCastle", MapId = 1011, MapX = 614, MapY = 490 },
                new SquamaMapCord() { MapName = "PhoenixCastle", MapId = 1011, MapX = 573, MapY = 685 },
                new SquamaMapCord() { MapName = "PhoenixCastle", MapId = 1011, MapX = 274, MapY = 652 },

                // Desert City (MapID 1000)
                new SquamaMapCord() { MapName = "DesertCity", MapId = 1000, MapX = 555, MapY = 666 },
                new SquamaMapCord() { MapName = "DesertCity", MapId = 1000, MapX = 320, MapY = 481 },
                new SquamaMapCord() { MapName = "DesertCity", MapId = 1000, MapX = 444, MapY = 555 },
                new SquamaMapCord() { MapName = "DesertCity", MapId = 1000, MapX = 678, MapY = 370 },
                new SquamaMapCord() { MapName = "DesertCity", MapId = 1000, MapX = 215, MapY = 775 },

                // Ape City (MapID 1020)
                new SquamaMapCord() { MapName = "ApeCity", MapId = 1020, MapX = 679, MapY = 534 },
                new SquamaMapCord() { MapName = "ApeCity", MapId = 1020, MapX = 389, MapY = 640 },
                new SquamaMapCord() { MapName = "ApeCity", MapId = 1020, MapX = 451, MapY = 471 },
                new SquamaMapCord() { MapName = "ApeCity", MapId = 1020, MapX = 512, MapY = 385 },
                new SquamaMapCord() { MapName = "ApeCity", MapId = 1020, MapX = 738, MapY = 479 },

                // Bird Island (MapID 1015)
                new SquamaMapCord() { MapName = "BirdIsland", MapId = 1015, MapX = 627, MapY = 546 },
                new SquamaMapCord() { MapName = "BirdIsland", MapId = 1015, MapX = 478, MapY = 235 },
                new SquamaMapCord() { MapName = "BirdIsland", MapId = 1015, MapX = 377, MapY = 561 },
                new SquamaMapCord() { MapName = "BirdIsland", MapId = 1015, MapX = 612, MapY = 324 },
                new SquamaMapCord() { MapName = "BirdIsland", MapId = 1015, MapX = 521, MapY = 656 },

                // MetZone (MapID 1101)
                new SquamaMapCord() { MapName = "MetZone", MapId = 1101, MapX = 306, MapY = 292 },
                new SquamaMapCord() { MapName = "MetZone", MapId = 1101, MapX = 406, MapY = 398 },
                new SquamaMapCord() { MapName = "MetZone", MapId = 1101, MapX = 502, MapY = 276 },
                new SquamaMapCord() { MapName = "MetZone", MapId = 1101, MapX = 428, MapY = 546 },
                new SquamaMapCord() { MapName = "MetZone", MapId = 1101, MapX = 584, MapY = 366 }
            };
        }
        public static string ConfigPath { get; set; } = "SquamaConfiguration.json";
        public static List<SquamaLocation> SquamaConfigurations { get; set; }
        public static void Init()
        {
            SquamaConfigurations = new List<SquamaLocation>();
            if (File.Exists(ConfigPath))
            {
                SquamaConfigurations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SquamaLocation>>(File.ReadAllText(ConfigPath));
            }
            else
            {
                Console.WriteLine($"Cannot read the {ConfigPath} Config File. Creating one config default file...");
                SquamaConfigurations.Clear();
                foreach(SquamaMapCord squamaCord in DefaultCords())
                {
                    SquamaConfigurations.Add(new SquamaLocation()
                    {
                        Name = squamaCord.MapName + "_" + squamaCord.MapX + "_" + squamaCord.MapY,
                        MapId = squamaCord.MapId,
                        MapX = squamaCord.MapX,
                        MapY = squamaCord.MapY,
                        Rewards = DefaultSquamaRewards()
                    });
                }
                File.WriteAllText(ConfigPath, Newtonsoft.Json.JsonConvert.SerializeObject(SquamaConfigurations));
            }
        }
    }
    public class SquamaLocation
    {
        public string Name { get; set; }
        public uint MapId { get; set; }
        public ushort MapX { get; set; }
        public ushort MapY { get; set; }
        public bool Claimed { get; set; }
        public List<SquamaReward> Rewards { get; set; }

        public SquamaLocation() { 
            Rewards = new List<SquamaReward>();
        }
    }
    public class SquamaReward
    {
        public bool IsDefaultReward { get; set; }
        public uint RewardPercentage { get; set; }
        public SquamaLocationRewardType RewardType { get; set; }
        public uint RewardValue { get; set; } // Can be the ID of item or the quantity if reward type are CPs or Gold
    }

    public class SquamaMapCord
    {
        public string MapName { get; set; }
        public uint MapId { get; set; }
        public ushort MapX { get; set; }
        public ushort MapY { get; set; }
    }

    public enum SquamaLocationRewardType
    {
        Item = 0,
        ConquerPoints = 1,
        Gold = 2,
        BoundConquerPoints = 3,
    }
}
