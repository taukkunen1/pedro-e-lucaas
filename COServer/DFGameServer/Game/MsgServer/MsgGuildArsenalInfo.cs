namespace GameServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe ServerSockets.Packet GuildArsenalInfoCreate(this ServerSockets.Packet stream, Role.Instance.Guild.Member Member, Role.Instance.Guild guild)
        {
            stream.InitWriter();

            stream.Write(0);
            stream.Write(guild.MyArsenal.GetFullBp());
            stream.Write(Member.ArsenalDonation);
            stream.Write(guild.ShareMemberPotency(Member.Rank));
            if (guild.MyArsenal == null)
            {
                stream.Finalize(GamePackets.ArsenalInfo);
                return stream;
            }

            stream.Write((uint)Role.Instance.Guild.Arsenal.Headgear);
            stream.Write(0);
            if (guild.MyArsenal.IsUnlock(Role.Instance.Guild.Arsenal.Headgear))
            {
                stream.Write(guild.MyArsenal.GetTypPotency(Role.Instance.Guild.Arsenal.Headgear));
                stream.Write(guild.MyArsenal.GetTypDonation(Role.Instance.Guild.Arsenal.Headgear));
                stream.Write((uint)1);
            }
            else
            {
                stream.ZeroFill(20);
            }

            stream.Write((uint)Role.Instance.Guild.Arsenal.Armor);
            if (guild.MyArsenal.IsUnlock(Role.Instance.Guild.Arsenal.Armor))
            {
                stream.Write(guild.MyArsenal.GetTypPotency(Role.Instance.Guild.Arsenal.Armor));
                stream.Write(guild.MyArsenal.GetTypDonation(Role.Instance.Guild.Arsenal.Armor));
                stream.Write((uint)1);
            }
            else
            {
                stream.ZeroFill(20);
            }

            stream.Write((uint)Role.Instance.Guild.Arsenal.Weapon);
            if (guild.MyArsenal.IsUnlock(Role.Instance.Guild.Arsenal.Weapon))
            {
                stream.Write(guild.MyArsenal.GetTypPotency(Role.Instance.Guild.Arsenal.Weapon));
                stream.Write(guild.MyArsenal.GetTypDonation(Role.Instance.Guild.Arsenal.Weapon));
                stream.Write((uint)1);
            }
            else
            {
                stream.ZeroFill(20);
            }

            stream.Write((uint)Role.Instance.Guild.Arsenal.Ring);
            if (guild.MyArsenal.IsUnlock(Role.Instance.Guild.Arsenal.Ring))
            {
                stream.Write(guild.MyArsenal.GetTypPotency(Role.Instance.Guild.Arsenal.Ring));
                stream.Write(guild.MyArsenal.GetTypDonation(Role.Instance.Guild.Arsenal.Ring));
                stream.Write((uint)1);
            }
            else
            {
                stream.ZeroFill(20);
            }

            stream.Write((uint)Role.Instance.Guild.Arsenal.Boots);
            if (guild.MyArsenal.IsUnlock(Role.Instance.Guild.Arsenal.Boots))
            {
                stream.Write(guild.MyArsenal.GetTypPotency(Role.Instance.Guild.Arsenal.Boots));
                stream.Write(guild.MyArsenal.GetTypDonation(Role.Instance.Guild.Arsenal.Boots));
                stream.Write((uint)1);
            }
            else
            {
                stream.ZeroFill(20);
            }

            stream.Write((uint)Role.Instance.Guild.Arsenal.Necklace);
            if (guild.MyArsenal.IsUnlock(Role.Instance.Guild.Arsenal.Necklace))
            {
                stream.Write(guild.MyArsenal.GetTypPotency(Role.Instance.Guild.Arsenal.Necklace));
                stream.Write(guild.MyArsenal.GetTypDonation(Role.Instance.Guild.Arsenal.Necklace));
                stream.Write((uint)1);
            }
            else
            {
                stream.ZeroFill(20);
            }

            stream.Write((uint)Role.Instance.Guild.Arsenal.Fan);
            if (guild.MyArsenal.IsUnlock(Role.Instance.Guild.Arsenal.Fan))
            {
                stream.Write(guild.MyArsenal.GetTypPotency(Role.Instance.Guild.Arsenal.Fan));
                stream.Write(guild.MyArsenal.GetTypDonation(Role.Instance.Guild.Arsenal.Fan));
                stream.Write((uint)1);
            }
            else
            {
                stream.ZeroFill(20);
            }

            stream.Write((uint)Role.Instance.Guild.Arsenal.Tower);
            if (guild.MyArsenal.IsUnlock(Role.Instance.Guild.Arsenal.Tower))
            {
                stream.Write(guild.MyArsenal.GetTypPotency(Role.Instance.Guild.Arsenal.Tower));
                stream.Write(guild.MyArsenal.GetTypDonation(Role.Instance.Guild.Arsenal.Tower));
                stream.Write((uint)1);
            }
            else
            {
                stream.ZeroFill(20);
            }

            stream.Finalize(GamePackets.ArsenalInfo);
            return stream;
        }

        public static unsafe void GetArsenalInfo(this ServerSockets.Packet stream, out MsgGuildArsenalInfo.Action action, out uint ArsenalTyp
            , out uint ItemUID, out uint EnchatLevel)
        {
            action =  (MsgGuildArsenalInfo.Action)stream.ReadUInt32();
            ArsenalTyp = stream.ReadUInt32();
            ItemUID = stream.ReadUInt32();
            EnchatLevel = stream.ReadUInt32();
        }
    }
    public unsafe class MsgGuildArsenalInfo
    {
        public enum Action : uint
        {
            UnLock = 0,
            Inscribe = 1,
            Uninscribe = 2,
            Enhance = 3,
            Show = 4
        }


        [PacketAttribute(GamePackets.GetArsenal)]
        public unsafe static void HandlerArsenal(Client.GameClient user, ServerSockets.Packet stream)
        {
            Action Mode;
            uint ArsenalTyp;
            uint ItemUID;
            uint EnchatLevel;

            stream.GetArsenalInfo(out Mode, out ArsenalTyp, out ItemUID, out EnchatLevel);

            switch (Mode)
            {
                case Action.Enhance:
                    {
                        if (user.Player.MyGuild == null || user.Player.MyGuildMember == null)
                            break;
                        if (ArsenalTyp == 0)
                            ArsenalTyp = 8;
                        var guild = user.Player.MyGuild;
                        var arsenal = guild.MyArsenal.GetArsenal((byte)ArsenalTyp);
                        if (arsenal.IsDone) return;
                        uint cost = (uint)(20000000 + EnchatLevel * 40000000);
                        if (guild.Info.SilverFund >= cost)
                        {
                            guild.Info.SilverFund -= cost;
                            arsenal.Enchant = (byte)EnchatLevel;
                            arsenal.CreateArsenalPotency();
                        }
                        user.Player.GuildBattlePower = user.Player.MyGuild.ShareMemberPotency(user.Player.MyGuildMember.Rank);
                        break;
                    }
                case Action.Uninscribe:
                    {
                        if (user.Player.MyGuild == null || user.Player.MyGuildMember == null)
                            break;
                        if (ArsenalTyp == 0)
                            ArsenalTyp = 8;
                        Role.Instance.Guild.Arsenal.InscribeItem item = null;
                        if (user.Player.MyGuild.MyArsenal.TryGetArsenal((byte)ArsenalTyp, ItemUID, out item))
                        {
                            if (item.BaseItem.Inscribed == 0)
                                break;
                            if (user.Player.MyGuild.MyArsenal.Remove((byte)ArsenalTyp, ItemUID))
                            {
                                MsgGameItem my_item = null;
                                if (user.TryGetItem(ItemUID, out my_item))
                                {
                                    my_item.Inscribed = 0;
                                    my_item.Mode = Role.Flags.ItemMode.Update;
                                    my_item.Send(user,stream);

                                    user.Player.MyGuildMember.ArsenalDonation -= GetItemDonation(my_item);
                                }
                                user.Player.GuildBattlePower = user.Player.MyGuild.ShareMemberPotency(user.Player.MyGuildMember.Rank);
                            }
                        }
                        break;
                    }
                case Action.Inscribe:
                    {

                        if (user.Player.MyGuild == null || user.Player.MyGuildMember == null)
                            break;
                        if (ArsenalTyp == 0)
                            ArsenalTyp = 8;

                        MsgGameItem item = null;
                        if (user.Inventory.TryGetItem(ItemUID, out item))
                        {
                            if (item.Inscribed != 0)
                                break;

                            if (user.Player.MyGuild.MyArsenal.Add((byte)ArsenalTyp, new Role.Instance.Guild.Arsenal.InscribeItem() { BaseItem = item, Name = user.Player.Name, UID = user.Player.UID }))
                            {
                                item.Inscribed = 1;
                                item.Mode = Role.Flags.ItemMode.Update;
                                item.Send(user,stream);
                                user.Player.MyGuildMember.ArsenalDonation += GetItemDonation(item);
                                user.Player.GuildBattlePower = user.Player.MyGuild.ShareMemberPotency(user.Player.MyGuildMember.Rank);
                            }
                        }
                        break;
                    }
                case Action.UnLock:
                    {
                        if (user.Player.MyGuild == null || user.Player.MyGuildMember == null)
                            break;
                        if (user.Player.MyGuildMember.Rank == Role.Flags.GuildMemberRank.GuildLeader)
                        {
                            if (ArsenalTyp >= 8) break;
                            if (user.Player.MyGuild.Info.SilverFund > NeedDonetion((byte)user.Player.MyGuild.MyArsenal.Unlockers()))
                            {

                                user.Player.MyGuild.Info.SilverFund -= NeedDonetion((byte)user.Player.MyGuild.MyArsenal.Unlockers());
                                if (ArsenalTyp == 0)
                                    ArsenalTyp = 8;
                                user.Player.MyGuild.MyArsenal.Unlock((byte)ArsenalTyp);

                                user.Send(stream.GuildArsenalInfoCreate(user.Player.MyGuildMember, user.Player.MyGuild));
                            }
                        }
                        break;
                    }
                case Action.Show:
                    {
                        if (user.Player.MyGuild == null || user.Player.MyGuildMember == null)
                            break;

                        user.Send(stream.GuildArsenalInfoCreate(user.Player.MyGuildMember, user.Player.MyGuild));
                        break;
                    }

            }
        }
        public static uint NeedDonetion(byte Unlocked)
        {
            switch (Unlocked)
            {
                case 0:
                case 1: return 5000000;
                case 2:
                case 3:
                case 4:
                case 5: return 10000000;
                case 6: return 15000000;
                case 7: return 20000000;
                default: return 0;
            }
        }
        private static uint GetItemDonation(MsgGameItem GameItem)
        {
            uint Return = 0;
            int id = (int)(GameItem.ITEM_ID % 10);
            switch (id)
            {
                case 8: Return = 1000; break;
                case 9: Return = 16660; break;
            }
            if (GameItem.SocketOne > 0 && GameItem.SocketTwo == 0)
                Return += 33330;
            if (GameItem.SocketOne > 0 && GameItem.SocketTwo > 0)
                Return += 133330;

            switch (GameItem.Plus)
            {
                case 1: Return += 90; break;
                case 2: Return += 490; break;
                case 3: Return += 1350; break;
                case 4: Return += 4070; break;
                case 5: Return += 12340; break;
                case 6: Return += 37030; break;
                case 7: Return += 111110; break;
                case 8: Return += 333330; break;
                case 9: Return += 1000000; break;
                case 10: Return += 1033330; break;
                case 11: Return += 1101230; break;
                case 12: Return += 1212340; break;
                default: break;
            }

            return Return;
        }
    }
}
