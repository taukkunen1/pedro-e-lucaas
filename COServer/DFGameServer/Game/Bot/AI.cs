using GameServer.Database;
using GameServer.Game.MsgServer;
using System;

namespace GameServer.Bot
{
    public class AI
    {
        public static uint[] Class = new uint[]
        {
            15,
            25,
            45,
            55,
            135,
            145
        };
        public static string[] Names = new string[]
        {
            "Mace", "Falchion", "Montante", "Battleaxe", "Zweihander", "Hatchet",
            "Billhook", "Club", "Hammer", "Caltrop", "Maul", "Sledgehammer", "Longbow",
            "Bludgeon", "Harpoon", "Crossbow", "Lance", "Angon", "Pike", "Tiger Claw", "Fire Lance",
            "Poleaxe", "Brass Knuckle", "Matchlock", "Quarterstaff", "Gauntlet", "Bullwhip", "War Hammer", "Katar",
            "Flying Claw", "Spear", "Dagger", "Slungshot", "Katana", "Gladius", "Aspis", "Saber", "Cutlass",
            "Blade", "Broadsword", "Scimitar", "Lockback", "Claymore", "Espada", "Machete", "Grizzly", "Wolverine",
            "Deathstalker", "Snake", "Wolf", "Scorpion", "Vulture", "Claw", "Boomslang", "Falcon", "Fang", "Viper",
            "Ram", "Grip", "Sting", "Boar", "Black Mamba", "Lash", "Tusk", "Goshawk", "Gnaw", "Amazon", "Majesty",
            "Anomoly", "Malice", "Banshee", "Mannequin", "Belladonna", "Minx", "Beretta", "Mirage", "Black Beauty",
            "Nightmare", "Calypso", "Nova", "Carbon", "Pumps", "Cascade", "Raven", "Colada", "Resin", "Cosma", "Riveter",
            "Cougar", "Rogue", "Countess", "Roulette", "Enchantress", "Shadow", "Enigma", "Siren", "Femme Fatale", "Stiletto",
            "Firecracker", "Tattoo", "Geisha", "T-Back", "Goddess", "Temperance", "Half Pint", "Tequila", "Harlem", "Terror", "Heroin",
            "Thunderbird", "Infinity", "Ultra", "Insomnia", "Vanity", "Ivy", "Velvet", "Legacy", "Vixen", "Lithium", "Voodoo", "Lolita",
            "Wicked", "Lotus", "Widow", "Mademoiselle", "Xenon", "Kahina", "Teuta", "Isis", "Dihya", "Artemis", "Nefertiti", "RunningEagle",
            "Atalanta", "Sekhmet", "Colestah", "Athena", "Ishtar", "Calamity Jane", "Enyo", "Ashtart", "Pearl Heart", "Bellona", "Juno", "Belle Starr",
            "White Tights", "Tanit", "Hua Mulan", "Shieldmaiden", "Devi", "Boudica", "Valkyrie", "Selkie", "Medb", "Cleo", "Venus", "Fate", "Beguile", "Deviant",
            "Illusion", "Crafty", "Variance", "Delusion", "Deceit", "Caprice", "Deception", "Waylay", "Aberr", "Myth", "Ambush", "Variant", "Daydream", "Feint", "Hero",
            "Night Terror", "Catch-22", "Villain", "Figment", "Puzzler", "Daredevil", "Virtual", "Curio", "Mercenary", "Chicanery", "Prodigy", "Voyager", "Trick",
            "Breach", "Wanderer", "Vile", "Miss Fortune", "Audacity", "Horror", "Vex", "Swagger", "Dismay", "Grudge", "Nerve", "Phobia", "Enmity", "Egomania", "Fright",
            "Animus", "Scheme", "Panic", "Hostility", "Paramour", "Agony", "Rancor", "X-hibit", "Inferno", "Malevolence", "Charade", "Blaze", "Poison", "Hauteur", "Crucible",
            "Spite", "Vainglory", "Haunter", "Spitefulness", "Narcissus", "Bane", "Venom", "Brass","Volcano","Vampire","Hulk","DaRkFoxDeveloper"
        };

        public Client.GameClient BEntity;
        public Equipment Equipment;
        public ushort Body;
        public uint UID;
        public uint MapID;
        public ushort X;
        public ushort Y;
        public int HP;
        public DateTime StampJumbCallback = DateTime.Now;
        public DateTime StampHitCallback = DateTime.Now;
        public Role.GameMap Map = null;

