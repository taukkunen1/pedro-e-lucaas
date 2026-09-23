using GameServer.Game.MsgServer;

namespace GameServer.Game.MsgNpc.Dialogs
{
    /// <summary>
    /// Era 1 profession promotions.
    /// Promotion gates are 15, 40, 70, 100 and 110.
    /// Rewards here are deliberately limited to classic profession skills;
    /// later-version packs, garments, steed rewards and post-classic skills
    /// do not belong to Era 1.
    /// </summary>
    public static class Era1JobCenter
    {
        private const uint JobCenterMap = 1004;

        private static bool InJobCenter(Client.GameClient client) => client.Player.Map == JobCenterMap;

        private static void AddSpell(Client.GameClient client, ServerSockets.Packet stream, Role.Flags.SpellID spell)
        {
            var id = (ushort)spell;
            if (!client.MySpells.ClientSpells.ContainsKey(id))
                client.MySpells.Add(stream, id);
        }

        private static void NotReady(Client.GameClient client, ServerSockets.Packet stream, int level)
        {
            new Dialog(client, stream)
                .Text("You need to reach level " + level + " before your next promotion.")
                .Option("I understand.", byte.MaxValue)
                .AddAvatar(0)
                .FinalizeDialog();
        }

        private static void Done(Client.GameClient client, ServerSockets.Packet stream, string title)
        {
            new Dialog(client, stream)
                .Text("Promotion complete. You are now " + title + ".")
                .Option("Thank you.", byte.MaxValue)
                .AddAvatar(0)
                .FinalizeDialog();
        }

        private static void AddReward(Client.GameClient client, ServerSockets.Packet stream, uint itemId,
            Role.Flags.Gem socketOne = Role.Flags.Gem.NoSocket)
        {
            if (!client.Inventory.HaveSpace(1))
            {
                client.SendSysMesage("Please free one inventory slot to receive your promotion reward.");
                return;
            }

            client.Inventory.Add(stream, itemId, 1, 0, 0, 0, socketOne);
        }

        private static void AwardClassicPhysicalReward(Client.GameClient client, ServerSockets.Packet stream, string profession, int level)
        {
            // IDs verified against Database5700/itemtype.txt.
            if (profession == "Trojan")
            {
                if (level == 40) AddReward(client, stream, 410073);      // Normal lvl 40 Cutlass
                else if (level == 70) AddReward(client, stream, 130063); // Normal lvl 70 Rage Armor
            }
            else if (profession == "Warrior")
            {
                if (level == 40) AddReward(client, stream, 900003);      // Normal lvl 40 Soft Shield
                else if (level == 70) AddReward(client, stream, 131063); // Normal lvl 70 Light Armor
            }
            else if (profession == "Archer")
            {
                if (level == 15) AddReward(client, stream, 133003); // Normal lvl 15 Deerskin Coat
                else if (level == 40)
                    AddReward(client, stream, 500073, Role.Flags.Gem.EmptySocket); // Normal lvl 40 Horn Bow, 1 socket
            }
            else if (profession == "Taoist")
            {
                if (level == 15) AddReward(client, stream, 134003);      // Normal lvl 15 Tao Robe
                else if (level == 40) AddReward(client, stream, 421073); // Normal lvl 40 End Backsword
                else if (level == 70) AddReward(client, stream, 134063); // Normal lvl 70 Crane Vestment
            }

            // Classic level-100 reward for all four profession lines.
            if (level == 100)
                AddReward(client, stream, 700031); // Normal Rainbow Gem

            // Classic level-110 reward is one Dragon Ball after consuming the Moon Box.
            if (level == 110)
                AddReward(client, stream, Database.ItemType.DragonBall);
        }

        private static bool ConsumePromotionMaterial(Client.GameClient client, ServerSockets.Packet stream, int level, bool archer)
        {
            uint itemId = 0;
            uint amount = 1;
            string itemName = null;

            if (level == 40 && archer)
            {
                itemId = Database.ItemType.EuxeniteOre;
                amount = 5;
                itemName = "Euxenite Ores";
            }
            else if (level == 70)
            {
                itemId = Database.ItemType.Emerald;
                itemName = "Emerald";
            }
            else if (level == 100)
            {
                itemId = Database.ItemType.Meteor;
                itemName = "Meteor";
            }
            else if (level == 110)
            {
                // Moon Boxes exist as a small ID family in the database. HasMoonBox
                // returns the concrete box ID owned by the player.
                itemId = client.Inventory.HasMoonBox();
                itemName = "Moon Box";
                if (itemId == 0)
                {
                    MissingMaterial(client, stream, 1, itemName);
                    return false;
                }
            }
            else
            {
                return true;
            }

            if (client.Inventory.GetCountItem(itemId) < amount)
            {
                MissingMaterial(client, stream, amount, itemName);
                return false;
            }

            return client.Inventory.Remove(itemId, amount, stream);
        }

