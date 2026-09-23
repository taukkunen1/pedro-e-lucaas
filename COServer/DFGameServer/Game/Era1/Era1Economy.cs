namespace GameServer.Game.Era1
{
    /// <summary>
    /// Era 1 economy policy. MINT creates currency/resources, BURN permanently
    /// consumes them, TRANSFER only changes owner. Rates live here so telemetry
    /// can tune faucets without reintroducing 5695/custom reward paths.
    /// </summary>
    public static class Era1Economy
    {
        public const int RefinedDropEvery = 650;
        public const int UniqueDropEvery = 1500;
        public const int EliteDropEvery = 7500;
        public const int SuperDropEvery = 30000;
        public const int PlusOneDropEvery = 7500;
        // Direct +2 equipment is not part of the normal 5017 hunting faucet.
        public const int PlusTwoDropEvery = int.MaxValue;
        public const int DragonBallDropEvery = 50000;

        // Baseline hunting faucets. These are intentionally centralized/tunable:
        // historical sources establish relative rarity, not an authoritative 5017 RNG table.
        public const double MonsterMoneyPercent = 15.0;
        public const double MonsterEquipmentPercent = 1.2;
        public const double MonsterMeteorPercent = 0.20;
        public const double MonsterDragonBallPercent = 0.002;

        public const double MiningSuccessPercent = 50.0;
        public const double MiningDragonBallPercent = 0.05;
        public const double MiningMeteorPercent = 0.75;
        public const double MiningGemPercent = 4.0;
        public const double MiningRefinedGemPercent = 2.0;
        public const double MiningSuperGemPercent = 0.05;

        // 5695/custom kill-counter and boss reward scripts inject Souls, Study Points,
        // high +Stones and multi-DB bundles. The monster engine stays enabled in Era 1,
        // but those reward scripts must not participate in the 5017 economy.
        public static bool EnablePost5017MonsterRewards => false;

        // Classic special spawn exception. This is intentionally outside the normal
        // MonsterDragonBallPercent faucet and must stay visible in static/runtime audits.
        public static bool IsClassicDirectDragonBallMonster(uint monsterId)
        {
            return monsterId == 8419; // WaterDevil / DB Devil
        }

        public static byte RollEquipmentQuality()
        {
            // One shared denominator keeps the declared 1/N rates exact and mutually exclusive:
            // Refined 1/650, Unique 1/1500, Elite 1/7500, Super 1/30000.
            const int scale = 390000; // LCM(650, 1500, 7500, 30000)
            int roll = Pool.GetRandom.Next(1, scale + 1);
            int cursor = scale / SuperDropEvery;
            if (roll <= cursor) return 9;
            cursor += scale / EliteDropEvery;
            if (roll <= cursor) return 8;
            cursor += scale / UniqueDropEvery;
            if (roll <= cursor) return 7;
            cursor += scale / RefinedDropEvery;
            if (roll <= cursor) return 6;
            return 3;
        }

        public static byte RollHuntingPlus()
        {
            return Pool.GetRandom.Next(1, PlusOneDropEvery + 1) == 1 ? (byte)1 : (byte)0;
        }

        public static bool IsLaterEconomyDrop(MadeByDaRkFox.ConfigurableDropSystem.DropType type)
        {
            return type == MadeByDaRkFox.ConfigurableDropSystem.DropType.Stone
                || type == MadeByDaRkFox.ConfigurableDropSystem.DropType.ExpBall
                || type == MadeByDaRkFox.ConfigurableDropSystem.DropType.Letter
                || type == MadeByDaRkFox.ConfigurableDropSystem.DropType.PowerEXPBall;
        }

        public static string TrackedResource(uint itemId, byte plus = 0)
        {
            if (itemId == Database.ItemType.Meteor || itemId == Database.ItemType.MeteorTear) return "Meteor";
            if (itemId == Database.ItemType.DragonBall) return "DragonBall";
            if (IsClassicMiningGem(itemId))
            {
                byte quality = (byte)(itemId % 10);
                return quality == 3 ? "Gem.Super" : quality == 2 ? "Gem.Refined" : "Gem.Normal";
            }

            uint type = itemId / 1000;
            bool equipment = (type >= 110 && type <= 160)
                || (type >= 410 && type <= 590) || type == 900;
            if (equipment)
            {
                if (plus > 0) return "Equipment.Plus" + plus;
                byte quality = (byte)(itemId % 10);
                if (quality == 9) return "Equipment.Super";
                if (quality == 8) return "Equipment.Elite";
                if (quality == 7) return "Equipment.Unique";
                if (quality == 6) return "Equipment.Refined";
            }
            return null;
        }

        public static bool IsClassicMiningGem(uint id)
        {
            uint family = id / 10;
            return family >= 70000 && family <= 70007;
        }

        public static void RunSelfTest()
        {
            if (RefinedDropEvery <= 0 || UniqueDropEvery <= RefinedDropEvery
                || EliteDropEvery <= UniqueDropEvery || SuperDropEvery <= EliteDropEvery)
                throw new System.InvalidOperationException("Era 1 quality rarity ordering failed.");
            if (PlusOneDropEvery <= 0 || PlusTwoDropEvery <= PlusOneDropEvery)
                throw new System.InvalidOperationException("Era 1 +item rarity ordering failed.");
            if (MonsterDragonBallPercent >= MonsterMeteorPercent
                || MiningDragonBallPercent >= MiningMeteorPercent || MiningMeteorPercent >= MiningGemPercent)
                throw new System.InvalidOperationException("Era 1 mining faucet ordering failed.");
            if (TrackedResource(Database.ItemType.Meteor) != "Meteor"
                || TrackedResource(Database.ItemType.DragonBall) != "DragonBall"
                || TrackedResource(700031) != "Gem.Normal"
                || TrackedResource(410079) != "Equipment.Super"
                || TrackedResource(410073, 1) != "Equipment.Plus1")
                throw new System.InvalidOperationException("Era 1 resource MINT/BURN classification failed.");
            if (Game.Era1.Era1Items.IsBlockedEquipment(410073)
                || !Game.Era1.Era1Items.IsBlockedEquipment(201003))
                throw new System.InvalidOperationException("Era 1 item boundary/economy integration failed.");
            if (EnablePost5017MonsterRewards)
                throw new System.InvalidOperationException("Post-5017 monster reward scripts must stay disabled in Era 1.");
            if (!IsClassicDirectDragonBallMonster(8419)
                || IsClassicDirectDragonBallMonster(20300)
                || IsClassicDirectDragonBallMonster(213883))
                throw new System.InvalidOperationException("Era 1 direct Dragon Ball exception policy failed.");

            System.Console.WriteLine("ERA1 ECONOMY SELFTEST PASS");
        }
    }
}
