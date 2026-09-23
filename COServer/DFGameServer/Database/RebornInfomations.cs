using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GameServer.Game.MsgServer;
using static GameServer.Pool;

namespace GameServer.Database
{
    public class RebornInfomations : List<Tuple<byte, byte, byte, RebornInfomations.Action, List<ushort>>>
    {
        public List<ushort> StaticSpells = new List<ushort>()
        {
            1360,1260,1290,5050,5040,5030,5020,5010,1045,1046,11000 ,11005,7000,7010,7020,7030,7040,11190,7001,7002,7003,1300
        };
        public List<ushort> PureSpells = new List<ushort>()
        {
            10405,30000,11040,6003,10315,10310,10311,10313
        };
        public enum Action : byte
        {
            Delete = 0,
            Downgrade = 2,
            AllClassSpells = 4,//???
            PureSpell = 5,
            Add = 6,
            AddRebornSpells = 7
        }

        public void Load()
        {

            string magictypeDatPath = Path.Combine(ServerConfig.DbLocation, "magictypeop.dat");
            if (File.Exists(magictypeDatPath))
            {
                Console.WriteLine("[Detected magictypeop.dat] You need apply the changes from magictypeop.dat to magictype.txt? [Y/N]", ConsoleColor.DarkYellow);
                string keyYN = Console.ReadLine();
                if (keyYN.ToLower() == "y")
                {
                    Core.Utils.DecryptCommonDat(magictypeDatPath);
                    System.Threading.Thread.Sleep(1000);
                    File.Delete(magictypeDatPath);
                }
            }
            string[] baseplusText = File.ReadAllLines(Path.Combine(ServerConfig.DbLocation, "magictypeop.txt"));
            foreach (string line in baseplusText)
            {
                string[] data = line.Split(',');
                byte Reborn = byte.Parse(data[1]);
                byte MyClass = byte.Parse(data[2]);
                byte RebornClass = byte.Parse(data[3]);

                // 5017 Era 1: load only first/second rebirth rules for the five
                // classic profession lines. Ignore reincarnation/pure-skill metadata
                // and every later profession embedded in the 5695 data file.
                if (Reborn != 1 && Reborn != 2)
                    continue;
                if (!IsEra1DataClass(MyClass) || !IsEra1DataClass(RebornClass))
                    continue;

                Action Info = (Action)byte.Parse(data[4]);
                List<ushort> Spells = new List<ushort>();
                for (int x = 5; x < data.Length; x++)
                {
                    ushort ID = ushort.Parse(data[x]);
                    if (ID != 0)
                        Spells.Add(ID);
                }
                this.Add(new Tuple<byte, byte, byte, Action, List<ushort>>(Reborn, MyClass, RebornClass, Info, Spells));
            }
        }
        public byte ExtraAtributePoints(byte level, byte mClass)
        {
            if (mClass == 135)
            {
                if (level <= 110)
                    return 0;
                switch (level)
                {
                    case 112: return 1;
                    case 114: return 3;
                    case 116: return 6;
                    case 118: return 10;
                    case 120: return 15;
                    case 121: return 15;
                    case 122: return 21;
                    case 123: return 21;
                    case 124: return 28;
                    case 125: return 28;
                    case 126: return 36;
                    case 127: return 36;
                    case 128: return 45;
                    case 129: return 45;
                    default:
                        return 55;
                }
            }
            else
            {
                if (level <= 120)
                    return 0;
                switch (level)
                {
                    case 121: return 1;
                    case 122: return 3;
                    case 123: return 6;
                    case 124: return 10;
                    case 125: return 15;
                    case 126: return 21;
                    case 127: return 28;
                    case 128: return 36;
                    case 129: return 45;
                    default:
                        return 55;
                }
            }
        }
        private static bool IsEra1DataClass(byte cls)
        {
            return (cls >= 10 && cls <= 15)
                || (cls >= 20 && cls <= 25)
                || (cls >= 40 && cls <= 45)
                || cls == 100 || cls == 101
                || (cls >= 132 && cls <= 135)
                || (cls >= 142 && cls <= 145);
        }

        private static bool IsEra1Class(byte cls)
        {
            return cls == 11 || cls == 21 || cls == 41 || cls == 132 || cls == 142;
        }

