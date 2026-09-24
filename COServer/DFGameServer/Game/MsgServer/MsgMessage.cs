using Core;
using GameServer.Database;
using GameServer.Game.MsgFloorItem;
using GameServer.Game.MsgTournaments;
using GameServer.ServerSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameServer.Game.MsgServer
{
    public class MsgMessage
    {
        public enum MsgColor : uint
        {
            black = 0x000000,// 	0,0,0
            blue = 0x0000ff,// 	0,0,255
            orange = 0xffa500,// 	255,165,0

            white = 0xffffff,//	255,255,255
            whitesmoke = 0xf5f5f5,// 	245,245,245
            yellow = 0xffff00,// 	255,255,0
            yellowgreen = 0x9acd32,//	154,205,50
            violet = 0xee82ee,//	238,130,238
            purple = 0x800080,//	128,0,128
            red = 0xff0000,//	255,0,0
            pink = 0xffc0cb,// 	255,192,203
            lightyellow = 0xffffe0,// 	255,255,224
            cyan = 0x00ffff,// 	0,255,255
            blueviolet = 0x8a2be2,// 	138,43,226
            antiquewhite = 0xfaebd7,// 	250,235,215
        }
        public enum ChatMode : uint
        {
            Talk = 2000,
            Whisper = 2001,
            Team = 2003,
            Guild = 2004,
            TopLeftSystem = 2005,
            Clan = 2006,
            System = 2000,//2007,
            Friend = 2009,
            Center = 2011,
            TopLeft = 2012,
            Die = 2013,
            Service = 2014,
            Tip = 2015,
            CrossServerIcon = 2016,
            Ally = 2025,
            WebSite = 2105,
            World = 2021,
            Qualifier = 2022,
            Study = 2024,
            JianHu = 2026,
            InnerPower = 2027,
            PopUP = 2100,
            Dialog = 2101,
            CrosTheServer = 2402,
            FirstRightCorner = 2108,
            ContinueRightCorner = 2109,
            SystemWhisper = 2110,
            GuildAnnouncement = 2111,

            Agate = 2115,
            BroadcastMessage = 2500,
            Monster = 2600,
            SlideFromRight = 100000,
            HawkMessage = 2104,
            SlideFromRightRedVib = 1000000,
            WhiteVibrate = 10000000
        }

        public string _From;
        public string _To;
        public ChatMode ChatType;
        public uint Color;
        public string __Message;
        public string ServerName = string.Empty;

        public uint Mesh;
        public uint MessageUID1 = 0;
        public uint MessageUID2 = 0;

        public MsgMessage(string _Message, MsgColor _Color, ChatMode _ChatType)
        {
            this.Mesh = 0;
            this.__Message = _Message;
            this._To = "ALL";
            this._From = "SYSTEM";
            this.Color = (uint)_Color;
            this.ChatType = _ChatType;
        }
        public MsgMessage(string _Message, string __To, MsgColor _Color, ChatMode _ChatType)
        {
            this.Mesh = 0;
            this.__Message = _Message;
            this._To = __To;
            this._From = "SYSTEM";
            this.Color = (uint)_Color;
            this.ChatType = _ChatType;
        }
        public MsgMessage(string _Message, string __To, string __From, MsgColor _Color, ChatMode _ChatType)
        {
            this.Mesh = 0;
            this.__Message = _Message;
            this._To = __To;
            this._From = __From;
            this.Color = (uint)_Color;
            this.ChatType = _ChatType;
        }
        public MsgMessage()
        {
            this.Mesh = 0;
        }
        public unsafe void Deserialize(ServerSockets.Packet stream)
        {
            Color = stream.ReadUInt32();
            ChatType = (ChatMode)stream.ReadUInt32();
            MessageUID1 = stream.ReadUInt32();
            MessageUID2 = stream.ReadUInt32();
            Mesh = stream.ReadUInt32();
   
            string[] str = stream.ReadStringList();

            _From = str[0];
            _To = str[1];
            __Message = str[3];

            if (str.Length > 6)
            {
                ServerName = str[6];
            }
        }
        public unsafe Packet GetArray(Packet stream, uint Rank = 0)
        {
            stream.InitWriter();
            stream.Write(this.Color);
            stream.Write((uint)this.ChatType);
            stream.Write(MessageUID1);
            stream.Write(MessageUID2);
            stream.Write(Mesh);
            stream.Write(_From, _To, string.Empty, __Message);
            stream.Finalize(GamePackets.Chat);
            return stream;
        }

        [PacketAttribute(GamePackets.Chat)]
        public unsafe static void MsgHandler(Client.GameClient client, Packet packet)
        {
            MsgMessage msg = new MsgMessage();
            msg.Deserialize(packet);

            if (!ChatCommands(client, msg))
            {
                try
                {
                    string[] lines = msg.__Message.Split(new string[] { "[" }, StringSplitOptions.RemoveEmptyEntries);

                    for (int x = 0; x < lines.Length; x++)
                    {
                        string str = lines[x];
                        if (str.Contains("Item "))
                        {
                            string[] line = str.Split(' ');
                            if (line != null && line.Length > 2)
                            {
                                uint UID = 0;
                                if (uint.TryParse(line[2], out UID))
                                {
                                    MsgGameItem msg_item;
                                    if (client.TryGetItem(UID, out msg_item))
                                    {
                                        Program.GlobalItems.Add(msg_item);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteException(e);
                }

                if (msg._From == "SYSTEM" || msg._To == "SYSTEM")
                    return;
                msg.Mesh = client.Player.Mesh;
                if (msg.ChatType == ChatMode.Die)
                    msg.ChatType = MsgMessage.ChatMode.Talk;
                switch (msg.ChatType)
                {
                    case ChatMode.CrosTheServer:
                        {
                            if (client.Inventory.Contain(3002218, 1))
                            {
                                packet.Seek(packet.Size - 8);
                                packet.Finalize(1004);
                                MsgInterServer.StaticConnexion.Send(packet);

                                client.Inventory.Remove(3002218, 1, packet);
                            }
                            break;
                        }
                    case ChatMode.Friend:
                        {
                            System.Collections.Concurrent.ConcurrentDictionary<uint, Role.Instance.AssociateGS.Member> friends;
                            if (client.Player.Associate.Associat.TryGetValue(Role.Instance.AssociateGS.Friends, out friends))
                            {
                                foreach (var user in Pool.GamePoll.Values)
                                {
                                    if (friends.ContainsKey(user.Player.UID))
                                        user.Send(msg.GetArray(packet));
                                }
                            }
                            break;
                        }
                    case ChatMode.HawkMessage:
                        {
                            if (client.IsVendor)
                            {
                                client.MyVendor.HalkMeesaje = msg;

                                client.Player.View.SendView(msg.GetArray(packet), true);
                            }
                            break;
                        }
                    case ChatMode.Team:
                        {
                            if (client.Team != null)
                                client.Team.SendTeam(msg.GetArray(packet), client.Player.UID);
                            break;
                        }
                    case MsgMessage.ChatMode.Talk:
                        {
                            client.Player.View.SendView(msg.GetArray(packet), false);
                            break;
                        }
                    case MsgMessage.ChatMode.World:
                        {
                            if (DateTime.Now > client.Player.LastWorldMessaj.AddSeconds(15))
                            {
                                client.Player.LastWorldMessaj = DateTime.Now;
                                foreach (var user in Pool.GamePoll.Values)
                                {
                                    if (user.Player.UID != client.Player.UID)
                                    {
                                        user.Send(msg.GetArray(packet));
                                    }
                                }
                            }
                            break;
                        }
                    case ChatMode.Whisper:
                        {
                            bool send = false;
                            foreach (var user in Pool.GamePoll.Values)
                            {
                                if (user.Player.Name == msg._To)
                                {
                                    if (user.Player.Away == 1)
                                    {
                                        Pool.AwayMsg(msg, user, client, msg.ChatType);
                                        send = true;
                                        break;
                                    }
                                    else
                                    {
                                        msg.Mesh = client.Player.Mesh;
                                        user.Send(msg.GetArray(packet));
                                        send = true;
                                        break;
                                    }
                                }
                            }
                            if (!send)
                            {
                                client.SendSysMesage("The player is not online.", ChatMode.System, MsgColor.white);
                            }
                            break;
                        }
                    case ChatMode.Guild:
                        {
                            if (client.Player.MyGuild != null)
                                client.Player.MyGuild.SendPacket(msg.GetArray(packet), client.Player.UID);
                            if (client.Player.MyGuild != null && client.Player.SendAllies)
                            {
                                msg._To = "[ALLIES]";
                                foreach (var guild in client.Player.MyGuild.Ally.Values)
                                    guild.SendPacket(msg.GetArray(packet));
                            }
                            break;
                        }
                    case ChatMode.Clan:
                        {
                            if (client.Player.MyClan != null)
                                client.Player.MyClan.Send(msg.GetArray(packet));
                            break;
                        }
                }

            }
        }
        public static uint TestGui = 0;

        public static unsafe bool ChatCommands(Client.GameClient client, MsgMessage msg)
        {
            string logss = "[Chat]" + msg._From + " to " + msg._To + " " + msg.__Message + "";
        
            if (!client.ProjectManager && !client.GameMaster)
            {
                msg.__Message = msg.__Message.Replace("#60", "").Replace("#61", "").Replace("#62", "").Replace("#63", "").Replace("#64", "").Replace("#65", "").Replace("#66", "").Replace("#67", "").Replace("#68", "");
                try
                {
                    if (msg.__Message.StartsWith("@"))
                    {
                        string logs = "[GMLogs]" + client.Player.Name + " ";

                        string Message = msg.__Message.Substring(1);//.ToLower();
                        string[] data = Message.Split(' ');
                        for (int x = 0; x < data.Length; x++)
                            logs += data[x] + " ";
                        Database.ServerDatabase.LoginQueue.Enqueue(logs);
                        switch (data[0])
                        {
                            case "autohunt":
                            case "ah":
                                {
                                    if (data.Length == 1 || data[1] == "settings")
                                        Catching.ShowSettings(client);
                                    else if (data[1] == "start")
                                        Catching.Start(client);
                                    else if (data[1] == "stop")
                                        Catching.End(client);
                                    else
                                        client.SendSysMesage("Usage: @autohunt [settings|start|stop]");
                                    break;
                                }
                            case "leave":
                                {
                                    if (client.Player.Map == 700 && UnlimitedArenaRooms.Maps.ContainsValue(client.Player.DynamicID))
                                        client.Teleport(428, 380, 1002);
                                    break;
                                }
                            case "stuck":
                                {
                                    if (client.Player.Map == 6000)
                                        client.Teleport(30, 74, 6000);
                                    break;
                                }
                            case "allieschat":
                                client.Player.SendAllies = !client.Player.SendAllies;
                                client.SendSysMesage("Allies Chat mode: {client.Player.SendAllies}");
                                break;
                            case "resetscores":
                                client.Player.TotalHits = client.Player.Hits = client.Player.Chains = client.Player.MaxChains = 0;
                                break;
                            case "dc":
                                client.Socket.Disconnect();
                                break;
                            case "visualeffects":
                                client.Player.ShowGemEffects = !client.Player.ShowGemEffects;
                                client.SendSysMesage("Visual effects status: " + client.Player.ShowGemEffects);
                                break;
                            case "clearinventory":
                                {
                                    client.Player.MessageBox("Clearing your inventory would delete your items! You cant undo it. Are you sure?", (p) =>
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                            client.Inventory.Clear(rec.GetStream());
                                    }, null, 60);
                                    break;
                                }
                            case "agi":
                                {
                                    ushort atr = 0;
                                    ushort.TryParse(data[1], out atr);
                                    if (client.Player.Atributes >= atr)
                                    {
                                        client.Player.Agility += atr;
                                        client.Player.Atributes -= atr;
                                        client.Equipment.QueryEquipment(client.Equipment.Alternante, false);
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                        client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                        client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                        client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                        client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                    }
                                    break;
                                }
                            case "str":
                                {
                                    ushort atr = 0;
                                    ushort.TryParse(data[1], out atr);
                                    if (client.Player.Atributes >= atr)
                                    {
                                        client.Player.Strength += atr;
                                        client.Player.Atributes -= atr;
                                        client.Equipment.QueryEquipment(client.Equipment.Alternante, false);
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                        client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                        client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                        client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                        client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                    }
                                    break;
                                }
                            case "vit":
                                {
                                    ushort atr = 0;
                                    ushort.TryParse(data[1], out atr);
                                    if (client.Player.Atributes >= atr)
                                    {
                                        client.Player.Vitality += atr;
                                        client.Player.Atributes -= atr;
                                        client.Equipment.QueryEquipment(client.Equipment.Alternante, false);
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                        client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                        client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                        client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                        client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                    }
                                    break;
                                }
                            case "spi":
                                {
                                    ushort atr = 0;
                                    ushort.TryParse(data[1], out atr);
                                    if (client.Player.Atributes >= atr)
                                    {
                                        client.Player.Spirit += atr;
                                        client.Player.Atributes -= atr;
                                        client.Equipment.QueryEquipment(client.Equipment.Alternante, false);
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                        client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                        client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                        client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                        client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                    }
                                    break;
                                }
                        }
                        if (client.Player.VipLevel == 6)
                        {
                            switch (data[0])
                            {

                                case "spook":
                                    {
                                        if (!client.Player.OnMyOwnServer)
                                            break;
                                        if (client.PokerPlayer != null)
                                            break;
                                        if (Pool.BlockTeleportMap.Contains(client.Player.Map))
                                        {
                                            client.SendSysMesage("You can`t use it in " + client.Map.Name + " ");
                                            break;
                                        }
                                        if (DateTime.Now < client.Player.BossTeleCooldown.AddMinutes(1))
                                        {
                                            client.SendSysMesage("You need to wait 1 minute before teleporting again.");
                                            break;
                                        }
                                        if (!client.Player.Alive || client.Player.DeadState)
                                            break;
                                        client.Player.BossTeleCooldown = DateTime.Now;
                                        client.Teleport(32, 30, 2090);
                                        client.SendSysMesage("You have arrived!");
                                        break;
                                    }
                                case "snow":
                                    {
                                        if (!client.Player.OnMyOwnServer)
                                            break;
                                        if (client.PokerPlayer != null)
                                            break;
                                        if (Pool.BlockTeleportMap.Contains(client.Player.Map))
                                        {
                                            client.SendSysMesage("You can`t use it in " + client.Map.Name + " ");
                                            break;
                                        }
                                        if (!client.Player.Alive || client.Player.DeadState)
                                            break;
                                        if (DateTime.Now < client.Player.BossTeleCooldown.AddMinutes(1))
                                        {
                                            client.SendSysMesage("You need to wait 1 minute before teleporting again.");
                                            break;
                                        }
                                        client.Player.BossTeleCooldown = DateTime.Now;
                                        client.Teleport(407, 425, 2054);
                                        client.SendSysMesage("You have arrived!");
                                        break;
                                    }

                                case "vipinfo":
                                    {
                                        if (client.Player.VipLevel >= 6)
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
                                        else client.SendSysMesage("You`re not VIP-6.");
                                        break;
                                    }


                                case "summonguild":
                                    {
                                        if (!client.Player.OnMyOwnServer)
                                            break;
                                        if (client.PokerPlayer != null)
                                            break;
                                        if (Pool.BlockTeleportMap.Contains(client.Player.Map) || client.Player.Map == 1038)
                                        {
                                            client.SendSysMesage("You can`t use it in " + client.Map.Name + " ");
                                            break;
                                        }
                                        if (client.Player.MyGuild == null || client.Player.GuildRank != Role.Flags.GuildMemberRank.GuildLeader
                                            || client.Player.GuildRank != Role.Flags.GuildMemberRank.LeaderSpouse)
                                        {
                                            client.SendSysMesage("You need to be GuildLeader to use this!");
                                            break;
                                        }
                                        if (DateTime.Now < client.Player.MyGuild.SummonGuild.AddMinutes(5))
                                        {
                                            client.SendSysMesage("You need to wait 5 minutes before summoning again.");
                                            break;
                                        }

                                        client.Player.SummonGuild = DateTime.Now;
                                        ushort X = client.Player.X, Y = client.Player.Y;
                                        uint Map = client.Player.Map, Dynamic = client.Player.DynamicID;
                                        foreach (var member in Pool.GamePoll.Values.Where(e => e.Player.GuildID != 0 && e.Player.GuildID == client.Player.GuildID
                                            && e.Player.UID != client.Player.UID))
                                        {
                                            member.Player.MessageBox("Your guild leader has summoned you to " + client.Map.Name + "! Would you like to go?",
                                                new Action<Client.GameClient>(user => user.Teleport((ushort)(X + Role.Core.Random.Next(0, 5)),
                                                    (ushort)(Y + Role.Core.Random.Next(0, 5)), Map, Dynamic)),
                                                null, 60);
                                        }
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(
                                                client.Player.Name + " guild leader of " + client.Player.MyGuild.GuildName + " has summoned his members to " + client.Map.Name, "ALLUSERS",
                                                MsgColor.red, ChatMode.TopLeft).GetArray(stream));
                                        }
                                        break;
                                    }

                            }
                        }
                    }
                    return false;
                }
                catch { client.SendSysMesage("Sorry cannot access to this command or not exists."); return false; }
            }
            if (client.ProjectManager == false)
                return false;
            try
            {
                msg.__Message = msg.__Message.Replace("#60", "").Replace("#61", "").Replace("#62", "").Replace("#63", "").Replace("#64", "").Replace("#65", "").Replace("#66", "").Replace("#67", "").Replace("#68", "");
                if (msg.__Message.StartsWith("!") || msg.__Message.StartsWith("@"))
                {
                    string Message = msg.__Message.Substring(1);//.ToLower();
                    string[] data = Message.Split(' ');
                    string logs = "[GMLogs]" + client.Player.Name + " ";
                    for (int x = 0; x < data.Length; x++)
                        logs += data[x] + " ";
                   
                    switch (data[0])
                    {
                        #region GM Commands
                        case "autohunt":
                        case "ah":
                            {
                                // Contas de PM/GM tambem precisam do comando para testar o Auto Hunt.
                                if (data.Length == 1 || data[1] == "settings")
                                    Catching.ShowSettings(client);
                                else if (data[1] == "start")
                                    Catching.Start(client);
                                else if (data[1] == "stop")
                                    Catching.End(client);
                                else
                                    client.SendSysMesage("Usage: @autohunt [settings|start|stop]");
                                break;
                            }
                        case "spawnmob":
                            switch(data[1].ToLower())
                            {
                                case "snowbanshee":
                                    MobsHandler.Generate(IDMonster.SnowBanshee);
                                    break;
                                case "teratodragon":
                                    MobsHandler.Generate(IDMonster.TeratoDragon);
                                    break;
                                case "thrillingspook":
                                    MobsHandler.Generate(IDMonster.ThrillingSpook);
                                    break;
                                case "darkmoondemon":
                                    MobsHandler.Generate(IDMonster.DarkmoonDemon);
                                    break;
                                case "corndevil":
                                    MobsHandler.Generate(IDMonster.CornDevil);
                                    break;
                                case "darkspearman":
                                    MobsHandler.Generate(IDMonster.DarkSpearman);
                                    break;
                                case "mummyskeleton":
                                    MobsHandler.Generate(IDMonster.MummySkeleton);
                                    break;
                                case "ganoderma":
                                    MobsHandler.Generate(IDMonster.Ganoderma);
                                    break;
                            }
                            break;
                        case "resetscores":
                            client.Player.TotalHits = client.Player.Hits = client.Player.Chains = 0;
                            break;
                        case "counter":
                            {
                                client.Status.Counteraction += 20;
                                break;
                            }
                        case "break":
                            {
                                client.Status.Breakthrough += 20;
                                break;
                            }
                        case "steedu":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Inventory.AddSteed(stream, 300000, 1, byte.Parse(data[4]), false, byte.Parse(data[1]), byte.Parse(data[2]), byte.Parse(data[3]));
                                }
                                break;
                            }
                        case "addguiitem":
                            {

                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var apacket = rec.GetStream();
                                    client.Inventory.AddReturnedItem(apacket, uint.Parse(data[1]), byte.Parse(data[2]));
                                }
                                break;
                            }
                        case "addgui":
                            {
                                ActionQuery action = new ActionQuery()
                                {
                                    Type = ActionType.OpenDialog,
                                    ObjId = client.Player.UID,
                                    wParam1 = client.Player.X,
                                    wParam2 = client.Player.Y,
                                    dwParam = uint.Parse(data[1])//MsgServer.DialogCommands.JiangHuSetName,
                                };
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var apacket = rec.GetStream();
                                    client.Send(apacket.ActionCreate(&action));
                                }
                                break;
                            }
                        #region MadeByDaRkFox
                        case "editnpc":
                            {
                                client.EditNPC = !client.EditNPC;
                                client.SendSysMesage($"NPC Edit Mode: {(client.EditNPC ? "YES" : "NO")}");
                                break;
                            }
                        #endregion
                        case "ali":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    client.Send(stream.GuildRequestCreate((MsgGuildProces.GuildAction)ushort.Parse(data[1]), client.Player.UID, new int[3], "Basta"));
                                }
                                break;
                            }
                        case "bc":
                            {
                                string fullMessage = Message.Replace(data[0], "");
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(fullMessage, "ALLUSERS", MsgColor.red, ChatMode.Center).GetArray(stream));
                                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(fullMessage, "ALLUSERS", MsgColor.red, ChatMode.BroadcastMessage).GetArray(stream));
                                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(fullMessage, "ALLUSERS", MsgColor.red, ChatMode.System).GetArray(stream));
                                }
                                break;
                            }
                        case "invisible":
                            {
                                client.Player.Invisible = true;
                                break;
                            }
                        case "visible":
                            {
                                client.Player.Invisible = false;
                                break;
                            }
                        case "battlepoints":
                            {
                                client.Player.BattleFieldPoints = ushort.Parse(data[1]);
                                break;
                            }
                        case "hit":
                            {
                                client.Player.HitPoints = ushort.Parse(data[1]);
                                client.Player.SendUpdateHP();
                                break;

                            }
                        case "reward":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    client.Player.DailySignUpDays |= (1ul << byte.Parse(data[1]));
                                    // client.Send(stream.MsgSignInCreate((MsgSignIn.ActionID)byte.Parse(data[1]), byte.Parse(data[2])
                                    //     , byte.Parse(data[3]), (1ul << byte.Parse(data[4]))));


                                }
                                break;
                            }

                        case "data":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    ActionQuery action = new ActionQuery()
                                    {
                                        Type = ActionType.OpenDialog,
                                        ObjId = client.Player.UID,
                                        dwParam = uint.Parse(data[1]),
                                        wParam1 = client.Player.X,
                                        wParam2 = client.Player.Y,

                                    };
                                    client.Send(stream.ActionCreate(&action));
                                }
                                break;
                            }
                        case "data1":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    ActionQuery action = new ActionQuery()
                                    {
                                        Type = ActionType.OpenCustom,
                                        ObjId = client.Player.UID,
                                        dwParam = uint.Parse(data[1]),
                                        wParam1 = client.Player.X,
                                        wParam2 = client.Player.Y,

                                    };
                                    client.Send(stream.ActionCreate(&action));
                                }
                                break;
                            }
                        case "clearspells":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    foreach (var spell in client.MySpells.ClientSpells.Values)
                                        client.MySpells.Remove(spell.ID, stream);
                                }
                                break;
                            }
                        case "incquest":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.QuestGUI.IncreaseQuestObjectives(stream, ushort.Parse(data[1]), ushort.Parse(data[2]), ushort.Parse(data[3]), ushort.Parse(data[4]));
                                }
                                break;
                            }
                        case "accquest":
                            {

                                client.Player.QuestGUI.Accept(Database.QuestInfo.AllQuests[uint.Parse(data[1])], 0);
                                break;
                            }
                        case "remquest":
                            {
                                client.Player.QuestGUI.AcceptedQuests.Remove(uint.Parse(data[1]));
                                client.Player.QuestGUI.src.Remove(uint.Parse(data[1]));
                                break;
                            }
                        case "finishquest":
                            {
                                client.Player.QuestGUI.FinishQuest(uint.Parse(data[1]));
                                break;
                            }
                        case "rr":
                            {
                                client.Player.TCCaptainTimes = 0;
                                break;
                            }
                        case "cards":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    stream.InitWriter();
                                    stream.Write((ushort)7);
                                    stream.Write((ushort)4);
                                    stream.ZeroFill(22);
                                    stream.Write((ushort)1);
                                    stream.Write(client.Player.UID);//starter or dealer?
                                    stream.Write(0);
                                    stream.Write(0);//??

                                    stream.Write(ushort.Parse(data[1]));
                                    stream.Write((ushort)1);
                                    stream.Write(client.Player.UID);


                                    stream.Finalize(GamePackets.PokerDrawCards);
                                    client.Send(stream);
                                }
                                break;
                            }

                        case "trans":
                            {

                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    //client.Player.Body = x;//ushort.Parse(data[1]);
                                    // client.Player.SendUpdate(stream, client.Player.Mesh, MsgUpdate.DataType.Mesh);
                                    client.Player.TransformInfo = new Role.ClientTransform(client.Player);
                                    client.Player.TransformInfo.CreateTransform(stream, 817, ushort.Parse(data[1]), (int)ushort.MaxValue - 1, 8213);
                                }


                                break;
                            }
                        case "pick":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.AddPick(stream, "Pick", 5);
                                }
                                break;
                            }
                        case "ag":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("" + client.Player.Name + " successfully acquired [Universal~Concept~(A)], getting closer to be a super hero! [Link Go acquire ###1 692]", MsgMessage.MsgColor.red, MsgMessage.ChatMode.InnerPower).GetArray(stream));
                                }
                                break;
                            }
                        case "addnpc":
                            {
                                Game.MsgNpc.Npc np = Game.MsgNpc.Npc.Create();
                                np.UID = (uint)Pool.GetRandom.Next(10000, 100000);
                                np.NpcType = (Role.Flags.NpcType)byte.Parse(data[1]);
                                np.Mesh = ushort.Parse(data[2]);
                                np.Map = client.Player.Map;//ushort.Parse(data[3]);
                                np.X = client.Player.X;//ushort.Parse(data[4]);
                                np.Y = client.Player.Y;//ushort.Parse(data[5]);
                                client.Map.AddNpc(np);
                                break;
                            }
                        case "itemstack":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Inventory.AddItemWitchStack(uint.Parse(data[1]), byte.Parse(data[2]), ushort.Parse(data[3]), stream);
                                }
                                break;
                            }
                        case "itemeffect":
                            {
                                MsgGameItem item;
                                if (client.Equipment.TryGetEquip(Role.Flags.ConquerItem.RightWeapon, out item))
                                {
                                    item.Effect = (Role.Flags.ItemEffect)ushort.Parse(data[1]);
                                    item.Mode = Role.Flags.ItemMode.Update;
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        item.Send(client, stream);
                                    }
                                }
                                break;
                            }
                        case "dura":
                            {
                                MsgServer.MsgGameItem GameItem;
                                if (client.Equipment.TryGetEquip((Role.Flags.ConquerItem)byte.Parse(data[1]), out GameItem))
                                {
                                    GameItem.Durability = GameItem.MaximDurability = ushort.Parse(data[2]);

                                    GameItem.Mode = Role.Flags.ItemMode.Update;
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        GameItem.Send(client, stream);
                                    }
                                }
                                break;
                            }
                        case "testct":
                            {
                                uint Count = ushort.Parse(data[1]);
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    stream.CaptureTheFlagRankingsCreate((MsgCaptureTheFlagRankings.ActionID)0, 0, 2, Count, 4, 5);
                                    for (int x = 0; x < Count; x++)
                                    {

                                        stream.AddItemCaptureTheFlagRankings(100, 200, "basta" + x.ToString(), (uint)(100 + x));
                                    }
                                    stream.CaptureTheFlagRankingsFinalize();
                                    client.Send(stream);
                                }

                                break;
                            }
                        case "realbp":
                            {
                                client.SendSysMesage("You real BatterPower is = " + client.Player.RealBattlePower + "");
                                break;
                            }
                        case "bp":
                            {
                                client.SendSysMesage("You BatterPower is = " + client.Player.BattlePower + "");
                                break;
                            }
                        case "inventoryinfo":
                            {
                                uint iItem = 0;
                                foreach (MsgGameItem gItem in client.Inventory.ClientItems.Values)
                                {
                                    client.SendSysMesage($"Item[{iItem}] {gItem.ITEM_ID}");
                                    iItem++;
                                }
                                break;
                            }
                        case "newsuper":
                            {
                                client.Status.MinAttack = uint.MaxValue - 10;
                                client.Status.MaxAttack = uint.MaxValue;
                                break;
                            }
                        case "vitalitypoints":
                            {
                                client.Player.Vitality += 538;
                                client.Player.Strength += 0;
                                client.Player.Spirit += 0;
                                client.Player.Agility += 0;

                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                    client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                    client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                    client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);

                                }
                                break;
                            }
                        case "removebuggedgarments":
                            {
                                foreach (var user in Pool.GamePoll.Values)
                                {
                                    while (user.Inventory.Contain(188465, 1) || user.Inventory.Contain(188465, 1, 1))
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            user.Inventory.Remove(188465, 1, stream);
                                            user.Player.ConquerPoints += 500;
                                        }
                                        user.SendSysMesage("You have a Garment that causes problems has been eliminated and you have been compensated with 500 CPs", ChatMode.System, MsgColor.white);
                                    }
                                    while (user.Inventory.Contain(191305, 1) || user.Inventory.Contain(191305, 1, 1))
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            user.Inventory.Remove(191305, 1, stream);
                                            user.Player.ConquerPoints += 500;
                                        }
                                        user.SendSysMesage("You have a Garment that causes problems has been eliminated and you have been compensated with 500 CPs", ChatMode.System, MsgColor.white);
                                    }
                                }
                                break;
                            }
                        case "removeitemandcompensate":
                            {
                                client.SendSysMesage("[Info] This command remove all item specificated from online players inventory.");
                                client.SendSysMesage("[Usage: removeitemandcompensate itemid playerCompensationCPs player(optional)]");
                                uint itemID = uint.Parse(data[1]);
                                uint playerCompensationCPs = uint.Parse(data[2]);
                                string playerName = null;
                                if (data.Length > 3)
                                {
                                    playerName = data[3];
                                }
                                List<Client.GameClient> fromClients = Pool.GamePoll.Values.ToList();
                                if (playerName != null && playerName.Length > 0)
                                {
                                    fromClients = Pool.GamePoll.Values.Where(x => x.Player.Name == playerName).ToList();
                                }
                                foreach (var user in fromClients)
                                {
                                    while (user.Inventory.Contain(itemID, 1) || user.Inventory.Contain(itemID, 1, 1))
                                    {
                                        Database.ItemType.DBItem item;
                                        Pool.ItemsBase.TryGetValue(itemID, out item);
                                        using (var rec = new RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            user.Inventory.Remove(itemID, 1, stream);
                                            user.Player.ConquerPoints += playerCompensationCPs;
                                        }
                                        user.SendSysMesage($"The PM has removed the item {item.Name} from your inventory and compensate you with {playerCompensationCPs}CPs", ChatMode.System, MsgColor.white);
                                    }
                                }
                                break;
                            }
                        case "superman":
                            {
                                //client.Player.Vitality += 500;
                                //client.Player.Strength += 5000;
                                //client.Player.Spirit += 500;
                                //client.Player.Agility += 5000;

                                //using (var rec = new ServerSockets.RecycledPacket())
                                //{
                                //    var stream = rec.GetStream();
                                //    client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                //    client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                //    client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                //    client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);

                                //}
                                client.Player.SuperManMode = !client.Player.SuperManMode;
                                if (client.Player.SuperManMode)
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.AddMapEffect(stream, client.Player.X, client.Player.Y, "ssch_wlhd_hit");
                                    }
                                    client.SendSysMesage($"Superman Mode [Enabled]", ChatMode.System, MsgColor.white);
                                } else
                                {
                                    client.SendSysMesage($"Superman Mode [Disabled]", ChatMode.System, MsgColor.white);
                                }
                                break;
                            }
                        case "resetstats":
                            {
                                client.Player.Vitality = 0;
                                client.Player.Strength = 0;
                                client.Player.Spirit = 0;
                                client.Player.Agility = 0;

                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                    client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                    client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                    client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);

                                }
                                break;
                            }
                        case "addmem":
                            {
                                for (int x = 0; x < ushort.Parse(data[1]); x++)
                                {
                                    client.Player.MyGuild.Members.TryAdd((uint)(client.Player.UID + x + 1000)
                                        , new Role.Instance.Guild.Member() { Name = "test " + x.ToString() + " ", Class = 15, Level = (byte)x, IsOnline = true, UID = (uint)(client.Player.UID + x + 1000) });
                                }
                                break;
                            }
                        case "resetnobility":
                            {
                                // Clean online users nobility
                                foreach (var user in Pool.GamePoll.Values)
                                {
                                    user.Player.Nobility.Donation = 0;
                                    Pool.NobilityRanking.RemoveAndUpdateTheRest(user.EntityID);
                                }
                                // Reset nobility from all players in db files
                                NobilityTable.Reset();
                                Console.WriteLine("All player donations for nobility has been reset. The best option is restart now the server!");
                                break;
                            }
                        case "give":
                            {
                                foreach (var user in Pool.GamePoll.Values)
                                {
                                    if (user.Player.Name.ToLower() == data[1].ToLower() || data[1].ToLower() == "me")
                                    {

                                        switch (data[2])
                                        {
                                            case "givenobil":
                                                {
                                                    try
                                                    {
                                                       
                                                        user.Player.PayNobilitySystem.Paid(int.Parse(data[3]), (Role.Instance.Nobility.NobilityRank)byte.Parse(data[4]));
                                                        user.Socket.Disconnect();
                                                    }
                                                    catch
                                                    {
                                                        Console.WriteLine("Syntax Error In Paid Nobility System.");
                                                    }
                                                    break;
                                                }
                                            case "donationpoints":
                                                {
                                                    user.Player.DonationPoints = uint.Parse(data[3]);
                                                    using (var rec = new RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        client.Player.SendUpdate(stream, user.Player.DonationPoints, MsgUpdate.DataType.RaceShopPoints);
                                                    }
                                                    break;
                                                }
                                            case "spell":
                                                {
                                                    ushort ID = 0;
                                                    if (!ushort.TryParse(data[3], out ID))
                                                    {
                                                        client.SendSysMesage("Invlid spell ID !");
                                                        break;
                                                    }
                                                    byte level = 0;
                                                    if (!byte.TryParse(data[4], out level))
                                                    {
                                                        client.SendSysMesage("Invlid spell Level ! ");
                                                        break;
                                                    }
                                                    int Experience = 0;
                                                    if (data.Length > 3)
                                                    {
                                                        if (!int.TryParse(data[3], out Experience))
                                                        {
                                                            client.SendSysMesage("Invalid spell Experience.");
                                                            break;
                                                        }
                                                    }

                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                        user.MySpells.Add(rec.GetStream(), ID, level, 0, 0, Experience);
                                                    break;
                                                }
                                            case "level":
                                                {
                                                    byte amount = 0;
                                                    if (byte.TryParse(data[3], out amount))
                                                    {
                                                        using (var rec = new ServerSockets.RecycledPacket())
                                                        {
                                                            var stream = rec.GetStream();
                                                            user.UpdateLevel(stream, amount, true);
                                                        }
                                                        ReallotPoints(user);
                                                    }
                                                    break;
                                                }
                                            case "money":
                                                {
                                                    user.Player.Money += uint.Parse(data[3]);
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                    {
                                                        var stream = rec.GetStream();
                                                        user.Player.SendUpdate(stream, user.Player.Money, MsgUpdate.DataType.Money);
                                                    }
                                                    break;
                                                }
                                            case "cps":
                                                {
                                                    user.Player.ConquerPoints += uint.Parse(data[3]);

                                                    break;
                                                }
                                            case "reborns":
                                                {
                                                    user.Player.Reborn = byte.Parse(data[3]);

                                                    break;
                                                }

                                            case "item":
                                                {
                                                    uint ID = 0;
                                                    if (!uint.TryParse(data[3], out ID))
                                                    {
                                                        client.SendSysMesage("Invalid item ID !");
                                                        break;
                                                    }
                                                    byte plus = 0;
                                                    if (!byte.TryParse(data[4], out plus))
                                                    {
                                                        client.SendSysMesage("Invalid item plus !");
                                                        break;
                                                    }
                                                    byte bless = 0;
                                                    if (!byte.TryParse(data[5], out bless))
                                                    {
                                                        client.SendSysMesage("Invalid item Bless !");
                                                        break;
                                                    }
                                                    byte enchant = 0;
                                                    if (!byte.TryParse(data[6], out enchant))
                                                    {
                                                        client.SendSysMesage("Invalid item Enchant !");
                                                        break;
                                                    }
                                                    byte sockone = 0;
                                                    if (!byte.TryParse(data[7], out sockone))
                                                    {
                                                        client.SendSysMesage("Invalid item Socket One !");
                                                        break;
                                                    }
                                                    byte socktwo = 0;
                                                    if (!byte.TryParse(data[8], out socktwo))
                                                    {
                                                        client.SendSysMesage("Invalid item Socket Two !");
                                                        break;
                                                    }
                                                    byte count = 1;
                                                    if (data.Length > 9)
                                                    {
                                                        if (!byte.TryParse(data[9], out count))
                                                        {
                                                            client.SendSysMesage("Invalid item count !");
                                                            break;
                                                        }
                                                    }
                                                    byte Effect = 0;
                                                    if (data.Length > 10)
                                                    {
                                                        if (!byte.TryParse(data[10], out Effect))
                                                        {
                                                            client.SendSysMesage("Invalid Effect Type !");
                                                            break;
                                                        }
                                                    }
                                                    using (var rec = new ServerSockets.RecycledPacket())
                                                        user.Inventory.Add(rec.GetStream(), ID, count, plus, bless, enchant, (Role.Flags.Gem)sockone, (Role.Flags.Gem)socktwo, false, (Role.Flags.ItemEffect)Effect, false, "");

                                                    break;
                                                }
                                        }
                                        break;
                                    }
                                }
                                break;
                            }
                        case "unbanstr":
                            {
                                Database.SystemBannedAccount.RemoveBan(data[1]);
                                break;
                            }
                        case "unbanuid":
                            {
                                Database.SystemBannedAccount.RemoveBan(uint.Parse(data[1]));
                                break;
                            }
                        case "ban":
                            {
                                foreach (var user in Pool.GamePoll.Values)
                                {
                                    if (user.Player.Name.ToLower() == data[1].ToLower())
                                    {
                                        if (data.Length > 3)
                                        {
                                            Database.SystemBannedAccount.AddBan(user.Player.UID, user.Player.Name, uint.Parse(data[2]), data[3]);
                                        } else
                                        {
                                            Database.SystemBannedAccount.AddBan(user.Player.UID, user.Player.Name, uint.Parse(data[2]));
                                        }
                                        user.SendSysMesage("You Account was Banned by [PM]/[GM].", ChatMode.System, MsgColor.white);
                                        user.Socket.Disconnect();
                                        break;
                                    }
                                }
                                break;
                            }
                        case "banip":
                            {
                                foreach (var user in Pool.GamePoll.Values)
                                {
                                    if (user.Player.Name.ToLower() == data[1].ToLower())
                                    {
                                        Database.SystemBanned.AddBan(user.Socket.RemoteIp, uint.Parse(data[2]));
                                        user.SendSysMesage("You Ip Address was Banned by [PM]/[GM].", ChatMode.System, MsgColor.white);
                                        user.Socket.Disconnect();
                                        break;
                                    }
                                }
                                break;
                            }
                       
                        case "kick":
                            {
                                foreach (var user in Pool.GamePoll.Values)
                                {
                                    if (user.Player.Name.ToLower() == data[1].ToLower())
                                    {
                                        user.Socket.Disconnect();
                                        break;
                                    }
                                }
                                break;
                            }
                        case "rev":
                        case "revive":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.Player.Revive(rec.GetStream());

                                break;
                            }
                        case "estats":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    stream.InitWriter();

                                    stream.Write((uint)2);//4
                                    stream.Write((uint)1);//8

                                    stream.Write(1000004);//12
                                    stream.Write((uint)0);//16
                                    stream.Write((uint)0);//20
                                    stream.Write((uint)0);//24
                                    stream.Write((uint)0);//28
                                    stream.Write((uint)0);//32
                                    stream.Write((uint)0);//36
                                    stream.Write((uint)0);//40
                                                          //stream.Write((uint)0);//16
                                    stream.Write(370);//3
                                                      //stream.Write(0);//36

                                    //stream.Write((uint)0);
                                    stream.Finalize(GamePackets.ElitePKMatchUI);
                                    client.Send(stream);
                                }
                                break;
                            }
                       
                        case "online":
                            {
                                client.SendSysMesage("Online Players : " + Pool.GamePoll.Count + " ", ChatMode.System);
                                client.SendSysMesage("Online Players : " + Pool.GamePoll.Count + " ");
                               
                                break;
                            }
                        case "addtitle":
                            {
                                byte title = byte.Parse(data[1]);
                                client.Player.Titles.Add(title);
                                client.Player.AddTitle(title, true);
                                break;
                            }
                        case "vip":
                            {
                                foreach (var user in Pool.GamePoll.Values)
                                {
                                    if (user.Player.Name.ToLower() == data[1].ToLower())
                                    {

                                        if (DateTime.Now > user.Player.ExpireVip)
                                            user.Player.ExpireVip = DateTime.Now.AddDays(30);
                                        else
                                            user.Player.ExpireVip = user.Player.ExpireVip.AddDays(30);

                                        user.Player.VipLevel = (byte)uint.Parse(data[2]);
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            user.Player.SendUpdate(stream, user.Player.VipLevel, MsgUpdate.DataType.VIPLevel);

                                            user.Player.UpdateVip(stream);
                                        }
                                        user.CreateBoxDialog("You`ve received vip level " + data[1] + " for 30 days. Thanks for your donation!");
                                        break;
                                    }
                                }
                                break;
                            }
                        case "vipnewplayer":
                            {
                                foreach (var user in Pool.GamePoll.Values)
                                {

                                    if (user.Player.Name.ToLower() == data[1].ToLower() || data[1].ToLower() == "all")
                                    {
                                        if (DateTime.Now > user.Player.ExpireVip)
                                            user.Player.ExpireVip = DateTime.Now.AddDays(5);
                                        else
                                            user.Player.ExpireVip = user.Player.ExpireVip.AddDays(5);

                                        user.Player.VipLevel = (byte)uint.Parse(data[2]);
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            user.Player.SendUpdate(stream, user.Player.VipLevel, MsgUpdate.DataType.VIPLevel);

                                            user.Player.UpdateVip(stream);
                                        }
                                        user.CreateBoxDialog("You`ve received vip6 (5 days) as a starter pack. Welcome to our server.");

                                        break;
                                    }
                                }
                                break;
                            }
                        case "info":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    foreach (var user in Pool.GamePoll.Values)
                                    {
                                        if (user.Player.Name.ToLower() == data[1].ToLower())
                                        {

                                            client.Send(new MsgMessage("[Info" + user.Player.Name + "]", MsgColor.yellow, ChatMode.FirstRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("UID = " + user.Player.UID + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("IP = " + user.Socket.RemoteIp + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("ConquerPoints = " + user.Player.ConquerPoints + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("Money = " + user.Player.Money + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("Map = " + user.Player.Map + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("X = " + user.Player.X + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("Y = " + user.Player.Y + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                            client.Send(new MsgMessage("BattlePower = " + user.Player.BattlePower + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));

                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        case "scroll":
                            {
                                switch (data[1].ToLower())
                                {
                                    case "lv": client.Teleport(5, 290, 1354, 8800); break;
                                    case "tc": client.Teleport(428, 378, 1002); break;
                                    case "pc": client.Teleport(195, 260, 1011); break;
                                    case "ac":
                                    case "am": client.Teleport(566, 563, 1020); break;
                                    case "dc": client.Teleport(500, 645, 1000); break;
                                    case "bi": client.Teleport(723, 573, 1015); break;
                                    case "pka": client.Teleport(050, 050, 1005); break;
                                    case "ma": client.Teleport(211, 196, 1036); break;
                                    case "ja": client.Teleport(100, 100, 6000); break;
                                }
                                break;
                            }
                        case "trace":
                            {
                                foreach (var user in Pool.GamePoll.Values)
                                {
                                    if (user.Player.Name.ToLower().Contains(data[1].ToLower()))
                                    {
                                        client.Teleport(user.Player.X, user.Player.Y, user.Player.Map, user.Player.DynamicID);
                                        break;
                                    }
                                }

                                break;
                            }
                        case "bring":
                            {
                                foreach (var user in Pool.GamePoll.Values)
                                {
                                    if (user.Player.Name.ToLower() == data[1].ToLower() || data[1].ToLower() == "all")
                                        if (data[1].ToLower() == "all")
                                        {
                                            user.Teleport(
                                                    (ushort)Role.Core.Random.Next(client.Player.X - 5, client.Player.X + 5),
                                                    (ushort)Role.Core.Random.Next(client.Player.Y - 5, client.Player.Y + 5), client.Player.Map);

                                        }
                                        else
                                            user.Teleport(client.Player.X, client.Player.Y, client.Player.Map);

                                }
                                break;
                            }

                        case "arenapoints":
                            {
                                client.HonorPoints = uint.Parse(data[1]);
                                break;
                            }
                        case "life":
                            {
                                client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                                client.Player.Mana = (ushort)client.Status.MaxMana;
                                client.Player.SendUpdateHP();
                                break;
                            }
                        case "donationpoints":
                            {
                                client.Player.DonationPoints = uint.Parse(data[1]);
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, client.Player.DonationPoints, MsgUpdate.DataType.RaceShopPoints);
                                }
                                break;
                            }
                        case "onlinepoints":
                            {
                                client.Player.OnlinePoints = uint.Parse(data[1]);

                                break;
                            }
                        case "staticrole":
                            {
                                var staticrole = new Role.StaticRole(client.Player.X, client.Player.Y);
                                staticrole.Map = client.Player.Map;

                                client.Map.AddStaticRole(staticrole);
                                break;
                            }
                        case "facke":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    for (int i = 0; i < ushort.Parse(data[1]); i++)
                                    {
                                        Client.GameClient pclient = new Client.GameClient(null);
                                        pclient.Fake = true;

                                        pclient.Player = new Role.Player(pclient);
                                        pclient.Inventory = new Role.Instance.Inventory(pclient);
                                        pclient.Equipment = new Role.Instance.Equip(pclient);
                                        pclient.Warehouse = new Role.Instance.Warehouse(pclient);
                                        pclient.MyProfs = new Role.Instance.Proficiency(pclient);
                                        pclient.MySpells = new Role.Instance.Spell(pclient);
                                        pclient.Achievement = new Database.AchievementCollection();
                                        pclient.Status = new MsgStatus();
                                        pclient.Player.Name = "[GM]Lords" + i;
                                        pclient.Player.Body = 1003;
                                        pclient.Player.UID = (uint)(1050000 + i * 10);
                                        pclient.Player.HitPoints = 1000;
                                        pclient.Status.MaxHitpoints = 1000;

                                        pclient.Player.X = (ushort)313;
                                        pclient.Player.Y = (ushort)285;
                                        pclient.Player.Map = 1002;
                                        pclient.Player.Level = 255;
                                        pclient.TeamArenaStatistic = new MsgTeamArena.User()
                                        {
                                            UID = pclient.Player.UID,
                                            Level = pclient.Player.Level,
                                            Mesh = pclient.Player.Mesh,
                                            Class = pclient.Player.Class

                                        };
                                        pclient.Player.ServerID = (ushort)Database.GroupServerList.MyServerInfo.ID;
                                        pclient.Player.Face = 153;
                                        pclient.Player.Action = Role.Flags.ConquerAction.Sit;
                                        pclient.Player.Angle = Role.Flags.ConquerAngle.SouthWest;
                                        pclient.Player.Hair = 774;

                                        client.Send(pclient.Player.GetArray(stream, false));

                                        pclient.Map = Pool.ServerMaps[1002];
                                        pclient.Map.Enquer(pclient);
                                        Pool.GamePoll.TryAdd(pclient.Player.UID, pclient);
                                    }
                                }
                                break;
                            }
                     
                        case "onlineminutes":
                            {
                                client.Player.OnlineMinutes = uint.Parse(data[1]);
                                break;
                            }
                        case "addsouls":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    List<string> Items = new List<string>();

                                    var array = Database.ItemType.PurificationItems[ushort.Parse(data[1])].Values.ToArray();
                                    for (int x = 0; x < array.Length; x++)
                                    {
                                        Items.Add(array[x].Name + " " + array[x].ID);
                                        client.Inventory.Add(stream, array[x].ID);
                                    }
                                    Database.DBActions.Write writer = new Database.DBActions.Write("Souls" + ushort.Parse(data[1]) + ".ini");
                                    foreach (var it in Items)
                                        writer.Add(it);
                                    writer.Execute(Database.DBActions.Mode.Open);
                                }
                                break;
                            }
                        case "addrefinary":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    List<string> Items = new List<string>();

                                    var array = Database.ItemType.Refinary[ushort.Parse(data[1])].Values.ToArray();
                                    for (int x = 0; x < array.Length; x++)
                                    {
                                        Items.Add(array[x].Name + " " + array[x].ItemID);
                                        client.Inventory.Add(stream, array[x].ItemID);
                                    }
                                    Database.DBActions.Write writer = new Database.DBActions.Write("Souls" + ushort.Parse(data[1]) + ".ini");
                                    foreach (var it in Items)
                                        writer.Add(it);
                                    writer.Execute(Database.DBActions.Mode.Open);
                                }
                                break;
                            }
                        case "statue":
                            {
                                //  Role.Statue.ElitePkStatue(client);
                                Role.Statue.CreateStatue(client, client.Player.X, client.Player.Y, (int)client.Player.Action, 0, false);
                                break;
                            }
                        case "sofoke":
                            {
                                client.Status.Defence = uint.MaxValue;
                                client.Status.MaxAttack = uint.Parse(data[1]);
                                client.Status.MinAttack = uint.Parse(data[1]) - 1;
                                break;
                            }
                        case "haire":
                            {
                                client.Player.Hair = ushort.Parse(data[1]); using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, client.Player.Hair, MsgUpdate.DataType.HairStyle);
                                }
                                break;
                            }
                        case "mapstat":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Send(stream.MapStatusCreate(client.Map.ID, client.Map.ID, (ulong)(1U << int.Parse(data[1]))));
                                }
                                break;
                            }
                        case "pkp":
                            {
                                client.Player.PKPoints = ushort.Parse(data[1]);
                                break;
                            }
                      
                        case "ctf":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    for (int x = 0; x < 30; x++)
                                    {
                                        stream.CaptureTheFlagUpdateCreate((MsgCaptureTheFlagUpdate.Mode)x, 3, 1);
                                        //326,447
                                        stream.AddX2LocationCaptureTheFlagUpdate(326, 447);
                                        stream.CaptureTheFlagUpdateFinalize();
                                        client.Send(stream);
                                    }
                                }
                                break;
                            }
                       
                        case "couplespk":
                            {
                                MsgSchedules.CouplesPKWar.Open();
                                break;
                            }

                       
                        case "epk":
                            {
                                Game.MsgTournaments.MsgSchedules.ElitePkTournament.Start();
                                break;
                            }
                        case "sktp":
                            {
                                Game.MsgTournaments.MsgSchedules.SkillTeamPkTournament.Start();
                                break;
                            }
                        case "tp":
                            {
                                Game.MsgTournaments.MsgSchedules.TeamPkTournament.Start();
                                break;
                            }
                        case "startelitegw":
                            {
                                Game.MsgTournaments.MsgSchedules.EliteGuildWar.Proces = Game.MsgTournaments.ProcesType.Alive;
                                Game.MsgTournaments.MsgSchedules.EliteGuildWar.Start();
                                break;
                            }
                        case "finishelitegw":
                            {
                                Game.MsgTournaments.MsgSchedules.EliteGuildWar.Proces = Game.MsgTournaments.ProcesType.Dead;
                                Game.MsgTournaments.MsgSchedules.EliteGuildWar.CompleteEndGuildWar();
                                break;
                            }
                        case "ctfon":
                            {
                                Game.MsgTournaments.MsgSchedules.CaptureTheFlag.Start();
                                break;
                            }
                        case "searchguard":
                            {
                                foreach (var mob in client.Map.View.GetAllMapRoles(Role.MapObjectType.Monster))
                                {
                                    if (mob.X == client.Player.X && mob.Y == client.Player.Y)
                                    {
                                        client.SendSysMesage("Location Spawn --> " + (mob as Game.MsgMonster.MonsterRole).LocationSpawn, ChatMode.System, MsgColor.red);
                                    }
                                }
                                break;
                            }


                        case "gui":
                            {
                                TestGui = ushort.Parse(data[1]);
                                break;
                            }
                        case "sound":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendString(stream, MsgStringPacket.StringID.Sound, false, "sound/wind.wav", "1");

                                }
                                break;
                            }
                        case "sound2":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendString(stream, MsgStringPacket.StringID.Sound, false, "sound/fc2.wav", "1");

                                }
                                break;
                            }
                        case "attackspell":
                            {// zf2-e263
                             //foreach (var spell in Pool.Magic)
                                {

                                    foreach (var monster in client.Player.View.Roles(Role.MapObjectType.Monster))
                                    {
                                        //for (ushort x = 12000; x < 12600; x += 10)
                                        {
                                            //    for (ushort x = 12830; x <= 13090; x+= 10)
                                            {
                                                using (var rec = new ServerSockets.RecycledPacket())
                                                {
                                                    var stream = rec.GetStream();

                                                    //    client.MySpells.Add(stream, x);
                                                    MsgSpellAnimation animation = new MsgSpellAnimation(monster.UID, 0, client.Player.X, client.Player.Y, ushort.Parse(data[1]), /*ushort.Parse(data[2])*/0, /*byte.Parse(data[3])*/0, 0);
                                                    var objat = new MsgSpellAnimation.SpellObj(client.Player.UID, 100, (MsgAttackPacket.AttackEffect)(1 << ushort.Parse(data[2])));//.None);

                                                    //objat.Hit = 0;
                                                    animation.Targets.Enqueue(objat);

                                                    animation.SetStream(stream);
                                                    animation.JustMe(client);

                                                    //                                                System.Threading.Thread.Sleep(400);






                                                }
                                            }
                                        }
                                        // Console.WriteLine(spell.Key);
                                        //   System.Threading.Thread.Sleep(500);
                                        /*  animation.Create();
                                          unsafe
                                          {
                                              foreach (var packet in animation.GetPackets())
                                              {
                                                  fixed (byte* ptr = packet)
                                                      client.Send(ptr);
                                              }
                                          }*/
                                        break;
                                    }
                                }
                                break;
                            }
                        case "attacknormal":
                            {
                                foreach (var monster in client.Player.View.Roles(Role.MapObjectType.Monster))
                                {


                                    //for (int x = 50; x < 60; x++)
                                    {

                                        InteractQuery attack = new InteractQuery();
                                        attack.UID = monster.UID;
                                        attack.AtkType = (MsgAttackPacket.AttackID)ushort.Parse(data[1]);
                                        // attack.SpellID = 12070;
                                        attack.Damage = 1;
                                        attack.OpponentUID = client.Player.UID;
                                        attack.X = client.Player.X;
                                        attack.Y = client.Player.Y;
                                        // attack.ResponseDamage = 12070;
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            client.Player.View.SendView(stream.InteractionCreate(&attack), true);
                                        }
                                    }
                                    break;
                                }
                                break;
                            }
                        case "ef":
                            {
                                Game.MsgServer.MsgMovement.eeffect = int.Parse(data[1]);
                                break;
                            }
                        case "teleback":
                            {
                                client.TeleportCallBack();
                                break;
                            }
                        case "map":
                            {
                                client.SendSysMesage("MapID = " + client.Player.Map, ChatMode.System);
                                break;
                            }
                        case "studypoints":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.Player.SubClass.AddStudyPoints(client, ushort.Parse(data[1]), rec.GetStream());
                                break;
                            }
                        case "expball":
                            {
                                client.GainExpBall(double.Parse(data[1]), true, Role.Flags.ExperienceEffect.angelwing);
                                break;
                            }
                        case "string_effect":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, data[1]);
                                }
                                break;
                            }
                        case "string_effect3":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    // for (int x = 0; x < 100; x++)
                                    {
                                        Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
                                        packet.ID = (MsgStringPacket.StringID)ushort.Parse(data[1]);
                                        packet.X = ushort.Parse(data[2]);
                                        packet.Y = ushort.Parse(data[3]);
                                        packet.Strings = new string[1] { "movego" }; ;
                                        client.Send(stream.StringPacketCreate(packet));
                                    }
                                }
                                break;
                            }
                        case "string_effect2":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    Game.MsgNpc.Npc npc = new MsgNpc.Npc();

                                    stream.InitWriter();

                                    stream.Write(1);
                                    stream.Write(0);
                                    stream.Write(0);
                                    stream.Write(0);
                                    stream.Write(ushort.Parse(data[1]));
                                    stream.Write(ushort.Parse(data[2]));
                                    stream.Write((ushort)385);
                                    stream.Write((ushort)26);
                                    stream.Write((uint)0);
                                    stream.Write((uint)0);
                                    stream.Write((uint)0);
                                    stream.Write(" ");
                                    stream.Finalize(Game.GamePackets.SobNpcs);

                                    client.Send(stream);
                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, 1, true, data[3]);

                                    /*System.Threading.Thread.Sleep(3000);
                                    var action = new ActionQuery()
                                    {
                                         Type = ActionType.RemoveEntity,
                                         ObjId =1
                                    };
                                   client.Send(stream.ActionCreate(&action));*/


                                }
                                break;
                            }
                        case "gh":
                            {
                                byte[] pp = new byte[]
                                {
                                0x02,0x00,0x00,0x00
,0x19,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x20,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x14,0x00,0x00,0x00
,0x2E,0x00,0x00,0x00,0x35,0x00,0x00,0x00,0x03,0x00,0x00,0x00,0x28,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
                                };
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    stream.InitWriter();
                                    stream.Write(Environment.TickCount);
                                    stream.Write(client.Player.UID);
                                    for (int x = 0; x < pp.Length; x++)
                                        stream.Write((byte)pp[x]);
                                    stream.Finalize(10017);
                                    client.Send(stream);
                                }
                                byte[] pp2 = new byte[]
                                {
                                0x01,0x00,0x00,0x00,0x35,0x00,0x00,0x00
,0x00,0x01,0x00,0x00,0x28,0x00,0x00,0x00,0x03,0x00,0x00,0x00,0,0,0,0
                                };
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    stream.InitWriter();

                                    stream.Write(client.Player.UID);
                                    for (int x = 0; x < pp2.Length; x++)
                                        stream.Write((byte)pp2[x]);
                                    stream.Finalize(2075);
                                    client.Send(stream);
                                }
                                break;
                            }
                        case "xp":
                            {
                                client.Player.AddFlag(MsgUpdate.Flags.XPList, 20, true);
                                break;
                            }
                        case "addflag":
                            {
                                client.Player.AddFlag((MsgUpdate.Flags)int.Parse(data[1]), 10, true, 0, 50, 39);
                                break;
                            }
                        case "remflag":
                            {
                                client.Player.RemoveFlag((MsgUpdate.Flags)int.Parse(data[1]));
                                break;
                            }
                        case "level":
                            {
                                byte amount = 0;
                                if (byte.TryParse(data[1], out amount))
                                {
                                    using (var rec = new RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.UpdateLevel(stream, amount, true);
                                    }
                                    ReallotPoints(client);
                                }
                                break;
                            }
                        case "incexp":
                            {
                                ulong exp;
                                if (ulong.TryParse(data[1], out exp))
                                {
                                    using (var rec = new RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.IncreaseExperience(stream, exp);
                                    }
                                }
                                break;
                            }
                        case "money":
                            {
                                long amount = 0;
                                if (long.TryParse(data[1], out amount))
                                {
                                    client.Player.Money = (uint)amount;
                                    using (var rec = new RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                                    }
                                }
                                break;
                            }
                        case "do":
                            {
                                int n = int.Parse(data[1]);
                                string rest = Message.Substring(3 + data[1].Length + 1);
                                for (int i = 0; i < n; i++)
                                    ChatCommands(client, new MsgMessage(rest, MsgColor.red, msg.ChatType));
                                break;
                            }
                        case "soulp":
                            {
                                using (var rec = new RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    uint level = uint.Parse(data[1]);
                                    SafeDictionaryAlt<uint, ItemType.DBItem> BaseInformations = new SafeDictionaryAlt<uint, ItemType.DBItem>();
                                    foreach (var item in Pool.ItemsBase.Values)
                                    {
                                        if (item.PurificationLevel == level)
                                            BaseInformations.Add(item.ID, item);
                                    }
                                    var itemarray = BaseInformations.Values.ToArray();
                                    foreach (var item in itemarray)
                                        client.Inventory.Add(stream, item.ID);
                                }
                                break;
                            }
                        case "refp":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    uint level = uint.Parse(data[1]);
                                    SafeDictionaryAlt<uint, Database.Refinery.Item> BaseInformations = new SafeDictionaryAlt<uint, Database.Refinery.Item>();
                                    foreach (var item in Pool.RefineryItems.Values)
                                    {
                                        if (item.Level == level)
                                            BaseInformations.Add(item.ItemID, item);
                                    }
                                    var itemarray = BaseInformations.Values.ToArray();
                                    foreach (var item in itemarray)
                                    {
                                        if (!(item.ItemName.Contains("(B)")))
                                            client.Inventory.Add(stream, item.ItemID);
                                    }
                                }
                                break;
                            }
                       
                        /*case "testaddflag":
                            {
                                client.Player.AddFlag((MsgUpdate.Flags)byte.Parse(data[1]), Role.StatusFlagsBigVector32.PermanentFlag, false);
                                break;
                            }*/
                        case "lotteryreset":
                            {
                                client.Player.LotteryEntries = 0;
                                break;
                            }
                        case "sendtick":
                            {
                                using (var rec = new RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    client.Send(stream.SendTick(client.Player));
                                }
                                break;
                            }
                        case "aura":
                            {
                                if (client.Player.UseAura != MsgUpdate.Flags.Normal)
                                    client.Player.AddAura(client.Player.UseAura, null, Role.StatusFlagsBigVector32.PermanentFlag);
                                break;
                            }
                        case "crit":
                            {
                                client.Status.CriticalStrike += 1000;
                                break;
                            }
                        /*case "testwh":
                            {
                                client.SendWhisper("You`ve been knocked out from PassTheBomb. Goodluck next time.", "[PassTheBomb]", client.Player.Name);
                                break;
                            }*/
                        case "testmoob":
                            {
                                if (client.Map.ContainMobID(uint.Parse(data[1])))
                                    break;
                                using (var rec = new ServerSockets.RecycledPacket())
                                    Database.Server.AddMapMonster(rec.GetStream(), client.Map, uint.Parse(data[1]), client.Player.X, client.Player.Y, ushort.Parse(data[2]), ushort.Parse(data[3]), byte.Parse(data[4]));
                                break;
                            }
                        
                        case "santaitems":
                            {
                                List<uint> SantaIngredients = new List<uint>() { 711390, 711391, 710642, 710646, 710647, 710648, 710649, 710657, 720157, 720158, 720159, 720160, 720161 };
                                foreach (var item in SantaIngredients)
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Inventory.AddItemWitchStack(item, 0, 20, stream);
                                    }
                                break;
                            }
                                                                                                           
                        case "bot":
                            {
                                if (!Core.Features.FeatureRegistry.IsKept("extras.bot")) break; // [feature-gate extras.bot]
                                for (int i = 0; i < ushort.Parse(data[1]); i++)
                                {
                                    Bot.AI bot = new Bot.AI()
                                    {
                                        Map = client.Map,
                                        HP = (int)client.Status.MaxHitpoints,
                                        X = client.Player.X,
                                        Y = client.Player.Y,
                                        Body = client.Player.Body,
                                        MapID = client.Player.Map,

                                    };
                                    bot.Add();
                                }
                                break;
                            }
                        case "abot":
                            {
                                if (!Core.Features.FeatureRegistry.IsKept("extras.bot")) break; // [feature-gate extras.bot]
                                for (int i = 0; i < ushort.Parse(data[1]); i++)
                                {
                                    Bot.AI bot = new Bot.AI()
                                    {
                                        Map = client.Map,
                                        HP = (int)client.Status.MaxHitpoints,
                                        X = client.Player.X,
                                        Y = client.Player.Y,
                                        Body = client.Player.Body,
                                        MapID = client.Player.Map,
                                    };
                                    bot.Add(true);
                                }
                                break;
                            }
                        case "citywar":
                            {
                                if (MsgSchedules.CityWar.Proces == ProcesType.Dead)
                                    MsgSchedules.CityWar.Start();
                                else
                                    MsgSchedules.CityWar.CompleteEndGuildWar();
                                break;
                            }
                        case "bomba":
                            {
                                MsgSchedules.CurrentTournament.Open();
                                MsgSchedules.CurrentTournament = Pool.Tournaments[TournamentType.PassTheBomb];
                                break;
                            }
                        case "dragonwar":
                            {
                                MsgSchedules.CurrentTournament.Open();
                                MsgSchedules.CurrentTournament = Pool.Tournaments[TournamentType.DragonWar];
                                break;
                            }
                        /*case "new1":
                            {
                                var Map = Pool.ServerMaps[1011];

                                if (!Map.ContainMobID(3130))
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        Database.Server.AddMapMonster(stream, Map, 3130, 659, 772, 1, 1, 1);
                                    }
                                }
                                break;
                                
                            }
                        case "new3":
                            {
                                var Map = Pool.ServerMaps[1000];

                                if (!Map.ContainMobID(3738))
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        Database.Server.AddMapMonster(stream, Map, 3738, 501, 708, 1, 1, 1);
                                    }
                                }
                                break;

                            }
                        case "new4":
                            {
                                var Map = Pool.ServerMaps[1002];

                                if (!Map.ContainMobID(3739))
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        Database.Server.AddMapMonster(stream, Map, 3739, 656, 677, 1, 1, 1);
                                    }
                                }
                                break;

                            }
                        case "new5":
                            {
                                var Map = Pool.ServerMaps[1002];

                                if (!Map.ContainMobID(4145))
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        Database.Server.AddMapMonster(stream, Map, 4145, 656, 677, 1, 1, 1);
                                    }
                                }
                                break;

                            }
                        case "new2":
                            {
                                var Map = Pool.ServerMaps[1015];

                                if (!Map.ContainMobID(3737))
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        Database.Server.AddMapMonster(stream, Map, 3737, 651, 675, 1, 1, 1);
                                    }
                                }
                                break;
                            }*/
                        case "testspawnmob":
                            {
                                using (var rec = new RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    uint monsterId = 0;
                                    uint.TryParse(data[1], out monsterId);
                                    uint quantitySpawn = 1;
                                    if (data.Length > 2)
                                    {
                                        uint.TryParse(data[2], out quantitySpawn);
                                    }
                                    if (monsterId != 0)
                                    {
                                        for(var i = 0; i < quantitySpawn; i++)
                                        {
                                            Server.AddMapMonster(stream, client.Map, monsterId, (ushort)(client.Player.X + i), (ushort)(client.Player.Y + i), 1, 1, 1, 0);
                                        }
                                    } else
                                    {
                                        client.SendSysMesage("Invalid Monster ID!");
                                    }
                                    client.Send(stream);
                                }
                                break;
                            }
                        case "topnobility":
                            {
                                MsgSchedules.CurrentTournament = Pool.Tournaments[TournamentType.TopFight];
                                MsgSchedules.CurrentTournament.Open();
                                break;
                            }
                        case "frozensky":
                            {
                                MsgSchedules.CurrentTournament = Pool.Tournaments[TournamentType.FrozenSky];
                                MsgSchedules.CurrentTournament.Open();
                                break;
                            }
                        case "frozenskyoff":
                            {
                                MsgSchedules.CurrentTournament = Pool.Tournaments[TournamentType.FrozenSky];
                                MsgSchedules.CurrentTournament.CheckUp();
                                break;
                            }
                        case "treasure":
                            {
                                MsgSchedules.CurrentTournament = Pool.Tournaments[TournamentType.TreasureThief];
                                MsgSchedules.CurrentTournament.Open();
                                break;
                            }
                        case "lastman":
                            {
                                if (MsgSchedules.CurrentTournament.Process == ProcesType.Dead)
                                {
                                    MsgSchedules.CurrentTournament = Pool.Tournaments[TournamentType.LastManStand];
                                    MsgSchedules.CurrentTournament.Open();
                                }
                                else
                                {
                                    Console.WriteLine("Cannot start lastman because already have a tournament in course...");
                                }
                                break;
                            }
                        case "trinitypoints":
                            {
                                uint amount = 0;
                                if (uint.TryParse(data[1], out amount))
                                {
                                    client.Player.TrinityPoints = amount;
                                }
                                break;
                            }
                        case "teraton":
                            {
                                var Map = Pool.ServerMaps[2056];
                                if (!Map.ContainMobID(20060))
                                {

                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        Database.Server.AddMapMonster(stream, Map, 20060, 328, 354, 1, 1, 1, 0);

                                    }
                                }
                                break;
                            }
                        case "ann":
                            {//segund
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("TrinityConquer " + client.Player.Name + ": " + Message.Remove(0, 3) + "", Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.World).GetArray(stream));

                                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("TrinityConquer " + client.Player.Name + ": " + Message.Remove(0, 3) + "", Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                }
                                break;

                            }
                        case "cps":
                            {
                                uint amount = 0;
                                if (uint.TryParse(data[1], out amount))
                                {
                                    client.Player.ConquerPoints = amount;

                                }
                                break;
                            }
                        case "a7a":
                            {
                                client.Player.AddFlag(MsgUpdate.Flags.TopSpouse, 100, true);
                                break;
                            }
                        
                        case "sofokedatos":
                            {
                                client.SendSysMesage("Cps hunted since restart : " + string.Format("{0:n0}", Program.CPsHuntedSinceRestart));
                                client.SendSysMesage("ExpBalls hunted since restart : " + string.Format("{0:n0}", Program.ExpBallsDropped));
                                client.SendSysMesage("+8 Elite: " + string.Format("{0:n0}", Program.Plus8));
                                client.SendSysMesage("Super1Sock: " + string.Format("{0:n0}", Program.Super1Soc));
                                client.SendSysMesage("Super2Sock: " + string.Format("{0:n0}", Program.Super2Soc));
                                client.SendSysMesage("SuperNoSock: " + string.Format("{0:n0}", Program.SuperNoSoc));
                                break;
                            }
                        case "boundcps":
                            {
                                int amount = 0;
                                if (int.TryParse(data[1], out amount))
                                {
                                    client.Player.BoundConquerPoints = amount;

                                }
                                break;
                            }
                        case "presentflag":
                            {
                                Console.WriteLine("");
                                Console.WriteLine(client.Map.cells[client.Player.X, client.Player.Y].ToString());
                                break;
                            }
                        case "remspell":
                            {
                                ushort ID = 0;
                                if (!ushort.TryParse(data[1], out ID))
                                {
                                    client.SendSysMesage("Invlid spell ID !");
                                    break;
                                }
                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.MySpells.Remove(ID, rec.GetStream());
                                break;
                            }
                        case "spell":
                            {
                                ushort ID = 0;
                                if (!ushort.TryParse(data[1], out ID))
                                {
                                    client.SendSysMesage("Invlid spell ID !");
                                    break;
                                }
                                byte level = 0;
                                if (!byte.TryParse(data[2], out level))
                                {
                                    client.SendSysMesage("Invalid spell Level.");
                                    break;
                                }
                                int Experience = 0;
                                if (data.Length > 3)
                                {
                                    if (!int.TryParse(data[3], out Experience))
                                    {
                                        client.SendSysMesage("Invalid spell Experience.");
                                        break;
                                    }
                                }
                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.MySpells.Add(rec.GetStream(), ID, level, 0, 0, Experience);
                                break;
                            }
                        case "prof":
                            {
                                ushort ID = 0;
                                if (!ushort.TryParse(data[1], out ID))
                                {
                                    client.SendSysMesage("Invalid prof ID.");
                                    break;
                                }
                                byte level = 0;
                                if (!byte.TryParse(data[2], out level))
                                {
                                    client.SendSysMesage("Invalid prof Level.");
                                    break;
                                }
                                uint Experience = 0;
                                if (data.Length > 3)
                                {
                                    if (!uint.TryParse(data[3], out Experience))
                                    {
                                        client.SendSysMesage("Invalid prof Experience.");
                                        break;
                                    }
                                }
                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.MyProfs.Add(rec.GetStream(), ID, level, Experience);
                                break;
                            }
                        case "clear":
                        case "clearinventory":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.Inventory.Clear(rec.GetStream());
                                break;
                            }
                        case "hhhj":
                            {
                                client.SendSysMesage("Congrats! Rm2015`s FantasyKnife has climbed to No.38 place on the Perfection Ranking. [Link I want to get on the list###1 345]", ChatMode.TopLeftSystem);
                                break;
                            }
                        case "dancastle":
                            {
                                client.Teleport(200, 200, 1601);
                                client.Player.MessageBox("Welcome back to Fifty Shades of Dan ;)", null, null);
                                break;
                            }
                        case "tele":
                            {
                                /*string Maps = "";
                                 foreach (var amap in Role.GameMap.MapContents)
                                {
                                      Console.Write(amap.Key + " / ");
                                    Maps += amap.Key;
                                    Maps += Environment.NewLine;

                                     System.IO.StreamWriter SW = new System.IO.StreamWriter(@"C:\PacketSniffing\PMaps" + 1 + ".txt", true);
                                      SW.WriteLine(Maps);
                                      SW.Flush();
                                      SW.Close();
                                }*/
                                client.TerainMask = 0;
                                uint mapid = 0;
                                if (!uint.TryParse(data[1], out mapid))
                                {
                                    client.SendSysMesage("Invlid Map ID !");
                                    break;
                                }
                                ushort X = 0;
                                if (!ushort.TryParse(data[2], out X))
                                {
                                    client.SendSysMesage("Invlid X !");
                                    break;
                                }
                                ushort Y = 0;
                                if (!ushort.TryParse(data[3], out Y))
                                {
                                    client.SendSysMesage("Invlid Y !");
                                    break;
                                }
                                if (mapid == 1601)
                                {
                                    client.SendSysMesage("You can`t go there, its nova`s office.");
                                    break;
                                }
                                //uint DinamicID = 0;
                                //if (!uint.TryParse(data[4], out DinamicID))
                                //{
                                //    client.SendSysMesage("Invlid DinamicID !");
                                //    break;
                                //}
                                //client.Teleport(X, Y, mapid, DinamicID);
                                client.Teleport(X, Y, mapid);
                                break;
                            }
                        case "effectfloor":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    //   for (uint x = 700; x < 750; x++)
                                    {
                                        MsgServer.MsgGameItem item = new MsgServer.MsgGameItem();
                                        item.Color = (Role.Flags.Color)2;
                                        item.ITEM_ID = uint.Parse(data[1]);//1182;
                                        MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(item, client.Player.X, client.Player.Y, MsgFloorItem.MsgItem.ItemType.Effect, 0, 0, client.Player.Map
                                               , 0, false, client.Map, 4);

                                        if (client.Map.EnqueueItem(DropItem))
                                            DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Effect);
                                    }
                                }
                                break;
                            }
                        case "tele2":
                            {
                                uint mapid = 0;
                                if (!uint.TryParse(data[1], out mapid))
                                {
                                    client.SendSysMesage("Invlid Map ID !");
                                    break;
                                }
                                ushort X = 0;
                                if (!ushort.TryParse(data[2], out X))
                                {
                                    client.SendSysMesage("Invlid X !");
                                    break;
                                }
                                ushort Y = 0;
                                if (!ushort.TryParse(data[3], out Y))
                                {
                                    client.SendSysMesage("Invlid Y !");
                                    break;
                                }
                                foreach (var map in Pool.ServerMaps.Values)
                                {
                                    mapid = map.ID;
                                    Console.WriteLine(map.ID);
                                    ActionQuery action = new ActionQuery()
                                    {
                                        ObjId = client.Player.UID,
                                        Type = ActionType.Teleport,
                                        dwParam = mapid,
                                        wParam1 = X,
                                        wParam2 = Y,
                                        dwParam3 = mapid
                                    };
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        client.Send(rec.GetStream().ActionCreate(&action));
                                        client.Send(rec.GetStream().MapStatusCreate(mapid, mapid, 8));
                                    }
                                    System.Threading.Thread.Sleep(1000);
                                }
                                break;
                            }
                        case "itemid":
                            {
                                if (data.Length < 2)
                                {
                                    client.SendSysMesage("Invalid syntaxy. Example @itemid iditem quantity or @itemid iditem");
                                }
                                else
                                {
                                    uint ID = 0;
                                    uint Quantity = 0;
                                    if (!uint.TryParse(data[1], out ID))
                                    {
                                        client.SendSysMesage("Invalid item ID specified.");
                                        break;
                                    }
                                    if (data.Length > 2 && !uint.TryParse(data[2], out Quantity))
                                    {
                                        client.SendSysMesage("Invalid item quantity specified.");
                                        break;
                                    }
                                    if (Quantity > 0)
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                            client.Inventory.AddItemWitchStack(ID, 0, (ushort)Quantity, rec.GetStream());
                                    }
                                    else
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                            client.Inventory.Add(rec.GetStream(), ID);
                                    }
                                }
                                break;
                            }
                        case "randomp1":
                            {
                                var array = Database.ItemType.PurificationItems[1].Values.ToArray();
                                int position = Pool.GetRandom.Next(0, array.Length);
                                var item = array[position].ID;


                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.Inventory.Add(rec.GetStream(), item, 10);
                                break;
                            }
                        case "randomp2":
                            {
                                var array = Database.ItemType.PurificationItems[2].Values.ToArray();
                                int position = Pool.GetRandom.Next(0, array.Length);
                                var item = array[position].ID;


                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.Inventory.Add(rec.GetStream(), item, 10);
                                break;
                            }
                        case "randomp3":
                            {
                                var array = Database.ItemType.PurificationItems[3].Values.ToArray();
                                int position = Pool.GetRandom.Next(0, array.Length);
                                var item = array[position].ID;


                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.Inventory.Add(rec.GetStream(), item, 10);
                                break;
                            }
                        case "randomp4":
                            {
                                var array = Database.ItemType.PurificationItems[4].Values.ToArray();
                                int position = Pool.GetRandom.Next(0, array.Length);
                                var item = array[position].ID;


                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.Inventory.Add(rec.GetStream(), item, 10);
                                break;
                            }
                        case "randomp5":
                            {
                                var array = Database.ItemType.PurificationItems[5].Values.ToArray();
                                int position = Pool.GetRandom.Next(0, array.Length);
                                var item = array[position].ID;


                                using (var rec = new ServerSockets.RecycledPacket())
                                    client.Inventory.Add(rec.GetStream(), item, 10);
                                break;
                            }
                        case "itemminute":
                            {
                                if (!uint.TryParse(data[1], out uint ID))
                                {
                                    client.SendSysMesage("Invalid item ID!");
                                    break;
                                }
                                if (!int.TryParse(data[2], out int minutes))
                                {
                                    client.SendSysMesage("Invalid item Minute Value!");
                                    break;
                                }
                                if (!byte.TryParse(data[3], out byte quantityItems))
                                {
                                    quantityItems = 1;
                                }
                                using var rec = new RecycledPacket();
                                client.Inventory.AddMinute(rec.GetStream(), ID, minutes, quantityItems);
                                break;
                            }
                        case "item":
                            {
                                uint ID = 0;
                                if (!uint.TryParse(data[1], out ID))
                                {
                                    client.SendSysMesage("Invlid item ID !");
                                    break;
                                }
                                byte plus = 0;
                                if (!byte.TryParse(data[2], out plus))
                                {
                                    client.SendSysMesage("Invlid item plus !");
                                    break;
                                }
                                byte bless = 0;
                                if (!byte.TryParse(data[3], out bless))
                                {
                                    client.SendSysMesage("Invlid item Enchant !");
                                    break;
                                }
                                byte enchant = 0;
                                if (!byte.TryParse(data[4], out enchant))
                                {
                                    client.SendSysMesage("Invlid item Enchant !");
                                    break;
                                }
                                byte sockone = 0;
                                if (!byte.TryParse(data[5], out sockone))
                                {
                                    client.SendSysMesage("Invlid item Socket One !");
                                    break;
                                }
                                byte socktwo = 0;
                                if (!byte.TryParse(data[6], out socktwo))
                                {
                                    client.SendSysMesage("Invlid item Socket Two !");
                                    break;
                                }
                                byte count = 1;
                                if (data.Length > 7)
                                {
                                    if (!byte.TryParse(data[7], out count))
                                    {
                                        client.SendSysMesage("Invlid item count !");
                                        break;
                                    }
                                }
                                byte Effect = 0;
                                if (data.Length > 8)
                                {
                                    if (!byte.TryParse(data[8], out Effect))
                                    {
                                        client.SendSysMesage("Invlid Effect Type !");
                                        break;
                                    }
                                }
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    client.Inventory.Add(rec.GetStream(), ID, count, plus, bless, enchant, (Role.Flags.Gem)sockone, (Role.Flags.Gem)socktwo, false, (Role.Flags.ItemEffect)Effect);
                                }
                                break;
                            }
                        case "bcps":
                            {
                                client.Player.BoundConquerPoints = int.Parse(data[1]);
                                break;
                            }
                        case "pkwar":
                            {
                                MsgSchedules.PkWar.Open();
                                break;
                            }
                        case "poledominationac":
                            {
                                MsgSchedules.PoleDomination.Began();
                                MsgSchedules.PoleDomination.Start();
                                MsgSchedules.SendInvitation("[PM Requested] ApeCity PoleDomination", "ConquerPoints", 576, 623, 1020, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                                break;
                            }
                        case "poledominationbi":
                            {
                                MsgSchedules.PoleDominationBI.Began();
                                MsgSchedules.PoleDominationBI.Start();
                                MsgSchedules.SendInvitation("[PM Requested] BirdIland PoleDomination", "ConquerPoints", 718, 573, 1015, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                                break;
                            }
                        case "poledominationpc":
                            {
                                MsgSchedules.PoleDominationPC.Began();
                                MsgSchedules.PoleDominationPC.Start();
                                MsgSchedules.SendInvitation("[PM Requested] PhoenixCastle PoleDomination", "ConquerPoints", 275, 288, 1011, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                                break;
                            }
                        case "poledominationdc":
                            {
                                MsgSchedules.PoleDominationDC.Began();
                                MsgSchedules.PoleDominationDC.Start();
                                MsgSchedules.SendInvitation("[PM Requested] DesertCity PoleDomination", "ConquerPoints", 469, 657, 1000, 0, 60, MsgServer.MsgStaticMessage.Messages.None);
                                break;
                            }
                        case "additemstack":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Inventory.AddItemWitchStack(uint.Parse(data[1]), 0, ushort.Parse(data[2]), stream);
                                }
                                break;
                            }
                        case "remitemstack":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Inventory.RemoveStackItem(uint.Parse(data[1]), ushort.Parse(data[2]), stream);
                                }
                                break;
                            }
                        case "fftest":
                            {
                                ushort x1 = ushort.Parse(data[1]);
                                ushort y1 = ushort.Parse(data[2]);
                                ushort x2 = ushort.Parse(data[3]);
                                ushort y2 = ushort.Parse(data[4]);
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    byte[] pp = new byte[]
                            {
                            0xE3,0xBC,0x82,0x00,0xFC,0xBB,0x0D,0x00,0xFA,0x05,0x00,0x00
,0x3C,0x01,0x9A,0x01,0x00,0x00,0x0A,0x00,0x00,0x00,0x00,0x0E,0x89,0x45,0x0F,0x00
,0x02,0x00,0x00,0x00,0x03,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x3B,0x01,0x9B
,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
                            };

                                    int size = stream.Size;

                                    stream.InitWriter();
                                    for (int x = 0; x < pp.Length; x++)
                                        stream.Write((byte)pp[x]);


                                    stream.Finalize(1101);

                                    stream.Seek(16);
                                    stream.Write(x1);
                                    stream.Write(y1);
                                    stream.Seek(60);
                                    stream.Write(x2);
                                    stream.Write(y2);

                                    stream.Seek(size);

                                    client.Send(stream);


                                    pp = new byte[]
                              {
                              0xFC,0xBB,0x0D,0x00,0x3B,0x01,0x9B,0x01,0xBE,0x32,0x00,0x00
,0x00,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00

                              };

                                    stream.InitWriter();
                                    for (int x = 0; x < pp.Length; x++)
                                        stream.Write((byte)pp[x]);

                                    size = stream.Size;
                                    stream.Seek(8);
                                    stream.Write(x2);
                                    stream.Write(y2);

                                    stream.Seek(size);
                                    stream.Finalize(1105);
                                    client.Send(stream);

                                    pp = new byte[]
                                    {
                                    0x21,0xC2,0x82,0x00,0xFC,0xBB,0x0D,0x00,0xFA,0x05,0x00,0x00
,0x3C,0x01,0x9A,0x01,0x00,0x00,0x0C,0x00,0x00,0x00,0x00,0x0E,0x89,0x45,0x0F,0x00
,0x02,0x00,0x00,0x00,0x03,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x3B,0x01,0x9B
,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00

                                    };
                                    stream.InitWriter();
                                    for (int x = 0; x < pp.Length; x++)
                                        stream.Write((byte)pp[x]);


                                    stream.Finalize(1101);
                                    size = stream.Size;
                                    stream.Seek(16);
                                    stream.Write(x1);
                                    stream.Write(y1);
                                    stream.Seek(60);
                                    stream.Write(x2);
                                    stream.Write(y2);

                                    stream.Seek(size);

                                    client.Send(stream);
                                }
                                break;
                            }
                        case "atest":
                            {

                                break;
                            }
                        
                        case "ftest":
                            {
                                int size = 0;

                                byte[] pp = new byte[]
                                {
                         0x89,0x45,0x0F,0x00,0x3C,0x01,0x9A,0x01,0xBE,0x32,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00
                                };

                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();


                                    stream.InitWriter();
                                    for (int x = 0; x < pp.Length; x++)
                                        stream.Write((byte)pp[x]);


                                    stream.Finalize(1105);
                                    size = stream.Size;
                                    stream.Seek(4);
                                    stream.Write(client.Player.UID);
                                    stream.Write((ushort)(client.Player.X - 1));
                                    stream.Write((ushort)(client.Player.Y - 1));
                                    stream.Seek(size);


                                    client.Send(stream);




                                    pp = new byte[]
                                    {
                                   0xE3,0xBC,0x82,0x00,0xFC,0xBB,0x0D,0x00,0xFA,0x05,0x00,0x00
,0x3C,0x01,0x9A,0x01,0x00,0x00,0x0A,0x00,0x00,0x00,0x00,0x0E,0x89,0x45,0x0F,0x00
,0x02,0x00,0x00,0x00,0x03,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x3B,0x01,0x9B
,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
                                    };

                                    stream.InitWriter();
                                    for (int x = 0; x < pp.Length; x++)
                                        stream.Write((byte)pp[x]);
                                    stream.Finalize(1101);
                                    size = stream.Size;
                                    stream.Seek(28);
                                    stream.Write(client.Player.UID);
                                    stream.Seek(16);
                                    stream.Write((ushort)(client.Player.X - 1));
                                    stream.Write((ushort)(client.Player.Y - 1));

                                    stream.Seek(size);
                                    client.Send(stream);



                                    pp = new byte[]
                                    {
                                 0xFC,0xBB,0x0D,0x00,0x3B,0x01,0x9B,0x01,0xBE,0x32,0x00,0x00
,0x00,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00
                                    };
                                    stream.InitWriter();
                                    for (int x = 0; x < pp.Length; x++)
                                        stream.Write((byte)pp[x]);
                                    stream.Finalize(1105);
                                    size = stream.Size;
                                    stream.Seek(4);
                                    stream.Write(client.Player.UID);
                                    stream.Write((ushort)(client.Player.X - 1));
                                    stream.Write((ushort)(client.Player.Y - 1));
                                    stream.Seek(size);


                                    client.Send(stream);
                                    pp = new byte[]
                                    {
                                  0x21,0xC2,0x82,0x00,0xFC,0xBB,0x0D,0x00,0xFA,0x05,0x00,0x00
,0x3C,0x01,0x9A,0x01,0x00,0x00,0x0C,0x00,0x00,0x00,0x00,0x0E,0x89,0x45,0x0F,0x00
,0x02,0x00,0x00,0x00,0x03,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x3B,0x01,0x9B
,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
                                    };


                                    stream.InitWriter();
                                    for (int x = 0; x < pp.Length; x++)
                                        stream.Write((byte)pp[x]);
                                    stream.Finalize(1101);
                                    size = stream.Size;
                                    stream.Seek(28);
                                    stream.Write(client.Player.UID);
                                    stream.Seek(16);
                                    stream.Write((ushort)(client.Player.X - 1));
                                    stream.Write((ushort)(client.Player.Y - 1));
                                    stream.Seek(size);
                                    client.Send(stream);


                                }
                                break;
                            }
                        case "floor":
                            {
                                Game.MsgFloorItem.MsgItemPacket FloorPacket = Game.MsgFloorItem.MsgItemPacket.Create();
                                FloorPacket.m_UID = Game.MsgFloorItem.MsgItem.UIDS.Next;
                                FloorPacket.m_ID = uint.Parse(data[1]);
                                FloorPacket.m_X = client.Player.X;
                                FloorPacket.m_Y = client.Player.Y;
                                FloorPacket.Timer = Role.Core.TqTimer(DateTime.Now.AddSeconds(4));
                                FloorPacket.m_Color = (byte)14;//4;
                                FloorPacket.m_Color2 = (byte)14;//14
                                FloorPacket.FlowerType = (byte)0;
                                FloorPacket.DropType = MsgDropID.Effect;
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var packet = rec.GetStream();
                                    client.Player.View.SendView(packet.ItemPacketCreate(FloorPacket), true);
                                }
                                break;
                            }
                        case "eat":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var packet = rec.GetStream();
                                    //   for(int i =0; i < 30; i++)
                                    //for(int x = 0; x< 30; x++)
                                    {
                                        Game.MsgFloorItem.MsgItemPacket effect = Game.MsgFloorItem.MsgItemPacket.Create();
                                        effect.m_UID = (uint)uint.Parse(data[1]);// Game.MsgFloorItem.MsgItemPacket.EffectMonsters.EarthquakeUpDown;
                                        effect.DropType = (MsgDropID)13;
                                        effect.m_X = client.Player.X;
                                        effect.m_Y = client.Player.Y;
                                        effect.ItemOwnerUID = client.Player.UID;
                                        client.Send(packet.ItemPacketCreate(effect));
                                    }
                                }
                                break;
                            }
                        case "activetrap":
                            {
                                // for (ushort x = ushort.Parse(data[1]); x < ushort.Parse(data[2]); x++)
                                {
                                    Game.MsgFloorItem.MsgItemPacket FloorPacket = Game.MsgFloorItem.MsgItemPacket.Create();
                                    FloorPacket.m_UID = Game.MsgFloorItem.MsgItem.UIDS.Count;
                                    FloorPacket.m_ID = 1390;
                                    FloorPacket.m_X = client.Player.X;
                                    FloorPacket.m_Y = client.Player.Y;

                                    FloorPacket.ItemOwnerUID = client.Player.UID;


                                    FloorPacket.m_Color = (byte)0;//4;
                                    FloorPacket.m_Color2 = (byte)14;
                                    FloorPacket.FlowerType = 3;
                                    FloorPacket.DropType = Game.MsgFloorItem.MsgDropID.RemoveEffect;
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var packet = rec.GetStream();
                                        client.Send(packet.ItemPacketCreate(FloorPacket));
                                    }
                                }
                                break;
                            }
                        case "bodynpcs":
                            {
                                Game.MsgServer.MsgMovement.Bodyyyy = uint.Parse(data[1]);
                                break;
                            }
                        case "addd":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    stream.ElitePkRankingCreate((MsgElitePkRanking.RankType)3, 2, MsgElitePKBrackets.GuiTyp.GUI_Top8Ranking, 0, client.Player.UID);
                                    stream.ElitePkRankingFinalize();
                                    client.Send(stream);
                                }
                                break;
                            }
                        case "aelite":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    stream.ElitePKBracketsCreate((MsgElitePKBrackets.Action)ushort.Parse(data[1]), 0, 0, MsgTournaments.MsgEliteTournament.GroupTyp.EPK_Lvl130Plus, MsgElitePKBrackets.GuiTyp.GUI_Top8Ranking, 0, 3);
                                    stream.ElitePKBracketsFinalize();
                                    Program.SendGlobalPackets.Enqueue(stream);
                                }
                                break;
                            }
                        case "teelite":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Send(stream.MsgInterServerIdentifier(0, client.Player.UID, uint.Parse(data[1]), client.GetAllMainItems().ToArray()));
                                    client.Player.SendString(stream, MsgStringPacket.StringID.ServerName, false, data[2]);
                                }
                                break;
                            }
                        case "transfer":
                            {
                                client.Player.InitializeTransfer(ushort.Parse(data[1]));
                                break;
                            }
                        case "inter":
                            {

                                MsgInterServer.PipeClient.Connect(client, Database.GroupServerList.InterServer.IPAddress, Database.GroupServerList.InterServer.Port);
                                break;
                            }
                        case "hp":
                            {
                                client.Player.HitPoints = int.Parse(data[1]);
                                client.Player.SendUpdateHP();
                                break;
                            }
                        case "championpoints":
                            {
                                client.Player.AddChampionPoints(uint.Parse(data[1]));
                                break;
                            }
                        case "tgui":
                            {
                                /*Data datapacket = new Data(true);
                                        datapacket.UID = client.Entity.UID;
                                        datapacket.ID = 162;
                                        datapacket.dwParam = 4020;
                                        datapacket.Facing = (Game.Enums.ConquerAngle)client.Entity.Facing;
                                        datapacket.wParam1 = 73;
                                        datapacket.wParam2 = 98;
                                        client.Send(datapacket);*/
                                var action = new ActionQuery()
                                {
                                    ObjId = client.Player.UID,
                                    Type = (ActionType)443,
                                    dwParam = uint.Parse(data[1])

                                };
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var packet = rec.GetStream();
                                    client.Send(packet.ActionCreate(&action));

                                }
                                break;
                            }
                        case "hair":
                            {
                                client.Player.Hair = (ushort)((client.Player.Hair - (client.Player.Hair % 100)) + ushort.Parse(data[1]));
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var packet = rec.GetStream();
                                    client.Player.SendUpdate(packet, client.Player.Hair, MsgServer.MsgUpdate.DataType.HairStyle);

                                }
                                break;
                            }
                        case "interip":
                            {

                                MsgInterServer.PipeClient.Connect(client, data[1], ushort.Parse(data[2]));
                                break;
                            }
                        case "dcinter":
                            {

                                client.Socket.Disconnect();
                                break;
                            }



                        case "floor2":
                            {
                                //for (ushort x = ushort.Parse(data[1]); x < ushort.Parse(data[2]); x++)
                                {
                                    Game.MsgFloorItem.MsgItemPacket FloorPacket = Game.MsgFloorItem.MsgItemPacket.Create();
                                    FloorPacket.m_UID = Game.MsgFloorItem.MsgItem.UIDS.Next;
                                    FloorPacket.m_ID = 930;
                                    FloorPacket.m_X = client.Player.X;
                                    FloorPacket.m_Y = client.Player.Y;
                                    FloorPacket.m_Color = 13;
                                    FloorPacket.FlowerType = 2;
                                    FloorPacket.Name = "AuroraLotus";
                                    FloorPacket.DropType = Game.MsgFloorItem.MsgDropID.Effect;
                                    FloorPacket.Timer = Role.Core.TqTimer(DateTime.Now.AddSeconds(5));
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var packet = rec.GetStream();
                                        client.Send(packet.ItemPacketCreate(FloorPacket));
                                    }
                                    //ItemPacketCreate
                                }
                                break;
                            }
                        case "wea":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var packet = rec.GetStream();
                                    packet.WeatherCreate((MsgWeather.WeatherType)ushort.Parse(data[1]), uint.Parse(data[2]), uint.Parse(data[3]), (uint)uint.Parse(data[4]), uint.Parse(data[5]));
                                    client.Send(packet);
                                }
                                break;
                            }
                        case "activefairi":
                            {
                                unsafe
                                {
                                    if (client.Player.testtttttttttt != 0)
                                    {
                                        MsgTransformFairy afair = MsgTransformFairy.Create();
                                        afair.Mode = MsgTransformFairy.Action.Dezactive;
                                        afair.FairyType = client.Player.testtttttttttt;
                                        afair.UID = client.Player.UID;


                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var packet = rec.GetStream();
                                            packet.TransformFairyCreate(MsgTransformFairy.Action.Dezactive, client.Player.testtttttttttt, client.Player.UID);
                                            client.Send(packet);
                                        }
                                    }


                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var packet = rec.GetStream();
                                        packet.TransformFairyCreate(MsgTransformFairy.Action.Active, uint.Parse(data[1]), client.Player.UID);
                                        client.Send(packet);
                                    }



                                    client.Player.testtttttttttt = uint.Parse(data[1]);
                                }
                                break;
                            }
                        case "gift":
                            {
                                client.Player.MainFlag = 0;
                                break;
                            }
                        
                        case "quiz":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();




                                    client.Send(stream.QuizShowCreate((MsgServer.MsgQuizShow.AcotionID)ushort.Parse(data[1]), (ushort)5, 0, 0, 0, (ushort)900, 600, 300,
                                     "TEst1", "TEst1", "TEst1", "TEst1", "TEst1"));


                                }
                                break;
                            }
                        case "learnspells":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    foreach (var spell in Pool.Magic.Values)
                                    {
                                        if (spell.Keys.Count > 0)
                                        {
                                            var sp = spell[(ushort)(spell.Keys.Count - 1)];
                                            client.MySpells.Add(stream, sp.ID, sp.Level);
                                            System.Threading.Thread.Sleep(2);
                                        }
                                    }

                                }
                                break;
                            }
                        case "testupd":
                            {


                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    for (int i = 100; i < 110; i++)
                                        for (int x = 50; x < 220; x++)
                                        {

                                            Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, client.Player.UID, 1);
                                            stream = packet.Append(stream, (MsgUpdate.DataType)i, (uint)x, 30, 300, 300);
                                            stream = packet.GetArray(stream);
                                            client.Player.View.SendView(stream, true);
                                        }
                                }
                                break;
                            }
                        case "asd":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    for (int x = 50; x < 200; x++)
                                    {
                                        client.Player.AddFlag((MsgUpdate.Flags)171, 10, true, 0);
                                        client.Player.SendUpdate(stream, (Game.MsgServer.MsgUpdate.Flags)171, 60
                            , 1, 4, (MsgUpdate.DataType)x, true);
                                    }
                                }
                                break;
                            }
                        case "exp":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    client.Player.SendUpdate(stream, uint.Parse(data[1]), MsgUpdate.DataType.Experience, false);
                                }
                                break;
                            }

                        case "testsinglepacket":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    //   for (int x = 100; x < 200; x++)
                                    {
                                        stream.Seek(4);

                                        byte[] pack = new byte[]
                                        {
                                        0x0A ,0x18 ,0x08 ,0x1A ,0x10 ,0xCE ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20
,0xC1 ,0x03 ,0x28 ,0xB9 ,0x03 ,0x30 ,0x00 ,0x3A ,0x05 ,0x4B ,0x79 ,0x6C ,0x69 ,0x6E ,0x0A ,0x1A
,0x08 ,0x15 ,0x10 ,0xC9 ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20 ,0xA3 ,0x01 ,0x28 ,0x9F ,0x01 ,0x30
,0x00 ,0x3A ,0x07 ,0x50 ,0x79 ,0x72 ,0x61 ,0x6D ,0x69 ,0x64 ,0x0A ,0x18 ,0x08 ,0x16 ,0x10 ,0xCA
,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20 ,0xF7 ,0x01 ,0x28 ,0x94 ,0x01 ,0x30 ,0x00 ,0x3A ,0x05 ,0x48
,0x65 ,0x62 ,0x62 ,0x79 ,0x0A ,0x1B ,0x08 ,0x17 ,0x10 ,0xCB ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20
,0xE4 ,0x02 ,0x28 ,0xCE ,0x01 ,0x30 ,0x00 ,0x3A ,0x08 ,0x42 ,0x61 ,0x73 ,0x69 ,0x6C ,0x69 ,0x73
,0x6B ,0x0A ,0x1A ,0x08 ,0x18 ,0x10 ,0xCC ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20 ,0xBC ,0x03 ,0x28
,0x87 ,0x02 ,0x30 ,0x00 ,0x3A ,0x07 ,0x46 ,0x72 ,0x65 ,0x65 ,0x64 ,0x6F ,0x6D ,0x0A ,0x18 ,0x08
,0x19 ,0x10 ,0xCD ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20 ,0xC7 ,0x03 ,0x28 ,0xE8 ,0x02 ,0x30 ,0x00
,0x3A ,0x05 ,0x48 ,0x6F ,0x6E ,0x6F ,0x72 ,0x0A ,0x17 ,0x08 ,0x1B ,0x10 ,0xCF ,0x87 ,0x01 ,0x18
,0xDF ,0x1E ,0x20 ,0xE7 ,0x02 ,0x28 ,0xCB ,0x03 ,0x30 ,0x00 ,0x3A ,0x04 ,0x4C ,0x69 ,0x6F ,0x6E
,0x0A ,0x1B ,0x08 ,0x1C ,0x10 ,0xD0 ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20 ,0x88 ,0x02 ,0x28 ,0xC0
,0x03 ,0x30 ,0x00 ,0x3A ,0x08 ,0x41 ,0x71 ,0x75 ,0x61 ,0x72 ,0x69 ,0x75 ,0x73 ,0x0A ,0x18 ,0x08
,0x1D ,0x10 ,0xD1 ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20 ,0xB4 ,0x01 ,0x28 ,0xE0 ,0x02 ,0x30 ,0x00
,0x3A ,0x05 ,0x45 ,0x61 ,0x67 ,0x6C ,0x65 ,0x0A ,0x1C ,0x08 ,0x1E ,0x10 ,0xD2 ,0x87 ,0x01 ,0x18
,0xDF ,0x1E ,0x20 ,0x9C ,0x01 ,0x28 ,0x8A ,0x02 ,0x30 ,0x00 ,0x3A ,0x09 ,0x4C ,0x69 ,0x67 ,0x68
,0x74 ,0x6E ,0x69 ,0x6E ,0x67 ,0x0A ,0x19 ,0x08 ,0x67 ,0x10 ,0x80 ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E
,0x20 ,0xB0 ,0x02 ,0x28 ,0xB4 ,0x02 ,0x30 ,0x01 ,0x3A ,0x06 ,0x52 ,0x65 ,0x61 ,0x6C ,0x6D ,0x33

                                        };

                                        for (int x = 0; x < pack.Length; x++)
                                            stream.Write((byte)pack[x]);
                                        //        stream.Write(byte.Parse(data[1]));//109576
                                        //        stream.Write((byte)71);
                                        //   stream.Write((uint)uint.Parse(data[2]));
                                        //  stream.Write(0);
                                        //     stream.Write(uint.MaxValue);
                                        //     stream.Write(uint.MaxValue); stream.Write(uint.MaxValue);

                                        stream.Finalize(2501);
                                        client.Send(stream);
                                    }
                                }
                                break;
                            }
                        case "testpacket":
                            {
                                using (var rec2 = new ServerSockets.RecycledPacket())
                                {
                                    var stream2 = rec2.GetStream();
                                    stream2.InitWriter();
                                    stream2.ZeroFill(4);
                                    stream2.Write((ushort)41);
                                    stream2.Write((ushort)40);
                                    stream2.Write((ushort)10183);
                                    stream2.ZeroFill(2);
                                    stream2.Write((uint)1);
                                    stream2.Write((uint)0);
                                    stream2.ZeroFill(8);
                                    stream2.Finalize(0x451);

                                    client.Player.Send(stream2);
                                }
                                break;
                            }
                        case "test":
                            {

                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    for (int u = 30; u < 200; u++)
                                    {
                                        stream.Seek(4);
                                        //   stream.Write(DateTime.Now.Value);
                                        stream.Write(client.Player.UID);
                                        stream.Write(1);

                                        stream.Write(u);//78
                                        stream.Write(620);//173);
                                        stream.Write(172);//0x02);//2
                                        stream.Write(4);//300
                                        for (int x = 0; x < 10; x++)
                                        {
                                            stream.Write(0);
                                        }
                                        stream.Finalize(10017);
                                        client.Send(stream);
                                    }
                                }
                                break;
                            }
                        case "ada":
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    for (ushort x = ushort.Parse(data[1]); x < ushort.Parse(data[2]); x++)
                                    {


                                        InteractQuery inter = new InteractQuery();
                                        inter.AtkType = (MsgAttackPacket.AttackID)x;
                                        inter.OpponentUID = client.Player.UID;
                                        inter.UID = client.Player.UID;
                                        inter.Damage = 3;
                                        inter.dwParam = 3;
                                        inter.SpellID = 12550;
                                        inter.X = client.Player.X;
                                        inter.Y = client.Player.Y;

                                        stream.InteractionCreate(&inter);
                                        client.Send(stream);
                                    }

                                }
                                break;
                            }
                        case "tttt":
                            {
                                MsgTournaments.MsgSchedules.SendInvitation("GuildWar", "ConquerPoints", 200, 254, 1038, 0, 60, MsgServer.MsgStaticMessage.Messages.GuildWar);
                                break;
                            }
                        case "reborn":
                            {
                                client.Player.Reborn = byte.Parse(data[1]);
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, client.Player.Reborn, MsgUpdate.DataType.Reborn);
                                }
                                break;
                            }
                        case "class":
                            {
                                client.Player.Class = byte.Parse(data[1]);
                                break;
                            }
                        case "agi":
                            {
                                ushort atr = 0;
                                ushort.TryParse(data[1], out atr);
                                if (client.Player.Atributes >= atr)
                                {
                                    client.Player.Agility += atr;
                                    client.Player.Atributes -= atr;
                                    client.Equipment.QueryEquipment(client.Equipment.Alternante, false);
                                }
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                    client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                    client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                    client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                    client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                }
                                break;
                            }
                        case "str":
                            {
                                ushort atr = 0;
                                ushort.TryParse(data[1], out atr);
                                if (client.Player.Atributes >= atr)
                                {
                                    client.Player.Strength += atr;
                                    client.Player.Atributes -= atr;
                                    client.Equipment.QueryEquipment(client.Equipment.Alternante, false);
                                }
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                    client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                    client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                    client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                    client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                }
                                break;
                            }
                        case "vit":
                            {
                                ushort atr = 0;
                                ushort.TryParse(data[1], out atr);
                                if (client.Player.Atributes >= atr)
                                {
                                    client.Player.Vitality += atr;
                                    client.Player.Atributes -= atr;
                                    client.Equipment.QueryEquipment(client.Equipment.Alternante, false);
                                }
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                    client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                    client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                    client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                    client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                }
                                break;
                            }
                        case "testflag":
                            {
                                ushort.TryParse(data[1], out ushort type);
                                MsgClassPKWar.TournamentType Typ = (MsgClassPKWar.TournamentType)type;
                                var aura = MsgSchedules.ClassPkWar.ObtainAura(Typ);
                                if (aura != MsgServer.MsgUpdate.Flags.Normal)
                                    client.Player.AddFlag(aura, 5, false);
                                break;
                            }
                        case "testeffect":
                            {
                                using (var rec = new RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    ushort cordX = (ushort)(client.Player.X - 5);
                                    ushort cordY = (ushort)(client.Player.Y - 5);
                                    Task.Run(() =>
                                    {
                                        for (var i = 9; i >= 0; i--)
                                        {
                                            //client.Player.AddMapEffect(stream, cordX, cordY, "upnumeber"+i);
                                            client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "upnumeber" + i);
                                            System.Threading.Thread.Sleep(1000);
                                        }
                                    });
                                    //client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, data[1]);
                                }
                                break;
                            }
                        case "reallot":
                            {
                                client.Player.Agility = 0;
                                client.Player.Strength = 0;
                                client.Player.Vitality = 1;
                                client.Player.Spirit = 0;

                                client.CreateBoxDialog("You have successfully reloaded your attribute points.");
                                if (client.Player.Reborn == 0)
                                {
                                    client.Player.Atributes = 0;
                                    Database.DataCore.AtributeStatus.ResetStatsNonReborn(client.Player);
                                    if (Database.AtributesStatus.IsWater(client.Player.Class))
                                    {
                                        if (client.Player.Level > 110)
                                            client.Player.Atributes = (ushort)((client.Player.Level - 110) * 3 + client.Player.ExtraAtributes);
                                    }
                                    else
                                    {
                                        if (client.Player.Level > 120)
                                            client.Player.Atributes = (ushort)((client.Player.Level - 120) * 3 + client.Player.ExtraAtributes);
                                    }
                                }
                                else if (client.Player.Reborn == 1)
                                {
                                    client.Player.Atributes = (ushort)(Pool.RebornInfo.ExtraAtributePoints(client.Player.FirstRebornLevel, client.Player.FirstClass)
                                        + 52 + 3 * (client.Player.Level - 15) + client.Player.ExtraAtributes);
                                }
                                else
                                {
                                    if (client.Player.SecoundeRebornLevel == 0)
                                        client.Player.SecoundeRebornLevel = 130;
                                    client.Player.Atributes = (ushort)(Pool.RebornInfo.ExtraAtributePoints(client.Player.FirstRebornLevel, client.Player.FirstClass) +
                                        Pool.RebornInfo.ExtraAtributePoints(client.Player.SecoundeRebornLevel, client.Player.SecondClass) + 52 + 3 * (client.Player.Level - 15) + client.Player.ExtraAtributes);
                                }
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                    client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                    client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                    client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                    client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                }
                                client.Equipment.QueryEquipment(client.Equipment.Alternante);

                                break;
                            }
                        case "spi":
                            {
                                ushort atr = 0;
                                ushort.TryParse(data[1], out atr);
                                if (client.Player.Atributes >= atr)
                                {
                                    client.Player.Spirit += atr;
                                    client.Player.Atributes -= atr;
                                    client.Equipment.QueryEquipment(client.Equipment.Alternante, false);
                                }
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                    client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                    client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                    client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                    client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                }
                                break;
                            }
                        case "reload":
                            {
                                bool reloadItems = true;
                                bool reloadSobNpcs = true;
                                bool reloadMaps = true;
                                bool reloadLotteryItems = true;
                                if (data.Length >= 2)
                                {
                                    reloadItems = data[1] == "--items";
                                    reloadSobNpcs = data[1] == "--sobnpcs";
                                    reloadMaps = data[1] == "--maps";
                                    reloadLotteryItems = data[1] == "--lottery";
                                }
                                if (reloadItems || reloadSobNpcs || reloadMaps || reloadLotteryItems)
                                {
                                    if (reloadItems)
                                    {
                                        Pool.ItemsBase.Loading(true);
                                        Console.WriteLine("Itemtype Reloaded", ConsoleColor.DarkGreen);
                                    }
                                    if (reloadSobNpcs)
                                    {
                                        foreach (var kvP in Pool.ServerMaps.Base)
                                        {
                                            uint MapId = kvP.Value.ID;
                                            NpcServer.LoadSobNpcs(MapId);
                                        }
                                        Console.WriteLine("Sob NPCs Reloaded", ConsoleColor.DarkGreen);
                                    }
                                    if (reloadMaps)
                                    {
                                        Role.GameMap.LoadMaps();
                                        Console.WriteLine("Maps Reloaded", ConsoleColor.DarkGreen);
                                    }
                                    if (reloadLotteryItems)
                                    {
                                        Pool.NewLottery.LoadLotteryItems();
                                        Console.WriteLine("Lottery items Reloaded", ConsoleColor.DarkGreen);
                                    }
                                } else
                                {
                                    Console.WriteLine($"Specify what need reload or leave second parameter empty. Example: @reload --items or @reload --sobnpcs or @reload maps", ConsoleColor.DarkGreen);
                                }
                                break;
                            }
                            #endregion
                    }

                    return true;
                }
            }
            catch
            {
                client.SendSysMesage("Sorry cannot access to this command or not exists.");

                return false;
            }
            return false;
        }

        public static void ReallotPoints(Client.GameClient client)
        {
            client.Player.Agility = 0;
            client.Player.Strength = 0;
            client.Player.Vitality = 1;
            client.Player.Spirit = 0;
            if (client.Player.Reborn == 0)
            {
                client.Player.Atributes = 0;
                DataCore.AtributeStatus.ResetStatsNonReborn(client.Player);
                if (AtributesStatus.IsWater(client.Player.Class))
                {
                    if (client.Player.Level > 110)
                        client.Player.Atributes = (ushort)((client.Player.Level - 110) * 3 + client.Player.ExtraAtributes);
                }
                else
                {
                    if (client.Player.Level > 120)
                        client.Player.Atributes = (ushort)((client.Player.Level - 120) * 3 + client.Player.ExtraAtributes);
                }
            }
            else if (client.Player.Reborn == 1)
            {
                client.Player.Atributes = (ushort)(Pool.RebornInfo.ExtraAtributePoints(client.Player.FirstRebornLevel, client.Player.FirstClass)
                    + 52 + 3 * (client.Player.Level - 15) + client.Player.ExtraAtributes);
            }
            else
            {
                if (client.Player.SecoundeRebornLevel == 0)
                    client.Player.SecoundeRebornLevel = 130;
                client.Player.Atributes = (ushort)(Pool.RebornInfo.ExtraAtributePoints(client.Player.FirstRebornLevel, client.Player.FirstClass) +
                    Pool.RebornInfo.ExtraAtributePoints(client.Player.SecoundeRebornLevel, client.Player.SecondClass) + 52 + 3 * (client.Player.Level - 15) + client.Player.ExtraAtributes);
            }
            using (var rec = new RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Player.SendUpdate(stream, client.Player.Strength, MsgUpdate.DataType.Strength);
                client.Player.SendUpdate(stream, client.Player.Agility, MsgUpdate.DataType.Agility);
                client.Player.SendUpdate(stream, client.Player.Spirit, MsgUpdate.DataType.Spirit);
                client.Player.SendUpdate(stream, client.Player.Vitality, MsgUpdate.DataType.Vitality);
                client.Player.SendUpdate(stream, client.Player.Atributes, MsgUpdate.DataType.Atributes);

            }
            client.Equipment.QueryEquipment(client.Equipment.Alternante);
        }
    }
}
