using Core;
using Core.Interfaces.GameServer;
using Core.Models.GameServer;
using GameServer.Database.DBActions;
using GameServer.Game.MsgTournaments;
using GameServer.Role.Instance;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static GameServer.Game.MsgTournaments.MsgGuildWar;
using static GameServer.Role.Instance.AssociateGS;
using static GameServer.Role.Instance.House;
using static GameServer.Role.KOBoard;

namespace GameServer.MadeByDaRkFox
{
    public interface IDataManager
    {
        public void Init();
        public void Save();
        public void Load();
    }
    public class TeamElitePKDataManager : IDataManager
    {
        public TeamElitePKDataManager()
        {
            Init();
        }
        public void Init()
        {
        }
        public void Save()
        {
            if (ServerConfig.DbFromFiles)
            {
                Write writer = new Write("TeamElitePK.ini");
                for (int x = 0; x < MsgTeamPkTournament.EliteGroups.Length; x++)
                {
                    var Tournament = MsgTeamPkTournament.EliteGroups[x];
                    for (int i = 0; i < Tournament.Top8.Length; i++)
                    {
                        WriteLine writerline = new WriteLine('/');
                        var element = Tournament.Top8[i];
                        writerline.Add(x).Add(i).Add(element.UID).Add(element.Name).Add(element.Mesh).Add(element.ClaimReward).Add(element.LeaderUID);
                        writer.Add(writerline.Close());
                    }
                }
                writer.Execute(Mode.Open);
            }
            else
            {
                List<TeamElitePK> toUpdate = new List<TeamElitePK>();
                for (int x = 0; x < MsgTeamPkTournament.EliteGroups.Length; x++)
                {
                    var Tournament = MsgTeamPkTournament.EliteGroups[x];
                    for (int i = 0; i < Tournament.Top8.Length; i++)
                    {
                        var element = Tournament.Top8[i];
                        toUpdate.Add(new TeamElitePK
                        {
                            Type = Core.Interfaces.GameServer.EliteTournamentType.TeamElitePK,
                            Tournament = (byte)x,
                            Rank = (byte)i,
                            PlayerId = element.UID,
                            PlayerName = element.Name,
                            PlayerMesh = element.Mesh,
                            ClaimReward = element.ClaimReward,
                            LeaderUID = element.LeaderUID
                        });
                    }
                }
                RestApiHelper.AddUpdateTeamElitePKS(toUpdate);
            }
        }
        public void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                Read Reader = new Read("TeamElitePK.ini");
                if (Reader.Reader())
                {
                    int count = Reader.Count;
                    for (int x = 0; x < count; x++)
                    {
                        ReadLine Readline = new ReadLine(Reader.ReadString(""), '/');
                        byte Tournament = Readline.Read((byte)0);
                        byte Rank = Readline.Read((byte)0);
                        uint UID = Readline.Read((uint)0);
                        string Name = Readline.Read("");
                        uint Mesh = Readline.Read((uint)0);
                        MsgTeamEliteGroup.FighterStats status = new MsgTeamEliteGroup.FighterStats(UID, Name, Mesh, null);
                        status.ClaimReward = Readline.Read((byte)0);
                        status.LeaderUID = Readline.Read((uint)0);
                        if (!BaseFunc.UserIsNormal(Name) || !BaseFunc.UserExists(UID))
                        {
                            continue;
                        }
                        MsgTeamPkTournament.EliteGroups[Tournament].Top8[Rank] = status;
                    }
                }
            }
            else
            {
                var elitePKs = RestApiHelper.GetTeamElitePKS(Core.Interfaces.GameServer.EliteTournamentType.TeamElitePK);
                foreach(var item in elitePKs)
                {
                    byte Tournament = item.Tournament;
                    byte Rank = item.Rank;
                    uint UID = item.PlayerId;
                    string Name = item.PlayerName;
                    uint Mesh = item.PlayerMesh;
                    MsgTeamEliteGroup.FighterStats status = new MsgTeamEliteGroup.FighterStats(UID, Name, Mesh, null);
                    status.ClaimReward = item.ClaimReward;
                    status.LeaderUID = item.LeaderUID;
                    if (!BaseFunc.UserIsNormal(Name) || !BaseFunc.UserExists(UID))
                    {
                        continue;
                    }
                    MsgTeamPkTournament.EliteGroups[Tournament].Top8[Rank] = status;
                }
            }
        }
    }
    public class SkillTeamPKDataManager : IDataManager
    {
        public SkillTeamPKDataManager()
        {
            Init();
        }
        public void Init()
        {
        }
        public void Save()
        {
            if (ServerConfig.DbFromFiles)
            {
                Write writer = new Write("SkillTeamPK.ini");
                for (int x = 0; x < MsgSkillTeamPkTournament.EliteGroups.Length; x++)
                {
                    var Tournament = MsgSkillTeamPkTournament.EliteGroups[x];

                    for (int i = 0; i < Tournament.Top8.Length; i++)
                    {
                        WriteLine writerline = new WriteLine('/');
                        var element = Tournament.Top8[i];
                        writerline.Add(x).Add(i).Add(element.UID).Add(element.Name).Add(element.Mesh).Add(element.ClaimReward).Add(element.LeaderUID);
                        writer.Add(writerline.Close());
                    }

                }
                writer.Execute(Mode.Open);
            }
            else
            {
                List<TeamElitePK> toUpdate = new List<TeamElitePK>();
                for (int x = 0; x < MsgTeamPkTournament.EliteGroups.Length; x++)
                {
                    var Tournament = MsgTeamPkTournament.EliteGroups[x];
                    for (int i = 0; i < Tournament.Top8.Length; i++)
                    {
                        var element = Tournament.Top8[i];
                        toUpdate.Add(new TeamElitePK
                        {
                            Type = Core.Interfaces.GameServer.EliteTournamentType.SkillTeamElitePK,
                            Tournament = (byte)x,
                            Rank = (byte)i,
                            PlayerId = element.UID,
                            PlayerName = element.Name,
                            PlayerMesh = element.Mesh,
                            ClaimReward = element.ClaimReward,
                            LeaderUID = element.LeaderUID
                        });
                    }
                }
                RestApiHelper.AddUpdateTeamElitePKS(toUpdate);
            }
        }
        public void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                Read Reader = new Read("SkillTeamPK.ini");
                if (Reader.Reader())
                {
                    int count = Reader.Count;
                    for (int x = 0; x < count; x++)
                    {
                        ReadLine Readline = new ReadLine(Reader.ReadString(""), '/');
                        byte Tournament = Readline.Read((byte)0);
                        byte Rank = Readline.Read((byte)0);
                        uint UID = Readline.Read((uint)0);
                        string Name = Readline.Read("");
                        uint Mesh = Readline.Read((uint)0);
                        MsgTeamEliteGroup.FighterStats status = new MsgTeamEliteGroup.FighterStats(UID, Name, Mesh, null);
                        status.ClaimReward = Readline.Read((byte)0);
                        status.LeaderUID = Readline.Read((uint)0);
                        if (!BaseFunc.UserIsNormal(Name) || !BaseFunc.UserExists(UID))
                        {
                            continue;
                        }
                        MsgSkillTeamPkTournament.EliteGroups[Tournament].Top8[Rank] = status;
                    }
                }
            }
            else
            {
                var elitePKs = RestApiHelper.GetTeamElitePKS(Core.Interfaces.GameServer.EliteTournamentType.SkillTeamElitePK);
                foreach (var item in elitePKs)
                {
                    byte Tournament = item.Tournament;
                    byte Rank = item.Rank;
                    uint UID = item.PlayerId;
                    string Name = item.PlayerName;
                    uint Mesh = item.PlayerMesh;
                    MsgTeamEliteGroup.FighterStats status = new MsgTeamEliteGroup.FighterStats(UID, Name, Mesh, null);
                    status.ClaimReward = item.ClaimReward;
                    status.LeaderUID = item.LeaderUID;
                    if (!BaseFunc.UserIsNormal(Name) || !BaseFunc.UserExists(UID))
                    {
                        continue;
                    }
                    MsgTeamPkTournament.EliteGroups[Tournament].Top8[Rank] = status;
                }
            }
        }
    }
    public class ClassPKWarManager : IDataManager
    {
        public void Init()
        {
        }

        public void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                Read reader = new Read(MsgClassPKWar.FileName);
                if (reader.Reader())
                {
                    for (int x = 0; x < reader.Count; x++)
                    {
                        ReadLine line = new ReadLine(reader.ReadString(""), '/');
                        byte typ = line.Read((byte)0);
                        byte level = line.Read((byte)0);
                        uint Winner = line.Read((uint)0);
                        Game.MsgServer.MsgUpdate.Flags LastFlag = (Game.MsgServer.MsgUpdate.Flags)line.Read((int)0);
                        MsgSchedules.ClassPkWar.PkWars[typ][level].Winner = Winner;
                        MsgSchedules.ClassPkWar.PkWars[typ][level].LastFlag = LastFlag;
                    }
                }
            }
            else
            {
                List<ClassPKWar> pkWars = RestApiHelper.GetClassPKWars();
                foreach(ClassPKWar pkWar in pkWars.OrderBy(x => x.Type).ThenBy(x => x.Level).ToList())
                {
                    MsgSchedules.ClassPkWar.PkWars[pkWar.Type][pkWar.Level].Winner = pkWar.Winner;
                    MsgSchedules.ClassPkWar.PkWars[pkWar.Type][pkWar.Level].LastFlag = (Game.MsgServer.MsgUpdate.Flags)pkWar.LastFlag;
                }
            }
        }

        public void Save()
        {
            if (ServerConfig.DbFromFiles)
            {

                Write writer = new Write(MsgClassPKWar.FileName);
                foreach (var tournament in MsgSchedules.ClassPkWar.PkWars)
                {
                    foreach (var war in tournament)
                    {
                        WriteLine line = new WriteLine('/');
                        line.Add((byte)war.Typ).Add((byte)war.Level).Add(war.Winner).Add((int)war.LastFlag);
                        writer.Add(line.Close());
                    }
                }
                writer.Execute(Mode.Open);
            }
            else
            {
                List<ClassPKWar> toUpdate = new List<ClassPKWar>();
                foreach (var tournament in MsgSchedules.ClassPkWar.PkWars)
                {
                    foreach (var war in tournament)
                    {
                        toUpdate.Add(new ClassPKWar() { Winner = war.Winner, Type = (byte)war.Typ, Level = (byte)war.Level, LastFlag = (uint)war.LastFlag });
                    }
                }
                RestApiHelper.AddUpdateClassPKWars(toUpdate);
            }
        }
    }
    public class ElitePKDataManager : IDataManager
    {
        public ElitePKDataManager()
        {
            Init();
        }
        public void Init()
        {
        }
        public void Save()
        {
            if (ServerConfig.DbFromFiles)
            {
                if (!ServerConfig.IsInterServer)
                {
                    Write writer = new Write("ElitePk.ini");
                    for (int x = 0; x < MsgEliteTournament.EliteGroups.Length; x++)
                    {
                        var Tournament = MsgEliteTournament.EliteGroups[x];

                        for (int i = 0; i < Tournament.Top8.Length; i++)
                        {
                            WriteLine writerline = new WriteLine('/');
                            var element = Tournament.Top8[i];
                            writerline.Add(x).Add(i).Add(element.UID).Add(element.Name).Add(element.Mesh).Add(element.ClaimReward).Add(element.ServerID);
                            writer.Add(writerline.Close());
                        }

                    }
                    writer.Execute(Mode.Open);
                }
            } else
            {
                if (!ServerConfig.IsInterServer)
                {
                    List<ElitePK> toUpdate = new List<ElitePK>();
                    for (int x = 0; x < MsgEliteTournament.EliteGroups.Length; x++)
                    {
                        var Tournament = MsgEliteTournament.EliteGroups[x];
                        for (int i = 0; i < Tournament.Top8.Length; i++)
                        {
                            var element = Tournament.Top8[i];
                            toUpdate.Add(new ElitePK
                            {
                                Tournament = (byte)x,
                                Rank = (byte)i,
                                PlayerId = element.UID,
                                PlayerName = element.Name,
                                PlayerMesh = element.Mesh,
                                ClaimReward = element.ClaimReward,
                                ServerID = element.ServerID
                            });
                        }
                    }
                    RestApiHelper.AddUpdateElitePKS(toUpdate);
                }
            }
        }
        public void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                if (!ServerConfig.IsInterServer)
                {
                    Read Reader = new Read("ElitePk.ini");
                    if (Reader.Reader())
                    {
                        int count = Reader.Count;
                        for (int x = 0; x < count; x++)
                        {
                            ReadLine Readline = new ReadLine(Reader.ReadString(""), '/');
                            byte Tournament = Readline.Read((byte)0);
                            byte Rank = Readline.Read((byte)0);
                            uint UID = Readline.Read((uint)0);
                            string Name = Readline.Read("");
                            uint Mesh = Readline.Read((uint)0);
                            MsgEliteGroup.FighterStats status = new MsgEliteGroup.FighterStats(UID, Name, Mesh, 0, 0);
                            status.ClaimReward = Readline.Read((byte)0);
                            status.ServerID = Readline.Read((byte)0);
                            if (!BaseFunc.UserIsNormal(Name) || !BaseFunc.UserExists(UID))
                            {
                                continue;
                            }
                            MsgEliteTournament.EliteGroups[Tournament].Top8[Rank] = status;
                        }
                    }
                }
            }
            else
            {
                if (!ServerConfig.IsInterServer)
                {
                    var elitePKs = RestApiHelper.GetElitePKS();
                    foreach (var item in elitePKs)
                    {
                        byte Tournament = item.Tournament;
                        byte Rank = item.Rank;
                        uint UID = item.PlayerId;
                        string Name = item.PlayerName;
                        uint Mesh = item.PlayerMesh;
                        MsgEliteGroup.FighterStats status = new MsgEliteGroup.FighterStats(UID, Name, Mesh, 0, 0);
                        status.ClaimReward = item.ClaimReward;
                        status.ServerID = item.ServerID;
                        if (!BaseFunc.UserIsNormal(Name) || !BaseFunc.UserExists(UID))
                        {
                            continue;
                        }
                        MsgEliteTournament.EliteGroups[Tournament].Top8[Rank] = status;
                    }
                }
            }
        }
    }
    public class CouplesPKManager : IDataManager
    {
        public CouplesPKManager()
        {
            Init();
        }
        public void Init()
        {
        }

        public void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                Read reader = new Read(MsgCouples.FileName);
                if (reader.Reader())
                {
                    for (int x = 0; x < reader.Count; x++)
                    {
                        ReadLine line = new ReadLine(reader.ReadString(""), '/');
                        MsgSchedules.CouplesPKWar.Winner1 = line.Read("NONE");
                        MsgSchedules.CouplesPKWar.Winner2 = line.Read("NONE");
                    }
                }
            } else
            {
                List<Couple> couples = RestApiHelper.GetCouples();
                if (couples.Count > 0)
                {
                    var firstCouple = couples.FirstOrDefault();
                    MsgSchedules.CouplesPKWar.Winner1 = firstCouple.Winner1;
                    MsgSchedules.CouplesPKWar.Winner2 = firstCouple.Winner2;
                }
            }
        }

        public void Save()
        {
            if (ServerConfig.DbFromFiles)
            {
                Write writer = new Write(MsgCouples.FileName);
                WriteLine line = new WriteLine('/');
                line.Add(MsgSchedules.CouplesPKWar.Winner1).Add(MsgSchedules.CouplesPKWar.Winner2);
                writer.Add(line.Close());
                writer.Execute(Mode.Open);
            }
            else
            {
                RestApiHelper.AddUpdateCouples(new List<Couple> { new Couple() { Winner1 = MsgSchedules.CouplesPKWar.Winner1, Winner2 = MsgSchedules.CouplesPKWar.Winner2 } });
            }
        }
    }
    public class CityWarsManager : IDataManager
    {
        public CityWarsManager() {
            Init();
        }

        public void Init()
        {
        }

        public void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                IniFileHelper reader = new(Path.Combine(ServerConfig.DbLocation, "CityWar.ini"));
                MsgSchedules.CityWar.WinnerTC.GuildID = reader.ReadUInt32("Info", "TCID", 0);
                MsgSchedules.CityWar.WinnerTC.Name = reader.ReadString("Info", "TCName", "None");
                MsgSchedules.CityWar.Furnitures[821].Name = reader.ReadString("TCPole", "Name", "None");
                MsgSchedules.CityWar.Furnitures[821].HitPoints = reader.ReadInt32("TCPole", "HitPoints", 0);

                MsgSchedules.CityWar.WinnerPC.GuildID = reader.ReadUInt32("Info", "PCID", 0);
                MsgSchedules.CityWar.WinnerPC.Name = reader.ReadString("Info", "PCName", "None");
                MsgSchedules.CityWar.Furnitures[822].Name = reader.ReadString("PCPole", "Name", "None");
                MsgSchedules.CityWar.Furnitures[822].HitPoints = reader.ReadInt32("PCPole", "HitPoints", 0);

                MsgSchedules.CityWar.WinnerAC.GuildID = reader.ReadUInt32("Info", "ACID", 0);
                MsgSchedules.CityWar.WinnerAC.Name = reader.ReadString("Info", "ACName", "None");
                MsgSchedules.CityWar.Furnitures[823].Name = reader.ReadString("ACPole", "Name", "None");
                MsgSchedules.CityWar.Furnitures[823].HitPoints = reader.ReadInt32("ACPole", "HitPoints", 0);

                MsgSchedules.CityWar.WinnerDC.GuildID = reader.ReadUInt32("Info", "DCID", 0);
                MsgSchedules.CityWar.WinnerDC.Name = reader.ReadString("Info", "DCName", "None");
                MsgSchedules.CityWar.Furnitures[824].Name = reader.ReadString("DCPole", "Name", "None");
                MsgSchedules.CityWar.Furnitures[824].HitPoints = reader.ReadInt32("DCPole", "HitPoints", 0);

                MsgSchedules.CityWar.WinnerBI.GuildID = reader.ReadUInt32("Info", "BIID", 0);
                MsgSchedules.CityWar.WinnerBI.Name = reader.ReadString("Info", "BIName", "None");
                MsgSchedules.CityWar.Furnitures[825].Name = reader.ReadString("BIPole", "Name", "None");
                MsgSchedules.CityWar.Furnitures[825].HitPoints = reader.ReadInt32("BIPole", "HitPoints", 0);
            } else
            {
                List<CityWar> CityWars = RestApiHelper.GetCityWars();
                foreach(CityWar CityWar in CityWars)
                {
                    switch(CityWar.CityWarType)
                    {
                        case Core.Interfaces.GameServer.CityWarType.TC:
                            {
                                MsgSchedules.CityWar.WinnerTC.GuildID = CityWar.GuildId;
                                MsgSchedules.CityWar.WinnerTC.Name = CityWar.GuildName;
                                MsgSchedules.CityWar.Furnitures[821].Name = CityWar.GuildName;
                                MsgSchedules.CityWar.Furnitures[821].HitPoints = (int)CityWar.PoleHitPoints;
                                break;
                            }
                        case Core.Interfaces.GameServer.CityWarType.PC:
                            {
                                MsgSchedules.CityWar.WinnerPC.GuildID = CityWar.GuildId;
                                MsgSchedules.CityWar.WinnerPC.Name = CityWar.GuildName;
                                MsgSchedules.CityWar.Furnitures[822].Name = CityWar.GuildName;
                                MsgSchedules.CityWar.Furnitures[822].HitPoints = (int)CityWar.PoleHitPoints;
                                break;
                            }
                        case Core.Interfaces.GameServer.CityWarType.AC:
                            {
                                MsgSchedules.CityWar.WinnerAC.GuildID = CityWar.GuildId;
                                MsgSchedules.CityWar.WinnerAC.Name = CityWar.GuildName;
                                MsgSchedules.CityWar.Furnitures[823].Name = CityWar.GuildName;
                                MsgSchedules.CityWar.Furnitures[823].HitPoints = (int)CityWar.PoleHitPoints;
                                break;
                            }
                        case Core.Interfaces.GameServer.CityWarType.DC:
                            {
                                MsgSchedules.CityWar.WinnerDC.GuildID = CityWar.GuildId;
                                MsgSchedules.CityWar.WinnerDC.Name = CityWar.GuildName;
                                MsgSchedules.CityWar.Furnitures[824].Name = CityWar.GuildName;
                                MsgSchedules.CityWar.Furnitures[824].HitPoints = (int)CityWar.PoleHitPoints;
                                break;
                            }
                        case Core.Interfaces.GameServer.CityWarType.BI:
                            {
                                MsgSchedules.CityWar.WinnerBI.GuildID = CityWar.GuildId;
                                MsgSchedules.CityWar.WinnerBI.Name = CityWar.GuildName;
                                MsgSchedules.CityWar.Furnitures[825].Name = CityWar.GuildName;
                                MsgSchedules.CityWar.Furnitures[825].HitPoints = (int)CityWar.PoleHitPoints;
                                break;
                            }
                    }
                }
            }
        }

        public void Save()
        {

            if (ServerConfig.DbFromFiles)
            {
                IniFileHelper write = new(Path.Combine(ServerConfig.DbLocation, "CityWar.ini"));
                if (MsgSchedules.CityWar.Proces == ProcesType.Dead)
                {
                    write.Write<uint>("Info", "TCID", MsgSchedules.CityWar.WinnerTC.GuildID);
                    write.WriteString("Info", "TCName", MsgSchedules.CityWar.WinnerTC.Name);
                    write.Write<uint>("Info", "PCID", MsgSchedules.CityWar.WinnerPC.GuildID);
                    write.WriteString("Info", "PCName", MsgSchedules.CityWar.WinnerPC.Name);
                    write.Write<uint>("Info", "ACID", MsgSchedules.CityWar.WinnerAC.GuildID);
                    write.WriteString("Info", "ACName", MsgSchedules.CityWar.WinnerAC.Name);
                    write.Write<uint>("Info", "DCID", MsgSchedules.CityWar.WinnerDC.GuildID);
                    write.WriteString("Info", "DCName", MsgSchedules.CityWar.WinnerDC.Name);
                    write.Write<uint>("Info", "BIID", MsgSchedules.CityWar.WinnerBI.GuildID);
                    write.WriteString("Info", "BIName", MsgSchedules.CityWar.WinnerBI.Name);
                    write.WriteString("TCPole", "Name", MsgSchedules.CityWar.WinnerTC.Name);
                    write.Write<int>("TCPole", "HitPoints", MsgSchedules.CityWar.Furnitures[821].HitPoints);
                    write.WriteString("PCPole", "Name", MsgSchedules.CityWar.WinnerPC.Name);
                    write.Write<int>("PCPole", "HitPoints", MsgSchedules.CityWar.Furnitures[822].HitPoints);
                    write.WriteString("ACPole", "Name", MsgSchedules.CityWar.WinnerAC.Name);
                    write.Write<int>("ACPole", "HitPoints", MsgSchedules.CityWar.Furnitures[823].HitPoints);
                    write.WriteString("DCPole", "Name", MsgSchedules.CityWar.WinnerDC.Name);
                    write.Write<int>("DCPole", "HitPoints", MsgSchedules.CityWar.Furnitures[824].HitPoints);
                    write.WriteString("BIPole", "Name", MsgSchedules.CityWar.WinnerBI.Name);
                    write.Write<int>("BIPole", "HitPoints", MsgSchedules.CityWar.Furnitures[825].HitPoints);
                }
            }
            else
            {
                List<CityWar> CityWars =
                [
                    new CityWar() {
                        CityWarType = Core.Interfaces.GameServer.CityWarType.TC,
                        GuildId = MsgSchedules.CityWar.WinnerTC.GuildID,
                        GuildName = MsgSchedules.CityWar.WinnerTC.Name,
                        PoleHitPoints = (uint)MsgSchedules.CityWar.Furnitures[821].HitPoints
                    },
                    new CityWar() {
                        CityWarType = Core.Interfaces.GameServer.CityWarType.PC,
                        GuildId = MsgSchedules.CityWar.WinnerPC.GuildID,
                        GuildName = MsgSchedules.CityWar.WinnerPC.Name,
                        PoleHitPoints = (uint)MsgSchedules.CityWar.Furnitures[822].HitPoints
                    },
                    new CityWar() {
                        CityWarType = Core.Interfaces.GameServer.CityWarType.AC,
                        GuildId = MsgSchedules.CityWar.WinnerAC.GuildID,
                        GuildName = MsgSchedules.CityWar.WinnerAC.Name,
                        PoleHitPoints = (uint)MsgSchedules.CityWar.Furnitures[823].HitPoints
                    },
                    new CityWar() {
                        CityWarType = Core.Interfaces.GameServer.CityWarType.DC,
                        GuildId = MsgSchedules.CityWar.WinnerDC.GuildID,
                        GuildName = MsgSchedules.CityWar.WinnerDC.Name,
                        PoleHitPoints = (uint)MsgSchedules.CityWar.Furnitures[824].HitPoints
                    },
                    new CityWar() {
                        CityWarType = Core.Interfaces.GameServer.CityWarType.BI,
                        GuildId = MsgSchedules.CityWar.WinnerBI.GuildID,
                        GuildName = MsgSchedules.CityWar.WinnerBI.Name,
                        PoleHitPoints = (uint)MsgSchedules.CityWar.Furnitures[825].HitPoints
                    },
                ];
                RestApiHelper.AddUpdateCityWars(CityWars);
            }
        }
    }
    public class GuildWarsManager
    {
        public GuildWarsManager()
        {
            Init();
        }
        public void Init()
        {

        }
        public void Load(GuildWarType Type)
        {
            if (ServerConfig.DbFromFiles)
            {
                if (Type == GuildWarType.EliteGuildWar)
                {
                    IniFileHelper reader = new(Path.Combine(ServerConfig.DbLocation, "Elite.ini"));
                    MsgSchedules.EliteGuildWar.Winner.GuildID = reader.ReadUInt32("Info", "ID", 0);
                    MsgSchedules.EliteGuildWar.Winner.Name = reader.ReadString("Info", "Name", "None");
                    MsgSchedules.EliteGuildWar.Winner.LeaderReward = reader.ReadInt32("Info", "LeaderReward", 0);
                    MsgSchedules.EliteGuildWar.Winner.DeputiLeaderReward = reader.ReadInt32("Info", "DeputiLeaderReward", 0);
                    MsgSchedules.EliteGuildWar.RewardLeader.Add(reader.ReadUInt32("Info", "LeaderTop0", 0));
                    for (int x = 0; x < 8; x++)
                    {
                        MsgSchedules.EliteGuildWar.RewardDeputiLeader.Add(reader.ReadUInt32("Info", "DeputiTop" + x.ToString() + "", 0));
                    }
                    MsgSchedules.EliteGuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].Name = reader.ReadString("Pole", "Name", "None");
                    MsgSchedules.EliteGuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints = reader.ReadInt32("Pole", "HitPoints", 0);
                    MsgSchedules.EliteGuildWar.GuildWarMap = Pool.ServerMaps[2071];
                }
                else if (Type == GuildWarType.GuildWar)
                {
                    IniFileHelper reader = new IniFileHelper(Path.Combine(ServerConfig.DbLocation, "GuildWarInfo.ini"));
                    MsgSchedules.GuildWar.Winner.GuildID = reader.ReadUInt32("Info", "ID", 0);
                    MsgSchedules.GuildWar.Winner.Name = reader.ReadString("Info", "Name", "None");
                    MsgSchedules.GuildWar.Winner.LeaderReward = reader.ReadInt32("Info", "LeaderReward", 0);
                    MsgSchedules.GuildWar.Winner.DeputiLeaderReward = reader.ReadInt32("Info", "DeputiLeaderReward", 0);

                    MsgSchedules.GuildWar.RewardLeader.Add(reader.ReadUInt32("Info", "LeaderTop0", 0));
                    for (int x = 0; x < 8; x++)
                    {
                        MsgSchedules.GuildWar.RewardDeputiLeader.Add(reader.ReadUInt32("Info", "DeputiTop" + x.ToString() + "", 0));
                    }

                    MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].Name = reader.ReadString("Pole", "Name", "None");
                    MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints = reader.ReadInt32("Pole", "HitPoints", 0);

                    for (int x = 0; x < 4; x++)
                    {
                        GuildConductor conductor = new GuildConductor();
                        conductor.Load(reader.ReadString("Condutors", "GuildConductor" + (x + 1).ToString() + "", ""), (Game.MsgNpc.NpcID)(101614 + x * 2));
                        MsgSchedules.GuildWar.GuildConductors.Add((Game.MsgNpc.NpcID)(101614 + x * 2), conductor);
                        if (conductor.Npc.Map != 0)
                        {
                            if (Pool.ServerMaps.ContainsKey(conductor.Npc.Map))
                                Pool.ServerMaps[conductor.Npc.Map].AddNpc(conductor.Npc);
                        }
                    }
                    MsgSchedules.GuildWar.GuildWarMap = Pool.ServerMaps[1038];
                }
            } else
            {
                if (Type == GuildWarType.EliteGuildWar) {
                    List<GuildWar> guildWars = RestApiHelper.GetGuildWars(GuildWarType.EliteGuildWar);
                    if (guildWars.Count == 0)
                        return;
                    GuildWar eliteGW = guildWars.FirstOrDefault();
                    if (eliteGW != null)
                    {
                        MsgSchedules.EliteGuildWar.Winner.GuildID = eliteGW.WinnerGuildID;
                        MsgSchedules.EliteGuildWar.Winner.Name = eliteGW.WinnerName;
                        MsgSchedules.EliteGuildWar.Winner.LeaderReward = eliteGW.LeaderReward;
                        MsgSchedules.EliteGuildWar.Winner.DeputiLeaderReward = eliteGW.DeputiLeaderReward;
                        MsgSchedules.EliteGuildWar.RewardLeader = eliteGW.RewardLeaders;
                        MsgSchedules.EliteGuildWar.RewardDeputiLeader = eliteGW.RewardDeputies;
                        MsgSchedules.EliteGuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].Name = eliteGW.WinnerName;
                        MsgSchedules.EliteGuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints = eliteGW.PoleHitPoints;
                        MsgSchedules.EliteGuildWar.GuildWarMap = Pool.ServerMaps[2071];
                    }
                }
                else if (Type == GuildWarType.GuildWar)
                {
                    List<GuildWar> guildWars = RestApiHelper.GetGuildWars(GuildWarType.GuildWar);
                    if (guildWars.Count == 0)
                        return;
                    GuildWar GW = guildWars.FirstOrDefault();
                    if (GW != null)
                    {
                        MsgSchedules.GuildWar.Winner.GuildID = GW.WinnerGuildID;
                        MsgSchedules.GuildWar.Winner.Name = GW.WinnerName;
                        MsgSchedules.GuildWar.Winner.LeaderReward = GW.LeaderReward;
                        MsgSchedules.GuildWar.Winner.DeputiLeaderReward = GW.DeputiLeaderReward;
                        MsgSchedules.GuildWar.RewardLeader = GW.RewardLeaders;
                        MsgSchedules.GuildWar.RewardDeputiLeader = GW.RewardDeputies;
                        MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].Name = GW.WinnerName;
                        MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints = GW.PoleHitPoints;
                        for (int x = 0; x < 4; x++)
                        {
                            GuildConductor conductor = new GuildConductor();
                            if (x == 0)
                            {
                                conductor.Load(GW.GuildConductor1, (Game.MsgNpc.NpcID)(101614 + x * 2));
                            } else if (x == 1)
                            {
                                conductor.Load(GW.GuildConductor2, (Game.MsgNpc.NpcID)(101614 + x * 2));
                            } else if (x == 2)
                            {
                                conductor.Load(GW.GuildConductor3, (Game.MsgNpc.NpcID)(101614 + x * 2));
                            } else if (x == 3)
                            {
                                conductor.Load(GW.GuildConductor4, (Game.MsgNpc.NpcID)(101614 + x * 2));
                            }
                            MsgSchedules.GuildWar.GuildConductors.Add((Game.MsgNpc.NpcID)(101614 + x * 2), conductor);
                        }
                        MsgSchedules.GuildWar.GuildWarMap = Pool.ServerMaps[1038];
                    }
                }
            }
        }
        public void Save(GuildWarType Type)
        {
            if (ServerConfig.DbFromFiles)
            {
                if (Type == GuildWarType.EliteGuildWar)
                {
                    IniFileHelper write = new IniFileHelper(Path.Combine(ServerConfig.DbLocation, "Elite.ini"));
                    if (MsgSchedules.EliteGuildWar.Proces == ProcesType.Dead)
                    {
                        write.Write<uint>("Info", "ID", MsgSchedules.EliteGuildWar.Winner.GuildID);
                        write.WriteString("Info", "Name", MsgSchedules.EliteGuildWar.Winner.Name);
                        write.Write<int>("Info", "LeaderReward", MsgSchedules.EliteGuildWar.Winner.LeaderReward);
                        write.Write<int>("Info", "DeputiLeaderReward", MsgSchedules.EliteGuildWar.Winner.DeputiLeaderReward);

                        for (int x = 0; x < MsgSchedules.EliteGuildWar.RewardLeader.Count; x++)
                            write.Write<uint>("Info", "LeaderTop" + x.ToString() + "", MsgSchedules.EliteGuildWar.RewardLeader[x]);
                        for (int x = 0; x < 8; x++)
                        {
                            if (x >= MsgSchedules.EliteGuildWar.RewardDeputiLeader.Count)
                                break;
                            write.Write<uint>("Info", "DeputiTop" + x.ToString() + "", MsgSchedules.EliteGuildWar.RewardDeputiLeader[x]);
                        }
                        write.WriteString("Pole", "Name", MsgSchedules.EliteGuildWar.Winner.Name);
                        write.Write<int>("Pole", "HitPoints", MsgSchedules.EliteGuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints);
                    }
                } else if (Type == GuildWarType.GuildWar)
                {
                    IniFileHelper write = new IniFileHelper(Path.Combine(ServerConfig.DbLocation, "GuildWarInfo.ini"));
                    if (MsgSchedules.GuildWar.Proces == ProcesType.Dead)
                    {
                        write.Write<uint>("Info", "ID", MsgSchedules.GuildWar.Winner.GuildID);
                        write.WriteString("Info", "Name", MsgSchedules.GuildWar.Winner.Name);
                        write.Write<int>("Info", "LeaderReward", MsgSchedules.GuildWar.Winner.LeaderReward);
                        write.Write<int>("Info", "DeputiLeaderReward", MsgSchedules.GuildWar.Winner.DeputiLeaderReward);

                        for (int x = 0; x < MsgSchedules.GuildWar.RewardLeader.Count; x++)
                            write.Write<uint>("Info", "LeaderTop" + x.ToString() + "", MsgSchedules.GuildWar.RewardLeader[x]);
                        for (int x = 0; x < 8; x++)
                        {
                            if (x >= MsgSchedules.GuildWar.RewardDeputiLeader.Count)
                                break;
                            write.Write<uint>("Info", "DeputiTop" + x.ToString() + "", MsgSchedules.GuildWar.RewardDeputiLeader[x]);
                        }
                        write.WriteString("Pole", "Name", MsgSchedules.GuildWar.Winner.Name);
                        write.Write<int>("Pole", "HitPoints", MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints);
                    }

                    write.WriteString("Condutors", "GuildConductor1", MsgSchedules.GuildWar.GuildConductors[Game.MsgNpc.NpcID.TeleGuild1].ToString());
                    write.WriteString("Condutors", "GuildConductor2", MsgSchedules.GuildWar.GuildConductors[Game.MsgNpc.NpcID.TeleGuild2].ToString());
                    write.WriteString("Condutors", "GuildConductor3", MsgSchedules.GuildWar.GuildConductors[Game.MsgNpc.NpcID.TeleGuild3].ToString());
                    write.WriteString("Condutors", "GuildConductor4", MsgSchedules.GuildWar.GuildConductors[Game.MsgNpc.NpcID.TeleGuild4].ToString());
                }
            }
            else
            {
                List<GuildWar> guildWars = new List<GuildWar>();
                if (Type == GuildWarType.EliteGuildWar)
                {
                    guildWars.Add(new GuildWar() {
                        Type = GuildWarType.EliteGuildWar,
                        WinnerGuildID = MsgSchedules.EliteGuildWar.Winner.GuildID,
                        WinnerName = MsgSchedules.EliteGuildWar.Winner.Name,
                        LeaderReward = MsgSchedules.EliteGuildWar.Winner.LeaderReward,
                        DeputiLeaderReward = MsgSchedules.EliteGuildWar.Winner.DeputiLeaderReward,
                        RewardLeaders = MsgSchedules.EliteGuildWar.RewardLeader,
                        RewardDeputies = MsgSchedules.EliteGuildWar.RewardDeputiLeader,
                        PoleHitPoints = MsgSchedules.EliteGuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints,
                    });
                } else if (Type == GuildWarType.GuildWar)
                {
                    guildWars.Add(new GuildWar()
                    {
                        Type = GuildWarType.GuildWar,
                        WinnerGuildID = MsgSchedules.GuildWar.Winner.GuildID,
                        WinnerName = MsgSchedules.GuildWar.Winner.Name,
                        LeaderReward = MsgSchedules.GuildWar.Winner.LeaderReward,
                        DeputiLeaderReward = MsgSchedules.GuildWar.Winner.DeputiLeaderReward,
                        RewardLeaders = MsgSchedules.GuildWar.RewardLeader,
                        RewardDeputies = MsgSchedules.GuildWar.RewardDeputiLeader,
                        PoleHitPoints = MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints,
                        GuildConductor1 = MsgSchedules.GuildWar.GuildConductors[Game.MsgNpc.NpcID.TeleGuild1].ToString(),
                        GuildConductor2 = MsgSchedules.GuildWar.GuildConductors[Game.MsgNpc.NpcID.TeleGuild2].ToString(),
                        GuildConductor3 = MsgSchedules.GuildWar.GuildConductors[Game.MsgNpc.NpcID.TeleGuild3].ToString(),
                        GuildConductor4 = MsgSchedules.GuildWar.GuildConductors[Game.MsgNpc.NpcID.TeleGuild4].ToString(),
                    });
                }
                RestApiHelper.AddUpdateGuildWars(guildWars);
            }
        }
    }
    public class VIPSharesManager : IDataManager
    {
        public VIPSharesManager() {
            Init();
        }

        public void Init()
        {
        }

        public void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                using (Read Reader = new Read("Share.txt"))
                {
                    if (Reader.Reader())
                    {
                        uint count = (uint)Reader.Count;
                        for (uint i = 0; i < count; i++)
                        {
                            ReadLine readline = new ReadLine(Reader.ReadString(""), '/');
                            Database.ShareVIP.Client x = new();
                            x.UID = readline.Read((uint)0);
                            x.ShareUID = readline.Read((uint)0);
                            x.ShareName = readline.Read("");
                            x.ShareLevel = readline.Read((byte)0);
                            x.ShareEnds = DateTime.FromBinary(readline.Read((long)0));
                            Database.ShareVIP.SharedPoll.Add(x);
                        }
                    }
                }
            } else
            {
                List<VIPShare> VIPShares = RestApiHelper.GetVIPShares();
                foreach (VIPShare share in VIPShares)
                {
                    Database.ShareVIP.Client x = new()
                    {
                        UID = share.PlayerUID,
                        ShareUID = share.ShareUID,
                        ShareName = share.ShareName,
                        ShareLevel = share.ShareLevel,
                        ShareEnds = share.ShareExpiration
                    };
                    Database.ShareVIP.SharedPoll.Add(x);
                }
            }
        }

        public void Save()
        {
            if (ServerConfig.DbFromFiles)
            {
                using (Write writer = new Write("Share.txt"))
                {
                    foreach (var x in Database.ShareVIP.SharedPoll.GetValues())
                    {
                        writer.Add(x.ToString());
                    }
                    writer.Execute(Mode.Open);
                }
            }
            else
            {
                List<VIPShare> VIPShares = new List<VIPShare>();
                foreach (var x in Database.ShareVIP.SharedPoll.GetValues())
                {
                    VIPShares.Add(new VIPShare() {
                        PlayerUID = x.UID,
                        ShareUID = x.ShareUID,
                        ShareLevel = x.ShareLevel,
                        ShareName = x.ShareName,
                        ShareExpiration = x.ShareEnds
                    });
                }
                RestApiHelper.AddUpdateVIPShares(VIPShares);
            }
        }
    }
    public class KOBoardRanksManager : IDataManager
    {
        private string Filename = "KOBoardRanks.txt";
        public KOBoardRanksManager()
        {
            Init();
        }
        public void Init()
        {
        }

        public void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                using (Read reader = new(Filename))
                {
                    if (reader.Reader())
                    {
                        int count = reader.Count;
                        for (int x = 0; x < count; x++)
                        {
                            ReadLine line = new(reader.ReadString("/"), '/');
                            Entry item = new()
                            {
                                UID = line.Read((uint)0),
                                Name = line.Read(""),
                                Points = line.Read((uint)0)
                            };
                            if (!BaseFunc.UserIsNormal(item.Name) || !BaseFunc.UserExists(item.UID))
                            {
                                continue;
                            }
                            KOBoardRanking.AddItem(item, false);
                        }
                    }
                }
            } else
            {
                List<KOBoardRank> KOBoardRanks = RestApiHelper.GetKOBoardRanks();
                foreach (KOBoardRank item in KOBoardRanks)
                {
                    Entry entry = new()
                    {
                        UID = item.PlayerUID,
                        Name = item.PlayerName,
                        Points = item.Points
                    };
                    if (!BaseFunc.UserIsNormal(entry.Name) || !BaseFunc.UserExists(entry.UID))
                    {
                        continue;
                    }
                    KOBoardRanking.AddItem(entry, false);
                }
            }
        }

        public void Save()
        {
            if (ServerConfig.DbFromFiles)
            {
                using (Write writer = new(Filename))
                {
                    foreach (var obj in KOBoardRanking.Items.GetValues())
                    {
                        WriteLine line = new('/');
                        line.Add(obj.UID).Add(obj.Name).Add(obj.Points);

                        writer.Add(line.Close());
                    }
                    writer.Execute(Mode.Open);
                }
            } else
            {
                List<KOBoardRank> KOBoardRanksToSave = new List<KOBoardRank>();
                foreach (var obj in KOBoardRanking.Items.GetValues())
                {
                    KOBoardRanksToSave.Add(new KOBoardRank() { PlayerName = obj.Name, PlayerUID = obj.UID, Points = obj.Points });
                }
                RestApiHelper.AddUpdateKOBoardRanks(KOBoardRanksToSave);
            }
        }
    }
    public class BanUIDSManager : IDataManager
    {
        private string Filename = "BanUID.txt";
        public BanUIDSManager()
        {
            Init();
        }
        public void Init()
        {
        }

        public void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                using (Read Reader = new Read(Filename))
                {

                    if (Reader.Reader())
                    {
                        uint count = (uint)Reader.Count;
                        for (uint x = 0; x < count; x++)
                        {
                            ReadLine readline = new ReadLine(Reader.ReadString(""), '/');
                            Database.SystemBannedAccountClient msg = new Database.SystemBannedAccountClient();
                            msg.UID = readline.Read((uint)0);
                            msg.Hours = readline.Read((uint)0);
                            msg.StartBan = readline.Read((long)0);
                            msg.Name = readline.Read("");
                            msg.Reason = readline.Read("");
                            Database.SystemBannedAccount.BannedPoll.Add(msg.UID, msg);
                        }
                    }
                }
            } else
            {
                List<BanUID> bans = RestApiHelper.GetBanUIDs();
                foreach (BanUID ban in bans)
                {
                    Database.SystemBannedAccountClient msg = new Database.SystemBannedAccountClient
                    {
                        UID = ban.PlayerUID,
                        Hours = ban.Hours,
                        StartBan = ban.StartBan,
                        Name = ban.Name,
                        Reason = ban.Reason
                    };
                    Database.SystemBannedAccount.BannedPoll.Add(msg.UID, msg);
                }
            }
        }

        public void Save()
        {
            if (ServerConfig.DbFromFiles)
            {
                using (Write writer = new Write(Filename))
                {
                    foreach (var ban in Database.SystemBannedAccount.BannedPoll.Values)
                    {
                        writer.Add(ban.ToString());
                    }
                    writer.Execute(Mode.Open);
                }
            } else
            {
                List<BanUID> bans = new List<BanUID>();
                foreach (var ban in Database.SystemBannedAccount.BannedPoll.Values)
                {
                    bans.Add(new BanUID() { PlayerUID = ban.UID, Name = ban.Name, Reason = ban.Reason, StartBan = ban.StartBan, Hours = ban.Hours });
                }
                RestApiHelper.AddUpdateBanUIDs(bans);
            }
        }

        public void Remove(uint UID)
        {
            if (ServerConfig.DbFromFiles)
            {
                // no need in that case
            } else
            {
                RestApiHelper.RemoveBanUID(UID);
            }
        }
    }
    public class BanIPSManager : IDataManager
    {
        public BanIPSManager() {
            Init();
        }
        public void Init()
        {
        }

        public void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                using (Read Reader = new Read("BanIp.txt"))
                {

                    if (Reader.Reader())
                    {
                        uint count = (uint)Reader.Count;
                        for (uint x = 0; x < count; x++)
                        {
                            ReadLine readline = new ReadLine(Reader.ReadString(""), '/');
                            Database.SystemBanned.Client msg = new Database.SystemBanned.Client();
                            msg.IP = readline.Read((string)"");
                            msg.Hours = readline.Read((uint)0);
                            msg.StartBan = readline.Read((long)0);
                            Database.SystemBanned.BannedPoll.Add(msg.IP, msg);
                        }
                    }
                }
            } else
            {
                List<BanIP> banIPs = RestApiHelper.GetBanIPs();
                foreach(BanIP banIP in banIPs)
                {
                    Database.SystemBanned.Client msg = new()
                    {
                        IP = banIP.IP,
                        Hours = banIP.Hours,
                        StartBan = banIP.StartBan
                    };
                    Database.SystemBanned.BannedPoll.Add(msg.IP, msg);
                }
            }
        }

        public void Save()
        {
            if (ServerConfig.DbFromFiles)
            {
                using (Write writer = new Write("BanIp.txt"))
                {
                    foreach (var ban in Database.SystemBanned.BannedPoll.Values)
                    {
                        writer.Add(ban.ToString());
                    }
                    writer.Execute(Mode.Open);
                }
            }
            else
            {
                List<BanIP> banIPs = [];
                foreach (var ban in Database.SystemBanned.BannedPoll.Values)
                {
                    banIPs.Add(new BanIP() { IP = ban.IP, Hours = ban.Hours, StartBan = ban.StartBan });
                }
                RestApiHelper.AddUpdateBanIPs(banIPs);
            }
        }
    }
    public class AssociatesManager : IDataManager
    {
        private string Filename = "Associate.txt";
        public AssociatesManager()
        {
            Init();
        }
        public void Init()
        {
        }

        public void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                try
                {
                    using (Read r = new Read(Filename))
                    {
                        if (r.Reader())
                        {
                            int count = r.Count;
                            for (uint x = 0; x < count; x++)
                            {
                                string[] data = r.ReadString("").Split('/');
                                uint UID = uint.Parse(data[0]);
                                byte Mod = byte.Parse(data[1]);
                                uint MentorExpBalls = uint.Parse(data[2]);
                                uint MentorBless = uint.Parse(data[3]);
                                uint MentorStone = uint.Parse(data[4]);
                                Member membru = new()
                                {
                                    UID = uint.Parse(data[8]),
                                    Timer = ulong.Parse(data[9]),
                                    ExpBalls = uint.Parse(data[10]),
                                    Stone = uint.Parse(data[11]),
                                    Blessing = uint.Parse(data[12]),
                                    Name = data[13],
                                    KillsCount = ushort.Parse(data[14]),
                                    BattlePower = ushort.Parse(data[15]),
                                    Map = data[16]
                                };
                                if (Associates.ContainsKey(UID))
                                {
                                    if (Associates[UID].Associat.ContainsKey(Mod))
                                        Associates[UID].Associat[Mod].TryAdd(membru.UID, membru);
                                    else
                                    {
                                        Associates[UID].Associat.TryAdd(Mod, new ConcurrentDictionary<uint, Member>());
                                        Associates[UID].Associat[Mod].TryAdd(membru.UID, membru);
                                    }
                                }
                                else
                                {
                                    MyAsociats assoc = new MyAsociats(UID);
                                    assoc.MyUID = UID;
                                    assoc.Mentor_ExpBalls = MentorExpBalls;
                                    assoc.Mentor_Blessing = MentorBless;
                                    assoc.Mentor_Stones = MentorStone;
                                    assoc.Associat.TryAdd(Mod, new ConcurrentDictionary<uint, Member>());
                                    assoc.Associat[Mod].TryAdd(membru.UID, membru);
                                    Associates.TryAdd(UID, assoc);

                                }
                            }
                        }
                    }
                    GC.Collect();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            } else
            {
                List<Associate> associates = RestApiHelper.GetAssociates();
                foreach (Associate associate in associates)
                {
                    uint MentorExpBalls = associate.MentorExpballs;
                    uint MentorBless = associate.MentorBlessing;
                    uint MentorStone = associate.MentorStones;
                    if (!Associates.ContainsKey(associate.PlayerUID))
                    {
                        MyAsociats assoc = new MyAsociats(associate.PlayerUID);
                        assoc.MyUID = associate.PlayerUID;
                        assoc.Mentor_ExpBalls = MentorExpBalls;
                        assoc.Mentor_Blessing = MentorBless;
                        assoc.Mentor_Stones = MentorStone;
                        if (associate.Members == null)
                        {
                            associate.Members = [];
                        }
                        var members = associate.Members.ToList();
                        foreach (var member in members)
                        {
                            if (!assoc.Associat.ContainsKey((byte)member.Type))
                            {
                                assoc.Associat.TryAdd((byte)member.Type, new ConcurrentDictionary<uint, Member>());
                            }
                            assoc.Associat[(byte)member.Type].TryAdd(member.UID, new Member() { UID = member.UID, Name = member.Name, Map = member.MapName, Stone = member.Stone, Blessing = member.Blessing, ExpBalls = member.ExpBalls, BattlePower = member.BattlePower, KillsCount = member.KillsCount, Timer = member.Timer });
                        }
                        Associates.TryAdd(associate.PlayerUID, assoc);
                    }
                }
            }
        }

        public void Save()
        {
            if (ServerConfig.DbFromFiles)
            {
                try
                {
                    using (Write _wr = new(Filename))
                    {
                        foreach (var x in Associates)
                        {
                            foreach (var member in x.Value.ToStringMember())
                            {
                                _wr.Add(member);
                            }
                        }
                        _wr.Execute(Mode.Open);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            } else
            {
                List<Associate> existingAssociates = RestApiHelper.GetAssociates();
                // Recover all the data from api and update only the data of members
                foreach (var xAssociate in Associates)
                {
                    foreach (var memberKV in xAssociate.Value.Associat)
                    {
                        byte type = memberKV.Key; // Key = Type (Friend, Enemy etc)
                        uint PlayerUID = xAssociate.Value.MyUID;
                        Associate assocDb = existingAssociates.FirstOrDefault(x => x.PlayerUID == PlayerUID);
                        if (assocDb != null)
                        {
                            if (assocDb.Members == null)
                            {
                                assocDb.Members = [];
                            }
                            foreach (var member in memberKV.Value.Values)
                            {
                                var existingMember = assocDb.Members.FirstOrDefault(x => x.UID == member.UID && x.Type == (AssociateMemberType)type);
                                if (existingMember != null)
                                {
                                    existingMember.Name = member.Name;
                                    existingMember.MapName = member.Map;
                                    existingMember.Stone = member.Stone;
                                    existingMember.Blessing = member.Blessing;
                                    existingMember.ExpBalls = member.ExpBalls;
                                    existingMember.BattlePower = member.BattlePower;
                                    existingMember.KillsCount = member.KillsCount;
                                    existingMember.Timer = member.Timer;
                                } else
                                {
                                    assocDb.Members.Add(new AssociateMember
                                    {
                                        UID = member.UID,
                                        Type = (AssociateMemberType)type,
                                        Name = member.Name,
                                        MapName = member.Map,
                                        Stone = member.Stone,
                                        Blessing = member.Blessing,
                                        ExpBalls = member.ExpBalls,
                                        BattlePower = member.BattlePower,
                                        KillsCount = member.KillsCount,
                                        Timer = member.Timer
                                    });
                                }
                            }
                        } else
                        {
                            Associate newAssociate = new Associate
                            {
                                PlayerUID = xAssociate.Value.MyUID,
                                MentorExpballs = xAssociate.Value.Mentor_ExpBalls,
                                MentorBlessing = xAssociate.Value.Mentor_Blessing,
                                MentorStones = xAssociate.Value.Mentor_Stones,
                                Members = new List<AssociateMember>(),
                            };
                            foreach (var member in memberKV.Value.Values)
                            {
                                newAssociate.Members.Add(new AssociateMember
                                {
                                    UID = member.UID,
                                    Type = (AssociateMemberType)type,
                                    Name = member.Name,
                                    MapName = member.Map,
                                    Stone = member.Stone,
                                    Blessing = member.Blessing,
                                    ExpBalls = member.ExpBalls,
                                    BattlePower = member.BattlePower,
                                    KillsCount = member.KillsCount,
                                    Timer = member.Timer
                                });
                            }
                            existingAssociates.Add(newAssociate);
                        }
                    }
                }
                RestApiHelper.AddUpdateAssociates(existingAssociates);
            }
        }

        public void Remove(byte Mode, uint UID)
        {
            RestApiHelper.RemoveAssociateMember((AssociateMemberType)Mode, UID);
        }
    }
    public class ClanWarsManager
    {
        public ClanWarsManager()
        {
            Init();
        }

        public void Init()
        {
        }

        public void Load(MsgClanWar.CityWar ClanWarCityInstance)
        {
            if (ServerConfig.DbFromFiles)
            {
                Read reader = new Read(Path.Combine("ClanWar", $"{ClanWarCityInstance.Type}.txt"));

                if (reader.Reader())
                {
                    for (int x = 0; x < reader.Count; x++)
                    {
                        ReadLine line = new ReadLine(reader.ReadString(""), '/');
                        ClanWarCityInstance.Winner.ClainID = line.Read((uint)0);
                        ClanWarCityInstance.Pole.Name = ClanWarCityInstance.Winner.Name = line.Read("None");
                        ClanWarCityInstance.Winner.ClaimReward = line.Read((uint)0);
                        ClanWarCityInstance.Winner.NextReward = line.Read((uint)0);
                        ClanWarCityInstance.Winner.OccupationDays = line.Read((uint)0);
                        ClanWarCityInstance.Winner.Reward = line.Read((uint)0);
                        ClanWarCityInstance.BestWinner.ClainID = line.Read((uint)0);
                        ClanWarCityInstance.BestWinner.Name = line.Read("None");
                        ClanWarCityInstance.BestWinner.ClaimReward = line.Read((uint)0);
                        ClanWarCityInstance.BestWinner.NextReward = line.Read((uint)0);
                        ClanWarCityInstance.BestWinner.OccupationDays = line.Read((uint)0);
                        ClanWarCityInstance.BestWinner.Reward = line.Read((uint)0);
                    }
                }
            }
            else
            {
                List<ClanWar> ClanWars = RestApiHelper.GetClanWars();
                if (ClanWars.Count == 0)
                    return;
                ClanWars.Where(x => x.Type == ClanWarCityInstance.Type).ToList().ForEach(war =>
                {
                    ClanWarCityInstance.Winner.ClainID = war.BestWinnerClanId;
                    ClanWarCityInstance.Pole.Name = ClanWarCityInstance.Winner.Name = war.PoleName;
                    ClanWarCityInstance.Winner.ClaimReward = war.WinnerClaimReward;
                    ClanWarCityInstance.Winner.NextReward = war.WinnerNextReward;
                    ClanWarCityInstance.Winner.OccupationDays = war.WinnerOccupationDays;
                    ClanWarCityInstance.Winner.Reward = war.WinnerReward;
                    ClanWarCityInstance.BestWinner.ClainID = war.BestWinnerClanId;
                    ClanWarCityInstance.BestWinner.Name = war.BestWinnerName;
                    ClanWarCityInstance.BestWinner.ClaimReward = war.BestWinnerClaimReward;
                    ClanWarCityInstance.BestWinner.NextReward = war.BestWinnerNextReward;
                    ClanWarCityInstance.BestWinner.OccupationDays = war.BestWinnerOccupationDays;
                    ClanWarCityInstance.BestWinner.Reward = war.BestWinnerReward;
                });
            }
        }

        public void Save(MsgClanWar.CityWar ClanWarCityInstance)
        {
            if (ServerConfig.DbFromFiles)
            {
                Write writer = new Write(Path.Combine("ClanWar", ClanWarCityInstance.Type + ".txt"));
                WriteLine line = new WriteLine('/');
                line.Add(ClanWarCityInstance.Winner.ClainID).Add(ClanWarCityInstance.Winner.Name).Add(ClanWarCityInstance.Winner.ClaimReward).Add(ClanWarCityInstance.Winner.NextReward).Add(ClanWarCityInstance.Winner.OccupationDays).Add(ClanWarCityInstance.Winner.Reward)
                 .Add(ClanWarCityInstance.BestWinner.ClainID).Add(ClanWarCityInstance.BestWinner.Name).Add(ClanWarCityInstance.BestWinner.ClaimReward).Add(ClanWarCityInstance.BestWinner.NextReward).Add(ClanWarCityInstance.BestWinner.OccupationDays).Add(ClanWarCityInstance.BestWinner.Reward);
                writer.Add(line.Close());
                writer.Execute(Mode.Open);
            }
            else
            {
                RestApiHelper.AddUpdateClanWars(
                [
                    new()
                    {
                        Type = ClanWarCityInstance.Type,
                        BestWinnerClanId = ClanWarCityInstance.BestWinner.ClainID,
                        PoleName = ClanWarCityInstance.Pole.Name,
                        WinnerClaimReward = ClanWarCityInstance.Winner.ClaimReward,
                        WinnerNextReward = ClanWarCityInstance.Winner.NextReward,
                        WinnerOccupationDays = ClanWarCityInstance.Winner.OccupationDays,
                        WinnerReward = ClanWarCityInstance.Winner.Reward,
                        BestWinnerName = ClanWarCityInstance.BestWinner.Name,
                        BestWinnerClaimReward = ClanWarCityInstance.BestWinner.ClaimReward,
                        BestWinnerNextReward = ClanWarCityInstance.BestWinner.NextReward,
                        BestWinnerOccupationDays = ClanWarCityInstance.BestWinner.OccupationDays,
                        BestWinnerReward = ClanWarCityInstance.BestWinner.Reward
                    }
                ]);
            }
        }
    }
    public class PlayerHousesManager
    {
        private readonly string HousesFolder = "Houses";
        public PlayerHousesManager()
        {
            Init();
        }
        public void Init()
        {
        }

        public void Load(Client.GameClient client)
        {
            if (ServerConfig.DbFromFiles)
            {
                if (HousePoll.ContainsKey(client.Player.UID))
                {
                    client.MyHouse = HousePoll[client.Player.UID];
                }
                else
                {
                    BinaryFileHelper binary = new();
                    if (File.Exists(Path.Combine(ServerConfig.DbLocation, HousesFolder, client.Player.UID + ".bin")))
                    {
                        if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, HousesFolder, client.Player.UID + ".bin"), FileMode.Open))
                        {
                            int ItemCount = 0;
                            byte Level = 0;
                            Level = binary.ReadByte();

                            client.MyHouse = new House(client.Player.UID);
                            client.MyHouse.Level = Level;

                            ItemCount = binary.ReadInt();

                            Game.MsgNpc.Npc Furnitures;
                            for (int x = 0; x < ItemCount; x++)
                            {
                                DBNpc DBNpcc = new DBNpc();
                                DBNpcc = binary.Read<DBNpc>();
                                Furnitures = DBNpc.GetServerNpc(DBNpcc);
                                if (!client.MyHouse.Furnitures.ContainsKey(Furnitures.UID))
                                    client.MyHouse.Furnitures.TryAdd(Furnitures.UID, Furnitures);
                            }
                            binary.Close();
                        }
                    }
                }
            }
            else
            {
                List<PlayerHouse> dbHouses = RestApiHelper.GetPlayerHouses(client.Player.UID);
                PlayerHouse dbHouse = dbHouses.FirstOrDefault();
                if (dbHouse != null)
                {
                    client.MyHouse = new House(client.Player.UID);
                    client.MyHouse.Level = (byte)dbHouse.Level;
                    foreach (PlayerHouseFurniture dbFurniture in dbHouse.Furnitures ?? new List<PlayerHouseFurniture>())
                    {
                        DBNpc dbNpc = new()
                        {
                            UID = dbFurniture.UID,
                            Mesh = dbFurniture.Mesh,
                            NpcType = (Role.Flags.NpcType)dbFurniture.NpcType,
                            ObjType = (Role.MapObjectType)dbFurniture.ObjType,
                            Sort = dbFurniture.Sort,
                            X = dbFurniture.X,
                            Y = dbFurniture.Y,
                            Map = dbFurniture.Map,
                            DynamicID = dbFurniture.DynamicID
                        };
                        var serverNpc = DBNpc.GetServerNpc(dbNpc);
                        if (!client.MyHouse.Furnitures.ContainsKey(serverNpc.UID))
                            client.MyHouse.Furnitures.TryAdd(serverNpc.UID, serverNpc);
                    }
                }
            }

        }

        public void Save(Client.GameClient client)
        {
            if (ServerConfig.DbFromFiles)
            {
                if (client.MyHouse != null)
                {
                    BinaryFileHelper binary = new();
                    if (binary.LoadFile(Path.Combine(ServerConfig.DbLocation, HousesFolder, client.Player.UID + ".bin"), FileMode.Create))
                    {
                        byte level = client.MyHouse.Level;
                        int ItemCount = client.MyHouse.Furnitures.Count;
                        binary.WriteByte(level);
                        binary.WriteInt(ItemCount);
                        foreach (var furniture in client.MyHouse.Furnitures.Values)
                        {
                            Game.MsgNpc.Npc Furnitures = furniture;
                            DBNpc Db_npc = DBNpc.Create(Furnitures);
                            binary.Write(Db_npc);
                        }
                        binary.Close();
                    }
                }
            }
            else
            {
                if (client.MyHouse != null)
                {
                    PlayerHouse playerHouse = new PlayerHouse
                    {
                        PlayerUID = client.Player.UID,
                        Level = client.MyHouse.Level,
                        Furnitures = new List<PlayerHouseFurniture>()
                    };

                    foreach (Game.MsgNpc.Npc furniture in client.MyHouse.Furnitures.Values)
                    {
                        PlayerHouseFurniture dbFurniture = new()
                        {
                            UID = furniture.UID,
                            Mesh = furniture.Mesh,
                            NpcType = (Core.Enums.SharedEnums.NpcType)furniture.NpcType,
                            ObjType = (Core.Enums.SharedEnums.MapObjectType)furniture.ObjType,
                            Sort = furniture.Sort,
                            X = furniture.X,
                            Y = furniture.Y,
                            Map = furniture.Map,
                            DynamicID = furniture.DynamicID,
                            Data = furniture.UnKnow
                        };
                        playerHouse.Furnitures.Add(dbFurniture);
                    }
                    RestApiHelper.AddUpdatePlayerHouses([playerHouse]);
                }
            }
        }
    }
}