using Core;
using GameServer.Database.DBActions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Pool;
namespace GameServer.Database
{
    public class TheCrimeTable : ConcurrentDictionary<uint, TheCrimeTable.TheCrime>
    {
        public class TheCrime
        {
            public string OwnerName;
            public uint OwnerUID;
            public int Money;

            public override string ToString()
            {
                WriteLine writer = new WriteLine('/');
                writer.Add(OwnerName).Add(OwnerUID).Add(Money);
                return writer.Close();
            }
        }
        public void AddCrime(string OwnerName, uint OwnerUID, int Money)
        {
            TheCrime crime;
            if (TryGetValue(OwnerUID, out crime))
            {
                crime.OwnerName = OwnerName;
                crime.Money += Money;
            }
            else
            {
                crime = new TheCrime()
                {
                    OwnerName = OwnerName,
                    Money = Money,
                    OwnerUID = OwnerUID
                };
                TryAdd(OwnerUID, crime);
            }
        }
        public int Claim(out string name)
        {
            if (Count > 0)
            {
                TheCrime crime;
                if (TryRemove(this.Values.First().OwnerUID, out crime))
                {
                    name = crime.OwnerName;
                    return crime.Money;
                }
            }
            name = "None";
            return 0;
        }

        internal static void Load()
        {
            if (ServerConfig.DbFromFiles)
            {
                using (Read Reader = new Read("Crime.ini"))
                {
                    if (Reader.Reader())
                    {
                        for (int x = 0; x < Reader.Count; x++)
                        {
                            string Line = Reader.ReadString("^");
                            uint OwnerUID = uint.Parse(Line.Split('^')[0]);
                            ReadLine Readline = new ReadLine(Line.Split('^')[1], '/');
                            TheCrimeTable CrimeClient;
                            if (TheCrimePoll.TryGetValue(OwnerUID, out CrimeClient))
                            {
                                CrimeClient.AddCrime(Readline.Read(""), Readline.Read((uint)0), Readline.Read(0));
                            }
                            else
                            {
                                CrimeClient = new TheCrimeTable();
                                CrimeClient.AddCrime(Readline.Read(""), Readline.Read((uint)0), Readline.Read(0));
                                TheCrimePoll.TryAdd(OwnerUID, CrimeClient);
                            }
                        }
                    }
                }
            } else
            {
                List<TheCrime> apiCrimes = RestApiHelper.GetRequest<List<TheCrime>>("Crime/Get");
                foreach(var apiCrime in apiCrimes)
                {
                    TheCrimeTable CrimeClient;
                    if (TheCrimePoll.TryGetValue(apiCrime.OwnerUID, out CrimeClient))
                    {
                        CrimeClient.AddCrime(apiCrime.OwnerName, apiCrime.OwnerUID, apiCrime.Money);
                    }
                    else
                    {
                        CrimeClient = new TheCrimeTable();
                        CrimeClient.AddCrime(apiCrime.OwnerName, apiCrime.OwnerUID, apiCrime.Money);
                        TheCrimePoll.TryAdd(apiCrime.OwnerUID, CrimeClient);
                    }
                }
            }
        }
        internal static void Save()
        {
            List<Core.Models.GameServer.Crime> apiCrimes = new List<Core.Models.GameServer.Crime>();
            if (ServerConfig.DbFromFiles)
            {
                using (Write writer = new Write("Crime.ini"))
                {
                    foreach (var client in TheCrimePoll)
                    {
                        uint OwnerUID = client.Key;
                        foreach (var crimes in client.Value.Values)
                        {
                            writer.Add(OwnerUID.ToString() + "^" + crimes.ToString());
                        }
                    }
                    writer.Execute(Mode.Open);
                }
            } else
            {
                foreach (var client in TheCrimePoll)
                {
                    uint OwnerUID = client.Key;
                    foreach (var crime in client.Value.Values)
                    {
                        apiCrimes.Add(new Core.Models.GameServer.Crime() { Money = (uint)crime.Money, OwnerUID = crime.OwnerUID, OwnerName = crime.OwnerName });
                    }
                }
            }
            if (!ServerConfig.DbFromFiles)
            {
                RestApiHelper.PostRequestSuccessful("Crime/Update", apiCrimes);
            }
        }
    }
}
