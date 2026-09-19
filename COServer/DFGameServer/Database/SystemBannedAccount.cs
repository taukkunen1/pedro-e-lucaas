using Core;
using GameServer.MadeByDaRkFox;
using System;

namespace GameServer.Database
{
    public class SystemBannedAccountClient
    {
        public uint UID;
        public string Name;
        public uint Hours;
        public long StartBan;
        public string Reason = "";

        public override string ToString()
        {

            Database.DBActions.WriteLine writer = new DBActions.WriteLine('/');
            writer.Add(UID).Add(Hours).Add(StartBan).Add(Name).Add(Reason);
            return writer.Close();
        }
    }
    public class SystemBannedAccount
    {
        public static IDataManager DataManager = new BanUIDSManager();
        public static SafeDictionaryAlt<uint, SystemBannedAccountClient> BannedPoll = new SafeDictionaryAlt<uint, SystemBannedAccountClient>();
        public static void AddBan(uint UID,string name, uint Hours, string Reason = "")
        {
            SystemBannedAccountClient msg = new SystemBannedAccountClient();
            msg.UID = UID;
            msg.Hours = Hours;
            msg.Name = name;
            msg.StartBan = DateTime.Now.Ticks;
            msg.Reason = Reason;
            BannedPoll.Add(msg.UID, msg);
        }
        public static void RemoveBan(uint UID)
        {
            if (BannedPoll.ContainsKey(UID))
            {
                BannedPoll.Remove(UID);
                Remove(UID); // persist to db mysql (if are using)
            }
        }
        public static void RemoveBan(string name)
        {
            uint UID = 0;
            foreach (var obj in BannedPoll.Values)
            {
                if (obj.Name == name)
                {
                    UID = obj.UID;
                    break;
                }
            }
            if (UID != 0)
                RemoveBan(UID);
        }
        public static bool IsBanned(uint UID, out string Messaj)
        {
            if (BannedPoll.ContainsKey(UID))
            {
                var msg = BannedPoll[UID];
                if (DateTime.FromBinary(msg.StartBan).AddHours(msg.Hours) < DateTime.Now)
                {
                    BannedPoll.Remove(msg.UID);
                }
                else
                {
                    DateTime receiveban = DateTime.FromBinary(msg.StartBan);
                    DateTime TimerBan = receiveban.AddHours(msg.Hours);
                    TimeSpan time = TimeSpan.FromTicks(TimerBan.Ticks) - TimeSpan.FromTicks(DateTime.Now.Ticks);
                    Messaj = " " + time.Days + " Days " + time.Hours + " Hours " + time.Minutes + " Minutes";
                    return true;
                }
            }
            Messaj = "";
            return false;
        }

        public static void Save()
        {
            DataManager.Save();
        }

        public static void Load()
        {
            DataManager.Load();
        }

        public static void Remove(uint UID)
        {
            (DataManager as BanUIDSManager).Remove(UID);
        }
    }
}
