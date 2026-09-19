using Core;
using GameServer.MadeByDaRkFox;
using System;

namespace GameServer.Database
{
    public class SystemBanned
    {
        public static BanIPSManager BanManager = new BanIPSManager();
        public static SafeDictionaryAlt<string, Client> BannedPoll = new SafeDictionaryAlt<string, Client>();
        public class Client
        {
            public string IP = "";
            public uint Hours;
            public long StartBan;

            public override string ToString()
            {

                Database.DBActions.WriteLine writer = new DBActions.WriteLine('/');
                writer.Add(IP).Add(Hours).Add(StartBan);
                return writer.Close();
            }
        }

        public static void AddBan(string IP, uint Hours)
        {
            Client msg = new Client();
            msg.IP = IP;
            msg.Hours = Hours;
            msg.StartBan = DateTime.Now.Ticks;

            BannedPoll.Add(msg.IP, msg);
        }
        public static void RemoveBan(string IP)
        {
            Client msg = new Client();
            msg.IP = IP;

            BannedPoll.Remove(msg.IP);
        }
        public static bool IsBanned(string Ip, out string Messaj)
        {
            if (BannedPoll.ContainsKey(Ip))
            {
                var msg = BannedPoll[Ip];
                if (DateTime.FromBinary(msg.StartBan).AddHours(msg.Hours) < DateTime.Now)
                {
                    BannedPoll.Remove(msg.IP);
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
            BanManager.Save();
        }

        public static void Load()
        {
            BanManager.Load();
        }
    }
}
