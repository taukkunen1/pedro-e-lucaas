using Core;
using Core.Models.GameServer;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Database
{
    public class TutorInfo
    {
        public class TutorType
        {
            public int Index;
            public int MinLevel;
            public int MaxLevel;
            public int StudentNum;
            public int BattleLevShare;
        }
        public static List<TutorType> TutorTypes = new List<TutorType>();
        public static int[] BattleLimit = new int[400];

        public static void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                string[] baseText = File.ReadAllLines(Path.Combine(ServerConfig.DbLocation, "cq_tutor_type.txt"));
                foreach (var bas_line in baseText)
                {
                    string[] line = bas_line.Split(',');
                    TutorType obj = new TutorType();
                    obj.Index = int.Parse(line[0]);
                    obj.MinLevel = int.Parse(line[1]);
                    obj.MaxLevel = int.Parse(line[2]);
                    obj.StudentNum = int.Parse(line[3]);
                    obj.BattleLevShare = int.Parse(line[4]);
                    TutorTypes.Add(obj);
                }
                baseText = File.ReadAllLines(Path.Combine(ServerConfig.DbLocation, "cq_tutor_battle_limit_type.txt"));
                BattleLimit = new int[baseText.Length + 1];
                foreach (var bas_line in baseText)
                {
                    string[] line = bas_line.Split(',');
                    BattleLimit[int.Parse(line[0])] = int.Parse(line[1]);
                }
            } else
            {
                List<TutorType> tutorTypes = RestApiHelper.GetRequest<List<TutorType>>("TutorType/Get");
                foreach(var tutorType in tutorTypes)
                {
                    TutorTypes.Add(tutorType);
                }
                List<TutorBattleLimitType> tutorBattleLimitTypes = RestApiHelper.GetRequest<List<TutorBattleLimitType>>("TutorType/Get");
                BattleLimit = new int[tutorBattleLimitTypes.Count + 1];
                foreach (var tutorBattleLimitType in tutorBattleLimitTypes)
                {
                    BattleLimit[tutorBattleLimitType.BP] = (int)tutorBattleLimitType.SharedBPLimit;
                }
            }
        }

        public static TutorType GetTutorInfo(Client.GameClient user)
        {
            foreach (var obj in TutorTypes)
            {
                if (user.Player.Level >= obj.MinLevel && Math.Min(140, (int)user.Player.Level) <= obj.MaxLevel)
                    return obj;
            }
            return null;
        }
        public static int AddAppCount(Client.GameClient user)
        {
           var info = GetTutorInfo(user);
           if (info != null)
               return info.StudentNum;
            return 0;
        }

        public static int ShareBattle(Client.GameClient Mentor, int Student_battle)
        {
            if (Student_battle >= Mentor.Player.RealBattlePower)
                return 0;
            var Tutor = GetTutorInfo(Mentor);
            if (Tutor != null)
            {
                int battle_lev_share = Tutor.BattleLevShare;
                int share = (Mentor.Player.RealBattlePower - Student_battle) * battle_lev_share / 100;
                int share_limit = BattleLimit[Math.Min(BattleLimit.Length - 1, Student_battle)];
                return Math.Min(share, share_limit);
            }
            return 0;
        }

    }
}