        private static void MissingMaterial(Client.GameClient client, ServerSockets.Packet stream, uint amount, string itemName)
        {
            new Dialog(client, stream)
                .Text("This promotion requires " + amount + " " + itemName + (amount > 1 ? "." : "."))
                .Option("I will return with it.", byte.MaxValue)
                .AddAvatar(0)
                .FinalizeDialog();
        }

        [NpcAttribute(NpcID.PromotionTrojan)]
        public static void TrojanStar(Client.GameClient client, ServerSockets.Packet stream, byte option, string input, uint id)
        {
            if (!InJobCenter(client) || !Database.AtributesStatus.IsTrojan(client.Player.Class))
                return;

            byte cls = client.Player.Class;
            int required = cls <= 10 ? 15 : cls == 11 ? 40 : cls == 12 ? 70 : cls == 13 ? 100 : cls == 14 ? 110 : 0;
            if (required == 0)
            {
                Done(client, stream, "Trojan Master");
                return;
            }
            if (client.Player.Level < required)
            {
                NotReady(client, stream, required);
                return;
            }
            if (option == 0)
            {
                new Dialog(client, stream).Text("You qualify for the next Trojan promotion.")
                    .Option("Promote me.", 1).Option("Not now.", byte.MaxValue).AddAvatar(0).FinalizeDialog();
                return;
            }
            if (option != 1) return;
            if (!ConsumePromotionMaterial(client, stream, required, false)) return;

            client.Player.Class = (byte)(cls + 1);
            AwardClassicPhysicalReward(client, stream, "Trojan", required);
            if (required == 15)
            {
                AddSpell(client, stream, Role.Flags.SpellID.Cyclone);
                AddSpell(client, stream, Role.Flags.SpellID.FastBlader);
                AddSpell(client, stream, Role.Flags.SpellID.ScrenSword);
            }

            string title = required == 15 ? "Trojan" : required == 40 ? "Veteran Trojan" :
                required == 70 ? "Tiger Trojan" : required == 100 ? "Dragon Trojan" : "Trojan Master";
            Done(client, stream, title);
        }

        [NpcAttribute(NpcID.PromotionWarrior)]
        public static void WarriorGod(Client.GameClient client, ServerSockets.Packet stream, byte option, string input, uint id)
        {
            if (!InJobCenter(client) || !Database.AtributesStatus.IsWarrior(client.Player.Class))
                return;

            byte cls = client.Player.Class;
            int required = cls <= 20 ? 15 : cls == 21 ? 40 : cls == 22 ? 70 : cls == 23 ? 100 : cls == 24 ? 110 : 0;
            if (required == 0)
            {
                Done(client, stream, "Warrior Master");
                return;
            }
            if (client.Player.Level < required)
            {
                NotReady(client, stream, required);
                return;
            }
            if (option == 0)
            {
                new Dialog(client, stream).Text("You qualify for the next Warrior promotion.")
                    .Option("Promote me.", 1).Option("Not now.", byte.MaxValue).AddAvatar(0).FinalizeDialog();
                return;
            }
            if (option != 1) return;
            if (!ConsumePromotionMaterial(client, stream, required, false)) return;

            client.Player.Class = (byte)(cls + 1);
            AwardClassicPhysicalReward(client, stream, "Warrior", required);
            if (required == 15)
            {
                AddSpell(client, stream, Role.Flags.SpellID.Superman);
                AddSpell(client, stream, Role.Flags.SpellID.Shield);
                AddSpell(client, stream, Role.Flags.SpellID.Roar);
            }

            string title = required == 15 ? "Warrior" : required == 40 ? "Brass Warrior" :
                required == 70 ? "Silver Warrior" : required == 100 ? "Gold Warrior" : "Warrior Master";
            Done(client, stream, title);
        }

