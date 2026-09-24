namespace GameServer.Game.Era1
{
    /// <summary>
    /// Economy V4 policy for the 5017-era Market, vending, warehouse and NPC services.
    /// Keeps classic player-to-player commerce intact while removing later static
    /// system booths and remote-service shortcuts that bypass the physical economy.
    /// </summary>
    public static class Era1MarketServices
    {
        // Booths.txt belongs to the later 5695/custom layer and mints infinite stock.
        // Era 1 keeps only dynamic player stalls in the Market.
        public static bool EnableLegacyStaticBooths => false;

        // Remote warehouse access is a later VIP convenience (Patch 5072 era).
        public static bool EnableRemoteWarehouse => false;

        // Classic documented service fees.
        public const uint ConductressFeeSilver = 100;

        // Patch 5068 / classic CP Admin parity.
        public const uint DragonBallCpValue = 215;
        public const uint DragonBallScrollCpValue = 2150;

        public static bool IsAllowedVendorCurrency(Game.MsgServer.MsgItemView.ActionMode mode)
        {
            return mode == Game.MsgServer.MsgItemView.ActionMode.Gold
                || mode == Game.MsgServer.MsgItemView.ActionMode.CPs;
        }

        public static bool IsAllowedVendorItem(Game.MsgServer.MsgGameItem item)
        {
            if (item == null || item.ITEM_ID == 0)
                return false;

            if (Database.ItemType.unabletradeitem.Contains(item.ITEM_ID))
                return false;

            if (Game.Era1.Era1Items.IsBlockedEquipment(item.ITEM_ID)
                || global::Core.Features.FeatureRegistry.IsBlockedItem(item.ITEM_ID))
                return false;

            // Refineries/purifications are from the later item system and must not
            // re-enter Era 1 through player stalls.
            if (item.Refinary.InLife || item.Purification.InLife)
                return false;

            return item.Bound == 0
                && item.Inscribed == 0
                && item.Locked == 0
                && item.ITEM_ID != 750000;
        }

        public static bool IsClassicWarehouseNpc(uint npcId)
        {
            return npcId == (uint)Game.MsgNpc.NpcID.WHTwin
                || npcId == (uint)Game.MsgNpc.NpcID.WHMarket
                || npcId == (uint)Game.MsgNpc.NpcID.wHPheonix
                || npcId == (uint)Game.MsgNpc.NpcID.WHDesert
                || npcId == (uint)Game.MsgNpc.NpcID.WHApe
                || npcId == (uint)Game.MsgNpc.NpcID.WHBird;
        }

        public static bool CanUseClassicWarehouse(Client.GameClient client, uint npcId)
        {
            if (client == null || !IsClassicWarehouseNpc(npcId))
                return false;

            Game.MsgNpc.Npc npc;
            return client.Map != null
                && client.Map.SearchNpcInScreen(npcId, client.Player.X, client.Player.Y, out npc);
        }

        public static bool IsClassicCpAdminExchange(uint itemId, uint cps)
        {
            return (itemId == Database.ItemType.DragonBall && cps == DragonBallCpValue)
                || (itemId == Database.ItemType.DragonBallScroll && cps == DragonBallScrollCpValue);
        }

        public static void RunSelfTest()
        {
            if (EnableLegacyStaticBooths || EnableRemoteWarehouse)
                throw new System.InvalidOperationException("Post-5017 market/warehouse shortcuts must stay disabled.");

            if (!IsAllowedVendorCurrency(Game.MsgServer.MsgItemView.ActionMode.Gold)
                || !IsAllowedVendorCurrency(Game.MsgServer.MsgItemView.ActionMode.CPs)
                || IsAllowedVendorCurrency(Game.MsgServer.MsgItemView.ActionMode.ViewEquip))
                throw new System.InvalidOperationException("Era 1 vending currency policy failed.");

            if (!IsClassicWarehouseNpc((uint)Game.MsgNpc.NpcID.WHTwin)
                || !IsClassicWarehouseNpc((uint)Game.MsgNpc.NpcID.WHMarket)
                || IsClassicWarehouseNpc((uint)Game.MsgNpc.NpcID.WHPoker)
                || IsClassicWarehouseNpc((uint)Game.MsgNpc.NpcID.WHStone))
                throw new System.InvalidOperationException("Era 1 warehouse identity policy failed.");

            if (ConductressFeeSilver != 100
                || !IsClassicCpAdminExchange(Database.ItemType.DragonBall, 215)
                || !IsClassicCpAdminExchange(Database.ItemType.DragonBallScroll, 2150))
                throw new System.InvalidOperationException("Era 1 service/exchange prices failed.");

            System.Console.WriteLine("ERA1 MARKET/SERVICES SELFTEST PASS");
        }
    }
}
