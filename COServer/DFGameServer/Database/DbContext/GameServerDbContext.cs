//using Microsoft.EntityFrameworkCore;
//using System.IO;

//namespace GameServer
//{
//    public class GameServerDbContext : DbContext
//    {
//        public GameServerDbContext()
//        {
//        }

//        protected override void OnConfiguring(DbContextOptionsBuilder options)
//        {
//            if (File.Exists("GameServerConfig.json"))
//            {
//                string strCon = $"server={Program.GSConfig.DatabaseHostname};port={Program.GSConfig.DatabasePort};database={Program.GSConfig.DatabaseName};user={Program.GSConfig.DatabaseUsername};password={Program.GSConfig.DatabasePassword};Persist Security Info=False;Pooling=true; Min Pool Size = 32;Max Pool Size = 300;";
//                options.UseMySql(strCon, ServerVersion.AutoDetect(strCon));
//            }
//            options.UseCamelCaseNamingConvention();
//        }
//    }
//}