        [NpcAttribute(NpcID.PromotionArcher)]
        public static void ArcherGod(Client.GameClient client, ServerSockets.Packet stream, byte option, string input, uint id)
        {
            if (!InJobCenter(client) || !Database.AtributesStatus.IsArcher(client.Player.Class))
                return;

            byte cls = client.Player.Class;
            int required = cls <= 40 ? 15 : cls == 41 ? 40 : cls == 42 ? 70 : cls == 43 ? 100 : cls == 44 ? 110 : 0;
            if (required == 0)
            {
                Done(client, stream, "Archer Master");
                return;
            }
            if (client.Player.Level < required)
            {
                NotReady(client, stream, required);
                return;
            }
            if (option == 0)
            {
                new Dialog(client, stream).Text("You qualify for the next Archer promotion.")
                    .Option("Promote me.", 1).Option("Not now.", byte.MaxValue).AddAvatar(0).FinalizeDialog();
                return;
            }
            if (option != 1) return;
            if (!ConsumePromotionMaterial(client, stream, required, true)) return;

            client.Player.Class = (byte)(cls + 1);
            AwardClassicPhysicalReward(client, stream, "Archer", required);
            if (required == 15)
                AddSpell(client, stream, Role.Flags.SpellID.XpFly);
            else if (required == 70)
                AddSpell(client, stream, Role.Flags.SpellID.Fly);

            string title = required == 15 ? "Archer" : required == 40 ? "Eagle Archer" :
                required == 70 ? "Tiger Archer" : required == 100 ? "Dragon Archer" : "Archer Master";
            Done(client, stream, title);
        }

        [NpcAttribute(NpcID.PromotionTaoist)]
        public static void TaoistMoon(Client.GameClient client, ServerSockets.Packet stream, byte option, string input, uint id)
        {
            if (!InJobCenter(client) || !Database.AtributesStatus.IsTaoist(client.Player.Class))
                return;

            byte cls = client.Player.Class;
            var dialog = new Dialog(client, stream);

            // Intern Taoist -> Taoist
            if (cls == 100)
            {
                if (client.Player.Level < 15) { NotReady(client, stream, 15); return; }
                if (option == 0)
                {
                    dialog.Text("You qualify to become a Taoist.").Option("Promote me.", 1)
                        .Option("Not now.", byte.MaxValue).AddAvatar(0).FinalizeDialog();
                    return;
                }
                if (option == 1)
                {
                    client.Player.Class = 101;
                    AwardClassicPhysicalReward(client, stream, "Taoist", 15);
                    AddSpell(client, stream, Role.Flags.SpellID.Thunder);
                    AddSpell(client, stream, Role.Flags.SpellID.Cure);
                    Done(client, stream, "Taoist");
                }
                return;
            }

            // Taoist -> Water/Fire Taoist
            if (cls == 101)
            {
                if (client.Player.Level < 40) { NotReady(client, stream, 40); return; }
                if (option == 0)
                {
                    dialog.Text("At level 40 a Taoist must choose a path. This choice determines your profession line.")
                        .Option("Water Taoist.", 2).Option("Fire Taoist.", 3)
                        .Option("Not now.", byte.MaxValue).AddAvatar(0).FinalizeDialog();
                    return;
                }
                if (option == 2)
                {
                    client.Player.Class = 132;
                    AwardClassicPhysicalReward(client, stream, "Taoist", 40);
                    AddSpell(client, stream, Role.Flags.SpellID.Revive);
                    AddSpell(client, stream, Role.Flags.SpellID.HealingRain);
                    Done(client, stream, "Water Taoist");
                }
                else if (option == 3)
                {
                    client.Player.Class = 142;
                    AwardClassicPhysicalReward(client, stream, "Taoist", 40);
                    AddSpell(client, stream, Role.Flags.SpellID.Vulcano);
                    Done(client, stream, "Fire Taoist");
                }
                return;
            }

            bool water = Database.AtributesStatus.IsWater(cls);
            bool fire = Database.AtributesStatus.IsFire(cls);
            if (!water && !fire)
                return;

            int required;
            byte nextClass;
            if (water)
            {
                required = cls == 132 ? 70 : cls == 133 ? 100 : cls == 134 ? 110 : 0;
                nextClass = (byte)(cls + 1);
            }
            else
            {
                required = cls == 142 ? 70 : cls == 143 ? 100 : cls == 144 ? 110 : 0;
                nextClass = (byte)(cls + 1);
            }

            if (required == 0)
            {
                Done(client, stream, water ? "Water Saint" : "Fire Saint");
                return;
            }
            if (client.Player.Level < required) { NotReady(client, stream, required); return; }
            if (option == 0)
            {
                dialog.Text("You qualify for your next Taoist promotion.")
                    .Option("Promote me.", 1).Option("Not now.", byte.MaxValue).AddAvatar(0).FinalizeDialog();
                return;
            }
            if (option != 1) return;
            if (!ConsumePromotionMaterial(client, stream, required, false)) return;

            client.Player.Class = nextClass;
            AwardClassicPhysicalReward(client, stream, "Taoist", required);
            if (water && required == 70)
                AddSpell(client, stream, Role.Flags.SpellID.Pray);

            string title = water
                ? (required == 70 ? "Water Wizard" : required == 100 ? "Water Master" : "Water Saint")
                : (required == 70 ? "Fire Wizard" : required == 100 ? "Fire Master" : "Fire Saint");
            Done(client, stream, title);
        }
    }
}
