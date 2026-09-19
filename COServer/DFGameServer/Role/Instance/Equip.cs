using GameServer.Game.MsgServer;
using GameServer.Instance;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Role.Instance
{
    public class Equip
    {
        #region ItemsTime
        public bool Contains(uint UID)
        {
            return ClientItems.ContainsKey(UID);
        }
        public bool Delete(Role.Flags.ConquerItem position, ServerSockets.Packet stream)
        {
            if (!FreeEquip(position))
            {
                if ((byte)position > 20)
                {
                    return DeleteAlternante(position, stream);
                }
                bool Accept = Owner.Inventory.HaveSpace(1);
                if (Accept)
                {
                    Game.MsgServer.MsgGameItem itemdata;
                    if (TryGetEquip(position, out itemdata))
                    {
                        if (ClientItems.TryRemove(itemdata.UID, out itemdata))
                        {
                            if (position == Flags.ConquerItem.Garment || position == Flags.ConquerItem.SteedMount)
                            {
                                itemdata.Position = 0;
                            }
                            else
                            {
                                Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsageID.Unequip, itemdata.UID, itemdata.Position, 0, 0, 0, 0));
                                itemdata.Position = 0;
                                itemdata.Mode = Flags.ItemMode.AddItem;
                                Owner.Inventory.Update(itemdata, AddMode.REMOVE, stream);
                            }
                        }
                    }
                }
                else
                {

                    Owner.SendSysMesage("Your Inventory Is Full.");


                }
                return Accept;
            }
            else
                return false;
        }
        public bool DeleteAlternante(Role.Flags.ConquerItem position, ServerSockets.Packet stream)
        {
            bool Accept = Owner.Inventory.HaveSpace(1);
            if (Accept)
            {
                Game.MsgServer.MsgGameItem itemdata;
                if (TryGetEquip(position, out itemdata))
                {
                    if (ClientItems.TryRemove(itemdata.UID, out itemdata))
                    {
                        Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsageID.Unequip, itemdata.UID, itemdata.Position, 0, 0, 0, 0));
                        itemdata.Position = 0;
                        itemdata.Mode = Flags.ItemMode.AddItem;
                        Owner.Inventory.Update(itemdata, AddMode.REMOVE, stream);
                    }
                }
            }
            else
            {

                Owner.SendSysMesage("Your Inventory Is Full.");


            }
            return Accept;
        }
        #endregion
        public ConcurrentDictionary<uint, MsgGameItem> ClientItems = new ConcurrentDictionary<uint, MsgGameItem>();
        public uint SoulsPotency = 0;

        public int WeaponsMinAttack = 0;
        public uint ArmorID;
        public bool CreateSpawn = true;
        public bool SuperArmor = false;
        public bool FullSuper
        {
            get
            {
                if (!SuperArmor)
                    return false;
                foreach (var item in CurentEquip)
                {
                    if (item.Position != (ushort)Flags.ConquerItem.Steed
                        && item.Position != (ushort)Flags.ConquerItem.Garment
                        && item.Position != (ushort)Flags.ConquerItem.Bottle
                        && item.Position != (ushort)Flags.ConquerItem.AleternanteBottle
                        && item.Position != (ushort)Flags.ConquerItem.AlternateGarment
                        && item.Position != (ushort)Flags.ConquerItem.SteedMount
                        && item.Position != (ushort)Flags.ConquerItem.RightWeaponAccessory
                        && item.Position != (ushort)Flags.ConquerItem.LeftWeaponAccessory)
                    {
                        if (item.ITEM_ID % 10 != 9)
                            return false;
                    }
                }
                return true;
            }
        }
        public Flags.ItemEffect RightWeaponEffect = Flags.ItemEffect.None;
        public Flags.ItemEffect LeftWeaponEffect = Flags.ItemEffect.None;
        public Flags.ItemEffect RingEffect = Flags.ItemEffect.None;
        public Flags.ItemEffect NecklaceEffect = Flags.ItemEffect.None;

        public bool UseMonkEpicWeapon = false;
        public uint ShieldID = 0;
        public uint RidingCrop = 0;
        public uint HeadID;
        public uint RightWeapon = 0;
        public uint LeftWeapon = 0;
        public byte SteedPlus { get { return (byte)Owner.Player.SteedPlus; } }
        public uint SteedPlusPorgres = 0;

        public int rangeR = 0;
        public int rangeL = 0;
        public int SizeAdd = 0;

        public int SpeedR = 0;
        public int SpeedL = 0;
        public int SpeedRing = 0;

        public bool SuperDragonGem = false;
        public bool SuperPhoenixGem = false;
        public bool SuperVioletGem = false;
        public bool SuperRaibowGem = false;
        public bool SuperMoonGem = false;
        public bool SuprtTortoiseGem = false;
        public bool SuperKylinGem = false;
        public bool HaveBless = false;
        
        public int AttackSpeed(int MS_Delay)
        {
            MS_Delay = Math.Max(300, MS_Delay - 100);

            MS_Delay = Math.Max(300, MS_Delay - SpeedR);
            MS_Delay = Math.Max(300, MS_Delay - SpeedL);
            MS_Delay = Math.Max(300, MS_Delay - SpeedRing);
            MS_Delay = Math.Max(300, MS_Delay - Owner.Player.Agility / 2);

            if (Owner.Player.ContainFlag(MsgUpdate.Flags.Cyclone))
                MS_Delay = Math.Max(300, MS_Delay - 150);

            return MS_Delay;
        }
        public int Pirata(bool physical)
        {
            return AttackSpeed(1000);
            //int speed = 800;
            //speed = Math.Max(300, speed - SpeedR);
            //speed = Math.Max(300, speed - SpeedL);
            //speed = Math.Max(300, speed - SpeedRing);
            //speed = Math.Max(300, speed - Owner.Player.Agility / 2);

            //if (Owner.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Cyclone))
            //    speed = Math.Max(300, speed - 150);

            //return speed;
        }
        public int AttackSpeed(bool physical)
        {
            return AttackSpeed(800);
            //int speed = 800;
            //speed = Math.Max(300, speed - SpeedR);
            //speed = Math.Max(300, speed - SpeedL);
            //speed = Math.Max(300, speed - SpeedRing);
            //speed = Math.Max(300, speed - Owner.Player.Agility / 2);

            //if (Owner.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Cyclone))
            //    speed = Math.Max(300, speed - 150);

            //return speed;
        }
        public int GetAttackRange(int targetSizeAdd)
        {
            var range = 1;

            if (rangeR != 0 && rangeL != 0)
                range = (rangeR + rangeL) / 2;
            else if (rangeR != 0)
                range = rangeR;
            else if (rangeL != 0)
                range = rangeL;

            range += (SizeAdd + targetSizeAdd + 1) / 2;

            return range;
        }

        public bool Alternante = false;
        private Client.GameClient Owner;
        public Equip(Client.GameClient client)
        {
            Owner = client;
        }
        public MsgGameItem[] CurentEquip = new MsgGameItem[0];
        public unsafe bool Add(ServerSockets.Packet stream, uint ID, Flags.ConquerItem position, byte plus = 0, byte bless = 0, byte Enchant = 0
           , Flags.Gem sockone = Flags.Gem.NoSocket
            , Flags.Gem socktwo = Flags.Gem.NoSocket, bool bound = false, Flags.ItemEffect Effect = Flags.ItemEffect.None)
        {
            if (FreeEquip(position))
            {
                Database.ItemType.DBItem DbItem;
                if (Pool.ItemsBase.TryGetValue(ID, out DbItem))
                {
                    MsgGameItem ItemDat = new MsgGameItem();
                    //ItemDat.UID = Pool.ITEM_Counter.Next;
                    ItemDat.ITEM_ID = ID;
                    ItemDat.Effect = Effect;
                    ItemDat.Durability = ItemDat.MaximDurability = DbItem.Durability;
                    ItemDat.Plus = plus;
                    ItemDat.Bless = bless;
                    ItemDat.Enchant = Enchant;
                    ItemDat.SocketOne = sockone;
                    ItemDat.SocketTwo = socktwo;
                    ItemDat.Color = (Flags.Color)Pool.GetRandom.Next(3, 9);
                    ItemDat.Bound = (byte)(bound ? 1 : 0);
                    CheakUp(ItemDat);
                    ItemDat.Position = (ushort)position;
                    ItemDat.Mode = Flags.ItemMode.AddItem;

                    ItemDat.Send(Owner, stream);


                    Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsageID.Equip, ItemDat.UID, ItemDat.Position, 0, 0, 0, 0));

                    return true;

                }
            }
            return false;
        }
        private void CheakUp(MsgGameItem ItemDat)
        {
            if (ItemDat.UID == 0) return;
                //ItemDat.UID = Pool.ITEM_Counter.Next;
            if (!ClientItems.TryAdd(ItemDat.UID, ItemDat))
            {
                //do
                //{
                //    ItemDat.UID = Pool.ITEM_Counter.Next;
                //}
                //while
                //  (ClientItems.TryAdd(ItemDat.UID, ItemDat) == false);
            }
        }

        public bool Exist(Func<MsgGameItem, bool> predicate)
        {
            bool Exist = false;
            foreach (var item in CurentEquip)
                if (predicate(item))
                {
                    Exist = true;
                    break;
                }
            return Exist;
        }
        public void Have(Func<MsgGameItem, bool> predicate, out int count)
        {
            count = 0;
            foreach (var item in CurentEquip)
                if (predicate(item))
                {
                    count++;
                }

        }
        public bool Exist(Func<MsgGameItem, bool> predicate, int count)
        {
            int counter = 0;
            foreach (var item in CurentEquip)
                if (predicate(item))
                {
                    counter++;
                }
            return counter >= count;
        }
        public ICollection<MsgGameItem> AllItems
        {
            get { return ClientItems.Values; }
        }
        public bool TryGetValue(uint UID, out MsgGameItem itemdata)
        {
            return ClientItems.TryGetValue(UID, out itemdata);
        }
        public bool FreeEquip(Flags.ConquerItem position)
        {
            var item = ClientItems.Values.Where(p => p.Position == (ushort)position)
                .FirstOrDefault();
            return item == null;
        }
        public bool TryGetEquip(Flags.ConquerItem position, out MsgGameItem itemdata)
        {

            itemdata = ClientItems.Values.Where(p => p.Position == (ushort)position).FirstOrDefault();
            return itemdata != null;
        }
        public MsgGameItem TryGetEquip(Flags.ConquerItem position)
        {
            return ClientItems.Values.Where(p => p.Position == (ushort)position).FirstOrDefault();
        }
        public bool Remove(Flags.ConquerItem position, ServerSockets.Packet stream)
        {
            if (position == Flags.ConquerItem.Steed)
            {
                Owner.Player.RemoveFlag(MsgUpdate.Flags.Ride);
            }
            if (Owner.Player.ContainFlag(MsgUpdate.Flags.Fly))
                Owner.Player.RemoveFlag(MsgUpdate.Flags.Fly);
            if (!FreeEquip(position))
            {
                if ((byte)position > 20)
                {
                    return RemoveAlternante(position, stream);
                }
                bool Accept = Owner.Inventory.HaveSpace(1);
                if (Accept)
                {
                    MsgGameItem itemdata;
                    if (TryGetEquip(position, out itemdata))
                    {
                        if (ClientItems.TryRemove(itemdata.UID, out itemdata))
                        {
                            Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsageID.Unequip, itemdata.UID, itemdata.Position, 0, 0, 0, 0));

                            itemdata.Position = 0;
                            itemdata.Mode = Flags.ItemMode.AddItem;
                            if (itemdata.Fake)
                            {
                                Owner.Inventory.Update(itemdata, AddMode.REMOVE, stream);
                            }
                            else Owner.Inventory.Update(itemdata, AddMode.MOVE, stream);
                        }
                    }
                }
                else
                {
                    Owner.SendSysMesage("Your Inventory Is Full.");
                }
                return Accept;
            }
            else
                return false;
        }
        public bool RemoveAlternante(Flags.ConquerItem position, ServerSockets.Packet stream)
        {
            bool Accept = Owner.Inventory.HaveSpace(1);
            if (Accept)
            {
                MsgGameItem itemdata;
                if (TryGetEquip(position, out itemdata))
                {
                    if (ClientItems.TryRemove(itemdata.UID, out itemdata))
                    {
                        Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsageID.Unequip, itemdata.UID, itemdata.Position, 0, 0, 0, 0));
                        itemdata.Position = 0;
                        itemdata.Mode = Flags.ItemMode.AddItem;
                        Owner.Inventory.Update(itemdata, AddMode.MOVE, stream);
                    }
                }
            }
            else
            {
                Owner.SendSysMesage("Your Inventory Is Full.");
            }
            return Accept;
        }

        public void Add(MsgGameItem item, ServerSockets.Packet stream)
        {
            CheakUp(item);

            if (item.Position > 20)
            {
                AddAlternante(item, stream);
                return;
            }
            ClientItems.TryAdd(item.UID, item);
            item.Mode = Flags.ItemMode.AddItem;
            item.Send(Owner, stream);
        }
        public void AddAlternante(MsgGameItem itemdata, ServerSockets.Packet stream)
        {
            ClientItems.TryAdd(itemdata.UID, itemdata);
            itemdata.Mode = Flags.ItemMode.AddItem;

            Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsageID.Unequip, itemdata.UID, itemdata.Position, 0, 0, 0, 0));

            itemdata.Send(Owner, stream);
        }
        public void Show(ServerSockets.Packet stream)
        {
            foreach (var item in ClientItems.Values)
            {
                item.Mode = Flags.ItemMode.AddItem;
                item.Send(Owner, stream);
            }
            QueryEquipment(Alternante);
        }
        public unsafe void ClearItemSpawn()
        {
            Owner.Player.ClearItemsSpawn();
        }
        public unsafe void AddSpawn(MsgGameItem DataItem)
        {

            switch ((Flags.ConquerItem)DataItem.Position)
            {
                case Flags.ConquerItem.AleternanteArmor:
                case Flags.ConquerItem.Armor:
                    {
                        Owner.Player.ArmorId = DataItem.ITEM_ID;
                        Owner.Player.ColorArmor = (ushort)DataItem.Color;
                        Owner.Player.ArmorSoul = DataItem.Purification.PurificationItemID;

                        break;
                    }
                case Flags.ConquerItem.AleternanteHead:
                case Flags.ConquerItem.Head:
                    {

                        Owner.Player.HeadId = DataItem.ITEM_ID;
                        Owner.Player.ColorHelment = (ushort)DataItem.Color;
                        Owner.Player.HeadSoul = DataItem.Purification.PurificationItemID;
                        break;
                    }
                case Flags.ConquerItem.AleternanteLeftWeapon:
                case Flags.ConquerItem.LeftWeapon:
                    {
                        Owner.Player.LeftWeaponId = DataItem.ITEM_ID;
                        Owner.Player.LeftWeapsonSoul = DataItem.Purification.PurificationItemID;
                        break;
                    }
                case Flags.ConquerItem.LeftWeaponAccessory:
                    {
                        Owner.Player.LeftWeaponAccessoryId = DataItem.ITEM_ID;
                        break;
                    }
                case Flags.ConquerItem.AleternanteRightWeapon:
                case Flags.ConquerItem.RightWeapon:
                    {
                        Owner.Player.RightWeaponId = DataItem.ITEM_ID;
                        Owner.Player.ColorShield = (ushort)DataItem.Color;
                        Owner.Player.RightWeapsonSoul = DataItem.Purification.PurificationItemID;
                        break;
                    }
                case Flags.ConquerItem.RightWeaponAccessory:
                    {
                        Owner.Player.RightWeaponAccessoryId = DataItem.ITEM_ID;
                        break;
                    }
                case Flags.ConquerItem.Steed:
                    {
                        Owner.Player.SteedId = DataItem.ITEM_ID;
                        Owner.Player.SteedColor = DataItem.SocketProgress;
                        Owner.Player.SteedPlus = DataItem.Plus;
                        SteedPlusPorgres = DataItem.PlusProgress;
                        break;
                    }
                case Flags.ConquerItem.SteedMount:
                    {
                        Owner.Player.MountArmorId = DataItem.ITEM_ID;
                        break;
                    }
                case Flags.ConquerItem.AlternateGarment:
                case Flags.ConquerItem.Garment:
                    {

                        Owner.Player.GarmentId = DataItem.ITEM_ID;
                        break;
                    }
            }
        }
        public unsafe void UpdateStats(MsgGameItem[] MyGear, ServerSockets.Packet stream)
        {

            try
            {
                SoulsPotency = 0;
                rangeR = rangeL = SizeAdd = 0;
                SpeedR = SpeedL = SpeedRing = 0;
                RightWeapon = 0;
                LeftWeapon = 0;
                UseMonkEpicWeapon = false;
                SuperArmor = false;
                HeadID = 0;
                WeaponsMinAttack = 0;
                HaveBless = false;
                RingEffect = Flags.ItemEffect.None;
                RightWeaponEffect = Flags.ItemEffect.None;
                LeftWeaponEffect = Flags.ItemEffect.None;
                SteedPlusPorgres = 0;
                Owner.Status.MaxVigor = 0;
                RidingCrop = 0;
                if (CreateSpawn)
                {
                    lock (CurentEquip)
                        CurentEquip = MyGear;
                    ClearItemSpawn();
                }
                Owner.Status = new MsgStatus();
                Owner.Status.UID = Owner.Player.UID;

                Owner.Status.MaxAttack = (ushort)(Owner.Player.Strength + 1);
                Owner.Status.MinAttack = (ushort)(Owner.Player.Strength);
                Owner.Status.MagicAttack = Owner.Player.Spirit;

                Owner.Gems = new ushort[13];

                foreach (var item in MyGear)
                {

                    if (Database.ItemType.ItemPosition(item.ITEM_ID) == (ushort)Flags.ConquerItem.Head
                        || Database.ItemType.ItemPosition(item.ITEM_ID) == (ushort)Flags.ConquerItem.AleternanteHead)
                        HeadID = item.ITEM_ID;
                    try
                    {

                        if (CreateSpawn)
                            AddSpawn(item);

                        if (item.Durability == 0)
                            continue;

                        ushort ItemPostion = (ushort)(item.Position % 20);

                        ushort durabilityPercentToReduce = (ushort)Owner.GemValues(Flags.Gem.NormalKylinGem);
                        ushort duraItemToReduce = (ushort)(item.Durability * durabilityPercentToReduce / 100);
                        item.Durability = (ushort)(item.Durability - duraItemToReduce);

                        if (item.Bless >= 1)
                            HaveBless = true;
                        if (ItemPostion == (ushort)Flags.ConquerItem.Armor)
                        {
                            SuperArmor = (item.ITEM_ID % 10) == 9;
                            ArmorID = item.ITEM_ID;
                        }
                        if (item.SocketOne != Flags.Gem.NoSocket && item.SocketOne != Flags.Gem.EmptySocket)
                        {
                            if (item.SocketOne == Flags.Gem.SuperTortoiseGem)
                                SuprtTortoiseGem = true;
                            if (item.SocketOne == Flags.Gem.SuperDragonGem)
                                SuperDragonGem = true;
                            if (item.SocketOne == Flags.Gem.SuperPhoenixGem)
                                SuperPhoenixGem = true;
                            if (item.SocketOne == Flags.Gem.SuperVioletGem)
                                SuperVioletGem = true;
                            if (item.SocketOne == Flags.Gem.SuperRainbowGem)
                                SuperRaibowGem = true;
                            if (item.SocketOne == Flags.Gem.SuperMoonGem)
                                SuperMoonGem = true;
                            if (item.SocketOne == Flags.Gem.SuperKylinGem)
                                SuperKylinGem = true;
                        }

                        if (ItemPostion == (ushort)Flags.ConquerItem.RidingCrop)
                        {
                            RidingCrop = item.ITEM_ID;
                            Owner.Status.MaxVigor += 1000;
                        }
                        if (ItemPostion == (ushort)Flags.ConquerItem.LeftWeapon)
                        {
                            LeftWeapon = item.ITEM_ID;
                            LeftWeaponEffect = item.Effect;
                            if (Database.ItemType.IsShield(item.ITEM_ID))
                                ShieldID = item.ITEM_ID;
                        }
                        if (ItemPostion == (ushort)Flags.ConquerItem.RightWeapon)
                        {
                            RightWeaponEffect = item.Effect;
                            RightWeapon = item.ITEM_ID;
                        }

                        if (ItemPostion == (ushort)Flags.ConquerItem.Ring)
                            RingEffect = item.Effect;

                        if (ItemPostion == (ushort)Flags.ConquerItem.Necklace)
                            NecklaceEffect = item.Effect;

                        AddGem(item.SocketOne);
                        AddGem(item.SocketTwo);
                        if (!Pool.ItemsBase.ContainsKey(item.ITEM_ID))
                            continue;
                        var DBItem = Pool.ItemsBase[item.ITEM_ID];
                        if (ItemPostion == (byte)Flags.ConquerItem.Fan)
                        {
                            Owner.Status.PhysicalDamageIncrease += DBItem.MaxAttack;
                            Owner.Status.MagicDamageIncrease += DBItem.MagicAttack;
                        }
                        else
                        {
                            if (ItemPostion == (ushort)Flags.ConquerItem.Ring)
                            {
                                SpeedRing = DBItem.Frequency;
                            }
                            if (ItemPostion == (ushort)Flags.ConquerItem.LeftWeapon)
                            {
                                rangeL = DBItem.AttackRange;
                                SpeedL = DBItem.Frequency;

                                WeaponsMinAttack += (int)(DBItem.MaxAttack / 2);
                                Owner.Status.MaxAttack += (uint)(DBItem.MaxAttack / 2);
                                Owner.Status.MinAttack += (uint)(DBItem.MinAttack / 2);
                                Owner.Status.MagicAttack += (uint)(DBItem.MagicAttack / 2);
                            }
                            else
                            {
                                if (ItemPostion == (ushort)Flags.ConquerItem.RightWeapon)
                                {
                                    WeaponsMinAttack += DBItem.MinAttack;
                                    rangeR = DBItem.AttackRange;
                                    SpeedR = DBItem.Frequency;
                                }

                                Owner.Status.MaxAttack += DBItem.MaxAttack;
                                Owner.Status.MinAttack += DBItem.MinAttack;
                                Owner.Status.MagicAttack += DBItem.MagicAttack;
                            }
                        }
                        if (ItemPostion == (byte)Flags.ConquerItem.Tower)
                        {
                            Owner.Status.MagicDamageDecrease += DBItem.MagicDefence;
                            Owner.Status.PhysicalDamageDecrease += DBItem.PhysicalDefence;
                        }
                        else
                        {
                            Owner.Status.Immunity += DBItem.Imunity;
                            Owner.Status.CriticalStrike += DBItem.Crytical;
                            Owner.Status.SkillCStrike += DBItem.SCrytical;
                            Owner.Status.Breakthrough += DBItem.BreackTrough;
                            Owner.Status.Counteraction += DBItem.ConterAction;
                            Owner.Status.MDefence += (byte)DBItem.MagicDefence;
                            Owner.Status.Defence += DBItem.PhysicalDefence;
                        }

                        if (ItemPostion != (byte)Flags.ConquerItem.Steed)
                        {
                            Owner.Status.Dodge += DBItem.Dodge;
                            Owner.Status.AgilityAtack += DBItem.Frequency;
                            Owner.Status.ItemBless += item.Bless;
                            Owner.Status.MaxHitpoints += item.Enchant;
                        }
                        Owner.Status.MaxHitpoints += DBItem.ItemHP;
                        Owner.Status.MaxMana += DBItem.ItemMP;

                        if (item.Purification.InLife)
                        {
                            var purificare = Pool.ItemsBase[item.Purification.PurificationItemID];
                            Owner.Status.MaxAttack += purificare.MaxAttack;
                            Owner.Status.MinAttack += purificare.MinAttack;
                            Owner.Status.MagicAttack += purificare.MagicAttack;
                            Owner.Status.MDefence += (byte)purificare.MagicDefence;
                            Owner.Status.Defence += purificare.PhysicalDefence;

                            Owner.Status.CriticalStrike += purificare.Crytical;
                            Owner.Status.SkillCStrike += purificare.SCrytical;
                            Owner.Status.Immunity += purificare.Imunity;
                            Owner.Status.Penetration += purificare.Penetration;
                            Owner.Status.Block += purificare.Block;
                            Owner.Status.Breakthrough += purificare.BreackTrough;
                            Owner.Status.Counteraction += purificare.ConterAction;
                            Owner.Status.Detoxication += purificare.Detoxication;

                            Owner.Status.MetalResistance += purificare.MetalResistance;
                            Owner.Status.WoodResistance += purificare.WoodResistance;
                            Owner.Status.FireResistance += purificare.FireResistance;
                            Owner.Status.EarthResistance += purificare.EarthResistance;
                            Owner.Status.WaterResistance += purificare.WaterResistance;

                            Owner.Status.MaxHitpoints += purificare.ItemHP;
                            Owner.Status.MaxMana += purificare.ItemMP;
                            SoulsPotency += item.Purification.PurificationLevel;
                        }

                        if (item.Refinary.InLife)
                        {
                            IncreaseRifainaryStatus(item.Refinary.EffectID);
                        }
                        if (item.Plus > 0)
                        {
                            var extraitematributes = DBItem.Plus[item.Plus];
                            if (extraitematributes != null)
                            {
                                if (ItemPostion == (ushort)Flags.ConquerItem.LeftWeapon || ItemPostion == (ushort)Flags.ConquerItem.RightWeapon)
                                {
                                    Owner.Status.Accuracy += extraitematributes.Agility;
                                }

                                if (ItemPostion == (byte)Flags.ConquerItem.Steed)
                                    Owner.Status.MaxVigor = extraitematributes.Agility;

                                if (ItemPostion == (byte)Flags.ConquerItem.Fan)
                                {
                                    Owner.Status.PhysicalDamageIncrease += extraitematributes.MaxAttack;
                                    Owner.Status.MagicDamageIncrease += extraitematributes.MagicAttack;
                                }
                                else
                                {
                                    {
                                        Owner.Status.MaxAttack += extraitematributes.MaxAttack;
                                        Owner.Status.MinAttack += extraitematributes.MinAttack;
                                        Owner.Status.MagicAttack += extraitematributes.MagicAttack;
                                    }
                                }
                                if (ItemPostion == (byte)Flags.ConquerItem.Tower)
                                {
                                    Owner.Status.MagicDamageDecrease += extraitematributes.MagicDefence;
                                    Owner.Status.PhysicalDamageDecrease += extraitematributes.PhysicalDefence;
                                }
                                else
                                {
                                    Owner.Status.MagicDefence += extraitematributes.MagicDefence;
                                    Owner.Status.Defence += extraitematributes.PhysicalDefence;
                                }


                                if (ItemPostion != (byte)Flags.ConquerItem.Steed)
                                {
                                    Owner.Status.Dodge += extraitematributes.Dodge;
                                }
                                Owner.Status.MaxHitpoints += extraitematributes.ItemHP;
                            }
                            else
                                Console.WriteLine("Invalid Plus -> item " + item.ITEM_ID.ToString() + " ->  plus " + item.Plus.ToString() + "");
                        }
                    }
                    catch (Exception e) { Console.WriteLine(e.ToString()); }
                }

                Owner.Status.MagicAttack += 1;
                Owner.Status.MagicDefence += Owner.GemValues(Flags.Gem.NormalGloryGem);
                Owner.Status.PhysicalDamageDecrease += Owner.GemValues(Flags.Gem.NormalGloryGem);
                Owner.Status.PhysicalDamageIncrease += Owner.GemValues(Flags.Gem.NormalThunderGem);
                Owner.Status.MagicDamageIncrease += Owner.GemValues(Flags.Gem.NormalThunderGem);
                AddChiAtribute(Owner.Player.MyChi);
                Owner.Status.MaxHitpoints += Owner.CalculateHitPoint();
                Owner.Status.MaxMana += Owner.CalculateMana();

                Owner.Vigor = (ushort)Math.Min((int)Owner.Vigor, (int)Owner.Status.MaxVigor);
                if (CreateSpawn)
                    Owner.Send(stream.ServerInfoCreate(MsgServerInfo.Action.Vigor, Owner.Vigor));
                CalculateBattlePower();

                if (CreateSpawn)
                    Owner.Player.View.SendView(Owner.Player.GetArray(stream, false), false);

                Owner.Player.CheckAura();

                if (Owner.Player.SubClass != null)
                {
                    Owner.Player.SubClass.UpdateStatus(Owner);
                }
                Owner.Send(stream.StatusCreate(Owner.Status));
                SendMentorShare(stream);

                Owner.Status.Damage = Owner.GemValues(Flags.Gem.SuperTortoiseGem);
                if (Owner.Player.Mana > Owner.Status.MaxMana)
                    Owner.Player.Mana = (ushort)Owner.Status.MaxMana;
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        public void AddChiAtribute(Chi chi, uint percent = 100)
        {
            if (chi != null)
            {
                Owner.Status.CriticalStrike += chi.CriticalStrike * percent / 100;
                Owner.Status.SkillCStrike += chi.SkillCriticalStrike * percent / 100;
                Owner.Status.Immunity += chi.Immunity * percent / 100;
                Owner.Status.Counteraction += chi.Counteraction * percent / 100;
                Owner.Status.Breakthrough += chi.Breakthrough * percent / 100;
                Owner.Status.MaxHitpoints += chi.MaxLife * percent / 100;
                Owner.Status.MaxAttack += chi.AddAttack * percent / 100;
                Owner.Status.MinAttack += chi.AddAttack * percent / 100;
                Owner.Status.MagicAttack += chi.AddMagicAttack * percent / 100;
                Owner.Status.MagicDefence += chi.AddMagicDefense * percent / 100;

                Owner.Status.PhysicalDamageIncrease += chi.FinalAttack * percent / 100;
                Owner.Status.PhysicalDamageDecrease += chi.FinalDefense * percent / 100;
                Owner.Status.MagicDamageIncrease += chi.FinalMagicAttack * percent / 100;
                Owner.Status.MagicDamageDecrease += chi.FinalMagicDefense * percent / 100;
            }
        }
        public unsafe void SendMentorShare(ServerSockets.Packet stream)
        {
            MsgApprenticeInformation Information = MsgApprenticeInformation.Create();
            Information.Mode = MsgApprenticeInformation.Action.Mentor;
            if (Owner.Player.MyMentor != null)
            {
                if (Owner.Player.Associate.Associat.ContainsKey(AssociateGS.Mentor))
                {
                    if (Owner.Player.MyMentor.MyClient != null)
                    {
                        if (Owner.Player.Associate.Associat[AssociateGS.Mentor].ContainsKey(Owner.Player.MyMentor.MyUID))
                        {
                            Player mentor = Owner.Player.MyMentor.MyClient.Player;
                            Owner.Player.SetMentorBattlePowers(mentor.GetShareBattlePowers((uint)Owner.Player.RealBattlePower), (uint)mentor.RealBattlePower);

                            Information.Mentor_ID = mentor.UID;
                            Information.Apprentice_ID = Owner.Player.UID;
                            Information.Enrole_date = (uint)Owner.Player.Associate.Associat[AssociateGS.Mentor][mentor.UID].Timer;
                            Information.Fill(mentor.Owner);
                            Information.Shared_Battle_Power = mentor.GetShareBattlePowers((uint)Owner.Player.RealBattlePower);
                            Information.WriteString(mentor.Name, Owner.Player.Spouse, Owner.Player.Name);
                            Owner.Send(Information.GetArray(stream));
                        }
                    }

                }
            }
            if (Owner.Player.Associate != null)
            {
                if (!Owner.Player.Associate.Associat.ContainsKey(AssociateGS.Apprentice))
                    return;

                foreach (var Apprentice in Owner.Player.Associate.OnlineApprentice.Values)
                {
                    if (!Owner.Player.Associate.Associat[AssociateGS.Apprentice].ContainsKey(Apprentice.Player.UID))
                        continue;
                    Player target = Apprentice.Player;

                    target.SetMentorBattlePowers(Owner.Player.GetShareBattlePowers((uint)target.RealBattlePower), (uint)Owner.Player.RealBattlePower);

                    Information.Apprentice_ID = target.UID;
                    Information.Enrole_date = (uint)Owner.Player.Associate.Associat[AssociateGS.Apprentice][target.UID].Timer;
                    Information.Level = (byte)Owner.Player.Level;
                    Information.Class = Owner.Player.Class;
                    Information.PkPoints = Owner.Player.PKPoints;
                    Information.Mesh = Owner.Player.Mesh;
                    Information.Online = 1;
                    Information.Shared_Battle_Power = Owner.Player.GetShareBattlePowers((uint)target.RealBattlePower);
                    Information.WriteString(Owner.Player.Name, Owner.Player.Spouse, target.Name);
                    target.Owner.Send(Information.GetArray(stream));
                }
            }

        }
        public void IncreaseRifainaryStatus(uint ID)
        {
            Database.Refinery.Item refinery;
            if (Pool.RefineryItems.TryGetValue(ID, out refinery))
            {
                // var refinery = Database.Server.RifineryItems[ID];
                switch (refinery.Type)
                {
                    case Database.Refinery.RefineryType.CriticalStrike:
                        {
                            Owner.Status.CriticalStrike += refinery.Procent * 100;
                            break;
                        }
                    case Database.Refinery.RefineryType.SkillCriticalStrike:
                        {
                            Owner.Status.SkillCStrike += refinery.Procent * 100;
                            break;
                        }
                    case Database.Refinery.RefineryType.Break:
                        {
                            Owner.Status.Breakthrough += refinery.Procent * 10;
                            break;
                        }
                    case Database.Refinery.RefineryType.Detoxication:
                        {
                            Owner.Status.Detoxication += refinery.Procent;
                            break;
                        }
                    case Database.Refinery.RefineryType.MDefence:
                        {
                            Owner.Status.PhysicalDamageDecrease += refinery.Procent;
                            Owner.Status.MDefence += refinery.Procent;
                            break;
                        }

                    case Database.Refinery.RefineryType.Block:
                        {
                            Owner.Status.Block += refinery.Procent * 100;
                            break;
                        }

                    case Database.Refinery.RefineryType.Immunity:
                        {
                            Owner.Status.Immunity += refinery.Procent * 100;
                            break;
                        }

                    case Database.Refinery.RefineryType.Penetration:
                        {
                            Owner.Status.Penetration += refinery.Procent * 100;
                            break;
                        }
                    case Database.Refinery.RefineryType.Intensification:
                        {
                            Owner.Status.MaxHitpoints += refinery.Procent;
                            break;
                        }
                    case Database.Refinery.RefineryType.Counteraction:
                        {
                            Owner.Status.Counteraction += refinery.Procent * 10;
                            break;
                        }
                    case Database.Refinery.RefineryType.FinalMDamage:
                        {

                            Owner.Status.PhysicalDamageIncrease += refinery.Procent;

                            break;
                        }
                    case Database.Refinery.RefineryType.FinalMAttack:
                        {
                            Owner.Status.MagicDamageIncrease += refinery.Procent;
                            break;
                        }
                }
                switch (refinery.Type2)
                {
                    case Database.Refinery.RefineryType.CriticalStrike:
                        {
                            Owner.Status.CriticalStrike += refinery.Procent2 * 100;
                            break;
                        }
                    case Database.Refinery.RefineryType.SkillCriticalStrike:
                        {
                            Owner.Status.SkillCStrike += refinery.Procent2 * 100;
                            break;
                        }
                    case Database.Refinery.RefineryType.Break:
                        {
                            Owner.Status.Breakthrough += refinery.Procent2 * 10;
                            break;
                        }
                    case Database.Refinery.RefineryType.Detoxication:
                        {
                            Owner.Status.Detoxication += refinery.Procent2;
                            break;
                        }
                    case Database.Refinery.RefineryType.MDefence:
                        {
                            Owner.Status.PhysicalDamageDecrease += refinery.Procent2;
                            break;
                        }

                    case Database.Refinery.RefineryType.Block:
                        {
                            Owner.Status.Block += refinery.Procent2;
                            break;
                        }

                    case Database.Refinery.RefineryType.Immunity:
                        {
                            Owner.Status.Immunity += refinery.Procent2 * 100;
                            break;
                        }

                    case Database.Refinery.RefineryType.Penetration:
                        {
                            Owner.Status.Penetration += refinery.Procent2 * 100;
                            break;
                        }
                    case Database.Refinery.RefineryType.Intensification:
                        {
                            Owner.Status.MaxHitpoints += refinery.Procent2;
                            break;
                        }
                    case Database.Refinery.RefineryType.Counteraction:
                        {
                            Owner.Status.Counteraction += refinery.Procent2;
                            break;
                        }
                    case Database.Refinery.RefineryType.FinalMDamage:
                        {

                            Owner.Status.PhysicalDamageIncrease += refinery.Procent2;

                            break;
                        }
                    case Database.Refinery.RefineryType.FinalMAttack:
                        {
                            Owner.Status.MagicDamageIncrease += refinery.Procent2;
                            break;
                        }
                }
            }
            else
            {
                Console.WriteLine("error refinery id " + ID);
            }
        }
        public void AddGem(Flags.Gem gem)
        {
            switch (gem)
            {
                case Flags.Gem.SuperThunderGem:
                case Flags.Gem.SuperGloryGem: Owner.AddGem(gem, 500); break;
                case Flags.Gem.RefinedGloryGem:
                case Flags.Gem.RefinedThunderGem: Owner.AddGem(gem, 300); break;
                case Flags.Gem.NormalGloryGem:
                case Flags.Gem.NormalThunderGem: Owner.AddGem(gem, 100); break;
                case Flags.Gem.NormalPhoenixGem:
                case Flags.Gem.NormalDragonGem: Owner.AddGem(gem, 5); break;
                case Flags.Gem.RefinedPhoenixGem:
                case Flags.Gem.RefinedDragonGem: Owner.AddGem(gem, 10); break;
                case Flags.Gem.SuperPhoenixGem:
                case Flags.Gem.SuperDragonGem: Owner.AddGem(gem, 15); break;
                case Flags.Gem.NormalTortoiseGem: Owner.AddGem(gem, 2); break;
                case Flags.Gem.RefinedTortoiseGem: Owner.AddGem(gem, 4); break;
                case Flags.Gem.SuperTortoiseGem: Owner.AddGem(gem, 6); break;
                case Flags.Gem.SuperRainbowGem: Owner.AddGem(gem, 25); break;
                case Flags.Gem.RefinedRainbowGem: Owner.AddGem(gem, 15); break;
                case Flags.Gem.NormalRainbowGem: Owner.AddGem(gem, 10); break;
                case Flags.Gem.SuperKylinGem: Owner.AddGem(gem, 50); break;
                case Flags.Gem.RefinedKylinGem: Owner.AddGem(gem, 100); break;
                case Flags.Gem.NormalKylinGem: Owner.AddGem(gem, 200); break;

            }
        }
        public int BattlePower = 0;
        public void CalculateBattlePower()
        {
            BattlePower = 0;
            int val = 0;
            int val_item = 0;

            foreach (var item in CurentEquip)
            {
                if (Database.ItemType.ItemPosition(item.ITEM_ID) == (ushort)Flags.ConquerItem.Bottle
                    || Database.ItemType.ItemPosition(item.ITEM_ID) == (ushort)Flags.ConquerItem.Garment
                    || Database.ItemType.ItemPosition(item.ITEM_ID) == (ushort)Flags.ConquerItem.LeftWeaponAccessory
                    || Database.ItemType.ItemPosition(item.ITEM_ID) == (ushort)Flags.ConquerItem.RightWeaponAccessory
                    || Database.ItemType.ItemPosition(item.ITEM_ID) == (ushort)Flags.ConquerItem.SteedMount)
                    continue;
                val_item = 0;
                byte Quality = (byte)(item.ITEM_ID % 10);
                switch (Quality)
                {
                    case 9: val_item += 4; break;
                    case 8: val_item += 3; break;
                    case 7: val_item += 2; break;
                    case 6: val_item += 1; break;
                }
                val_item += item.Plus;

                if (item.SocketOne != Flags.Gem.NoSocket)
                    val_item += 1;
                if ((byte)(((byte)item.SocketOne % 10) - 3) == 0)
                    val_item += 1;
                if (item.SocketTwo != Flags.Gem.NoSocket)
                    val_item += 1;
                if ((byte)(((byte)item.SocketTwo % 10) - 3) == 0)
                    val_item += 1;

                if (Database.ItemType.IsBacksword(item.ITEM_ID))
                {
                    val_item *= 2;
                }
                else if (Database.ItemType.IsTwoHand(item.ITEM_ID) && FreeEquip(Flags.ConquerItem.LeftWeapon) && FreeEquip(Flags.ConquerItem.AleternanteLeftWeapon))
                {
                    val_item += val_item;
                }

                val += val_item;
            }
            BattlePower = val;
        }
        public void OnDequeue()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                try
                {

                    Dictionary<uint, MsgGameItem> statusitens = new Dictionary<uint, MsgGameItem>();
                    foreach (var it in AllItems)
                        if (it.Position < 20)
                            if (!statusitens.ContainsKey(it.Position))
                                statusitens.Add(it.Position, it);
                    if (Alternante)
                    {
                        if (!FreeEquip(Flags.ConquerItem.RightWeapon) && !FreeEquip(Flags.ConquerItem.LeftWeapon))
                        {
                            MsgGameItem _left;
                            TryGetEquip(Flags.ConquerItem.LeftWeapon, out _left);
                            MsgGameItem _right;
                            TryGetEquip(Flags.ConquerItem.RightWeapon, out _right);
                            if (Database.ItemType.IsArrow(_left.ITEM_ID) && Database.ItemType.IsBow(_right.ITEM_ID))
                            {
                                MsgGameItem _bow;
                                if (TryGetEquip(Flags.ConquerItem.AleternanteRightWeapon, out _bow) && FreeEquip(Flags.ConquerItem.AleternanteLeftWeapon))
                                {
                                    foreach (var it in AllItems)
                                        if (it.Position > 20)
                                        {
                                            if (statusitens.ContainsKey((ushort)(it.Position - 20)))
                                                statusitens.Remove((ushort)(it.Position - 20));
                                            statusitens.Add((ushort)(it.Position - 20), it);
                                        }
                                    if (statusitens.ContainsKey((ushort)Flags.ConquerItem.LeftWeapon))
                                    {
                                        statusitens.Remove((ushort)Flags.ConquerItem.LeftWeapon);
                                    }
                                    goto jmp;
                                }
                            }
                        }
                        foreach (var it in AllItems)
                            if (it.Position > 20)
                            {
                                if (it.Position == (byte)Flags.ConquerItem.AleternanteRightWeapon)
                                {
                                    if (Database.ItemType.IsTwoHand(it.ITEM_ID))
                                    {
                                        if (Database.ItemType.IsBow(it.ITEM_ID) == false)
                                        {
                                            if (statusitens.ContainsKey((ushort)(it.Position - 19)))
                                            {
                                                statusitens.Remove((ushort)(it.Position - 19));
                                                Remove((Flags.ConquerItem)((it.Position - 19)), stream);
                                            }
                                        }
                                    }
                                }
                                if (statusitens.ContainsKey((ushort)(it.Position - 20)))
                                    statusitens.Remove((ushort)(it.Position - 20));
                                statusitens.Add((ushort)(it.Position - 20), it);
                            }
                    }
                    jmp:
                    AppendItems(CreateSpawn, statusitens.Values.ToArray(), stream);
                    UpdateStats(statusitens.Values.ToArray(), stream);

                    Owner.Player.HitPoints = Math.Min((int)Owner.Player.HitPoints, (int)Owner.Status.MaxHitpoints);
                    if (Owner.Player.OnTransform && Owner.Player.TransformInfo != null)
                        Owner.Player.TransformInfo.UpdateStatus();
                    else
                        Owner.Player.SendUpdateHP();

                    Owner.ClanShareBP();

                }
                catch (Exception e)
                {
                    Console.SaveException(e);
                }


            }
        }
        public void AppendItems(bool CreateSpawn, MsgGameItem[] Items, ServerSockets.Packet stream)
        {
            MsgShowEquipment ShowEquip = new MsgShowEquipment();
            ShowEquip.wParam = MsgShowEquipment.Show;
            ShowEquip.Alternante = (byte)(Alternante ? 1 : 0);

            if (CreateSpawn)
            {
                foreach (var item in Items)
                {
                    if (item != null)
                    {
                        switch ((Flags.ConquerItem)item.Position)
                        {
                            case Flags.ConquerItem.Ring:
                            case Flags.ConquerItem.AleternanteRing: ShowEquip.Ring = item.UID; break;
                            case Flags.ConquerItem.AleternanteHead:
                            case Flags.ConquerItem.Head: ShowEquip.Head = item.UID; break;
                            case Flags.ConquerItem.AleternanteNecklace:
                            case Flags.ConquerItem.Necklace: ShowEquip.Necklace = item.UID; break;
                            case Flags.ConquerItem.AleternanteRightWeapon:
                            case Flags.ConquerItem.RightWeapon: ShowEquip.RightWeapon = item.UID; break;
                            case Flags.ConquerItem.AleternanteLeftWeapon:
                            case Flags.ConquerItem.LeftWeapon: ShowEquip.LeftWeapon = item.UID; break;
                            case Flags.ConquerItem.AleternanteArmor:
                            case Flags.ConquerItem.Armor:
                                {
                                    ShowEquip.Armor = item.UID;
                                    break;
                                }
                            case Flags.ConquerItem.AleternanteBoots:
                            case Flags.ConquerItem.Boots: ShowEquip.Boots = item.UID; break;
                            case Flags.ConquerItem.AleternanteBottle:
                            case Flags.ConquerItem.Bottle: ShowEquip.Bottle = item.UID; break;
                            case Flags.ConquerItem.SteedMount: ShowEquip.SteedMount = item.UID; break;
                            case Flags.ConquerItem.AlternateGarment:
                            case Flags.ConquerItem.Garment:
                                {

                                    ShowEquip.Garment = item.UID;
                                    break;
                                }
                            case Flags.ConquerItem.RidingCrop: ShowEquip.RidingCrop = item.UID; break;
                            case Flags.ConquerItem.LeftWeaponAccessory: ShowEquip.LeftWeaponAccessory = item.UID; break;
                            case Flags.ConquerItem.RightWeaponAccessory: ShowEquip.RightWeaponAccessory = item.UID; break;
                        }
                    }
                }
                //if (Owner.Player.SpecialGarment != 0)
                //    ShowEquip.Garment = uint.MaxValue - 1;
                Owner.Send(stream.ShowEquipmentCreate(ShowEquip));


            }
        }
        public unsafe void SendAlowAlternante(ServerSockets.Packet stream)
        {
            MsgShowEquipment ShowEquip = new MsgShowEquipment();
            ShowEquip.wParam = MsgShowEquipment.AlternanteAllow;
            ShowEquip.Alternante = (byte)(Alternante ? 1 : 0);
            Owner.Send(stream.ShowEquipmentCreate(ShowEquip));
        }
        public unsafe void QueryEquipment(bool Alternantes, bool CallItems = true)
        {
            this.Alternante = Alternantes;
            CreateSpawn = CallItems;
            OnDequeue();
        }

        public bool DestoyArrow(Flags.ConquerItem position, ServerSockets.Packet stream)
        {
            if (!FreeEquip(position))
            {
                MsgGameItem itemdata;
                if (TryGetEquip(position, out itemdata))
                {
                    if (!(itemdata.ITEM_ID >= 1050000 && itemdata.ITEM_ID <= 1051000))
                        return false;
                    if (ClientItems.TryRemove(itemdata.UID, out itemdata))
                    {
                        Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsageID.Unequip, itemdata.UID, itemdata.Position, 0, 0, 0, 0));
                        itemdata.Position = 0;
                        itemdata.Mode = Flags.ItemMode.AddItem;
                        Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsageID.RemoveInventory, itemdata.UID, 0, 0, 0, 0, 0));
                    }
                }
            }
            return false;
        }
    }
}
