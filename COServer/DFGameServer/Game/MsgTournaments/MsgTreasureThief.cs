using System;
using System.Linq;

namespace GameServer.Game.MsgTournaments
{
    public class MsgTreasureThief : ITournament
    {
        public const ushort
            MapID = 5263;
        public ProcesType Process { get; set; }
        public int CurrentBoxes = 0;
        public DateTime StartTimer = new DateTime();
        public DateTime BoxesStamp = new DateTime();
        Role.GameMap _map;
        public Role.GameMap Map
        {
            get
            {
                if (_map == null)
                    _map = Pool.ServerMaps[MapID];
                return _map;
            }
        }
        public TournamentType Type { get; set; }
        public MsgTreasureThief(TournamentType _type)
        {
            Type = _type;
            Process = ProcesType.Dead;
        }
        public bool InTournament(Client.GameClient user)
        {
            return user.Player.Map == MapID;
        }
        public void Open()
        {
            if (!Core.Features.FeatureRegistry.IsKept("events.custom-minigames")) return; // [feature-gate]
            if (Process != ProcesType.Alive)
            {
                Create();
                foreach (var user in Pool.GamePoll.Values)
                    user.Player.CurrentTreasureBoxes = 0;
                Process = ProcesType.Alive;
                StartTimer = DateTime.Now.AddMinutes(5);
                BoxesStamp = DateTime.Now.AddSeconds(30);
                MsgSchedules.SendInvitation("TreasureThief", "ConquerPoints,Money,Vip and others treasures", 446, 249, 1002, 0, 60);
            }
        }
        public bool Join(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (user.Player.Level < 100)
            {
                user.SendSysMesage("Need to be level 100 at least.");
                return false;
            }
            if (Process == ProcesType.Alive)
            {
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                user.Teleport(x, y, MapID);
                return true;
            }
            return false;
        }
        private void Create()
        {
            GenerateBoxes();
        }
        private void GenerateBoxes()
        {
            for (int i = CurrentBoxes; i < 10; i++)
            {
                byte rand = (byte)Pool.GetRandom.Next(0, 5);
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);

                Game.MsgNpc.Npc np = Game.MsgNpc.Npc.Create();
                while (true)
                {
                    np.UID = (uint)Pool.GetRandom.Next(10000, 100000);
                    if (Map.View.Contain(np.UID, x, y) == false)
                        break;
                }
                np.NpcType = Role.Flags.NpcType.Talker;
                switch (rand)
                {
                    case 0: np.Mesh = 26586; break;
                    case 1: np.Mesh = 26586; break;
                    case 2: np.Mesh = 26586; break;
                    case 3: np.Mesh = 26586; break;
                    case 4: np.Mesh = 26586; break;
                    default: np.Mesh = 26586; break;
                }
                np.Map = MapID;
                np.Name = "TreasureBox";
                np.X = x;
                np.Y = y;
                Map.AddNpc(np);
            }
            CurrentBoxes = 10;
        }
        public void CheckUp()
        {
            if (Process == ProcesType.Alive)
            {
                if (DateTime.Now > StartTimer)
                {
                    MsgSchedules.SendSysMesage("All Players of Treasure Thief Stage 1 has teleported to Twincity!", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                    foreach (var user in Map.Values)
                        user.Teleport(428, 379, 1002, 0, true);
                    Process = ProcesType.Dead;
                }
                else if (DateTime.Now > BoxesStamp)
                {
                    GenerateBoxes();
                    BoxesStamp = DateTime.Now.AddSeconds(20);
                }
            }
        }
        public void Reward(Client.GameClient user, Game.MsgNpc.Npc npc, ServerSockets.Packet stream)
        {
            if (user.Player.CurrentTreasureBoxes > 10 || Process == ProcesType.Dead)
            {
                user.Teleport(428, 379, 1002, 0, true);
                user.SendSysMesage("You cannot open more boxes, wait to next event. You are teleported to Twin City Center.");
            }
            else
            {
                CurrentBoxes -= 1;
                byte rand = (byte)Pool.GetRandom.Next(0, 6);
                switch (rand)
                {
                    #region Oro
                    case 0://money
                        {
                            uint value = (uint)Pool.GetRandom.Next(100000, 1000000);
                            user.Player.Money += value;
                            user.Player.SendUpdate(stream, user.Player.Money, MsgServer.MsgUpdate.DataType.Money);
#if Arabic
                         user.CreateBoxDialog("You've received "+value+" Money.");
                        MsgSchedules.SendSysMesage(user.Player.Name + " got " + value.ToString() + " Money while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                       
#else
                            user.CreateBoxDialog("You've received " + value + " Money.");
                            MsgSchedules.SendSysMesage(user.Player.Name + " got " + value.ToString() + " Money while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);

#endif
                            break;
                        }
                    #endregion
                    #region WaterDevil Spawn
                    case 1:
                        {
                            //user.Teleport(428, 379, 1002, 0, true);
                            Database.Server.AddMapMonster(stream, Map, 8419, (ushort)(user.Player.X+5), (ushort)(user.Player.Y + 5), 1, 1, 1, 0);
                            MsgSchedules.SendSysMesage(user.Player.Name + " found WaterDevil Monster while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                            break;
                        }
                    #endregion
                    #region Stones +4
                    case 2://stones
                        {
                            uint ID = DropMob.StoneId(4);//ID STONE +4

                            Database.ItemType.DBItem DBItem;
                            if (Pool.ItemsBase.TryGetValue(ID, out DBItem))
                            {
                                if (user.Inventory.HaveSpace(1))
                                    user.Inventory.Add(stream, DBItem.ID, 1, 4);
                                else
                                    user.Inventory.AddReturnedItem(stream, DBItem.ID, 1, 4);
#if Arabic
                                  MsgSchedules.SendSysMesage(user.Player.Name + " got " + DBItem.Name + " while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                      
#else
                                MsgSchedules.SendSysMesage(user.Player.Name + " got " + DBItem.Name + " while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);

#endif
                            }

                            break;
                        }
                    #endregion
                    #region Stones +5
                    case 3://stones
                        {
                            uint ID = DropMob.StoneId(5);//ID STONE +5

                            Database.ItemType.DBItem DBItem;
                            if (Pool.ItemsBase.TryGetValue(ID, out DBItem))
                            {
                                if (user.Inventory.HaveSpace(1))
                                    user.Inventory.Add(stream, DBItem.ID, 1, 5);
                                else
                                    user.Inventory.AddReturnedItem(stream, DBItem.ID, 1, 5);
#if Arabic
                                  MsgSchedules.SendSysMesage(user.Player.Name + " got " + DBItem.Name + " while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                      
#else
                                MsgSchedules.SendSysMesage(user.Player.Name + " got " + DBItem.Name + " while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);

#endif
                            }

                            break;
                        }
                    #endregion
                    #region Stones +6
                    case 4://stones
                        {
                            uint ID = DropMob.StoneId(6);//ID STONE +6

                            Database.ItemType.DBItem DBItem;
                            if (Pool.ItemsBase.TryGetValue(ID, out DBItem))
                            {
                                if (user.Inventory.HaveSpace(1))
                                    user.Inventory.Add(stream, DBItem.ID, 1, 6);
                                else
                                    user.Inventory.AddReturnedItem(stream, DBItem.ID, 1, 6);
                                MsgSchedules.SendSysMesage(user.Player.Name + " got " + DBItem.Name + " while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                            }

                            break;
                        }
                    #endregion
                    #region Rate
                    case 5:
                        {
                            uint[] Items = new uint[]
                           {

                            Database.ItemType.MoonBox,
                            Database.ItemType.MeteorScroll,
                            700013, // DragonGem(S)
                            723727, // PenitenceAmulet
                            729534, // HotGarmentPack
                            720598, // DragonPill
                            724002, // SmallLotteryTicketPack
                            723094, // SpookStudyToken
                            723695, // BigPermanentStone
                            723342, // StudyBook
                            Database.ItemType.DragonBall,
                            724002, // SmallLotteryTicketPack

                           };
                            uint ItemID = Items[Pool.GetRandom.Next(0, Items.Length)];
                            Database.ItemType.DBItem DBItem;
                            if (Pool.ItemsBase.TryGetValue(ItemID, out DBItem))
                            {
                                if (user.Inventory.HaveSpace(1))
                                    user.Inventory.Add(stream, DBItem.ID);
                                else
                                    user.Inventory.AddReturnedItem(stream, DBItem.ID);
                                MsgSchedules.SendSysMesage(user.Player.Name + " got " + DBItem.Name + " while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                            }
                            break;
                        }
                    #endregion
                    #region Garments/StudyPoints/DragonPill/Mounts/SmallLotteryTicketPack
                    case 6:
                        {
                            uint[] Items = new uint[]
                            {
                            // Gaments
                            //188465,
                            181955,
                            //191305,
                            191405,
                            184305,
                            183395,
                            184315,
                            187315,
                            185355,
                            183385,
                            183345,
                            183365,
                            183375,
                            183325,
                            183315,
                            181715,//
                            181705,
                            188575,
                            184325,
                            192695,
                            194310,
                            194370,
                            720598, // DragonPill
                            723094, // SpookStudyToken
                            #region Monts
                            200000, 200001, 200002, 200003, 200004, 200005, 200006, 200007, 200008, 200009, 200010, 200011, 200012, 200015, 200016, 200017, 200018, 200019, 200200, 200201, 200203, 200204, 200410, 200411, 200412, 200415, 200416, 200417, 200418, 200419, 200420, 200427, 200421, 200431, 200433, 200437, 200438, 200439, 200440,
                            #endregion
                            723342, // StudyBook
                            724002, // SmallLotteryTicketPack



                            };
                            uint ItemID = Items[Pool.GetRandom.Next(0, Items.Length)];
                            Database.ItemType.DBItem DBItem;
                            if (Pool.ItemsBase.TryGetValue(ItemID, out DBItem))
                            {
                                if (user.Inventory.HaveSpace(1))
                                    user.Inventory.Add(stream, DBItem.ID);
                                else
                                    user.Inventory.AddReturnedItem(stream, DBItem.ID);
                                MsgSchedules.SendSysMesage(user.Player.Name + " got " + DBItem.Name + " while opening the TreasureBox!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                            }
                            break;
                        }
                        #endregion

                }
                user.Player.CurrentTreasureBoxes += 1;
                user.Player.SendString(stream, MsgServer.MsgStringPacket.StringID.Effect, true, "accession1");
                Map.RemoveNpc(npc, stream);
                ShuffleGuildScores(stream);
            }
        }
        public void ShuffleGuildScores(ServerSockets.Packet stream)
        {
            foreach (var user in Map.Values)
            {
#if Arabic
                 Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("---Your Score: " + user.Player.CurrentTreasureBoxes + "---", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                
#else
                Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("---Your Score: " + user.Player.CurrentTreasureBoxes + "---", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);

#endif
                user.Send(msg.GetArray(stream));
            }
            var array = Map.Values.OrderByDescending(p => p.Player.CurrentTreasureBoxes).ToArray();
            for (int x = 0; x < Math.Min(10, Map.Values.Length); x++)
            {
                var element = array[x];
#if Arabic
                   Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("No " + (x + 1).ToString() + "- " + element.Player.Name + " Opened " + element.Player.CurrentTreasureBoxes.ToString() + " Boxes!", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
             
#else
                Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("No " + (x + 1).ToString() + "- " + element.Player.Name + " Opened " + element.Player.CurrentTreasureBoxes.ToString() + " Boxes!", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

#endif
                Send(msg.GetArray(stream));
            }
        }
        public void Send(ServerSockets.Packet stream)
        {
            foreach (var user in Map.Values)
                user.Send(stream);
        }
    }
}
