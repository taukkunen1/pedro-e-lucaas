using GameServer.Client;
using GameServer.Game.MsgServer;

namespace GameServer.MadeByDaRkFox
{
    public static class CharacterCreationDefaultSet
    {
        public static void Init(GameClient client, ServerSockets.Packet stream)
        {
            client.Equipment.Add(stream, 132006, Role.Flags.ConquerItem.Armor);
            if (Database.AtributesStatus.IsTaoist(client.Player.Class))
            {
                client.Equipment.Add(stream, 152005, Role.Flags.ConquerItem.Ring);
                client.Equipment.Add(stream, 421301, Role.Flags.ConquerItem.RightWeapon);
            } else if (Database.AtributesStatus.IsArcher(client.Player.Class))
            {
                client.Equipment.Add(stream, 150003, Role.Flags.ConquerItem.Ring);
                client.Equipment.Add(stream, 500006, Role.Flags.ConquerItem.RightWeapon);
            }
            else
            {
                client.Equipment.Add(stream, 150003, Role.Flags.ConquerItem.Ring);
                if (Database.AtributesStatus.IsPirate(client.Player.Class))
                {
                    client.Equipment.Add(stream, 611301, Role.Flags.ConquerItem.RightWeapon);
                }
                else if (Database.AtributesStatus.IsTrojan(client.Player.Class))
                {
                    client.Equipment.Add(stream, 420301, Role.Flags.ConquerItem.RightWeapon);
                }
                else if (Database.AtributesStatus.IsMonk(client.Player.Class))
                {
                    client.Equipment.Add(stream, 610301, Role.Flags.ConquerItem.RightWeapon);
                }
                else if (Database.AtributesStatus.IsNinja(client.Player.Class))
                {
                    client.Equipment.Add(stream, 601301, Role.Flags.ConquerItem.RightWeapon);
                }
                else if (Database.AtributesStatus.IsWarrior(client.Player.Class))
                {
                    client.Equipment.Add(stream, 561301, Role.Flags.ConquerItem.RightWeapon);
                }
                else
                    client.Equipment.Add(stream, 410301, Role.Flags.ConquerItem.RightWeapon);
            }
            client.Inventory.Add(stream, 723753, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true); // Welcome pack
            client.Player.Money += 50000;
            if (!client.FullLoading) GameServer.Telemetry.Economy.RecordManual(client.Player.UID, client.Player.Name, client.Player.Map, GameServer.Telemetry.Currency.Gold, 50000, "Code:NewCharacter");
            client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
        }
    }
}
