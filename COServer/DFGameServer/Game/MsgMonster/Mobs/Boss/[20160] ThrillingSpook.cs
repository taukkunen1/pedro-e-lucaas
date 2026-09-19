using GameServer.Game.MsgMonster;
using System.Collections.Generic;

namespace GameServer.Mobs
{
    public class MobID_20160 : Base
    {
        public MobID_20160(MonsterFamily _mob)
            : base(_mob)
        {
            ID = 20160;
            MapName = "Join from market";
            MapID = 2090;
            X = 40;
            Y = 39;
        }
        public override void Run()
        {
            base.Run();
        }
        public override void Reward(MonsterRole MobRole, Client.GameClient killer)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                base.Reward(MobRole, killer);
                ushort xx = MobRole.X;
                ushort yy = MobRole.Y;
                List<uint> DropItems;
                killer.Player.BossPoints++;

                if (Role.MyMath.Success(40))
                {
                    #region Generate p6
                    DropItems = base.Mob.ItemGenerator.GenerateSoulsItems(6, 1);//level , count
                    foreach (var ids in DropItems)
                    {
                        xx = (ushort)Pool.GetRandom.Next(MobRole.X - 7, MobRole.X + 7);
                        yy = (ushort)Pool.GetRandom.Next(MobRole.Y - 7, MobRole.Y + 7);
                        if (killer.Map.AddGroundItem(ref xx, ref yy))
                        {
                            MobRole.DropItem(stream, killer.Player.UID, killer.Map, ids, xx, yy, Game.MsgFloorItem.MsgItem.ItemType.Item, 0, false, 0, null, null, true);
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Generate p5
                    int cantidad = 0;
                    List<uint> DropIems2 = base.Mob.ItemGenerator.GenerateSoulsItems(5);
                    foreach (var ids in DropIems2)
                    {
                        if (cantidad >= 1)
                            break;
                        xx = (ushort)Pool.GetRandom.Next(MobRole.X - 7, MobRole.X + 7);
                        yy = (ushort)Pool.GetRandom.Next(MobRole.Y - 7, MobRole.Y + 7);
                        if (killer.Map.AddGroundItem(ref xx, ref yy))
                        {
                            MobRole.DropItem(stream, killer.Player.UID, killer.Map, ids, xx, yy, Game.MsgFloorItem.MsgItem.ItemType.Item, 0, false, 0, null, null, true);
                        }
                    }
                    #endregion
                }
                if (Role.MyMath.Success(40))
                {
                    #region big and per
                    for (int x = 0; x < 2; x++)
                    {
                        uint id = 0;
                        if (x == 1)
                            id = 723695; // BigPermanentStone
                        if (x == 2)
                            id = 723694; // PermanentStone
                        xx = (ushort)Pool.GetRandom.Next(MobRole.X - 7, MobRole.X + 7);
                        yy = (ushort)Pool.GetRandom.Next(MobRole.Y - 7, MobRole.Y + 7);
                        if (killer.Map.AddGroundItem(ref xx, ref yy))
                        {
                            MobRole.DropItem(stream, killer.Player.UID, killer.Map, id, xx, yy, Game.MsgFloorItem.MsgItem.ItemType.Item, 0, false, 0, null, null, true);
                        }

                    }
                    #endregion
                }
                #region Other items
                if (Role.MyMath.Success(70))
                {
                    uint id = 721003; // PVEPoints
                    xx = (ushort)Pool.GetRandom.Next(MobRole.X - 7, MobRole.X + 7);
                    yy = (ushort)Pool.GetRandom.Next(MobRole.Y - 7, MobRole.Y + 7);
                    if (killer.Map.AddGroundItem(ref xx, ref yy))
                    {
                        MobRole.DropItem(stream, killer.Player.UID, killer.Map, id, xx, yy, Game.MsgFloorItem.MsgItem.ItemType.Item, 0, false, 0, null, null, true);
                    }
                }
                if (Role.MyMath.Success(60))
                {
                    uint id = 721026; // 50CPs Bag
                    xx = (ushort)Pool.GetRandom.Next(MobRole.X - 7, MobRole.X + 7);
                    yy = (ushort)Pool.GetRandom.Next(MobRole.Y - 7, MobRole.Y + 7);
                    if (killer.Map.AddGroundItem(ref xx, ref yy))
                    {
                        MobRole.DropItem(stream, killer.Player.UID, killer.Map, id, xx, yy, Game.MsgFloorItem.MsgItem.ItemType.Item, 0, false, 0, null, null, true);
                    }
                }
                if (Role.MyMath.Success(50))
                {
                    uint id = 721027; // 100CPs Bag
                    xx = (ushort)Pool.GetRandom.Next(MobRole.X - 7, MobRole.X + 7);
                    yy = (ushort)Pool.GetRandom.Next(MobRole.Y - 7, MobRole.Y + 7);
                    if (killer.Map.AddGroundItem(ref xx, ref yy))
                    {
                        MobRole.DropItem(stream, killer.Player.UID, killer.Map, id, xx, yy, Game.MsgFloorItem.MsgItem.ItemType.Item, 0, false, 0, null, null, true);
                    }
                }
                if (Role.MyMath.Success(40))
                {
                    uint id = 723342; // StudyBook
                    xx = (ushort)Pool.GetRandom.Next(MobRole.X - 7, MobRole.X + 7);
                    yy = (ushort)Pool.GetRandom.Next(MobRole.Y - 7, MobRole.Y + 7);
                    if (killer.Map.AddGroundItem(ref xx, ref yy))
                    {
                        MobRole.DropItem(stream, killer.Player.UID, killer.Map, id, xx, yy, Game.MsgFloorItem.MsgItem.ItemType.Item, 0, false, 0, null, null, true);
                    }
                }
                if (Role.MyMath.Success(30))
                {
                    uint id = 730005; // Stone (+5)
                    xx = (ushort)Pool.GetRandom.Next(MobRole.X - 7, MobRole.X + 7);
                    yy = (ushort)Pool.GetRandom.Next(MobRole.Y - 7, MobRole.Y + 7);
                    if (killer.Map.AddGroundItem(ref xx, ref yy))
                    {
                        MobRole.DropItem(stream, killer.Player.UID, killer.Map, id, xx, yy, Game.MsgFloorItem.MsgItem.ItemType.Item, 0, false, 0, null, null, true);
                    }
                }
                if (Role.MyMath.Success(20))
                {
                    uint id = 730006; // Stone (+6)
                    xx = (ushort)Pool.GetRandom.Next(MobRole.X - 7, MobRole.X + 7);
                    yy = (ushort)Pool.GetRandom.Next(MobRole.Y - 7, MobRole.Y + 7);
                    if (killer.Map.AddGroundItem(ref xx, ref yy))
                    {
                        MobRole.DropItem(stream, killer.Player.UID, killer.Map, id, xx, yy, Game.MsgFloorItem.MsgItem.ItemType.Item, 0, false, 0, null, null, true);
                    }
                }
                if (Role.MyMath.Success(10))
                {
                    uint id = 730007; // Stone (+7)
                    xx = (ushort)Pool.GetRandom.Next(MobRole.X - 7, MobRole.X + 7);
                    yy = (ushort)Pool.GetRandom.Next(MobRole.Y - 7, MobRole.Y + 7);
                    if (killer.Map.AddGroundItem(ref xx, ref yy))
                    {
                        MobRole.DropItem(stream, killer.Player.UID, killer.Map, id, xx, yy, Game.MsgFloorItem.MsgItem.ItemType.Item, 0, false, 0, null, null, true);
                    }
                }
                #endregion
            }
        }
    }
}
