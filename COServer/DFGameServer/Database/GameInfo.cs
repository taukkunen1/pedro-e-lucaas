namespace GameServer.Database
{
    public class GameInfo
    {
        public int AccountKey;
        public uint UID;
        public ushort Body;
        public ushort Face;
        public string Name = "None";
        public string Spouse = "None";
        public byte Class;
        public byte FirstClass;
        public byte SecoundeClass;
        public ushort Avatar;
        public ushort Map = 1010;
        public ushort X = 61;
        public ushort Y = 109;
        public ushort Agility;
        public ushort Strength;
        public ushort Spirit;
        public ushort Vitaliti;
        public ushort Atributes;
        public byte Reborn;
        public ushort Level = 1;
        public ushort Haire;
        public ulong Experience;
        public int MinHitPoints;
        public ushort MinMana;
        public int ConquerPoints;
        public int BoundConquerPoints;
        public int Money;
        public uint VirtutePoints;
        public ushort PkPoints;
        public uint QuizPoints;
        public ushort Enilghten;
        public ushort EnlightenReceive;
        public byte VipLevel;
        public AchievementCollection Achivement;


        public override string ToString()
        {
            return new global::GameServer.Database.DBActions.WriteLine('/').Add(AccountKey).Add(UID).Add(Body).Add(Face)
                .Add(Name).Add(Spouse).Add(Class).Add(FirstClass).Add(SecoundeClass).Add(Avatar)
                .Add(Map).Add(X).Add(Y).Add(Agility).Add(Strength).Add(Spirit)
                .Add(Vitaliti).Add(Atributes).Add(Reborn).Add(Level).Add(Haire)
                .Add(Experience).Add(MinHitPoints).Add(MinMana)
                .Add(ConquerPoints).Add(BoundConquerPoints).Add(Money).Add(VirtutePoints)
                .Add(PkPoints).Add(QuizPoints)
                .Add(Enilghten).Add(EnlightenReceive).Add(VipLevel).Close();
        }
        public void Load(string data)
        {
            using (global::GameServer.Database.DBActions.ReadLine line = new global::GameServer.Database.DBActions.ReadLine(data, '/'))
            {
                AccountKey = line.Read(0);
                UID = line.Read(0u);
                Body = line.Read((ushort)0);
                Face = line.Read((ushort)0);
                Name = line.Read("None");
                Spouse = line.Read("None");
                Class = line.Read((byte)0);
                FirstClass = line.Read((byte)0);
                SecoundeClass = line.Read((byte)0);
                Avatar = line.Read((ushort)0);
                Map = line.Read((ushort)1002);
                X = line.Read((ushort)400);
                Y = line.Read((ushort)400);
                Agility = line.Read((ushort)0);
                Strength = line.Read((ushort)0);
                Spirit = line.Read((ushort)0);
                Vitaliti = line.Read((ushort)0);
                Atributes = line.Read((ushort)0);
                Reborn = line.Read((byte)0);
                Level = line.Read((ushort)1);
                Haire = line.Read((ushort)0);
                Experience = line.Read(0ul);
                MinHitPoints = line.Read(0);
                MinMana = line.Read((ushort)0);
                ConquerPoints = line.Read(0);
                BoundConquerPoints = line.Read(0);
                Money = line.Read(0);
                VirtutePoints = line.Read(0u);
                PkPoints = line.Read((ushort)0);
                QuizPoints = line.Read(0u);
                Enilghten = line.Read((ushort)0);
                EnlightenReceive = line.Read((ushort)0);
                VipLevel = line.Read((byte)0);
            }
        }
    }
}
