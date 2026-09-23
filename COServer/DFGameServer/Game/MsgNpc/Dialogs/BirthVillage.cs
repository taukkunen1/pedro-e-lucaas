using GameServer.Game.MsgServer;

namespace GameServer.Game.MsgNpc.Dialogs
{
    /// <summary>
    /// Era 1 Birth Village tutorial NPCs.
    ///
    /// The layout and responsibilities mirror the classic Birth Village:
    /// tutorial NPCs explain basic systems, Taoist Star teaches Thunder/Cure,
    /// Old General Yang teaches Boreas to Trojans/Warriors, and Know-It-All
    /// completes the tutorial by sending the novice to Twin City.
    /// </summary>
    public static class BirthVillage
    {
        private const uint BirthVillageMap = 1010;
        private const uint TwinCityMap = 1002;

        private static Dialog NewDialog(Client.GameClient client, ServerSockets.Packet stream)
            => new Dialog(client, stream);

        private static bool IsInBirthVillage(Client.GameClient client)
            => client.Player.Map == BirthVillageMap;

        [NpcAttribute(NpcID.BirthGuide)]
        public static void Guide(Client.GameClient client, ServerSockets.Packet stream, byte option, string input, uint id)
        {
            if (!IsInBirthVillage(client))
                return;

            var dialog = NewDialog(client, stream);
            dialog.Text("Welcome to Birth Village. Speak with the people here to learn the basics of Conquer. When you are ready, visit Know-It-All to travel to Twin City.")
                .Option("I understand.", byte.MaxValue)
                .AddAvatar(0)
                .FinalizeDialog();
        }

        [NpcAttribute(NpcID.BirthMrZeal)]
        public static void MrZeal(Client.GameClient client, ServerSockets.Packet stream, byte option, string input, uint id)
        {
            if (!IsInBirthVillage(client))
                return;

            NewDialog(client, stream)
                .Text("To speak with an NPC, move close to them and click on them. Read their words and choose one of the available answers.")
                .Option("Thanks.", byte.MaxValue)
                .AddAvatar(0)
                .FinalizeDialog();
        }

        [NpcAttribute(NpcID.BirthBlacksmith)]
        public static void Blacksmith(Client.GameClient client, ServerSockets.Packet stream, byte option, string input, uint id)
        {
            if (!IsInBirthVillage(client))
                return;

            NewDialog(client, stream)
                .Text("Blacksmiths sell and repair weapons. Open your inventory to equip or remove a weapon, and keep an eye on its durability.")
                .Option("Got it.", byte.MaxValue)
                .AddAvatar(0)
                .FinalizeDialog();
        }

        [NpcAttribute(NpcID.BirthWarehouseman)]
        public static void Warehouseman(Client.GameClient client, ServerSockets.Packet stream, byte option, string input, uint id)
        {
            if (!IsInBirthVillage(client))
                return;

            NewDialog(client, stream)
                .Text("Warehousemen keep your items and silver safe. In the cities, use the warehouse before carrying valuables into dangerous areas.")
                .Option("Got it.", byte.MaxValue)
                .AddAvatar(0)
                .FinalizeDialog();
        }

        [NpcAttribute(NpcID.BirthArmorer)]
        public static void Armorer(Client.GameClient client, ServerSockets.Packet stream, byte option, string input, uint id)
        {
            if (!IsInBirthVillage(client))
                return;

            NewDialog(client, stream)
                .Text("Armor reduces the damage you take. Armorers in the cities sell equipment appropriate for different levels and professions.")
                .Option("Got it.", byte.MaxValue)
                .AddAvatar(0)
                .FinalizeDialog();
        }

        [NpcAttribute(NpcID.BirthPharmacist)]
        public static void Pharmacist(Client.GameClient client, ServerSockets.Packet stream, byte option, string input, uint id)
        {
            if (!IsInBirthVillage(client))
                return;

            NewDialog(client, stream)
                .Text("Pharmacists sell medicine. HP potions restore life and MP potions restore mana used by magic.")
                .Option("Got it.", byte.MaxValue)
                .AddAvatar(0)
                .FinalizeDialog();
        }

