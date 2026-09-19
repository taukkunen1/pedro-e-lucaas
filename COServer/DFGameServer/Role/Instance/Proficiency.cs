using GameServer.Game.MsgServer;
using System;
using System.Collections.Concurrent;

namespace GameServer.Role.Instance
{
    public class Proficiency
    {
        public const uint MaxExperience = 1000000;

        public ConcurrentDictionary<uint, Game.MsgServer.MsgProficiency> ClientProf = new ConcurrentDictionary<uint, Game.MsgServer.MsgProficiency>();

        private Client.GameClient Owner;

        public Proficiency(Client.GameClient _own)
        {
            Owner = _own;
        }
        public bool CheckProf(ushort ID, byte level)
        {
            Game.MsgServer.MsgProficiency prof;
            if (ClientProf.TryGetValue(ID, out prof))
            {
                return prof.Level >= level;
            }
            return false;
        }
        public unsafe void Add(ServerSockets.Packet stream, uint ID, uint Level = 0, uint Experience = 0, byte PreviousLevel = 0, bool ClearExp = false)
        {
            Game.MsgServer.MsgProficiency prof;
            if (ClientProf.TryGetValue(ID, out prof))
            {
                prof.UID = Owner.Player.UID;
                prof.Level = Level;
                prof.Experience = Experience;
                prof.PreviousLevel = PreviousLevel;
                if (prof.Level == 20 || ClearExp)
                    prof.Experience = 0;
            }
            else
            {
                prof = new MsgProficiency();
                prof.ID = ID;
                prof.UID = Owner.Player.UID;
                prof.Level = Level;
                prof.Experience = Experience;
                if (prof.Level == 20 || ClearExp)
                    prof.Experience = 0;
                prof.PreviousLevel = PreviousLevel;
                ClientProf.TryAdd(prof.ID, prof);
            }
            Owner.Send(stream.ProficiencyCreate(prof.ID, prof.Level, prof.Experience, Owner.Player.UID));
            UpdSpell(prof.ID, prof.Level, prof.Experience, stream);
        }


        private unsafe void UpdSpell(uint ID, uint level, uint Experience, ServerSockets.Packet stream)
        {
            Owner.Send(stream.UpdateProfExperienceCreate(Experience, Owner.Player.UID, ID));
        }
        public unsafe void CheckUpdate(uint ID, double GetExperience, ServerSockets.Packet stream)
        {
            if (GetExperience == 0)
                return;
            //if (ID == 1050)
            //    return;
            if (Enum.IsDefined(typeof(Database.MagicType.WeaponsType), (Database.MagicType.WeaponsType)ID))
            {
                Game.MsgServer.MsgProficiency prof;
                if (ClientProf.TryGetValue(ID, out prof))
                {
                    if (prof.Level < 20)
                    {
                        if (prof.Level < 5)
                        {
                            GetExperience *= 1.1;
                        }
                        else if (prof.Level > 5)
                        {
                            GetExperience *= 1.05;
                        }
                        else if (prof.Level > 12)
                        {
                            GetExperience *= 0.125;
                        }
                        GetExperience *= 1.1;

                        prof.Experience += (uint)GetExperience;
                        prof.Experience += (uint)(GetExperience * Owner.GemValues(Role.Flags.Gem.NormalVioletGem)) / 100;

                        bool leveled = false;

                        while (prof.Experience >= MaxExperience)
                        {
                            prof.Experience -= MaxExperience;
                            prof.Level++;
                            Owner.Equipment.QueryEquipment(false);
                            if (prof.Level == 20)
                            {
                                prof.Experience = 0;
                                Owner.Send(stream.ProficiencyCreate(prof.ID, prof.Level, prof.Experience, Owner.Player.UID));
                                Owner.SendSysMesage("You have just leveled your proficiency.", Game.MsgServer.MsgMessage.ChatMode.System);
                                UpdSpell(prof.ID, prof.Level, prof.Experience, stream);
                                return;
                            }
                            leveled = true;
                            Owner.SendSysMesage("You have just leveled your proficiency.", Game.MsgServer.MsgMessage.ChatMode.System);
                            if (prof.PreviousLevel != 0)
                            {
                                if (prof.Level >= prof.PreviousLevel / 2 && prof.Level < prof.PreviousLevel)
                                {
                                    prof.Level = prof.PreviousLevel;
                                    prof.Experience = 0;
                                }
                            }
                        }
                        if (leveled)
                        {
                            Owner.Send(stream.ProficiencyCreate(prof.ID, prof.Level, prof.Experience, Owner.Player.UID));
                        }
                        UpdSpell(prof.ID, prof.Level, prof.Experience, stream);

                    }
                }
                else
                {
                    Add(stream, ID);
                }
            }
        }
        public unsafe void SendAll(ServerSockets.Packet stream)
        {
            foreach (var prof in ClientProf.Values)
                Owner.Send(stream.ProficiencyCreate(prof.ID, prof.Level, prof.Experience, Owner.Player.UID));
        }
    }
}
