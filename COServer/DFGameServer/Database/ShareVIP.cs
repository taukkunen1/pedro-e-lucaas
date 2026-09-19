using Core;
using GameServer.MadeByDaRkFox;
using System;
using System.Linq;

namespace GameServer.Database
{
    public static class ShareVIP
    {
        public static MyList<Client> SharedPoll = new MyList<Client>();
        public static IDataManager DataManager = new VIPSharesManager();
        public class Client
        {
            public uint UID;
            public uint ShareUID;
            public string ShareName;
            public byte ShareLevel;
            public DateTime ShareEnds;
            public override string ToString()
            {
                Database.DBActions.WriteLine writer = new DBActions.WriteLine('/');
                writer.Add(UID).Add(ShareUID).Add(ShareName).Add(ShareLevel).Add(ShareEnds.Ticks);
                return writer.Close();
            }
        }
        public static bool CanShare(uint UID, uint ShareUID)
        {
            if (SharedPoll.GetValues().Where(p => p.ShareUID == UID).ToList().Count > 0)
            {
                return false;
            }
            if (SharedPoll.GetValues().Where(p => p.ShareUID == ShareUID).ToList().Count > 0)
            {
                return false;
            }
            if (SharedPoll.GetValues().Where(p => p.UID == ShareUID).ToList().Count > 0)
            {
                return false;
            }
            if (SharedPoll.GetValues().Where(p => p.UID == UID).ToList().Count > 0)
            {
                return false;
            }
            return true;
        }
        public static bool Add(Client x)
        {
            if (CanShare(x.UID, x.ShareUID))
            {
                SharedPoll.Add(x);
                return true;
            }
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
    }
}
