using Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameServer.Game.MsgServer
{
    public static partial class MsgBuilder
    {

        public static void GetItemPacketPacket(this ServerSockets.Packet stream, out MsgGameItem item)
        {
            item = new MsgGameItem();
            item.UID = stream.ReadUInt32();
            item.ITEM_ID = stream.ReadUInt32();
            item.Durability = stream.ReadUInt16();
            item.MaximDurability = stream.ReadUInt16();
            item.Mode = (Role.Flags.ItemMode)stream.ReadUInt16();
            item.Position = stream.ReadUInt16();
            item.SocketProgress = stream.ReadUInt32();
            item.SocketOne = (Role.Flags.Gem)stream.ReadUInt8();
            item.SocketTwo = (Role.Flags.Gem)stream.ReadUInt8();
            stream.SeekForward(sizeof(ushort));
            item.Effect = (Role.Flags.ItemEffect)stream.ReadUInt32();
            stream.SeekForward(sizeof(byte));
            item.Plus = stream.ReadUInt8();
            item.Bless = stream.ReadUInt8();
            item.Bound = stream.ReadUInt8();
            item.Enchant = stream.ReadUInt8();
            stream.SeekForward(9);
            item.Locked = stream.ReadUInt8();
            stream.SeekForward(sizeof(byte));
            item.Color = (Role.Flags.Color)stream.ReadUInt32();
            item.PlusProgress = stream.ReadUInt32();
            item.Inscribed = stream.ReadUInt32();
            stream.ReadUInt32();
            stream.ReadUInt32();
            item.StackSize = stream.ReadUInt16();
        }
    }
    public class MsgGameItem
    {
        public MsgItemExtra.Purification Purification;
        public MsgItemExtra.Refinery Refinary;
        public ConcurrentDictionary<uint, MsgGameItem> Deposite = new ConcurrentDictionary<uint, MsgGameItem>();
        public string AgateStr;
        public Dictionary<uint, string> Agate_map { get; set; }
        public MsgGameItem()
        {
            this.Agate_map = new Dictionary<uint, string>(10);
            this.UID = Pool.ItemUIDCounter.Next;
        }


        public void Send(MsgInterServer.PipeClient user, ServerSockets.Packet stream)
        {
            user.Send(ItemCreate(stream, this));
            if (Purification.ItemUID != 0 || Refinary.ItemUID != 0)
            {
                MsgItemExtra extra = new MsgItemExtra();
                if (Purification.InLife)
                {
                    if (Purification.SecondsLeft == 0)
                        Purification.Typ = MsgItemExtra.Typing.Stabilization;
                    else
                        Purification.Typ = MsgItemExtra.Typing.PurificationAdding;
                    extra.Purifications.Add(Purification);
                }
                if (Refinary.InLife)
                {
                    Refinary.Typ = MsgItemExtra.Typing.RefinaryAdding;
                    if (Refinary.EffectDuration == 0)
                        Refinary.Typ = MsgItemExtra.Typing.PermanentRefinery;
                    extra.Refinerys.Add(Refinary);
                }
                user.Send(extra.CreateArray(stream, true));
            }
        }

        public unsafe Role.Instance.Inventory Send(Client.GameClient client, ServerSockets.Packet stream)
        {
            if (Mode == Role.Flags.ItemMode.Update)
            {

                string logs = "[Item]" + client.Player.Name + " update [" + UID + "]" + ITEM_ID + " plus [" + Plus + "] s1[" + SocketOne + "]s2[" + SocketTwo + "] at : " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
                Database.ServerDatabase.LoginQueue.Enqueue(logs);
            }
            if (MaximDurability == 0)
            {
                Database.ItemType.DBItem DBItem;
                if (Pool.ItemsBase.TryGetValue(ITEM_ID, out DBItem))
                    MaximDurability = DBItem.Durability;
            }
            ushort position = Database.ItemType.ItemPosition(ITEM_ID);
            if (Plus > 0)
            {
                if (position == 0)
                    Plus = 0;
            }
            if (ITEM_ID >= 730001 && ITEM_ID <= 730008)
                Plus = (byte)(ITEM_ID % 10);


            client.Send(ItemCreate(stream, this));
            SendItemExtra(client, stream);
            SendItemLocked(client, stream);

            return client.Inventory;
        }
        public void SendItemExtra(Client.GameClient client, ServerSockets.Packet stream)
        {
            if (Purification.ItemUID != 0 || Refinary.ItemUID != 0)
            {
                MsgItemExtra extra = new MsgItemExtra();
                if (Purification.InLife)
                {
                    if (Purification.SecondsLeft == 0)
                        Purification.Typ = MsgItemExtra.Typing.Stabilization;
                    else
                        Purification.Typ = MsgItemExtra.Typing.PurificationAdding;
                    extra.Purifications.Add(Purification);
                }
                if (Refinary.InLife)
                {
                    Refinary.Typ = MsgItemExtra.Typing.RefinaryAdding;
                    if (Refinary.EffectDuration == 0)
                        Refinary.Typ = MsgItemExtra.Typing.PermanentRefinery;
                    extra.Refinerys.Add(Refinary);
                }
                client.Send(extra.CreateArray(stream));
            }

        }
        public void SendItemLocked(Client.GameClient client, ServerSockets.Packet stream)
        {
            if (Locked == 2)
            {
                if (client.Player.OnMyOwnServer)
                {
                    if (UnLockTimer == 0)
                    {
                        Locked = 0;
                        Mode = Role.Flags.ItemMode.Update;
                        client.Send(ItemCreate(stream, this));
                    }
                    else
                    {
                        if (DateTime.Now > Role.Core.GetTimer(UnLockTimer))
                        {
                            Locked = 0;
                            Mode = Role.Flags.ItemMode.Update;
                            client.Send(ItemCreate(stream, this));
                        }
                        else
                        {
                            client.Send(stream.ItemLockCreate(UID, MsgItemLock.TypeLock.UnlockDate, 0, (uint)UnLockTimer));
                        }
                    }
                }
            }
        }
        public ServerSockets.Packet ItemCreate(ServerSockets.Packet stream, MsgGameItem item)
        {
            stream.InitWriter();
            stream.Write(item.UID);
            stream.Write(item.ITEM_ID);
            stream.Write(item.Durability);
            stream.Write(item.MaximDurability);
            stream.Write((ushort)item.Mode);
            stream.Write(item.Position);//18
            stream.Write(item.SocketProgress);//20
            stream.Write((byte)item.SocketOne);//24
            stream.Write((byte)item.SocketTwo);//25
            stream.Write((ushort)0);//26
            stream.Write((uint)item.Effect);//28
            stream.Write((byte)0);//32
            stream.Write(item.Plus);//33
            stream.Write(item.Bless);//34
            stream.Write(item.Bound);//35
            stream.Write(item.Enchant);//36
            stream.ZeroFill(3);//37
            stream.Write(item.ProgresGreen);//40
            stream.Write((ushort)item.Suspicious);//45
            stream.Write((ushort)item.Locked);//46
            stream.Write((uint)item.Color);//48
            stream.Write(item.PlusProgress);//52
            stream.Write(item.Inscribed);//56
            if (item.Activate == 0)
            {
                stream.Write(item.Activate);//53
                stream.Write(item.RemainingTime);
            }
            else
            {
                if (item.Activate == 1 && item.RemainingTime > 0 && item.RemainingTime < uint.MaxValue)
                {

                    stream.Write((uint)(item.RemainingTime));
                }
                else
                {
                    stream.Write(item.RemainingTime);
                }
            }
            stream.Write(item.StackSize);
            stream.Finalize(GamePackets.Item);
            return stream;
        }

        public bool IsWeapon
        {
            get
            {
                return (Database.ItemType.ItemPosition(ITEM_ID) == (ushort)Role.Flags.ConquerItem.RightWeapon
                    || Database.ItemType.ItemPosition(ITEM_ID) == (ushort)Role.Flags.ConquerItem.LeftWeapon) && !Database.ItemType.IsArrow(ITEM_ID);
            }
        }
        public bool IsEquip
        {
            get
            {
                return Database.ItemType.ItemPosition(ITEM_ID) != 0;
            }
        }


        public ushort Leng;
        public ushort PacketID;
        public uint UID;
        public uint ITEM_ID;
        public ushort Durability;
        public ushort MaximDurability;
        public Role.Flags.ItemMode Mode;
        public ushort Position;
        public uint SocketProgress;
        public bool Fake = false;
        public int IDEvent;
        public uint RemainingTime
        {
            get
            {
                if (Activate == 1)
                {
                    TimeSpan Span = EndDate - DateTime.Now;
                    return (uint)Span.TotalSeconds;
                }
                return 0;
            }
        }
        public Role.Flags.Gem SocketOne;
        public Role.Flags.Gem SocketTwo;
        public ushort padding;
        public Role.Flags.ItemEffect Effect;
        public byte Plus;
        public byte Bless;
        public byte Bound;
        public byte Enchant;//36 // Steed  -> ProgresBlue 
        public uint ProgresGreen;//39 // for steed
        public byte Suspicious;
        public byte Locked;
        public Role.Flags.Color Color;
        public uint PlusProgress;//52
        public uint Inscribed;
        public DateTime EndDate = DateTime.FromBinary(0);
        public uint Activate;
        public uint TimeLeftInMinutes;//64
        public ushort StackSize;//68
        public ushort UnKnow;
        public uint WH_ID;

        public int UnLockTimer;

        internal unsafe void SendAgate(Client.GameClient client)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var str = rec.GetStream();
                str.InitWriter();
                str.Write((uint)0);
                str.Write(UID);//8
                str.Write((uint)Agate_map.Count);// Agate Map Count - 12
                str.Write((ulong)Agate_map.Count);// Agate Map Count - 16
                str.Write((uint)Durability);//24
                str.Write((uint)Agate_map.Count);//28 Agate map count again?
                if (Agate_map.Count > 0)
                {
                    for (uint i = 0; i < Agate_map.Count; i++)
                    {
                        str.Write((uint)i);
                        str.Write(uint.Parse(Agate_map[i].Split(new char[] { '~' })[0].ToString()));
                        str.Write(uint.Parse(Agate_map[i].Split(new char[] { '~' })[1].ToString()));
                        str.Write(uint.Parse(Agate_map[i].Split(new char[] { '~' })[2].ToString()));
                        str.ZeroFill(32);
                    }
                }
                str.Finalize(GamePackets.MemoryAgate);
                client.Send(str);
            }
        }

    }
}
