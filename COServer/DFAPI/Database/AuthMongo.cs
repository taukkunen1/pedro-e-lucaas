using API.Models.AccountServer;
using Core.Models.SharedConfig;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace API
{
    /// <summary>
    /// Banco de autenticacao (contas, servidores, votos, online, configuracoes) no MongoDB.
    /// Substitui o antigo AuthDbContext (MySQL). Os ids numericos (EntityID, Id) sao sequenciais,
    /// gerados por uma colecao "counters" com $inc atomico.
    /// </summary>
    public class AuthMongo
    {
        public const uint FirstAccountId = 1000000;
        private readonly IMongoDatabase _db;

        static AuthMongo()
        {
            Map<Account>(x => x.EntityID);
            Map<Server>(x => x.Id);
            Map<Vote>(x => x.ID);
            Map<Online>(x => x.Id);
            Map<Configuration>(x => x.Id);
        }
        private static void Map<T>(System.Linq.Expressions.Expression<Func<T, object>> id)
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
                BsonClassMap.RegisterClassMap<T>(cm => { cm.AutoMap(); cm.MapIdMember(id); cm.SetIgnoreExtraElements(true); });
        }

        public AuthMongo() : this(Models.Utils.ReadAccountServerConfig()) { }

        public AuthMongo(AccountServerConfig cfg)
        {
            _db = CreateClient(cfg).GetDatabase(string.IsNullOrWhiteSpace(cfg.DatabaseName) ? "cq_auth" : cfg.DatabaseName);
        }

        public static MongoClient CreateClient(AccountServerConfig cfg)
        {
            var s = new MongoClientSettings
            {
                Server = new MongoServerAddress(string.IsNullOrWhiteSpace(cfg.DatabaseHostname) ? "localhost" : cfg.DatabaseHostname,
                                                 cfg.DatabasePort == 0 ? 27017 : (int)cfg.DatabasePort),
                ServerSelectionTimeout = TimeSpan.FromSeconds(5)
            };
            if (!string.IsNullOrEmpty(cfg.DatabaseUsername))
                s.Credential = MongoCredential.CreateCredential(string.IsNullOrWhiteSpace(cfg.DatabaseAuthSource) ? "admin" : cfg.DatabaseAuthSource,
                                                                cfg.DatabaseUsername, cfg.DatabasePassword ?? "");
            return new MongoClient(s);
        }

        public IMongoCollection<Account> Accounts => _db.GetCollection<Account>("accounts");
        public IMongoCollection<Server> Servers => _db.GetCollection<Server>("servers");
        public IMongoCollection<Vote> Votes => _db.GetCollection<Vote>("votes");
        public IMongoCollection<Online> Onlines => _db.GetCollection<Online>("online");
        public IMongoCollection<Configuration> Configurations => _db.GetCollection<Configuration>("configurations");
        private IMongoCollection<MongoDB.Bson.BsonDocument> Counters => _db.GetCollection<MongoDB.Bson.BsonDocument>("counters");

        /// <summary>Proximo id sequencial da colecao (atomico). start = primeiro id devolvido.</summary>
        public async Task<uint> NextIdAsync(string name, uint start = 1)
        {
            var doc = await Counters.FindOneAndUpdateAsync(
                Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", name),
                Builders<MongoDB.Bson.BsonDocument>.Update.Inc("seq", 1L).SetOnInsert("base", (long)start),
                new FindOneAndUpdateOptions<MongoDB.Bson.BsonDocument> { IsUpsert = true, ReturnDocument = ReturnDocument.After });
            return (uint)(doc["base"].ToInt64() + doc["seq"].ToInt64() - 1);
        }

        public async Task<bool> PingAsync()
        {
            try { await _db.RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ping:1}"); return true; }
            catch { return false; }
        }

        /// <summary>Cria os indices (substitui as migrations do MySQL). Pode rodar varias vezes.</summary>
        public async Task EnsureIndexesAsync()
        {
            await Accounts.Indexes.CreateOneAsync(new CreateIndexModel<Account>(Builders<Account>.IndexKeys.Ascending(x => x.Username), new CreateIndexOptions { Unique = true }));
            await Servers.Indexes.CreateOneAsync(new CreateIndexModel<Server>(Builders<Server>.IndexKeys.Ascending(x => x.Name)));
            await Onlines.Indexes.CreateOneAsync(new CreateIndexModel<Online>(Builders<Online>.IndexKeys.Ascending(x => x.Name)));
            await Configurations.Indexes.CreateOneAsync(new CreateIndexModel<Configuration>(Builders<Configuration>.IndexKeys.Ascending(x => x.Key), new CreateIndexOptions { Unique = true }));
            await Votes.Indexes.CreateOneAsync(new CreateIndexModel<Vote>(Builders<Vote>.IndexKeys.Ascending(x => x.EntityID)));
        }
    }
}
