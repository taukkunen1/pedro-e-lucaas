using System.Linq;

namespace GameServer.Game.Era1
{
    /// <summary>
    /// Migracao de login para itens que ja estavam com o jogador antes dos bloqueios 5017.
    /// Roda logo depois de carregar o personagem e antes de mostrar inventario/equipamento.
    ///  - Equipamento posterior ao 5017 sai do corpo e vai para o inventario (ou, sem espaco,
    ///    para o armazem de Twin City). Nada e' apagado.
    ///  - Itens de sistemas removidos (Demon Boxes, CP Packs, Lucky Bag, packs de corrida etc.)
    ///    sao apagados do inventario e dos armazens, com log para eventual reembolso manual.
    /// </summary>
    public static class Era1Migration
    {
        public const uint TwinCityWarehouse = 8; // NpcID.WHTwin

        static bool IsBlockedEquipment(uint itemId)
        {
            return Era1Items.IsBlockedEquipment(itemId)
                || global::Core.Features.FeatureRegistry.IsBlockedItem(itemId);
        }

        static bool IsDeadItem(uint itemId)
        {
            return global::Core.Features.FeatureRegistry.IsBlockedItem(itemId);
        }

        static void Log(Client.GameClient client, string what)
        {
            string line = "[Era1Migration] " + client.Player.Name + " (" + client.Player.UID + "): " + what;
            System.Console.WriteLine(line);
            Database.ServerDatabase.LoginQueue.Enqueue(line);
        }

        public static void Run(Client.GameClient client)
        {
            if (client == null || client.Player == null || client.Equipment == null || client.Inventory == null)
                return;

            int unequipped = 0;

            // 1) Equipamento posterior ao 5017.
            foreach (var item in client.Equipment.ClientItems.Values.ToArray())
            {
                if (!IsBlockedEquipment(item.ITEM_ID))
                    continue;
                Game.MsgServer.MsgGameItem removed;
                if (!client.Equipment.ClientItems.TryRemove(item.UID, out removed))
                    continue;

                ushort oldPosition = removed.Position;
                removed.Position = 0;
                if (IsDeadItem(removed.ITEM_ID))
                {
                    Log(client, "equipamento apagado " + removed.ITEM_ID + " uid " + removed.UID + " (posicao " + oldPosition + ")");
                }
                else if (client.Inventory.HaveSpace(1) && client.Inventory.ClientItems.TryAdd(removed.UID, removed))
                {
                    Log(client, "equipamento " + removed.ITEM_ID + " uid " + removed.UID + " movido para o inventario");
                }
                else if (client.Warehouse != null && client.Warehouse.AddItem(removed, TwinCityWarehouse))
                {
                    Log(client, "equipamento " + removed.ITEM_ID + " uid " + removed.UID + " movido para o armazem de Twin City");
                }
                else
                {
                    // Ultimo recurso: devolve ao corpo para nunca perder o item.
                    removed.Position = oldPosition;
                    client.Equipment.ClientItems.TryAdd(removed.UID, removed);
                    Log(client, "equipamento " + removed.ITEM_ID + " uid " + removed.UID + " mantido: sem espaco no inventario nem no armazem");
                    continue;
                }
                unequipped++;
            }

            // 2) Itens mortos no inventario.
            foreach (var item in client.Inventory.ClientItems.Values.ToArray())
            {
                if (!IsDeadItem(item.ITEM_ID))
                    continue;
                Game.MsgServer.MsgGameItem removed;
                if (client.Inventory.ClientItems.TryRemove(item.UID, out removed))
                    Log(client, "inventario: apagado " + removed.ITEM_ID + " x" + System.Math.Max((ushort)1, removed.StackSize));
            }

            // 3) Itens mortos nos armazens.
            if (client.Warehouse != null)
            {
                foreach (var wh in client.Warehouse.ClientItems)
                {
                    foreach (var item in wh.Value.Values.ToArray())
                    {
                        if (!IsDeadItem(item.ITEM_ID))
                            continue;
                        Game.MsgServer.MsgGameItem removed;
                        if (wh.Value.TryRemove(item.UID, out removed))
                            Log(client, "armazem " + wh.Key + ": apagado " + removed.ITEM_ID + " x" + System.Math.Max((ushort)1, removed.StackSize));
                    }
                }
            }

            if (unequipped > 0)
                client.SendSysMesage(unequipped + " item(ns) de equipamento que nao existem nesta versao foram movidos para o inventario ou armazem.");
        }
    }
}
