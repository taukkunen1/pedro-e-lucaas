using System.Collections.Generic;
using System.IO;
using System.Linq;
using static GameServer.MadeByDaRkFox.ConfigurableDropSystem;

namespace GameServer.MadeByDaRkFox
{
    public static class EventsRewards
    {
        public static List<EventRewardConfig> EventRewards { get; set; }
        public static string ConfigPath { get; set; } = "EventsRewardsConfig.json";

        public static void Init()
        {
            EventRewards = new List<EventRewardConfig>();
            if (File.Exists(ConfigPath))
            {
                EventRewards = Newtonsoft.Json.JsonConvert.DeserializeObject<List<EventRewardConfig>>(File.ReadAllText(ConfigPath));
            }
            else
            {
                Console.WriteLine($"Cannot read the {ConfigPath} Config File. Creating one config default file...");
                EventRewards.Add(new EventRewardConfig() { EventName = "CityWars", RewardValue = 1000, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "CaptureTheFlag", RewardValue = 500, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "CaptureTheFlag2nd", RewardValue = 400, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "CaptureTheFlag3rd", RewardValue = 300, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "CaptureTheFlagMoney", RewardValue = 3000000, TypeReward = MoneyType.Money });
                EventRewards.Add(new EventRewardConfig() { EventName = "FiveNOut", RewardValue = 500, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "PassTheBomb", RewardValue = 500, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "KingOfTheHill", RewardValue = 500, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "DragonWar", RewardValue = 500, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "LastMan", RewardValue = 500, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "ElitePK1st", RewardValue = 1000, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "ElitePK2nd", RewardValue = 800, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "ElitePK3rd", RewardValue = 700, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "ElitePK4th", RewardValue = 600, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "ElitePK8th", RewardValue = 300, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "TeamPK1st", RewardValue = 1000, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "TeamPK2nd", RewardValue = 900, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "TeamPK3rd", RewardValue = 800, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "TeamPK4th", RewardValue = 700, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "TeamPK8th", RewardValue = 400, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "MsgTopFightKing", RewardValue = 1000, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "MsgTopFightPrince", RewardValue = 900, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "MsgTopFightDuke", RewardValue = 800, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "MsgTopFightEarl", RewardValue = 700, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "FrozenSky", RewardValue = 400, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "MsgClassicClanWar", RewardValue = 250, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "MsgPoleDomination", RewardValue = 1000, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "MsgPoleDominationBI", RewardValue = 1000, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "MsgPoleDominationDC", RewardValue = 1000, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "MsgPoleDominationPC", RewardValue = 1000, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "FruitsQuest", RewardValue = 1000, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "GuildWar", RewardValue = 50000, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "EliteGuildWar", RewardValue = 2000, TypeReward = MoneyType.ConquerPoints });
                EventRewards.Add(new EventRewardConfig() { EventName = "KillMonstersQuest", RewardValue = 800, TypeReward = MoneyType.ConquerPoints });
                File.WriteAllText(ConfigPath, Newtonsoft.Json.JsonConvert.SerializeObject(EventRewards));
            }
        }

        public static EventRewardConfig EventReward(string name, MoneyType TypeReward = MoneyType.ConquerPoints)
        {
            EventRewardConfig eRewConfig = EventRewards?
                .FirstOrDefault(x => x.EventName == name && x.TypeReward == TypeReward);
            if (eRewConfig != null)
                return eRewConfig;

            // Economy V5: an unknown key is configuration debt, not a currency faucet.
            // Returning zero preserves availability while making the missing reward
            // visible in logs and preventing implicit 350-CP minting.
            Console.WriteLine($"Event reward {name} ({TypeReward}) not defined. Reward disabled [0].");
            return new EventRewardConfig()
            {
                EventName = name,
                RewardValue = Game.Era1.Era1Faucets.UnknownEventRewardValue,
                TypeReward = TypeReward
            };
        }
    }

    public class EventRewardConfig
    {
        public string EventName { get; set; }
        public MoneyType TypeReward { get; set; }
        public uint RewardValue { get; set; }
    }
}
