using API.Models;
using API.Models.GameServer;
using Microsoft.EntityFrameworkCore;

namespace API
{
    public class GameDbContext : DbContext
    {
        public DbSet<ApiSobNPC> SobNPCs { get; set; }
        public DbSet<ApiNPC> NPCs { get; set; }
        public DbSet<ApiFurniture> Furnitures { get; set; }
        public DbSet<ApiTrap> Traps { get; set; }
        public DbSet<ApiPortal> Portals { get; set; }
        public DbSet<ApiGameMap> GameMaps { get; set; }
        public DbSet<ApiGuild> Guilds { get; set; }
        public DbSet<ApiClan> Clans { get; set; }
        public DbSet<ApiTutorType> TutorTypes { get; set; }
        public DbSet<ApiTutorBattleLimitType> TutorBattleLimitTypes { get; set; }
        public DbSet<ApiTransformation> Transformations { get; set; }
        public DbSet<ApiCrime> Crimes { get; set; }
        public DbSet<ApiArenaUser> ArenaUsers { get; set; }
        public DbSet<ApiTeamArenaUser> TeamArenaUsers { get; set; }
        public DbSet<ApiStaticStatue> StaticStatues { get; set; }
        public DbSet<ApiPlayer> Players { get; set; }
        public DbSet<ApiQuest> Quests { get; set; }
        public DbSet<ApiPlayerItem> PlayerItems { get; set; }
        //public DbSet<ApiTrashedPlayerItem> TrashedPlayerItems { get; set; }
        public DbSet<ApiPlayerSpell> PlayerSpells { get; set; }
        public DbSet<ApiPlayerProficiency> PlayerProfs { get; set; }
        public DbSet<ApiVendorShop> VendorShops { get; set; }
        public DbSet<ApiTeamElitePK> TeamElitePKs { get; set; } // Used in TeamElitePK, SkillTeamPK
        public DbSet<ApiClassPKWar> ClassPKWars { get; set; }
        public DbSet<ApiElitePK> ElitePKs { get; set; }
        public DbSet<ApiCouple> Couples { get; set; }
        public DbSet<ApiCityWar> CityWars { get; set; }
        public DbSet<ApiGuildWar> GuildWars { get; set; }
        public DbSet<ApiVIPShare> VIPShares { get; set; }
        public DbSet<ApiKOBoardRank> KOBoardRanks { get; set; }
        public DbSet<ApiBanUID> BanUIDs { get; set; }
        public DbSet<ApiBanIP> BanIPs { get; set; }
        public DbSet<ApiAssociate> Associates { get; set; }
        public DbSet<ApiClanWar> ClanWars { get; set; }
        public DbSet<ApiPlayerHouse> PlayerHouses { get; set; }
        public DbSet<ApiPlayerHouseFurniture> PlayerHouseFurnitures { get; set; }

        public GameDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseInMemoryDatabase("TrinityConquerGameApi");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
