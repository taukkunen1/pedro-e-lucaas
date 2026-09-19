using GameServer.Game.MsgServer;
using GameServer.Role;

namespace GameServer.Base.Mining
{
    class Process
    {
        public static uint[] Ores = new uint[]
       {
            1072010, 1072011, 1072012, 1072013,
            1072054, 1072049, 1072041, 1072041,
            1072016, 1072017, 1072018, 1072047,
            1072026, 1072026, 1072025, 1072014,
            1072015, 1072025, 1072019, 1072011
       };
        public static uint[] Gems = new uint[]
        {
            700001, 700011, 700021, 700031,700071,
            700041, 700051, 700061, 700061, 700011
        };
        public static bool IsPickAxe(uint ID)
        {
            return ID == 562000;
        }
        private static ThreadSafeRandom SafeRandom = new ThreadSafeRandom();
        public static bool PercentSuccess(double _chance)
        {
            return SafeRandom.NextDouble() * 100 < _chance;
        }
        public static unsafe void Handler(Client.GameClient client)
        {
            if (!client.Mining)
                return;
            
            if (!GameMap.IsMiningMap(client.Player.Map))
            {
                client.StopMining();
                return;
            }

            MsgGameItem MiningWeapon;
            if (!client.Equipment.Alternante)
            {
                if (!client.Equipment.TryGetEquip(Role.Flags.ConquerItem.RightWeapon, out MiningWeapon))
                {
                    client.SendSysMesage("You have to wear PickAxe to start mining.");
                    client.StopMining();
                    return;
                }
            }
            else
            {
                if (!client.Equipment.TryGetEquip(Role.Flags.ConquerItem.AleternanteRightWeapon, out MiningWeapon))
                {
                    client.SendSysMesage("You have to wear PickAxe to start mining.");
                    client.StopMining();
                    return;
                }
            }
            if (MiningWeapon == null)
            {
                client.SendSysMesage("You have to wear PickAxe to start mining.");
                client.StopMining();
                return;
            }
            if (!IsPickAxe(MiningWeapon.ITEM_ID))
            {
                client.SendSysMesage("You have to wear PickAxe to start mining.");
                client.StopMining();
                return;
            }
            if (!client.Inventory.HaveSpace(1))
            {
                client.SendSysMesage("Your inventory is full. You can not mine anymore items.");
                client.StopMining();
                return;
            }
            if (client.MiningAttempts == 0)
            {
                client.SendSysMesage("Sorry, you need to get some rest come back tomorrow.");
                client.StopMining();
                return;
            }
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                ActionQuery daction = new ActionQuery()
                {
                   ObjId = client.Player.UID,
                    dwParam = client.MiningAttempts,
                    Type = ActionType.Mining,
                    wParam1 = 24,
                    wParam2 = 68,
                };
                client.Player.View.SendView(stream.ActionCreate(&daction), true);
                if (MyMath.Success(50.0)) // 50% any reward
                {
                    if (MyMath.Success(40.0))
                    {
                        client.Inventory.Add(stream, Ores[Role.Core.Random.Next(0, Ores.Length)], 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false, Role.Flags.ItemEffect.None, true, "~from~mining!");
                        client.MiningAttempts--;
                        return;
                    }
                    else if (MyMath.Success(10.0))
                    {
                        uint itemid = Gems[Role.Core.Random.Next(0, Gems.Length)];
                        if (PercentSuccess(0.01))
                            itemid += 2; // For Super Gem
                        client.Inventory.Add(stream, itemid, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false, Role.Flags.ItemEffect.None, true, "~from~mining!");
                        client.MiningAttempts--;
                        return;
                    }
                    else if (MyMath.Success(0.1))
                    {
                        client.Inventory.Add(stream, 1088000, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false, Role.Flags.ItemEffect.None, true, "~from~mining!");
                        client.MiningAttempts--;
                        return;
                    }
                    else if (MyMath.Success(1.0))
                    {
                        client.Inventory.Add(stream, 1088001, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false, Role.Flags.ItemEffect.None, true, "~from~mining!");
                        client.MiningAttempts--;
                        return;
                    }
                    else if (MyMath.Success(4.0))
                    {
                        client.Inventory.Add(stream, 730001, 1, 1, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false, Role.Flags.ItemEffect.None, true, "~from~mining!");
                        client.MiningAttempts--;
                        return;
                    }
                    else if (MyMath.Success(6.0))
                    {
                        client.Inventory.Add(stream, 730002, 1, 2, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false, Role.Flags.ItemEffect.None, true, "~from~mining!");
                        client.MiningAttempts--;
                        return;
                    }
                    else
                    {
                        client.Inventory.Add(stream, 1072031, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false, Role.Flags.ItemEffect.None, true, "~from~mining!");
                        client.MiningAttempts--;
                        return;
                    }
                }
            }
        }
    }
}
