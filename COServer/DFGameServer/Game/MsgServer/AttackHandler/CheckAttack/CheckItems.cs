namespace GameServer.Game.MsgServer.AttackHandler.CheckAttack
{
    public class CheckItems
    {
        private static bool EnableDurabilityReduceOnAttackAndDefense = false;

        public static void AttackDurability(Client.GameClient client, ServerSockets.Packet stream)
        {
            if (!EnableDurabilityReduceOnAttackAndDefense)
            {
                return;
            }
            int PercentRateDuraBreak = 4;
            if (client.Player.Rate(PercentRateDuraBreak))
            {
                bool dura_zero = false;
                foreach (var item in client.Equipment.CurentEquip)
                {
                    if (item != null)
                    {
                        if (item.Position == (ushort)Role.Flags.ConquerItem.RightWeapon
                            || item.Position == (ushort)Role.Flags.ConquerItem.LeftWeapon
                            || item.Position == (ushort)Role.Flags.ConquerItem.AleternanteRightWeapon
                            || item.Position == (ushort)Role.Flags.ConquerItem.AleternanteLeftWeapon
                            || item.Position == (ushort)Role.Flags.ConquerItem.Ring
                            || item.Position == (ushort)Role.Flags.ConquerItem.AleternanteRing
                            || item.Position == (ushort)Role.Flags.ConquerItem.Fan
                            || item.Position == (ushort)Role.Flags.ConquerItem.RidingCrop)
                        {
                            int maxDurabilityItem = item.MaximDurability / 100;
                            int durability = (maxDurabilityItem-1);
                            int newDurability = item.MaximDurability - ((durability - maxDurabilityItem) * 100);
                            durability = (newDurability - item.MaximDurability);
                            if (item.Durability > durability)
                                item.Durability -= (ushort)durability;
                            else
                            {
                                item.Durability = 0;
                                dura_zero = true;
                            }
                            item.Mode = Role.Flags.ItemMode.Update;
                        }
                        item.Send(client, stream);
                    }
                }
                if (dura_zero)
                    client.Equipment.QueryEquipment(client.Equipment.Alternante);
            }
        }
       public static void RespouseDurability(Client.GameClient client)
        {
            if (!EnableDurabilityReduceOnAttackAndDefense)
            {
                return;
            }
            int PercentRateDuraBreak = 4;
            #region DurabilityItems On Monster Attack
            if (client.Player.Rate(PercentRateDuraBreak))
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    bool dura_zero = false;
                    foreach (var item in client.Equipment.CurentEquip)
                    {
                        if (item != null)
                        {
                            if (item.Position == (ushort)Role.Flags.ConquerItem.Armor
                                || item.Position == (ushort)Role.Flags.ConquerItem.AleternanteArmor
                                || item.Position == (ushort)Role.Flags.ConquerItem.Necklace
                                || item.Position == (ushort)Role.Flags.ConquerItem.AleternanteNecklace
                                || item.Position == (ushort)Role.Flags.ConquerItem.Boots
                                || item.Position == (ushort)Role.Flags.ConquerItem.AleternanteBoots
                                || item.Position == (ushort)Role.Flags.ConquerItem.Head
                                || item.Position == (ushort)Role.Flags.ConquerItem.AleternanteHead
                                || item.Position == (ushort)Role.Flags.ConquerItem.Tower)
                            {
                                int maxDurabilityItem = item.MaximDurability / 100;
                                int durability = (maxDurabilityItem - 1);
                                int newDurability = item.MaximDurability - ((durability - maxDurabilityItem) * 100);
                                durability = (newDurability - item.MaximDurability);
                                if (item.Durability > durability)
                                    item.Durability -= (ushort)durability;
                                else
                                {
                                    item.Durability = 0;
                                    dura_zero = true;
                                }
                                item.Mode = Role.Flags.ItemMode.Update;
                            }
                            item.Send(client, stream);
                        }
                    }
                    if (dura_zero)
                        client.Equipment.QueryEquipment(client.Equipment.Alternante);
                }
            }
            #endregion
        }
    }
}