        public AI()
        {
            //ServerSocket server = null;
            //SecuritySocket Owner = null;
            //server = new ServerSockets.ServerSocket(new Action<ServerSockets.SecuritySocket>(p => new Client.GameClient(p)), GameServer.Receive, GameServer.Disconnect);
            //Owner = new ServerSockets.SecuritySocket(server, GameServer.Disconnect, GameServer.Receive);
            //Owner.IsBot = true;
            //Owner.Alive = true;
            //Owner.Connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Owner.Create(Owner.Connection);
            BEntity = new Client.GameClient(null);
        }
        public void Add(bool auto = false)
        {
            ushort x = (ushort)Pool.GetRandom.Next((int)X - 10, X + 11);
            ushort y = (ushort)Pool.GetRandom.Next((int)Y - 10, Y + 11);
            if (Map.IsValidFlagNpc(x, y))
            {
                Equipment = new Equipment(this);
                BEntity.Fake = true;
                BEntity.Player = new Role.Player(BEntity);
                BEntity.Inventory = new Role.Instance.Inventory(BEntity);
                BEntity.Equipment = new Role.Instance.Equip(BEntity);
                BEntity.Warehouse = new Role.Instance.Warehouse(BEntity);
                BEntity.MyProfs = new Role.Instance.Proficiency(BEntity);
                BEntity.MySpells = new Role.Instance.Spell(BEntity);
                BEntity.Achievement = new Database.AchievementCollection();
                BEntity.Status = new MsgStatus();
                string NamesID = Names[Pool.GetRandom.Next(0, Names.Length)];
                BEntity.Player.Name = NamesID;
                BEntity.Player.Body = (ushort)Pool.GetRandom.Next(1001, 1005);
                BEntity.Player.UID = this.UID = Pool.ClientCounter.Next;
                BEntity.Player.HitPoints = this.HP + 5000;
                BEntity.Status.MaxHitpoints = (uint)BEntity.Player.HitPoints;
                BEntity.Player.X = x;
                BEntity.Player.Y = y;
                BEntity.Player.Map = this.MapID;
                BEntity.Player.Level = (byte)Pool.GetRandom.Next(60, 120);
                BEntity.Player.Reborn = (byte)Pool.GetRandom.Next(0, 2);
                //BEntity.Player.NobilityRank = (Role.Instance.Nobility.NobilityRank)Pool.GetRandom.Next(0, 6);
                BEntity.Player.Face = 153;
                BEntity.Player.CountryID = (ushort)Pool.GetRandom.Next(1, 50);
                BEntity.Player.Action = Role.Flags.ConquerAction.Sit;
                uint ClassID = Class[Pool.GetRandom.Next(0, Class.Length)];
                if (auto)
                {
                    ClassID = 45;
                }
                else
                {
                    BEntity.Player.Away = 1;
                }
                BEntity.Player.Angle = (Role.Flags.ConquerAngle)Pool.GetRandom.Next(0, 7);
                BEntity.Player.Class = (byte)ClassID;
                BEntity.Player.FirstClass = (byte)ClassID;
                BEntity.Player.SecondClass = (byte)ClassID;
                BEntity.Player.ServerID = (ushort)Database.GroupServerList.MyServerInfo.ID;
                BEntity.Map = this.Map;
                BEntity.Player.Vitality = (ushort)((BEntity.Player.Level + BEntity.Player.BattlePower) * (BEntity.Player.Reborn + 1));
                //BEntity.GeneratorItemDrop(DropStatus.All);
                DataCore.AtributeStatus.GetStatus(BEntity.Player);
                DataCore.SetCharacterSides(BEntity.Player);
                DataCore.CreateHairStyle(BEntity);
                DataCore.LoadClient(BEntity.Player);
                Equipment.GetRandomEquipment((byte)ClassID).Send();
                BEntity.Map.Enquer(BEntity);
                StampJumbCallback = DateTime.Now;
                StampHitCallback = DateTime.Now;
                Pool.GamePoll.TryAdd(BEntity.Player.UID*100, BEntity);
                if (auto)
                    Program.CallBack.BotRegister(this);
            }
        }
    }
}
