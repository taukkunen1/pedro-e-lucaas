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

        // Refinery, purification, talisman socketing, steed composition and mentor
        // composition rewards belong to later patches and stay outside Era 1.
        public static bool EnablePost5017ItemExtra => false;
        public static bool EnablePost5017CompositionMentorRewards => false;
        // Later-patch convenience route that skips the classic material economy.
        public static bool EnableDirectLevelUpgradeWithCps => false;

        // Classic socket costs used by the 5017-era blacksmith/Wu Xing paths.
        public const byte WeaponFirstSocketDragonBalls = 1;
        public const byte WeaponSecondSocketDragonBalls = 5;
        public const byte EquipmentFirstSocketDragonBalls = 12;
        public const byte EquipmentSecondSocketStarDrills = 7;
        public const int EquipmentSecondSocketToughDrillChancePercent = 20;

        public static bool IsBlockedItemUsage(Game.MsgServer.MsgItemUsuagePacket.ItemUsageID action)
        {
            return action == Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.SocketTalismanWithCPs
                || action == Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.SocketTalismanWithItem
                || action == Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.UpdatePurity
                || action == Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.DegradeEquipment
                || action == Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.RepairItemVIP
                || action == Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.GarmentShop;
        }

        public static bool IsAllowedForgingShopItem(uint itemId)
        {
            return Era1Shops.IsAllowedForgingPurchase(itemId);
        }

        public static bool IsClassicEquipment(uint itemId)
        {
            uint type = itemId / 1000;
            return (type >= 111 && type <= 118)
                || type == 123
                || (type >= 120 && type <= 121)
                || (type >= 130 && type <= 139)
                || (type >= 141 && type <= 148)
                || (type >= 150 && type <= 152)
                || type == 160
                || (type >= 410 && type <= 490)
                || (type >= 500 && type <= 580)
                || type == 900;
        }

        public static bool IsClassicWeaponSocketTarget(uint itemId)
        {
            uint type = itemId / 1000;
            return IsClassicEquipment(itemId)
                && !Game.Era1.Era1Items.IsBlockedByIdFamily(itemId)
                && ((type >= 410 && type <= 490) || (type >= 500 && type <= 580));
        }

        internal static bool IsClassicEquipmentSocketFamily(uint itemId)
        {
            return IsClassicEquipment(itemId)
                && !IsClassicWeaponSocketTarget(itemId)
                && !Game.Era1.Era1Items.IsBlockedByIdFamily(itemId);
        }

        public static bool IsClassicEquipmentSocketTarget(uint itemId)
        {
            if (!IsClassicEquipmentSocketFamily(itemId)
                || Game.Era1.Era1Items.IsBlockedEquipment(itemId))
                return false;
            ushort position = Database.ItemType.ItemPosition(itemId);
            return position != 0 && Database.ItemType.AllowToUpdate((Role.Flags.ConquerItem)position);
        }

        public static bool IsPlusStone(uint itemId)
        {
            return itemId >= 730001 && itemId <= 730008;
        }

        public const byte ClassicComposeMinorCount = 2;
        public const byte ClassicHighPlusPlayerLevel = 130;

        public static bool IsClassicGem(uint itemId)
        {
            byte quality = (byte)(itemId % 10);
            return IsClassicMiningGem(itemId) && (quality == 1 || quality == 2 || quality == 3);
        }

        public static byte CompositionMaterialPlus(uint itemId, byte storedPlus)
        {
            if (IsPlusStone(itemId))
                return (byte)(itemId - 730000);
            return storedPlus;
        }

        public static byte ClassicHighPlusDragonBallCost(byte currentPlus)
        {
            if (currentPlus == 9) return 12;
            if (currentPlus == 10) return 25;
            if (currentPlus == 11) return 40;
            return 0;
        }

        public static byte ClassicComposeGemCost(uint targetItemId, byte currentPlus)
        {
            // The old composition rule requires gems when the result becomes +6 or higher:
            // two for weapons and one for other equipment.
            if (currentPlus < 5 || currentPlus >= 9)
                return 0;
            return IsClassicWeaponSocketTarget(targetItemId) ? (byte)2 : (byte)1;
        }

        static int CompositionFamily(uint itemId)
        {
            if (Database.ItemType.IsBow(itemId)) return 1;
            if (Database.ItemType.IsBacksword(itemId)) return 2;

            uint type = itemId / 1000;
            if (type >= 500 && type <= 580) return 3; // classic 2-handed weapons
            if (type >= 410 && type <= 490) return 4; // classic 1-handed weapons
            return 1000 + (int)type;                  // equipment category/type
        }

        public static bool IsClassicForgeTarget(uint itemId)
        {
            if (!IsClassicEquipment(itemId) || Game.Era1.Era1Items.IsBlockedEquipment(itemId))
                return false;
            ushort position = Database.ItemType.ItemPosition(itemId);
            return position != 0 && Database.ItemType.AllowToUpdate((Role.Flags.ConquerItem)position);
        }

        public static bool CanComposeTarget(uint itemId)
        {
            if (!IsClassicForgeTarget(itemId))
                return false;
            return Pool.ItemsBase.TryGetValue(itemId, out var dbItem) && dbItem.Level >= 15;
        }

        public static bool IsAllowedCompositionMaterial(uint mainItemId, uint materialItemId)
        {
            if (!CanComposeTarget(mainItemId))
                return false;
            if (IsPlusStone(materialItemId))
                return true;
            if (!IsClassicEquipment(materialItemId) || Game.Era1.Era1Items.IsBlockedEquipment(materialItemId))
                return false;
            if (CompositionFamily(mainItemId) != CompositionFamily(materialItemId))
                return false;

            // Minor quality may not exceed main quality.
            return (materialItemId % 10) <= (mainItemId % 10);
        }

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

        public static string[] TrackedResources(uint itemId, byte plus = 0)
        {
            if (itemId == Database.ItemType.Meteor || itemId == Database.ItemType.MeteorTear
                || itemId == Database.ItemType.MeteorScroll)
                return new[] { "Meteor" };
            if (itemId == Database.ItemType.DragonBall || itemId == Database.ItemType.DragonBallScroll)
                return new[] { "DragonBall" };
            if (itemId == Database.ItemType.ToughDrill) return new[] { "Drill.Tough" };
            if (itemId == Database.ItemType.StarDrill) return new[] { "Drill.Star" };
            if (IsPlusStone(itemId)) return new[] { "PlusStone.Plus" + (itemId - 730000) };
            if (IsClassicMiningGem(itemId))
            {
                byte gemQuality = (byte)(itemId % 10);
                return new[] { gemQuality == 3 ? "Gem.Super" : gemQuality == 2 ? "Gem.Refined" : "Gem.Normal" };
            }

            if (IsClassicEquipment(itemId))
            {
                string quality = null;
                byte itemQuality = (byte)(itemId % 10);
                if (itemQuality == 9) quality = "Equipment.Super";
                else if (itemQuality == 8) quality = "Equipment.Elite";
                else if (itemQuality == 7) quality = "Equipment.Unique";
                else if (itemQuality == 6) quality = "Equipment.Refined";

                string plusResource = plus > 0 ? "Equipment.Plus" + plus : null;
                if (quality != null && plusResource != null) return new[] { quality, plusResource };
                if (quality != null) return new[] { quality };
                if (plusResource != null) return new[] { plusResource };
            }
            return System.Array.Empty<string>();
        }

        public static string TrackedResource(uint itemId, byte plus = 0)
        {
            var resources = TrackedResources(itemId, plus);
            return resources.Length == 0 ? null : resources[resources.Length - 1];
        }

        public static int TrackedResourceUnitMultiplier(uint itemId)
        {
            if (itemId == Database.ItemType.MeteorScroll || itemId == Database.ItemType.DragonBallScroll)
                return 10;
            return 1;
        }

        static bool ContainsResource(string[] resources, string resource)
        {
            for (int i = 0; i < resources.Length; i++)
                if (resources[i] == resource)
                    return true;
            return false;
        }

        public static void RecordEquipmentTransformation(GameServer.Client.GameClient client,
            uint oldItemId, byte oldPlus, uint newItemId, byte newPlus)
        {
            if (client == null) return;
            var before = TrackedResources(oldItemId, oldPlus);
            var after = TrackedResources(newItemId, newPlus);

            for (int i = 0; i < before.Length; i++)
                if (!ContainsResource(after, before[i]))
                    Telemetry.Economy.RecordResource(client.Player.UID, client.Player.Name, client.Player.Map, before[i], -1);
            for (int i = 0; i < after.Length; i++)
                if (!ContainsResource(before, after[i]))
                    Telemetry.Economy.RecordResource(client.Player.UID, client.Player.Name, client.Player.Map, after[i], 1);
        }

        public static void RecordSocketCreated(GameServer.Client.GameClient client, byte slot)
        {
            if (client == null || (slot != 1 && slot != 2)) return;
            Telemetry.Economy.RecordResource(client.Player.UID, client.Player.Name, client.Player.Map,
                "Equipment.Socket" + slot, 1);
        }

        public static void RecordEmbeddedGem(GameServer.Client.GameClient client, Role.Flags.Gem gem, long delta)
        {
            if (client == null || delta == 0 || gem == Role.Flags.Gem.NoSocket || gem == Role.Flags.Gem.EmptySocket)
                return;

            uint gemItemId = Database.ItemType.GetGemID(gem);
            string gemResource = TrackedResource(gemItemId);
            if (gemResource == null || !gemResource.StartsWith("Gem."))
                return;

            Telemetry.Economy.RecordResource(client.Player.UID, client.Player.Name, client.Player.Map,
                "Embedded" + gemResource, delta);
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
                || TrackedResource(Database.ItemType.MeteorScroll) != "Meteor"
                || TrackedResourceUnitMultiplier(Database.ItemType.MeteorScroll) != 10
                || TrackedResource(Database.ItemType.DragonBall) != "DragonBall"
                || TrackedResource(Database.ItemType.DragonBallScroll) != "DragonBall"
                || TrackedResourceUnitMultiplier(Database.ItemType.DragonBallScroll) != 10
                || TrackedResource(700031) != "Gem.Normal"
                || TrackedResource(410079) != "Equipment.Super"
                || TrackedResource(410073, 1) != "Equipment.Plus1"
                || TrackedResource(730001) != "PlusStone.Plus1"
                || TrackedResource(Database.ItemType.ToughDrill) != "Drill.Tough")
                throw new System.InvalidOperationException("Era 1 resource MINT/BURN classification failed.");
            if (ClassicComposeMinorCount != 2 || ClassicHighPlusPlayerLevel != 130
                || ClassicHighPlusDragonBallCost(9) != 12
                || ClassicHighPlusDragonBallCost(10) != 25
                || ClassicHighPlusDragonBallCost(11) != 40
                || ClassicComposeGemCost(410073, 5) != 2
                || ClassicComposeGemCost(130033, 5) != 1)
                throw new System.InvalidOperationException("Era 1 classic composition/refining policy failed.");
            if (!IsClassicWeaponSocketTarget(410073) || IsClassicWeaponSocketTarget(900003)
                || !IsClassicEquipmentSocketFamily(900003)
                || WeaponFirstSocketDragonBalls != 1 || WeaponSecondSocketDragonBalls != 5
                || EquipmentFirstSocketDragonBalls != 12 || EquipmentSecondSocketStarDrills != 7)
                throw new System.InvalidOperationException("Era 1 socket policy failed.");
            if (!IsBlockedItemUsage(Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.SocketTalismanWithCPs)
                || !IsBlockedItemUsage(Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.SocketTalismanWithItem)
                || !IsBlockedItemUsage(Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.DegradeEquipment)
                || !IsBlockedItemUsage(Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.RepairItemVIP)
                || !IsBlockedItemUsage(Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.GarmentShop)
                || EnablePost5017ItemExtra || EnablePost5017CompositionMentorRewards
                || EnableDirectLevelUpgradeWithCps)
                throw new System.InvalidOperationException("Post-5017 item systems must stay disabled in Era 1.");
            if (Game.Era1.Era1Items.IsBlockedByIdFamily(410073)
                || !Game.Era1.Era1Items.IsBlockedByIdFamily(201003))
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
