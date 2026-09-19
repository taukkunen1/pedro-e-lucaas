using Core;
using Core.Models.GameServer;
using GameServer.MadeByDaRkFox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameServer.Database
{
    public class NpcServer
    {
        public static Dictionary<uint, Furniture> FurnitureInformations = new Dictionary<uint, Furniture>();

        public static Furniture GetNpc(uint ItemID)
        {
            foreach (var npc in FurnitureInformations.Values)
            {
                if (npc.ItemID == ItemID)
                {
                    return npc;
                }
            }
            return null;
        }
        public static Furniture GetNpcFromMesh(uint mesh)
        {
            foreach (var npc in FurnitureInformations.Values)
            {
                if (npc.Mesh == mesh)
                {
                    return npc;
                }
            }
            return null;
        }
        public static void LoadSobNpcs(uint id = 0)
        {
            if (ServerConfig.DbFromFiles)
            {
                string[] baseText = File.ReadAllLines(Path.Combine(ServerConfig.DbLocation, "SobNpcs.txt"));
                foreach (var bas_line in baseText)
                {
                    Database.DBActions.ReadLine line = new DBActions.ReadLine(bas_line, ',');
                    Role.SobNpc npc = new Role.SobNpc();
                    npc.ObjType = Role.MapObjectType.SobNpc;
                    npc.UID = line.Read((uint)0);
                    npc.Name = line.Read("");
                    npc.Type = (Role.Flags.NpcType)line.Read((ushort)0);
                    npc.Mesh = (Role.SobNpc.StaticMesh)line.Read((ushort)0);
                    npc.Map = line.Read((ushort)0);
                    if (npc.Map != id && id != 0) continue;
                    if (npc.Map != 5001)
                    {
                        npc.X = line.Read((ushort)0);
                        npc.Y = line.Read((ushort)0);
                        npc.HitPoints = line.Read((int)0);
                        npc.MaxHitPoints = line.Read((int)0);
                        npc.Sort = line.Read((ushort)0);
                        if (npc.Map == 1039 && npc.UID != 180)
                            npc.MaxHitPoints = npc.HitPoints = 10000;
                        if (line.Read((byte)0) == 0)
                            npc.Name = null;
                        if (Pool.ServerMaps.ContainsKey(npc.Map))
                        {
                            Pool.ServerMaps[npc.Map].View.EnterMap<Role.IMapObj>(npc);

                            if (Role.GameMap.IsGate(npc.UID))
                                Pool.ServerMaps[npc.Map].SetGateFlagNpc(npc.X, npc.Y);
                            else
                                Pool.ServerMaps[npc.Map].SetFlagNpc(npc.X, npc.Y);
                        }
                    }
                }
            } else
            {
                IQueryable<SobNPC> sobNPCs = RestApiHelper.GetRequest<List<SobNPC>>("SobNPCs/Get").AsQueryable();
                if (id > 0)
                {
                    sobNPCs = sobNPCs.Where(x => x.Map == id);
                }
                foreach(SobNPC sobnpc in sobNPCs.ToList())
                {
                    Role.SobNpc npc = new Role.SobNpc();
                    npc.ObjType = (Role.MapObjectType)sobnpc.ObjType;
                    npc.UID = sobnpc.UID;
                    npc.Name = sobnpc.Name;
                    npc.Type = (Role.Flags.NpcType)sobnpc.Type;
                    npc.Mesh = (Role.SobNpc.StaticMesh)sobnpc.Mesh;
                    npc.Map = sobnpc.Map;
                    if (npc.Map != id && id != 0) continue;
                    if (npc.Map != 5001)
                    {
                        npc.X = sobnpc.X;
                        npc.Y = sobnpc.Y;
                        npc.HitPoints = (int)sobnpc.HitPoints;
                        npc.MaxHitPoints = (int)sobnpc.MaxHitPoints;
                        npc.Sort = sobnpc.Sort;
                        if (npc.Map == 1039 && npc.UID != 180)
                            npc.MaxHitPoints = npc.HitPoints = 10000;
                        if (!sobnpc.ShowName)
                        {
                            npc.Name = null;
                        }
                        if (Pool.ServerMaps.ContainsKey(npc.Map))
                        {
                            Pool.ServerMaps[npc.Map].View.EnterMap<Role.IMapObj>(npc);

                            if (Role.GameMap.IsGate(npc.UID))
                                Pool.ServerMaps[npc.Map].SetGateFlagNpc(npc.X, npc.Y);
                            else
                                Pool.ServerMaps[npc.Map].SetFlagNpc(npc.X, npc.Y);
                        }
                    }
                }
                Console.WriteLine($"Loaded {sobNPCs.Count()} SobNPCs in Map with ID {id}", System.ConsoleColor.DarkGray);
            }
        }
        public static void LoadServerTraps(uint id = 0, bool OnlySquama = false)
        {
            if (!OnlySquama)
            {
                if (ServerConfig.DbFromFiles)
                {
                    if (File.Exists(Path.Combine(ServerConfig.DbLocation, "Traps.txt")))
                    {
                        using (StreamReader read = File.OpenText(Path.Combine(ServerConfig.DbLocation, "Traps.txt")))
                        {
                            while (true)
                            {
                                string aline = read.ReadLine();
                                if (aline != null && aline != "")
                                {
                                    string[] line = aline.Split(',');

                                    uint ID = uint.Parse(line[3]);
                                    ushort map = ushort.Parse(line[0]);
                                    if (id != 0 && id != map) continue;
                                    var Item = new Game.MsgFloorItem.MsgItem(null, ushort.Parse(line[1]), ushort.Parse(line[2]), Game.MsgFloorItem.MsgItem.ItemType.Effect, 0, 0, ushort.Parse(line[0]), 0, false, Pool.ServerMaps[ushort.Parse(line[0])], 60 * 60 * 1000);
                                    Item.MsgFloor.m_ID = ID;
                                    Item.MsgFloor.m_Color = 2;
                                    Item.MsgFloor.DropType = Game.MsgFloorItem.MsgDropID.Effect;
                                    if (line.Length > 4)
                                        Item.AllowDynamic = byte.Parse(line[4]) == 1;
                                    Item.GMap.View.EnterMap<Role.IMapObj>(Item);

                                }
                                else
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    IQueryable<Trap> traps = RestApiHelper.GetRequest<List<Trap>>("Traps/Get").AsQueryable();
                    if (id > 0)
                    {
                        traps = traps.Where(x => x.Map == id);
                    }
                    foreach (Trap trap in traps.ToList())
                    {
                        if (id != 0 && id != trap.Map) continue;
                        var Item = new Game.MsgFloorItem.MsgItem(null, trap.X, trap.Y, Game.MsgFloorItem.MsgItem.ItemType.Effect, 0, 0, trap.Map, 0, false, Pool.ServerMaps[trap.Map], 60 * 60 * 1000);
                        Item.MsgFloor.m_ID = trap.TrapID;
                        Item.MsgFloor.m_Color = 2;
                        Item.MsgFloor.DropType = Game.MsgFloorItem.MsgDropID.Effect;
                        Item.AllowDynamic = trap.AllowDinamic;
                        Item.GMap.View.EnterMap<Role.IMapObj>(Item);
                    }
                    Console.WriteLine($"Loaded {traps.Count()} Traps in Map with ID {id}", System.ConsoleColor.DarkGray);
                }
            }
           
            #region Squama Locations
            foreach (SquamaLocation sqLocation in SquamaManager.SquamaConfigurations.Where(x => x.MapId == id))
            {
                var Item = new Game.MsgFloorItem.MsgItem(null, sqLocation.MapX, sqLocation.MapY, Game.MsgFloorItem.MsgItem.ItemType.Effect, 0, 0, sqLocation.MapId, 0, false, Pool.ServerMaps[sqLocation.MapId], 60 * 60 * 1000);
                if (DateTime.Now.Hour < 20) // If hour are less of 20 hide the squama in gm map
                {
                    Item.Map = 5000;
                    Item.X = 0;
                    Item.Y = 0;
                }
                Item.MsgFloor.m_ID = 11;
                Item.MsgFloor.m_Color = 2;
                Item.MsgFloor.DropType = Game.MsgFloorItem.MsgDropID.Effect;
                Item.AllowDynamic = false;
                Item.GMap.View.EnterMap<Role.IMapObj>(Item);
            }
            #endregion
        }
        public static void LoadNpcs(uint id = 0)
        {
            if (ServerConfig.DbFromFiles)
            {
                uint Count = 0;
                string PathNPCS = Path.Combine(ServerConfig.DbLocation, "Npcs.txt");
                if (File.Exists(PathNPCS))
                {
                    using (StreamReader read = File.OpenText(PathNPCS))
                    {
                        while (true)
                        {
                            string aline = read.ReadLine();
                            if (aline != null && aline != "")
                            {

                                string[] line = aline.Split(',');
                                if (id != 0 && id != ushort.Parse(line[3])) continue;
                                Game.MsgNpc.Npc np = Game.MsgNpc.Npc.Create();
                                np.UID = uint.Parse(line[0]);
                                np.NpcType = (Role.Flags.NpcType)byte.Parse(line[1]);
                                np.Mesh = ushort.Parse(line[2]);
                                np.Map = ushort.Parse(line[3]);
                                if (np.Map != 5000)
                                {
                                    np.X = ushort.Parse(line[4]);
                                    np.Y = ushort.Parse(line[5]);
                                    if (np.Mesh == 42580)
                                        continue;
                                    if (np.UID == 16851 || np.UID == 168052)
                                    {
                                        np.AllowDynamic = true;
                                    }
                                    else if (line.Length > 6)
                                        np.Name = line[6];
                                    if (Pool.ServerMaps.ContainsKey(np.Map))
                                    {
                                        Count++;
                                        Pool.ServerMaps[np.Map].AddNpc(np);
                                    }
                                }
                            }
                            else
                                break;
                        }
                        Console.WriteLine("Loading " + Count + " Npcs");
                    }
                }
                else
                {
                    Console.WriteLine($"Not found npcs [Location: {PathNPCS}]");
                }
                if (File.Exists(Path.Combine(ServerConfig.DbLocation, "furnitures.txt")))
                {
                    uint CountFurnitures = 0;
                    using (StreamReader read = File.OpenText(Path.Combine(ServerConfig.DbLocation, "furnitures.txt")))
                    {
                        while (true)
                        {
                            string aline = read.ReadLine();
                            if (aline != null)
                            {

                                string[] line = aline.Split(',');
                                if (id != 0 && id != ushort.Parse(line[3])) continue;
                                Game.MsgNpc.Npc np = Game.MsgNpc.Npc.Create();
                                np.UID = uint.Parse(line[0]);
                                np.NpcType = (Role.Flags.NpcType)byte.Parse(line[1]);
                                np.Mesh = ushort.Parse(line[2]);
                                np.Map = ushort.Parse(line[3]);
                                np.X = ushort.Parse(line[4]);
                                np.Y = ushort.Parse(line[5]);

                                Furniture furnit = new Furniture();
                                furnit.Name = line[6];
                                furnit.ItemID = uint.Parse(line[7]);
                                furnit.UID = np.UID;
                                furnit.Mesh = np.Mesh;
                                furnit.MoneyCost = uint.Parse(line[8]);
                                if (!FurnitureInformations.ContainsKey(np.UID))
                                    FurnitureInformations.Add(np.UID, furnit);

                                if (Pool.ServerMaps.ContainsKey(np.Map))
                                {
                                    CountFurnitures++;
                                    Pool.ServerMaps[np.Map].AddNpc(np);
                                }
                            }
                            else
                                break;
                        }
                        Console.WriteLine("Loading " + CountFurnitures + " Furnitures");
                    }
                }
            } else
            {
                IQueryable<NPC> NPCs = RestApiHelper.GetRequest<List<NPC>>("NPCs/Get").AsQueryable();
                if (id > 0)
                {
                    NPCs = NPCs.Where(x => x.Map == id);
                }
                foreach (NPC npc in NPCs.ToList())
                {
                    if (id != 0 && id != npc.Map) continue;
                    Game.MsgNpc.Npc np = Game.MsgNpc.Npc.Create();
                    np.UID = npc.UID;
                    np.NpcType = (Role.Flags.NpcType)npc.Type;
                    np.Mesh = npc.Mesh;
                    np.Map = npc.Map;
                    if (np.Map != 5000)
                    {
                        np.X = npc.X;
                        np.Y = npc.Y;
                        if (np.Mesh == 42580)
                            continue;
                        if (np.UID == 16851 || np.UID == 168052)
                        {
                            np.AllowDynamic = true;
                        }
                        if (npc.ShowName)
                        {
                            np.Name = npc.Name;
                        }
                        if (Pool.ServerMaps.ContainsKey(np.Map))
                        {
                            Pool.ServerMaps[np.Map].AddNpc(np);
                        }
                    }
                }
                Console.WriteLine($"Loaded {NPCs.Count()} NPCs in Map with ID {id}", System.ConsoleColor.DarkGray);
                IQueryable<Furniture> Furnitures = RestApiHelper.GetRequest<List<Furniture>>("Furnitures/Get").AsQueryable();
                if (id > 0)
                {
                    Furnitures = Furnitures.Where(x => x.Map == id);
                }
                foreach (Furniture furn in Furnitures.ToList())
                {
                    if (id != 0 && id != furn.Map) continue;
                    Game.MsgNpc.Npc np = Game.MsgNpc.Npc.Create();
                    np.UID = furn.UID;
                    np.NpcType = (Role.Flags.NpcType)furn.Type;
                    np.Mesh = furn.Mesh;
                    np.Map = furn.Map;
                    np.X = furn.X;
                    np.Y = furn.Y;
                    Furniture furnit = new();
                    furnit.Name = furn.Name;
                    furnit.ItemID = furn.ItemID;
                    furnit.UID = np.UID;
                    furnit.Mesh = np.Mesh;
                    furnit.MoneyCost = furn.MoneyCost;
                    if (!FurnitureInformations.ContainsKey(np.UID))
                        FurnitureInformations.Add(np.UID, furnit);
                    if (Pool.ServerMaps.ContainsKey(np.Map))
                    {
                        Pool.ServerMaps[np.Map].AddNpc(np);
                    }
                }
                Console.WriteLine($"Loaded {Furnitures.Count()} Furnitures in Map with ID {id}", System.ConsoleColor.DarkGray);
            }
        }
    }
}
