namespace GameServer.Game.Era1
{
    /// <summary>
    /// Server-side item boundary for the 5017-inspired Era 1.
    /// The 5695 item database remains intact for client/data compatibility, but later
    /// equipment families are not allowed to enter gameplay.
    /// </summary>
    public static class Era1Items
    {
        private static bool IsBlockedByIdFamily(uint itemId)
        {
            uint type = itemId / 1000;

            return type == 201
                || type == 202
                || type == 300
                || (type >= 601 && type <= 619)
                || type == 141
                || type == 142
                || type == 143;
        }

        public static bool IsBlockedEquipment(uint itemId)
        {
            // These post-5017 families are deterministic from the item id and
            // do not require the 5695 item database to be loaded.
            if (IsBlockedByIdFamily(itemId))
                return true;

            uint type = itemId / 1000;
            ushort position = Database.ItemType.ItemPosition(itemId);
            switch ((Role.Flags.ConquerItem)position)
            {
                // Added after patch 5017.
                case Role.Flags.ConquerItem.Fan:
                case Role.Flags.ConquerItem.Tower:
                case Role.Flags.ConquerItem.Steed:
                case Role.Flags.ConquerItem.SteedMount:
                case Role.Flags.ConquerItem.RidingCrop:
                case Role.Flags.ConquerItem.RightWeaponAccessory:
                case Role.Flags.ConquerItem.LeftWeaponAccessory:
                    return true;
            }

            // Patch 5035: escudos a partir do nivel 120.
            if (type == 900 && Pool.ItemsBase.TryGetValue(itemId, out var shield) && shield.Level >= 120)
                return true;

            return false;
        }

        public static bool IsAllowedGeneratedDrop(uint itemId)
        {
            return itemId != 0
                && !IsBlockedEquipment(itemId)
                && !global::Core.Features.FeatureRegistry.IsBlockedItem(itemId);
        }

        public static void RunSelfTest()
        {
            if (!IsBlockedEquipment(201003) || !IsBlockedEquipment(202003))
                throw new System.InvalidOperationException("Era 1 talisman item gate failed.");
            if (!IsBlockedEquipment(300000))
                throw new System.InvalidOperationException("Era 1 steed item gate failed.");
            if (!IsBlockedEquipment(141003) || !IsBlockedEquipment(142003))
                throw new System.InvalidOperationException("Era 1 patch 5035 headgear gate failed.");
            if (!IsBlockedEquipment(601000))
                throw new System.InvalidOperationException("Era 1 later-profession weapon gate failed.");
            // The promotion self-test runs before the 5695 item database is loaded.
            // Validate the deterministic family boundary here; runtime metadata
            // gates (ItemPosition and shield level) are exercised after database load.
            if (IsBlockedByIdFamily(410073) || IsBlockedByIdFamily(500073) || IsBlockedByIdFamily(900003))
                throw new System.InvalidOperationException("Era 1 classic equipment was blocked unexpectedly.");

            System.Console.WriteLine("ERA1 ITEMS SELFTEST PASS");
        }
    }
}
