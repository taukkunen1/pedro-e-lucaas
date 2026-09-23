namespace GameServer.Game.Era1
{
    /// <summary>
    /// Server-side item boundary for the 5017-inspired Era 1.
    /// The 5695 item database remains intact for client/data compatibility, but later
    /// equipment families are not allowed to enter gameplay.
    /// </summary>
    public static class Era1Items
    {
        public static bool IsBlockedEquipment(uint itemId)
        {
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

            uint type = itemId / 1000;
            // Later profession weapons embedded in the 5695 item database.
            if (type >= 601 && type <= 619)
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
            if (!IsBlockedEquipment(601000))
                throw new System.InvalidOperationException("Era 1 later-profession weapon gate failed.");
            if (IsBlockedEquipment(410073) || IsBlockedEquipment(500073) || IsBlockedEquipment(900003))
                throw new System.InvalidOperationException("Era 1 classic equipment was blocked unexpectedly.");

            System.Console.WriteLine("ERA1 ITEMS SELFTEST PASS");
        }
    }
}
