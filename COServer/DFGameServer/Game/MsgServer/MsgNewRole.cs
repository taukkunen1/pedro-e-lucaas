using GameServer.MadeByDaRkFox;

namespace GameServer.Game.MsgServer
{
    public static class MsgNewRole
    {
        public static void GetNewRoleInfo(this ServerSockets.Packet msg, out string name, out ushort Body, out byte Class)
        {
            msg.ReadBytes(20);
            name = msg.ReadCString(16);//24
            msg.ReadBytes(32);
            Body = msg.ReadUInt16();
            Class = msg.ReadUInt8();

        }
        [PacketAttribute(GamePackets.NewClient)]
        public unsafe static void CreateCharacter(Client.GameClient client, ServerSockets.Packet stream)
        {
            if ((client.ClientFlag & Client.ServerFlag.CreateCharacter) == Client.ServerFlag.CreateCharacter)
            {
                client.ClientFlag &= ~Client.ServerFlag.AcceptLogin;
                string CharacterName; ushort Body; byte Class;
                stream.GetNewRoleInfo(out CharacterName, out Body, out Class);
                byte attackType = 0;
                if (!ExitBody(Body))
                {
                    client.Send(new MsgMessage("AHAHAH! WRONG Body, NICE TRY", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
                    return;
                }
                if (!ExitClass(Class))
                {
                    client.Send(new MsgMessage("AHAHAH! WRONG Class, NICE TRY", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
                    return;
                }
                CharacterName = CharacterName.Replace("\0", "");
                if (Program.NameStrCheck(CharacterName))
                {
                    if (!Pool.NameUsed.Contains(CharacterName.GetHashCode()))
                    {
                        client.ClientFlag &= ~Client.ServerFlag.CreateCharacter;

                        lock (Pool.NameUsed)
                            Pool.NameUsed.Add(CharacterName.GetHashCode());

                        client.Player.Name = CharacterName;
                        client.Player.Class = Class;
                        client.Player.Body = Body;

                        client.Player.Level = 1;
                        // Era 1 characters begin in Birth Village and leave for Twin City
                        // through the classic tutorial flow.
                        client.Player.Map = 1010;
                        client.Player.X = 61;
                        client.Player.Y = 109;

                        Database.DataCore.LoadClient(client.Player);

                        client.Player.UID = client.ConnectionUID;
                        if (attackType == 1)
                            client.Player.MainFlag |= Role.Player.MainFlagType.OnMeleeAttack;

                        Database.DataCore.AtributeStatus.GetStatus(client.Player);

                        if (Body == 1003 || Body == 1004)
                            client.Player.Face = (ushort)Pool.GetRandom.Next(1, 50);
                        else
                            client.Player.Face = (ushort)Pool.GetRandom.Next(201, 250);

                        byte Color = (byte)Pool.GetRandom.Next(4, 8);
                        client.Player.Hair = (ushort)(Color * 100 + 10 + (byte)Pool.GetRandom.Next(4, 9));

                        CharacterCreationDefaultSet.Init(client, stream);

                        // Era 1: do not grant class/XP skills during character creation.
                        // New characters learn their abilities through the classic Birth Village,
                        // promotion and skill-training progression.

                        client.Send(new MsgMessage("ANSWER_OK", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
                        Database.Server.LastChar = client.Player.Name;
                        client.Status.MaxHitpoints = client.CalculateHitPoint();
                        client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                        client.ClientFlag |= Client.ServerFlag.CreateCharacterSucces;
                    }
                    else
                    {
                        client.Send(new MsgMessage("The name is in use! try other name", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
                    }
                }
                else
                {
                    client.Send(new MsgMessage("Invalid characters name!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
                }
            }
        }
        public static bool ExitBody(ushort _body)
        {
            return (_body == 1003 || _body == 1004 || _body == 2001 || _body == 2002);
        }

        public static bool ExitClass(byte cls)
        {
            return (cls == 10 || cls == 20 || cls == 40
                || cls == 100);
        }
    }
}
