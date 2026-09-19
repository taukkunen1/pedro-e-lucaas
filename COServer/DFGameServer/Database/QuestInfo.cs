using Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Database
{
    public class QuestInfo
    {
        /*[1]
TypeId=1
TaskNameColor=0xFFFFFFFF
CompleteFlag=2
ActivityType=0
MissionId=650
Name=The Armorer 
Lv_min=8
Lv_max=50
Auto=0
First=1
Prequest=608
Map=0
Profession=10,11,12,13,14,15
Sex=0
FinishTime=0
ActivityBeginTime=0
ActivityEndTime=0
ActivityType=0
BeginNpcId=2001,1002,307,338,TCCaptain,TwinCity
FinishNpcId=0004,1002,284,266,Armorer,TwinCity
Prize=EXP:  1000   [item 130007,0,0,0,3,0,0]
IntentionDesp=The TC Captain asked to to Speak with the Armorer.
IntentAmount=1
Intention1=Pay a visit to the [npc Armorer,0004,1002,284,266]
Content=Now you have acquired the basic ability to survive in this cruel world. Now have a word with the Armorer, to learn more about your equipment!
*/
        public class DBQuest
        {
            public class NpcInfo
            {
                public uint ID;
                public ushort Map;
                public ushort X;
                public ushort Y;
                public string Name;
                public string MapName;

                public NpcInfo(string line)
                {
                    if (line == "")
                        return;
                    try
                    {
                        string[] data = line.Split(',');
                        ID = uint.Parse(data[0]);
                        Map = ushort.Parse(data[1]);
                        X = ushort.Parse(data[2]);
                        Y = ushort.Parse(data[3]);
                        Name = data[4];
                        MapName = data[5];
                    }
                    catch { 
                    }
                }
            }
            public byte TypeId = 0;
            public byte CompleteFlag = 0;
            public uint ActivityType = 0;
            public ushort MissionId = 0;
            public string Name = "";
            public ushort Lv_min;
            public ushort Lv_max;
            public ushort First;
           // public ushort Prequest;
            public ushort Map;
            public byte[] Profession;
            public ulong Exp = 0;
            public ulong Gold = 0;
            public byte Sex;
            public uint FinishTime;
            public uint ActivityBeginTime;
            public uint ActivityEndTime;
            public NpcInfo BeginNpcId;
            public NpcInfo FinishNpcId;
            public uint Intentions;
            public uint[] Prize;
            public ushort[] Prequest;
            public bool CanAcceptQuest(byte Class)
            {
                if (Profession.Length == 0 || Profession.Length == 1)
                    return true;
                for (int x = 0; x < Profession.Length; x++)
                    if (Class == Profession[x])
                        return true;
                return false;
            }
        }
        public class ListNpcQuests
        {
            public List<DBQuest> Quests = new List<DBQuest>();
        }
        public static Dictionary<uint, DBQuest> AllQuests = new Dictionary<uint, DBQuest>();
        public static Dictionary<uint, ListNpcQuests> NpcQuests = new Dictionary<uint, ListNpcQuests>();

        public static DBQuest GetFinishQuest(uint NPCID, byte _class, uint QuestID = 0, Func<DBQuest, bool> P = null)
        {
            ListNpcQuests array;
            if (NpcQuests.TryGetValue(NPCID, out array))
            {
                for (int x = 0; x < array.Quests.Count; x++)
                {
                    var quest = array.Quests[x];

                    if (QuestID == 0)
                    {
                        if (quest.CanAcceptQuest(_class))
                        {
                            if (P != null)
                            {
                                if (P(quest))
                                    return quest;
                            }
                            else
                                return quest;
                        }
                    }
                    else if (quest.MissionId == QuestID)
                    {
                        return quest;
                    }
                }
            }
            else return AllQuests[0];
            return null;
        }
        public static List<uint> KingDomMissions = new List<uint>() { 35024, 35007, 35025, 35028, 35034 };
        public static bool IsKingDomMission(uint MissionID)
        {
            return KingDomMissions.Contains(MissionID);
              
        }


        public static void Init()
        {
            IniFileHelper reader = new(Path.Combine(ServerConfig.DbLocation, "Questinfo.ini"));
            int TotalMission = reader.ReadUInt16("TotalMission", "TotalMission", 0);
            for (ushort count = 0; count < TotalMission; count++)
            {
                DBQuest quest = new DBQuest();
                int index = count + 1;
                quest.TypeId = reader.ReadByte(index.ToString(), "TypeId", 0);
                quest.CompleteFlag = reader.ReadByte(index.ToString(), "CompleteFlag", 0);
                quest.ActivityType = reader.ReadByte(index.ToString(), "ActivityType", 0);
                quest.MissionId = reader.ReadUInt16(index.ToString(), "MissionId", 0);
                quest.Intentions = reader.ReadUInt16(index.ToString(), "IntentAmount", 0);
                string Prequest = reader.ReadString(index.ToString(), "Prequest", "");
                if (Prequest != "")
                {
                        if (Prequest.Contains('|'))
                        {
                            var preq2 = Prequest.Split('|');
                            quest.Prequest = new ushort[preq2.Length];
                            for (int x = 0; x < preq2.Length; x++)
                                quest.Prequest[x] = ushort.Parse(preq2[x]);
                        }
                        else if (Prequest.Contains(','))
                        {
                            var preq2 = Prequest.Split(',');
                            quest.Prequest = new ushort[preq2.Length];
                            for (int x = 0; x < preq2.Length; x++)
                                quest.Prequest[x] = ushort.Parse(preq2[x]);
                        }
                    else
                            quest.Prequest = new ushort[1] { ushort.Parse(Prequest) };

                }
                else
                    quest.Prequest = new ushort[1] { 0 };
                string profesion = reader.ReadString(index.ToString(), "Profession", "");
                if (profesion != "")
                {
                    if (profesion.Contains(','))
                    {
                        var profesion2 = profesion.Split(',');
                        quest.Profession = new byte[profesion2.Length];
                        for (int x = 0; x < profesion2.Length; x++)
                            quest.Profession[x] = byte.Parse(profesion2[x]);
                    }
                    else
                        quest.Profession = new byte[1] { byte.Parse(profesion) };
                }
                else
                    quest.Profession = new byte[1] { 0 };

                string Prize = reader.ReadString(index.ToString(), "Prize", "");
                if (Prize.Contains('['))
                {
                    
                    string[] items = Prize.Split('[');
                    quest.Prize = new uint[items.Length - 1];
                    try
                    {
                        for (int x = 1; x < items.Length; x++)
                        {
                            var element = items[x];
                            var sp1 = element.Split(' ');
                            var sp2 = sp1[1].Split(',');
                            quest.Prize[x-1] = uint.Parse(sp2[0]);
                        }
                    }
                    catch
                    {

                    }
                }
                quest.BeginNpcId = new DBQuest.NpcInfo(reader.ReadString(index.ToString(), "BeginNpcId", ""));
                quest.FinishNpcId = new DBQuest.NpcInfo(reader.ReadString(index.ToString(), "FinishNpcId", ""));
                /*BeginNpcId=2001,1002,307,338,TCCaptain,TwinCity
FinishNpcId=0004,1002,284,266,Armorer,TwinCity*/
                if (!AllQuests.ContainsKey(quest.MissionId))
                    AllQuests.Add(quest.MissionId, quest);
               // else
               //     Console.WriteLine(quest.MissionId);

                if (NpcQuests.ContainsKey(quest.BeginNpcId.ID))
                {
                    NpcQuests[quest.BeginNpcId.ID].Quests.Add(quest);
                }
                else
                {
                    NpcQuests.Add(quest.BeginNpcId.ID, new ListNpcQuests());
                    NpcQuests[quest.BeginNpcId.ID].Quests.Add(quest);
                }

            }
            DBQuest q = new DBQuest();
            q.MissionId = 0;

            if (!AllQuests.ContainsKey(q.MissionId))
                AllQuests.Add(q.MissionId, q);
        }
    }
}
