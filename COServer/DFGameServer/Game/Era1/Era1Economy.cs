namespace GameServer.Game.Era1
{
    /// <summary>
    /// Era 1 hunting/mining economy policy.
    /// MINT creates a currency/item; BURN permanently consumes it; TRANSFER only changes owner.
    /// Rates below are intentionally centralized so live telemetry can tune them without
    /// reintroducing the inflated 5695/custom reward paths.
    /// </summary>
    public static class Era1Economy
    {
        // Quality distribution for an equipment drop: 85% normal, 10% refined,
        // 4% unique, 0.9% elite, 0.1% super.
        public static byte RollEquipmentQuality()
        {
            int roll = Pool.GetRandom.Next(0, 10000);
            if (roll < 10) return 9;
            if (roll < 100) return 8;
            if (roll < 500) return 7;
            if (roll < 1500) return 6;
            return 3;
        }

        // +1 remains a scarce item MINT. Higher plus values must be produced by composition (BURN).
        public static byte RollHuntingPlus()
        {
            return Pool.GetRandom.Next(0, 10000) < 200 ? (byte)1 : (byte)0; // 2% of equipment drops
        }

        public static bool IsClassicMiningGem(uint id)
        {
            uint family = id / 10;
            return family == 70000 || family == 70001 || family == 70002 || family == 70003
                || family == 70004 || family == 70005 || family == 70006 || family == 70007;
        }

        public static void RunSelfTest()
        {
            if (Game.Era1.Era1Items.IsBlockedEquipment(410073))
                throw new System.InvalidOperationException("Era 1 economy blocked classic equipment.");
            if (!Game.Era1.Era1Items.IsBlockedEquipment(201003))
                throw new System.InvalidOperationException("Era 1 economy allowed a post-5017 talisman.");
            System.Console.WriteLine("ERA1 ECONOMY SELFTEST PASS");
        }
    }
}
