using System;
using System.Linq;

namespace GameServer.Game.MsgServer
{
    public struct MsgLoginClient
    {
        public uint AccountHash;
        public uint Key;
        [PacketAttribute((ushort)GamePackets.LoginGame)]
        public unsafe static void LoginGame(Client.GameClient client, ServerSockets.Packet packet)
        {
            uint[] Decrypt = Program.transferCipher.Decrypt(new uint[] { packet.ReadUInt32(), packet.ReadUInt32() });
            client.OnLogin = new MsgLoginClient()
            {
                Key = Decrypt[0],
                AccountHash = Decrypt[1],
            };
            client.ClientFlag |= Client.ServerFlag.OnLoggion;
            Database.ServerDatabase.LoginQueue.TryEnqueue(client);
        }
        public unsafe static void LoginHandler(Client.GameClient client, MsgLoginClient packet)
        {
            client.ClientFlag &= ~Client.ServerFlag.OnLoggion;
            if (client.Socket != null && client.Socket.RemoteIp == "NONE") return;
            try
            {
                var pool = Pool.GamePoll.Values.ToArray();
                #region Banned
                string BanMessaje;
                if (Database.SystemBanned.IsBanned(client.Socket.RemoteIp, out BanMessaje) || Database.SystemBannedAccount.IsBanned(client.OnLogin.Key, out BanMessaje))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        client.Send(new MsgServer.MsgMessage("You are banned " + BanMessaje, "ALLUSERS", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Dialog).GetArray(stream));
                        return;
                    }
                }
                #endregion
                #region Character Created
                if ((client.ClientFlag & Client.ServerFlag.CreateCharacterSucces) == Client.ServerFlag.CreateCharacterSucces)
                {
                    if (Database.ServerDatabase.AllowCreate(client.ConnectionUID))
                    {
                        client.ClientFlag &= ~Client.ServerFlag.CreateCharacterSucces;
                        if (client.Player.MyChi == null)
                        {
                            client.Player.MyChi = new Role.Instance.Chi(client.Player.UID);
                        }
                        if (client.Player.SubClass == null)
                            client.Player.SubClass = new Role.Instance.SubClass();
                        if (client.Player.Flowers == null)
                        {
                            client.Player.Flowers = new Role.Instance.Flowers(client.Player.UID, client.Player.Name);
                            client.Player.Flowers.FreeFlowers = 1;
                        }
                        if (client.Player.Nobility == null)
                            client.Player.Nobility = new Role.Instance.Nobility(client);
                        if (client.Player.Associate == null)
                        {
                            client.Player.Associate = new Role.Instance.AssociateGS.MyAsociats(client.Player.UID);
                            client.Player.Associate.MyClient = client;
                            client.Player.Associate.Online = true;
                        }
                        if (client.OnLogin.AccountHash == 2)
                        {
                            if (client.Player.Name.Length < 13 && !client.Player.Name.Contains("[PM]"))
                            {
                                client.Player.Name = client.Player.Name + "[PM]";
                            }
                        }
                        Database.ServerDatabase.CreateCharacter(client);
                        Database.ServerDatabase.SaveClient(client);
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            LoadPlayer(client, stream);
                        }
                        return;
                    }
                }
                #endregion
                if ((client.ClientFlag & Client.ServerFlag.AcceptLogin) == Client.ServerFlag.AcceptLogin) return;
                var login = client.OnLogin;
                client.ConnectionUID = client.OnLogin.Key;
                #region Character Gonna be Created
                if (Database.ServerDatabase.AllowCreate(client.ConnectionUID))
                {
                    client.ClientFlag |= Client.ServerFlag.CreateCharacter;
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        client.Send(new MsgServer.MsgMessage("NEW_ROLE", "ALLUSERS", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Dialog).GetArray(stream));
                    }
                    return;
                }
                #endregion
                #region Normal Behavior
                if ((client.ClientFlag & Client.ServerFlag.AcceptLogin) != Client.ServerFlag.AcceptLogin)
                {
                    if (Pool.DisconnectPool.ContainsKey(client.ConnectionUID))
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            client.Send(new MsgServer.MsgMessage("Please try again after a minute!", "ALLUSERS", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Dialog).GetArray(stream));
                        }
                        return;
                    }
                    Client.GameClient InGame = null;
                    if (Pool.GamePoll.TryGetValue(client.ConnectionUID, out InGame))
                    {
                        #region Already logged in
                        if (InGame.Player != null)
                        {
                            if (InGame.Player.UID == 0)
                            {
                                Pool.GamePoll.TryRemove(client.ConnectionUID, out InGame);
                                if (InGame != null && InGame.Player != null)
                                {
                                    if (InGame.Map != null)
                                        InGame.Map.Denquer(InGame);
                                }
                                InGame.Socket.Disconnect();
                            }
                        }
                        if (InGame.TRyDisconnect-- == 0)
                        {
                            InGame.SendSysMesage("You've been disconnected due to logging your account on another pc.", MsgMessage.ChatMode.TopLeftSystem, MsgMessage.MsgColor.red, false, true);
                            InGame.Socket.OnDisconnect = null;
                            InGame.Socket.Disconnect();
                            Program.Game_Disconnect(InGame.Socket);
                            //while (Pool.DisconnectPool.ContainsKey(InGame.ConnectionUID))
                            //    System.Threading.Thread.Sleep(50);
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                LoadPlayer(client, stream);
                            }
                        }
                        else
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Send(new MsgServer.MsgMessage("This account is already logged on. Please try again later!", "ALLUSERS", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Dialog).GetArray(stream));
                            }
                            InGame.SendSysMesage("Someone has tried to login to your account, Next time you'll get disconnected.", MsgMessage.ChatMode.TopLeftSystem, MsgMessage.MsgColor.red, false, true);
                        }
                        #endregion
                    }
                    else
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                             LoadPlayer(client, stream);
                        }
                    }
                }
                #endregion
            }
            catch (Exception e) { Console.WriteException(e); }
        }
        public unsafe static void LoadPlayer(Client.GameClient client, ServerSockets.Packet packet)
        {
            if ((client.ClientFlag & Client.ServerFlag.CreateCharacterSucces) != Client.ServerFlag.CreateCharacterSucces)
                Database.ServerDatabase.LoadCharacter(client, client.ConnectionUID);
            try { Game.Era1.Era1Migration.Run(client); } // itens posteriores ao 5017 que ja estavam com o jogador
            catch (Exception e) { Console.WriteException(e); }
            client.Send(new MsgServer.MsgMessage("ANSWER_OK", "ALLUSERS", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Dialog).GetArray(packet));
            try
            {
                if (Database.GroupServerList.MyServerInfo == null) { Console.WriteLine("Cannot find the Server on Realm config file: client_config.ini"); client.Socket.Disconnect(); return; }
                client.Player.ServerID = (ushort)Database.GroupServerList.MyServerInfo.ID;
                if (client.Player.VipLevel == 5)
                    client.Player.VipLevel = 0;
                client.Send(packet.HeroInfo(client.Player));
                MsgChiInfo.MsgHandleChi.SendInfo(client, MsgChiInfo.Action.Upgrade, client, 142);

                client.Send(packet.FlowerCreate(Role.Core.IsBoy(client.Player.Body) ? MsgFlower.FlowerAction.Flower : MsgFlower.FlowerAction.FlowerSender
                    , 0, 0, client.Player.Flowers.RedRoses, client.Player.Flowers.RedRoses.Amount2day
                    , client.Player.Flowers.Lilies, client.Player.Flowers.Lilies.Amount2day
                    , client.Player.Flowers.Orchids, client.Player.Flowers.Orchids.Amount2day
                    , client.Player.Flowers.Tulips, client.Player.Flowers.Tulips.Amount2day));


                if (client.Player.Flowers.FreeFlowers > 0)
                {
                    client.Send(packet.FlowerCreate(Role.Core.IsBoy(client.Player.Body)
                        ? MsgFlower.FlowerAction.FlowerSender : MsgFlower.FlowerAction.Flower
                        , 0, 0, client.Player.Flowers.FreeFlowers));
                }

                client.Send(packet.NobilityIconCreate(client.Player.Nobility));

                if (client.Player.Achievement != null)
                    client.Player.Achievement.Send(client, packet);

                if (client.Player.BlessTime > 0)
                    client.Player.SendUpdate(packet, client.Player.BlessTime, MsgUpdate.DataType.LuckyTimeTimer);

                client.Player.ProtectAttack(1000 * 10);//10 Seconds
                client.Player.CreateHeavenBlessPacket(packet, true);


                if (MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.QuizShow
                    && MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
                    MsgTournaments.MsgSchedules.CurrentTournament.Join(client, packet);


                if (client.Player.DExpTime > 0)
                    client.Player.CreateExtraExpPacket(packet);


                if (client.Player.MyClan != null)
                {
                    client.Player.MyClan.SendThat(packet, client);

                    foreach (var ally in client.Player.MyClan.Ally.Values)
                        client.Send(packet.ClanRelationCreate(client.Player.MyClan.ID, ally.Name, ally.LeaderName, MsgClan.Info.AddAlly));
                    foreach (var enemy in client.Player.MyClan.Enemy.Values)
                        client.Send(packet.ClanRelationCreate(client.Player.MyClan.ID, enemy.Name, enemy.LeaderName, MsgClan.Info.AddEnemy));
                }

                client.Equipment.Show(packet);
                client.Inventory.ShowALL(packet);
                //send chi------------- query
                foreach (var chipower in client.Player.MyChi)
                    client.Player.MyChi.SendQueryUpdate(client, chipower, packet);

                //send confiscator items
                foreach (var item in client.Confiscator.RedeemContainer.Values)
                {
                    var Dataitem = item;
                    Dataitem.DaysLeft = (uint)(TimeSpan.FromTicks(DateTime.Now.Ticks).Days - TimeSpan.FromTicks(Role.Instance.Confiscator.GetTimer(item.Date).Ticks).Days);
                    if (Dataitem.DaysLeft > 7)
                    {
                        Dataitem.Action = MsgDetainedItem.ContainerType.RewardCps;
                    }
                    if (Dataitem.Action != MsgDetainedItem.ContainerType.RewardCps)
                    {
                        Dataitem.Action = MsgDetainedItem.ContainerType.DetainPage;
                        Dataitem.Send(client, packet);
                    }
                    if (Dataitem.Action == MsgDetainedItem.ContainerType.RewardCps)
                        client.Confiscator.RedeemContainer.TryRemove(item.UID, out Dataitem);
                }
                foreach (var item in client.Confiscator.ClaimContainer.Values)
                {
                    var Dataitem = item;
                    Dataitem.DaysLeft = (uint)(TimeSpan.FromTicks(DateTime.Now.Ticks).Days - TimeSpan.FromTicks(Role.Instance.Confiscator.GetTimer(item.Date).Ticks).Days);
                    if (Dataitem.RewardConquerPoints != 0)
                    {
                        Dataitem.Action = MsgDetainedItem.ContainerType.RewardCps;
                    }
                    Dataitem.Send(client, packet);
                    client.Confiscator.ClaimContainer[item.UID] = Dataitem;
                }
                //-------------

                if (MsgTournaments.MsgSchedules.GuildWar.RewardDeputiLeader.Contains(client.Player.UID))
                    client.Player.AddFlag(MsgUpdate.Flags.TopDeputyLeader, Role.StatusFlagsBigVector32.PermanentFlag, false);
                if (MsgTournaments.MsgSchedules.GuildWar.RewardLeader.Contains(client.Player.UID))
                    client.Player.AddFlag(MsgUpdate.Flags.TopGuildLeader, Role.StatusFlagsBigVector32.PermanentFlag, false);
                #region EliteGuildWar
                if (MsgTournaments.MsgSchedules.EliteGuildWar.RewardDeputiLeader.Contains(client.Player.UID))
                    client.Player.AddFlag(MsgUpdate.Flags.ConuqerSuperUnderBlue, Role.StatusFlagsBigVector32.PermanentFlag, false);


                if (MsgTournaments.MsgSchedules.EliteGuildWar.RewardLeader.Contains(client.Player.UID))
                    client.Player.AddFlag(MsgUpdate.Flags.ConuqerSuperYellow, Role.StatusFlagsBigVector32.PermanentFlag, false);
                #endregion
                if (client.Player.CursedTimer > 0)
                {
                    client.Player.AddCursed(client.Player.CursedTimer);
                }

                client.Send(packet.ServerTimerCreate());


                MsgTournaments.MsgSchedules.ClassPkWar.LoginClient(client);
                MsgTournaments.MsgSchedules.ElitePkTournament.GetTitle(client, packet);
                MsgTournaments.MsgSchedules.TeamPkTournament.GetTitle(client, packet);
                MsgTournaments.MsgSchedules.SkillTeamPkTournament.GetTitle(client, packet);

                if (MsgTournaments.MsgSchedules.CouplesPKWar.Winner1 == client.Player.Name ||
                    MsgTournaments.MsgSchedules.CouplesPKWar.Winner2 == client.Player.Name)
                    client.Player.AddFlag(MsgUpdate.Flags.TopSpouse, Role.StatusFlagsBigVector32.PermanentFlag, false);

                if (MsgTournaments.MsgBroadcast.CurrentBroadcast.EntityID != 1)
                {
                    client.Send(new MsgServer.MsgMessage(MsgTournaments.MsgBroadcast.CurrentBroadcast.Message
                        , "ALLUSERS"
                        , MsgTournaments.MsgBroadcast.CurrentBroadcast.EntityName
                        , MsgServer.MsgMessage.MsgColor.red
                        , MsgServer.MsgMessage.ChatMode.BroadcastMessage
                        ).GetArray(packet));
                }


                if (client.Player.DonationPoints > 0)
                    client.Player.SendUpdate(packet, client.Player.DonationPoints, MsgUpdate.DataType.RaceShopPoints);
                client.Player.UpdateVip(packet);
                //update merchant
                client.Player.SendUpdate(packet, 255, MsgUpdate.DataType.Merchant);
                ActionQuery action = new ActionQuery()
                {
                    ObjId = client.Player.UID,
                    Type = (ActionType)157,
                    dwParam = 2
                };

                client.Send(packet.ActionCreate(&action));
                client.Send(packet.ServerConfig());
                if (client.Player.SecurityPassword != 0)
                {
                    client.Send(packet.SecondaryPasswordCreate(MsgSecondaryPassword.ActionID.PasswordCorrect, 1, 0));
                }
                else
                    client.Player.IsCheckedPass = true;

                MsgTournaments.MsgSchedules.PkWar.AddTop(client);

                // Welcome Messages.
                client.SendSysMesage("Welcome to " + ServerConfig.ServerName + ", visit the Guide Npc for help!", MsgMessage.ChatMode.Talk);
                client.SendSysMesage("Official Site: " + ServerConfig.OfficialWebSite, MsgMessage.ChatMode.Talk);
                client.SendSysMesage("Enjoy " + ServerConfig.ServerName + ".", MsgMessage.ChatMode.Talk);
               

                client.SendWhisper($"Welcome to server {ServerConfig.ServerName}", ServerConfig.ServerOwner, client.Player.Name);

                client.FruitsMobs = 0;
                client.CityMobs = 0;

                if (client.Player.VipLevel >= 1)
                {
                    TimeSpan timer1 = new TimeSpan(client.Player.ExpireVip.Ticks);
                    TimeSpan Now2 = new TimeSpan(DateTime.Now.Ticks);
                    int days_left = (int)(timer1.TotalDays - Now2.TotalDays);
                    int hour_left = (int)(timer1.TotalHours - Now2.TotalHours);
                    int left_minutes = (int)(timer1.TotalMinutes - Now2.TotalMinutes);
                    if (days_left > 0)
                        client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in : " + days_left + " days.", MsgMessage.ChatMode.System);
                    else if (hour_left > 0)
                        client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in : " + hour_left + " hours.", MsgMessage.ChatMode.System);
                    else if (left_minutes > 0)
                        client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in : " + left_minutes + " minutes.", MsgMessage.ChatMode.System);

                }
                if (client.Player.PayNobilitySystem.IsActive)
                {

                    TimeSpan timer1 = new TimeSpan(client.Player.PayNobilitySystem.PeriodTime.Ticks);
                    TimeSpan Now2 = new TimeSpan(DateTime.Now.Ticks);
                    int days_left = (int)(timer1.TotalDays - Now2.TotalDays);
                    int hour_left = (int)(timer1.TotalHours - Now2.TotalHours);
                    int left_minutes = (int)(timer1.TotalMinutes - Now2.TotalMinutes);
#if Arabic
                        if (days_left > 0)
                            client.SendSysMesage("Your VIP 6 will expire in : " + days_left + "(Days) .", MsgMessage.ChatMode.System);
                        else if(hour_left > 0)
                            client.SendSysMesage("Your VIP 6 will expire in : " + hour_left + "(Hours) .", MsgMessage.ChatMode.System);
                        else if (left_minutes > 0)
                            client.SendSysMesage("Your VIP 6 will expire in : " + left_minutes + "(Minutes) .", MsgMessage.ChatMode.System);
#else
                    if (days_left > 0)
                        client.SendSysMesage("Your nobility expires in : " + days_left + " Days.", MsgMessage.ChatMode.System);
                    else if (hour_left > 0)
                        client.SendSysMesage("Your nobility expires in : " + hour_left + " hours.", MsgMessage.ChatMode.System);
                    else if (left_minutes > 0)
                        client.SendSysMesage("Your nobility expires in " + left_minutes + " minutes.", MsgMessage.ChatMode.System);
#endif

                }

                if (Database.AtributesStatus.IsTrojan(client.Player.Class)
                    || Database.AtributesStatus.IsTrojan(client.Player.FirstClass)
                    || Database.AtributesStatus.IsTrojan(client.Player.SecondClass))
                {
                    if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Cyclone))
                        client.MySpells.Add(packet, (ushort)Role.Flags.SpellID.Cyclone);
                }


                if (client.Inventory.HaveSpace(1))
                {
                    foreach (var item in client.Equipment.ClientItems.Values)
                    {
                        if (item.Position >= (uint)Role.Flags.ConquerItem.Head && item.Position <= (uint)Role.Flags.ConquerItem.RidingCrop)
                        {
                            if (client.Inventory.HaveSpace(1) && item.Position == (uint)Role.Flags.ConquerItem.RightWeapon
                                && item.Position == (uint)Role.Flags.ConquerItem.LeftWeapon)
                            {
                                if (!Database.ItemType.IsShield(item.ITEM_ID))
                                {
                                    if (!Database.ItemType.Equipable(item.ITEM_ID, client))
                                    {
                                        client.Equipment.Remove((Role.Flags.ConquerItem)item.Position, packet);
                                    }
                                }
                            }
                        }
                        else if (item.Position >= (uint)Role.Flags.ConquerItem.AleternanteHead && item.Position <= (uint)Role.Flags.ConquerItem.AlternateGarment)
                        {
                            if (client.Inventory.HaveSpace(1) && item.Position == (uint)Role.Flags.ConquerItem.AleternanteRightWeapon
                                && item.Position == (uint)Role.Flags.ConquerItem.AleternanteLeftWeapon)
                            {
                                if (!Database.ItemType.IsShield(item.ITEM_ID))
                                {
                                    if (!Database.ItemType.Equipable(item.ITEM_ID, client))
                                    {
                                        client.Equipment.RemoveAlternante((Role.Flags.ConquerItem)item.Position, packet);
                                    }
                                }
                            }
                        }
                    }
                }
                client.Player.Mana = (ushort)client.Status.MaxMana;
                client.Warehouse.SendReturnedItems(packet);
                client.ClientFlag &= ~Client.ServerFlag.AcceptLogin;
                client.ClientFlag |= Client.ServerFlag.LoginFull;
                if (client.Player.ArenaCPS != 0)
                {
                    client.Player.Money += client.Player.ArenaCPS;
                    client.Player.MessageBox($"You got {client.Player.ArenaCPS} Silver for ranking in top10 arena.", null, null);
                    client.Player.ArenaCPS = 0;
                }
                client.SendWhisper("Help us to make grate game play, If u find any bug report and u will be rewarded..", ServerConfig.ServerOwner, client.Player.Name);
                Program.CallBack.Register(client);
                Pool.GamePoll.TryAdd(client.ConnectionUID, client);
                Console.WriteLine(client.Player.Name + $" has login [{client.Socket.RemoteIp}] [UID: {client.Player.UID}]", ConsoleColor.Yellow);
                client.IP = client.Socket.RemoteIp;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
