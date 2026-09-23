namespace GameServer.Game.Era1
{
    /// <summary>
    /// Economy V4 policy for classic Market/vending, warehouse access and
    /// fixed NPC service sinks that live outside Shop.dat.
    /// </summary>
    public static class Era1Services
    {
        public const ushort ClassicMarketMap = 1036;

        // Official classic service prices.
        public const uint ConductressFeeSilver = 100;
        public const uint ArenaEntryFeeSilver = 50;
        public const uint ClassicHairstyleFeeSilver = 500;
        public const uint DynastyHairstyleFeeSilver = 10000;
        public const uint AvatarChangeFeeSilver = 500;
        public const uint GuildCreationFeeSilver = 1000000;

        // The classic player Market charges no server-side transaction tax.
        // Gold/CP vending is a pure player-to-player transfer.
        public const uint PlayerVendingTaxBasisPoints = 0;

        // Static server booths in Database5700/Booths.txt are a 5695/custom
        // infinite-supply layer, not the classic player booth system.
        public static bool EnablePost5017StaticBooths => false;

        // Remote warehouse access was introduced by the later VIP system.
        public static bool EnablePost5017RemoteWarehouse => false;

        // Classic warehouse deposit/withdraw has no service fee.
        public const uint WarehouseFeeSilver = 0;

        public static bool IsClassicWarehouseNpc(uint npcId)
        {
            return Role.Instance.Warehouse.IsWarehouse((Game.MsgNpc.NpcID)npcId)
                && npcId != ushort.MaxValue
                && npcId != (uint)Game.MsgNpc.NpcID.WHPoker;
        }

        public static bool CanUseClassicWarehouse(Client.GameClient client, uint npcId)
        {
            if (client == null || client.IsConnectedInterServer())
                return false;

            // Some client builds echo the warehouse NPC in ItemUsage.id while
            // others rely on the last interacted NPC. Resolve both, but always
            // require the actual warehouse to still be in screen.
            uint resolvedNpc = IsClassicWarehouseNpc(npcId) ? npcId : client.ActiveNpc;
            if (!IsClassicWarehouseNpc(resolvedNpc))
                return false;

            if (EnablePost5017RemoteWarehouse)
                return true;

            Game.MsgNpc.Npc npc;
            return client.Map.SearchNpcInScreen(resolvedNpc, client.Player.X, client.Player.Y, out npc);
        }

        public static bool CanCreatePlayerBooth(Client.GameClient client)
        {
            return client != null
                && !client.IsConnectedInterServer()
                && client.Player.Map == ClassicMarketMap
                && client.Player.Alive;
        }

        public static bool IsAllowedPlayerVendingItem(uint itemId)
        {
            return itemId != 0
                && !Era1Items.IsBlockedEquipment(itemId)
                && !global::Core.Features.FeatureRegistry.IsBlockedItem(itemId)
                && !Database.ItemType.unabletradeitem.Contains(itemId);
        }

        public static bool IsValidVendingPrice(uint amount)
        {
            return amount > 0;
        }

        public static void RunSelfTest()
        {
            if (ClassicMarketMap != 1036
                || ConductressFeeSilver != 100
                || ArenaEntryFeeSilver != 50
                || ClassicHairstyleFeeSilver != 500
                || DynastyHairstyleFeeSilver != 10000
                || AvatarChangeFeeSilver != 500
                || GuildCreationFeeSilver != 1000000)
                throw new System.InvalidOperationException("Era 1 classic NPC service prices failed.");

            if (EnablePost5017StaticBooths
                || EnablePost5017RemoteWarehouse
                || WarehouseFeeSilver != 0
                || PlayerVendingTaxBasisPoints != 0)
                throw new System.InvalidOperationException("Era 1 Market/warehouse policy leaked a post-5017 fee or service.");

            if (!IsClassicWarehouseNpc((uint)Game.MsgNpc.NpcID.WHTwin)
                || !IsClassicWarehouseNpc((uint)Game.MsgNpc.NpcID.WHMarket)
                || IsClassicWarehouseNpc((uint)Game.MsgNpc.NpcID.WHPoker)
                || IsClassicWarehouseNpc(ushort.MaxValue))
                throw new System.InvalidOperationException("Era 1 warehouse NPC boundary failed.");

            if (!IsValidVendingPrice(1) || IsValidVendingPrice(0))
                throw new System.InvalidOperationException("Era 1 vending price boundary failed.");

            if (!IsAllowedPlayerVendingItem(Database.ItemType.DragonBall)
                || IsAllowedPlayerVendingItem(201003)
                || IsAllowedPlayerVendingItem(300000))
                throw new System.InvalidOperationException("Era 1 vending item boundary failed.");

            System.Console.WriteLine("ERA1 SERVICES SELFTEST PASS");
        }
    }
}
