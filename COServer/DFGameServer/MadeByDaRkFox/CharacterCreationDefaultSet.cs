using GameServer.Client;
using GameServer.Game.MsgServer;

namespace GameServer.MadeByDaRkFox
{
    /// <summary>
    /// Era 1 starter equipment.
    ///
    /// Character creation is intentionally conservative: no welcome pack,
    /// no injected Gold and no post-classic equipment. Skills are learned
    /// through normal game progression/Birth Village instead of being granted
    /// here.
    /// </summary>
    public static class CharacterCreationDefaultSet
    {
        public static void Init(GameClient client, ServerSockets.Packet stream)
        {
            // Basic level-1 clothing shared by the classic professions.
            client.Equipment.Add(stream, 132006, Role.Flags.ConquerItem.Armor);

            // Only the four Era 1 professions are valid at character creation.
            // Do not add rings or later-class starter gear here.
            if (Database.AtributesStatus.IsTaoist(client.Player.Class))
            {
                client.Equipment.Add(stream, 421301, Role.Flags.ConquerItem.RightWeapon);
            }
            else if (Database.AtributesStatus.IsArcher(client.Player.Class))
            {
                client.Equipment.Add(stream, 500006, Role.Flags.ConquerItem.RightWeapon);
            }
            else if (Database.AtributesStatus.IsTrojan(client.Player.Class))
            {
                client.Equipment.Add(stream, 420301, Role.Flags.ConquerItem.RightWeapon);
            }
            else if (Database.AtributesStatus.IsWarrior(client.Player.Class))
            {
                client.Equipment.Add(stream, 561301, Role.Flags.ConquerItem.RightWeapon);
            }
        }
    }
}
