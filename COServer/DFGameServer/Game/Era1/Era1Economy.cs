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

        public const double MiningSuccessPercent = 50.0;
        public const double MiningDragonBallPercent = 0.05;
        public const double MiningMeteorPercent = 0.75;
        public const double MiningGemPercent = 4.0;
        public const double MiningRefinedGemPercent = 2.0;
        public const double MiningSuperGemPercent = 0.05;

        public static byte RollEquipmentQuality()
        {
            // Contemporary 2008 hunting reports show the expected ordering:
            // Refined commonest, then Unique, Elite, with Super exceptional.
            int roll = Pool.GetRandom.Next(1, SuperDropEvery + 1);
            if (roll == 1) return 9;
            if (roll <= SuperDropEvery / EliteDropEvery) return 8;
            if (roll <= SuperDropEvery / UniqueDropEvery) return 7;
            if (roll <= SuperDropEvery / RefinedDropEvery) return 6;
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
            if (MiningDragonBallPercent >= MiningMeteorPercent || MiningMeteorPercent >= MiningGemPercent)
                throw new System.InvalidOperationException("Era 1 mining faucet ordering failed.");
            if (Game.Era1.Era1Items.IsBlockedEquipment(410073)
                || !Game.Era1.Era1Items.IsBlockedEquipment(201003))
                throw new System.InvalidOperationException("Era 1 item boundary/economy integration failed.");

            System.Console.WriteLine("ERA1 ECONOMY SELFTEST PASS");
        }
    }
}
