using Core;
using GameServer.Game.MsgServer;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameServer
{
    public class VendorShop
    {
        public uint UID;
        public ushort Mesh = 100;
        public string Name;
        public ushort Map;
        public ushort X;
        public ushort Y;
        public List<string> Items;
        public MsgItemView.ActionMode CostType;
    }
    class Booth
    {
        public static SafeDictionaryAlt<uint, VendorShop> Booths = new SafeDictionaryAlt<uint, VendorShop>();
        public static void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                string[] text = File.ReadAllLines(Path.Combine(ServerConfig.DbLocation, "Booths.txt"));
                VendorShop booth = new VendorShop();
                for (int x = 0; x < text.Length; x++)
                {
                    string line = text[x];
                    string[] split = line.Split('=');
                    if (split[0] == "ID")
                    {
                        if (booth.UID == 0)
                            booth.UID = uint.Parse(split[1]);
                        else
                        {
                            if (!Booths.ContainsKey(booth.UID))
                            {
                                Booths.Add(booth.UID, booth);
                                booth = new VendorShop();
                                booth.UID = uint.Parse(split[1]);
                            }
                        }
                    }
                    else if (split[0] == "CostType")
                    {
                        booth.CostType = (Game.MsgServer.MsgItemView.ActionMode)byte.Parse(split[1]);
                    }
                    else if (split[0] == "Name")
                    {
                        booth.Name = split[1];
                    }
                    else if (split[0] == "Mesh")
                    {
                        booth.Mesh = ushort.Parse(split[1]);
                    }
                    else if (split[0] == "Map")
                    {
                        booth.Map = ushort.Parse(split[1]);
                    }
                    else if (split[0] == "X")
                    {
                        booth.X = ushort.Parse(split[1]);
                    }
                    else if (split[0] == "Y")
                    {
                        booth.Y = ushort.Parse(split[1]);
                    }
                    else if (split[0] == "ItemAmount")
                    {
                        booth.Items = new List<string>(ushort.Parse(split[1]));
                    }
                    else if (split[0].Contains("Item") && split[0] != "ItemAmount")
                    {
                        string name = split[1];
                        booth.Items.Add(name);
                    }
                }
                if (!Booths.ContainsKey(booth.UID))
                    Booths.Add(booth.UID, booth);
                CreateBooths();
            } else
            {
                List<Core.Models.GameServer.VendorShop> vShop = RestApiHelper.GetVendorShops();
                foreach(Core.Models.GameServer.VendorShop booth in vShop)
                {
                    if (!Booths.ContainsKey(booth.UID))
                    {
                        Booths.Add(booth.UID, new VendorShop() { Mesh = booth.Mesh, Name = booth.Name, UID = booth.UID, Map = booth.Map, X = booth.X, Y = booth.Y, CostType = (MsgItemView.ActionMode)booth.CostType, Items = booth.Items });
                    }
                }
                CreateBooths();
            }
        }
        public static void CreateBooths()
        {
            foreach (var bo in Booths.Values)
            {
                Role.Instance.Vendor booth = new Role.Instance.Vendor();

                Role.SobNpc Base = new Role.SobNpc();
                Base.ObjType = Role.MapObjectType.SobNpc;
                Base.UID = bo.UID;
                if (Role.Instance.Vendor.Booths.ContainsKey(Base.UID))
                    Role.Instance.Vendor.Booths.Remove(Base.UID);
                Role.Instance.Vendor.Booths.Add(Base.UID, booth);
                Base.Mesh = (Role.SobNpc.StaticMesh)bo.Mesh;
                Base.Type = Role.Flags.NpcType.Booth;
                Base.Name = bo.Name;
                Base.Map = bo.Map;
                Base.X = bo.X;
                Base.Y = bo.Y;
                booth.VendorNpc = Base;

                if (Pool.ServerMaps.ContainsKey(bo.Map))
                {
                    if (Pool.ServerMaps[bo.Map].View.Contain(Base.UID, bo.X, bo.Y))
                        Pool.ServerMaps[bo.Map].View.LeaveMap<Role.IMapObj>(Base);
                    Pool.ServerMaps[bo.Map].View.EnterMap<Role.IMapObj>(Base);
                }

                for (int i = 0; i < bo.Items.Count; i++)
                {
                    var line = bo.Items[i].Split(new string[] { "@@", "@" }, StringSplitOptions.RemoveEmptyEntries);
                    Role.Instance.Vendor.VendorItem item = new Role.Instance.Vendor.VendorItem();

                    item.DataItem = new MsgGameItem();
                    //item.DataItem.UID = Pool.ITEM_Counter.Next;

                    item.DataItem.ITEM_ID = uint.Parse(line[0]);
                    if (line.Length >= 2)
                        item.AmountCost = uint.Parse(line[1]);
                    if (line.Length >= 3)
                        item.DataItem.Plus = byte.Parse(line[2]);
                    if (line.Length >= 4)
                        item.DataItem.Enchant = byte.Parse(line[3]);
                    if (line.Length >= 5)
                        item.DataItem.Bless = byte.Parse(line[4]);
                    if (line.Length >= 6)
                        item.DataItem.SocketOne = (Role.Flags.Gem)byte.Parse(line[5]);
                    if (line.Length >= 7)
                        item.DataItem.SocketTwo = (Role.Flags.Gem)byte.Parse(line[6]);
                    if (line.Length >= 8)
                        item.DataItem.StackSize = ushort.Parse(line[7]);

                    Database.ItemType.DBItem CIBI;
                    if (Pool.ItemsBase.TryGetValue(item.DataItem.ITEM_ID, out CIBI))
                    {
                        item.DataItem.Durability = CIBI.Durability;
                        item.DataItem.MaximDurability = CIBI.Durability;
                        item.CostType = bo.CostType;
                        booth.Items.TryAdd(item.DataItem.UID, item);
                    }
                }

            }
            Console.WriteLine("" + Role.Instance.Vendor.Booths.Count + " Booths Loaded.");
        }
    }
}