        public static bool CanEra1Rebirth(Role.Player player, byte rebornClass, out string reason)
        {
            if (player.Reborn >= 2)
            {
                reason = "Era 1 supports a maximum of two rebirths.";
                return false;
            }
            if (!IsEra1Class(rebornClass))
            {
                reason = "That profession is not available in Era 1.";
                return false;
            }

            bool water = AtributesStatus.IsWater(player.Class);
            int required = water && player.Reborn == 0 ? 110 : 120;
            if (player.Level < required)
            {
                reason = "You need to reach level " + required + " before rebirth.";
                return false;
            }

            reason = null;
            return true;
        }

        public unsafe void Reborn(Role.Player player, byte RebornClass, ServerSockets.Packet stream)
        {

            if (RebornClass != 0)
            {
                if (!CanEra1Rebirth(player, RebornClass, out var era1Reason))
                {
                    player.Owner.CreateBoxDialog(era1Reason);
                    return;
                }
                if (player.UseAura != MsgUpdate.Flags.Normal)
                    player.AddAura(player.UseAura, null, Role.StatusFlagsBigVector32.PermanentFlag);
                if (RebornClass % 10 == 1 || RebornClass == 132 || RebornClass == 142)
                {
                    switch (player.Reborn)
                    {
                        case 0:
                            {


                                foreach (var info in this)
                                {
                                    if (info.Item1 == 1 && info.Item2 == player.Class && info.Item3 == RebornClass)
                                    {
                                        switch (info.Item4)
                                        {
                                            case Action.AddRebornSpells:
                                            case Action.Add:
                                                {
                                                    foreach (var spellid in info.Item5)
                                                        player.Owner.MySpells.Add(stream, spellid);
                                                    break;
                                                }
                                            case Action.Delete:
                                                {
                                                    foreach (var spellid in info.Item5)
                                                    {
                                                        player.Owner.MySpells.Remove(spellid, stream);
                                                    }
                                                    break;
                                                }
                                            case Action.Downgrade:
                                                {
                                                    foreach (var spellid in info.Item5)
                                                        player.Owner.MySpells.RebornSpell(stream, spellid);
                                                    break;
                                                }

                                        }
                                    }
                                }
                                player.FirstRebornLevel = (byte)player.Level;
                                player.FirstClass = player.Class;
                                player.Class = RebornClass;
                                player.Reborn = 1;
                                player.SendUpdate(stream, player.Reborn, Game.MsgServer.MsgUpdate.DataType.Reborn);
                                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + player.Name + " has got " + player.Reborn + " reborns.", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                break;
                            }
                        case 1:
                            {

                                foreach (var info in this)
                                {
                                    if (info.Item1 == 2 && info.Item2 == player.Class && info.Item3 == RebornClass)
                                    {
                                        switch (info.Item4)
                                        {
                                            case Action.AddRebornSpells:
                                            case Action.Add:
                                                {
                                                    foreach (var spellid in info.Item5)
                                                        player.Owner.MySpells.Add(stream, spellid);
                                                    break;
                                                }
                                            case Action.Delete:
                                                {
                                                    foreach (var spellid in info.Item5)
                                                    {
                                                        player.Owner.MySpells.Remove(spellid, stream);
                                                    }
                                                    break;
                                                }
                                            case Action.Downgrade:
                                                {
                                                    foreach (var spellid in info.Item5)
                                                    {
                                                        player.Owner.MySpells.RebornSpell(stream, spellid);
                                                    }
                                                    break;
                                                }

                                        }
                                    }
                                }
                                player.SecoundeRebornLevel = (byte)player.Level;
                                player.SecondClass = player.Class;
                                player.Class = RebornClass;
                                player.Reborn = 2;
                                player.SendUpdate(stream, player.Reborn, Game.MsgServer.MsgUpdate.DataType.Reborn);
                                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + player.Name + " has got " + player.Reborn + " nd reborn.", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                break;
                            }
                        case 2:
                            {
                                player.Owner.CreateBoxDialog("Reincarnation is not available in Era 1.");
                                return;
                            }
                    }

                    try
                    {
                        foreach (var item in player.Owner.Equipment.ClientItems.Values)
                        {
                            if (item != null)
                            {
                                try
                                {
                                    if (item.Position != (ushort)Role.Flags.ConquerItem.AleternanteBottle
                                        && item.Position != (ushort)Role.Flags.ConquerItem.AlternateGarment
                                        && item.Position != (ushort)Role.Flags.ConquerItem.Bottle
                                        && item.Position != (ushort)Role.Flags.ConquerItem.Fan
                                        && item.Position != (ushort)Role.Flags.ConquerItem.Garment
                                        && item.Position != (ushort)Role.Flags.ConquerItem.LeftWeaponAccessory
                                        && item.Position != (ushort)Role.Flags.ConquerItem.RidingCrop
                                        && item.Position != (ushort)Role.Flags.ConquerItem.RightWeaponAccessory
                                        && item.Position != (ushort)Role.Flags.ConquerItem.Steed
                                        && item.Position != (ushort)Role.Flags.ConquerItem.SteedMount
                                        && item.Position != (ushort)Role.Flags.ConquerItem.Tower)
                                    {
                                        item.ITEM_ID = Pool.ItemsBase.DowngradeItem(item.ITEM_ID);
                                        item.Mode = Role.Flags.ItemMode.Update;
                                        item.Send(player.Owner, stream);
                                    }
                                }
                                catch (Exception e) { Console.WriteLine(e.ToString()); }
                            }
                        }
                    }
                    catch (Exception e) { Console.WriteLine(e.ToString()); }
                    player.Owner.Equipment.Remove(Role.Flags.ConquerItem.LeftWeapon, stream);
                    player.Owner.Equipment.Remove(Role.Flags.ConquerItem.AleternanteLeftWeapon, stream);


                    player.Level = 15;
                    player.Experience = LevelInfo[Database.DBLevExp.Sort.User][(byte)(player.Level - 1)].Experience;
                    player.SendUpdate(stream, (long)player.Experience, Game.MsgServer.MsgUpdate.DataType.Experience);


                    ActionQuery action = new ActionQuery()
                    {
                        ObjId = player.UID,
                        Type = ActionType.Leveled,
                        dwParam = player.Level
                    };
                    player.Owner.Send(stream.ActionCreate(&action));


                    player.SendUpdate(stream, player.Level, Game.MsgServer.MsgUpdate.DataType.Level);
                    // Database.DataCore.AtributeStatus.GetStatus(player, true);

                    player.Strength = player.Agility = player.Spirit = 0;
                    player.Vitality = 1;
                    if (player.Reborn == 1)
                    {
                        player.Atributes = (ushort)(RebornInfo.ExtraAtributePoints(player.FirstRebornLevel, player.FirstClass)
                            + 52 + 3 * (player.Level - 15) + player.ExtraAtributes);
                    }
                    else
                    {
                        player.Atributes = (ushort)(RebornInfo.ExtraAtributePoints(player.FirstRebornLevel, player.FirstClass) +
                            RebornInfo.ExtraAtributePoints(player.SecoundeRebornLevel, player.SecondClass) + 52 + 3 * (player.Level - 15) + player.ExtraAtributes);
                    }
                    player.SendUpdate(stream, player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                    player.SendUpdate(stream, player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                    player.SendUpdate(stream, player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                    player.SendUpdate(stream, player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                    player.SendUpdate(stream, player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);
                    player.SendUpdate(stream, player.FirstClass, Game.MsgServer.MsgUpdate.DataType.FirsRebornClass);
                    player.SendUpdate(stream, player.SecondClass, Game.MsgServer.MsgUpdate.DataType.SecondRebornClass);
                    player.Owner.Equipment.QueryEquipment(player.Owner.Equipment.Alternante, true);
                    // Era 1: rebirth does not re-grant the normal profession starter kit.
                    // Reborn-only spells come from magictypeop rules; normal class skills
                    // remain tied to their classic trainers/Job Center acquisition.


                }
            }
            else
            {
                foreach (var info in this)
                {
                    if (info.Item1 == 0 && info.Item2 == 0 && info.Item3 == player.Class)
                    {
                        if (info.Item4 == Action.AllClassSpells)
                        {
                            foreach (var spellid in info.Item5)
                            {
                                if (!StaticSpells.Contains(spellid) && !PureSpells.Contains(spellid))
                                    if (!player.Owner.MySpells.ClientSpells.ContainsKey(spellid))
                                        player.Owner.MySpells.Add(stream, spellid);
                            }
                        }
                    }
                }
            }
        }
    }
}
