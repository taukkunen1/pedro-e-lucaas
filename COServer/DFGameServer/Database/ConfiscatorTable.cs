using Core;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace GameServer.Database
{
    public class ConfiscatorTable
    {
        public ConcurrentDictionary<uint, Role.Instance.Confiscator> PollContainers = new ConcurrentDictionary<uint, Role.Instance.Confiscator>();


        public void QueueObj(uint UID, Role.Instance.Confiscator conainter)
        {
            if (!PollContainers.ContainsKey(UID))
                PollContainers.TryAdd(UID, conainter);
        }
        private int GetRedeemItemsCount()
        {
            return PollContainers.Values.Sum(p => p.RedeemContainer.Count);
        }
        private int GetClaimItemsCount()
        {
            return PollContainers.Values.Sum(p => p.ClaimContainer.Count);
        }
        internal unsafe void Save()
        {
            BinaryFileHelper binary = new();
            if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, "RedeemContainer.bin"), FileMode.Create))
            {
                int ItemCount;
                ItemCount = GetRedeemItemsCount();
                binary.Write(ItemCount);
                foreach (var Container in PollContainers.Values)
                {
                    foreach (var item in Container.RedeemContainer.Values)
                    {
                        Game.MsgServer.MsgDetainedItem packet = item;
                        binary.Write(packet);
                    }
                }
                binary.Close();
            }
            binary = new();
            if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, "ClaimContainer.bin"), FileMode.Create))
            {
                int ItemCount;
                ItemCount = GetClaimItemsCount();
                binary.Write(ItemCount);
                foreach (var Container in PollContainers.Values)
                {
                    foreach (var item in Container.ClaimContainer.Values)
                    {
                        Game.MsgServer.MsgDetainedItem packet = item;
                        binary.Write(packet);
                    }
                }
                binary.Close();
            }
        }

        internal unsafe void Load()
        {
            BinaryFileHelper binary = new();
            if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, "RedeemContainer.bin"), FileMode.Open))
            {
                Game.MsgServer.MsgDetainedItem Item;
                int ItemCount;
                ItemCount = binary.ReadInt();
                for (int x = 0; x < ItemCount; x++)
                {
                    Item = binary.Read<Game.MsgServer.MsgDetainedItem>();
                    if (Item.UID >= Role.Instance.Confiscator.CounterUID.Count)
                        Role.Instance.Confiscator.CounterUID.Set(Item.UID + 1);

                    if (!PollContainers.ContainsKey(Item.GainerUID))
                        PollContainers.TryAdd(Item.GainerUID, new Role.Instance.Confiscator());
                    if (!PollContainers.ContainsKey(Item.OwnerUID))
                        PollContainers.TryAdd(Item.OwnerUID, new Role.Instance.Confiscator());

                    Item.DaysLeft = (uint)(TimeSpan.FromTicks(DateTime.Now.Ticks).Days - TimeSpan.FromTicks(Role.Instance.Confiscator.GetTimer(Item.Date).Ticks).Days);
                    if (Item.Action == Game.MsgServer.MsgDetainedItem.ContainerType.RewardCps)
                    {
                        //Item.Action = Game.MsgServer.MsgDetainedItem.ContainerType.RewardCps;
                        PollContainers[Item.GainerUID].ClaimContainer.TryAdd(Item.UID, Item);
                        continue;
                    }
                    if (Item.DaysLeft > 7)
                    {
                       // Item.Action = Game.MsgServer.MsgDetainedItem.ContainerType.RewardCps;
                        PollContainers[Item.GainerUID].ClaimContainer.TryAdd(Item.UID, Item);
                        continue;
                    }
                    else
                    {
                        PollContainers[Item.GainerUID].ClaimContainer.TryAdd(Item.UID, Item);
                        PollContainers[Item.OwnerUID].RedeemContainer.TryAdd(Item.UID, Item);
                    }
                }
                binary.Close();
            }
            binary = new();
            if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, "ClaimContainer.bin"), FileMode.Open))
            {
                Game.MsgServer.MsgDetainedItem Item;
                int ItemCount = binary.ReadInt();
                for (int x = 0; x < ItemCount; x++)
                {
                    Item = binary.Read<Game.MsgServer.MsgDetainedItem>();

                    if (Item.UID >= Role.Instance.Confiscator.CounterUID.Count)
                        Role.Instance.Confiscator.CounterUID.Set(Item.UID + 1);

                    if (!PollContainers.ContainsKey(Item.GainerUID))
                        PollContainers.TryAdd(Item.GainerUID, new Role.Instance.Confiscator());
                    if (!PollContainers.ContainsKey(Item.OwnerUID))
                        PollContainers.TryAdd(Item.OwnerUID, new Role.Instance.Confiscator());

                    Item.DaysLeft = (uint)(TimeSpan.FromTicks(DateTime.Now.Ticks).Days - TimeSpan.FromTicks(Role.Instance.Confiscator.GetTimer(Item.Date).Ticks).Days);
                    PollContainers[Item.GainerUID].ClaimContainer.TryAdd(Item.UID, Item);
                }
                binary.Close();
            }
        }
    }
}