        [NpcAttribute(NpcID.BirthMrNosy)]
        public static void MrNosy(Client.GameClient client, ServerSockets.Packet stream, byte option, string input, uint id)
        {
            if (!IsInBirthVillage(client))
                return;

            string profession;
            if (Database.AtributesStatus.IsTrojan(client.Player.Class))
                profession = "Trojan: a close-combat profession focused on weapons and strong melee attacks.";
            else if (Database.AtributesStatus.IsWarrior(client.Player.Class))
                profession = "Warrior: a durable melee profession with defensive and combat skills.";
            else if (Database.AtributesStatus.IsArcher(client.Player.Class))
                profession = "Archer: a ranged profession that fights with bows and arrows.";
            else if (Database.AtributesStatus.IsTaoist(client.Player.Class))
                profession = "Taoist: a magic profession. Visit Taoist Star here in Birth Village to learn your first spells.";
            else
                profession = "Only the four classic Era 1 professions are available.";

            NewDialog(client, stream)
                .Text(profession)
                .Option("Thanks.", byte.MaxValue)
                .AddAvatar(0)
                .FinalizeDialog();
        }

        [NpcAttribute(NpcID.BirthTaoistStar)]
        public static void TaoistStar(Client.GameClient client, ServerSockets.Packet stream, byte option, string input, uint id)
        {
            if (!IsInBirthVillage(client))
                return;

            var dialog = NewDialog(client, stream);

            if (!Database.AtributesStatus.IsTaoist(client.Player.Class))
            {
                dialog.Text("I teach the first spells only to novice Taoists.")
                    .Option("I see.", byte.MaxValue)
                    .AddAvatar(0)
                    .FinalizeDialog();
                return;
            }

            if (option == 0)
            {
                dialog.Text("A novice Taoist should understand both attack and recovery magic. I can teach you Thunder and Cure.")
                    .Option("Teach me.", 1)
                    .Option("Maybe later.", byte.MaxValue)
                    .AddAvatar(0)
                    .FinalizeDialog();
                return;
            }

            if (option == 1)
            {
                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Thunder))
                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Thunder);

                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Cure))
                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Cure);

                dialog.Text("You have learned Thunder and Cure. Practice them well.")
                    .Option("Thank you.", byte.MaxValue)
                    .AddAvatar(0)
                    .FinalizeDialog();
            }
        }

        [NpcAttribute(NpcID.BirthOldGeneralYang)]
        public static void OldGeneralYang(Client.GameClient client, ServerSockets.Packet stream, byte option, string input, uint id)
        {
            if (!IsInBirthVillage(client))
                return;

            var isTrojanOrWarrior =
                Database.AtributesStatus.IsTrojan(client.Player.Class) ||
                Database.AtributesStatus.IsWarrior(client.Player.Class);

            var dialog = NewDialog(client, stream);

            if (!isTrojanOrWarrior)
            {
                dialog.Text("Boreas is training intended for novice Trojans and Warriors.")
                    .Option("I see.", byte.MaxValue)
                    .AddAvatar(0)
                    .FinalizeDialog();
                return;
            }

            if (option == 0)
            {
                dialog.Text("I can teach a novice Trojan or Warrior the Boreas skill.")
                    .Option("Teach me Boreas.", 1)
                    .Option("Maybe later.", byte.MaxValue)
                    .AddAvatar(0)
                    .FinalizeDialog();
                return;
            }

            if (option == 1)
            {
                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Boreas))
                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Boreas);

                dialog.Text("You have learned Boreas. Train hard before you face stronger enemies.")
                    .Option("Thank you.", byte.MaxValue)
                    .AddAvatar(0)
                    .FinalizeDialog();
            }
        }

        [NpcAttribute(NpcID.BirthKnowItAll)]
        public static void KnowItAll(Client.GameClient client, ServerSockets.Packet stream, byte option, string input, uint id)
        {
            if (!IsInBirthVillage(client))
                return;

            var dialog = NewDialog(client, stream);

            if (option == 0)
            {
                dialog.Text("When you finish learning the basics, I can send you to Twin City. Your Era 1 starter equipment has already been issued to you.")
                    .Option("Send me to Twin City.", 1)
                    .Option("I will stay a little longer.", byte.MaxValue)
                    .AddAvatar(0)
                    .FinalizeDialog();
                return;
            }

            if (option == 1)
            {
                client.Teleport(428, 378, TwinCityMap);
            }
        }
    }
}
