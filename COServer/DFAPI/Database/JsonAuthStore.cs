using API.Models.AccountServer;
using System.Text.Json;

namespace API.Models
{
    public class JsonAuthStore
    {
        public const uint FirstAccountId = 1000000;
        private readonly string _authDir;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public JsonAuthStore()
        {
            _authDir = Path.Combine(AppContext.BaseDirectory, "LocalData", "Auth");
            Directory.CreateDirectory(_authDir);
            MigrateLegacyAuthJson();
        }

        public Task<bool> PingAsync()
        {
            Directory.CreateDirectory(_authDir);
            return Task.FromResult(true);
        }

        public Task EnsureIndexesAsync()
        {
            Directory.CreateDirectory(_authDir);
            return Task.CompletedTask;
        }

        public async Task<List<Account>> GetAccountsAsync()
        {
            return await ReadAsync<Account>("accounts");
        }

        public async Task<Account> GetAccountByUsernameAsync(string username)
        {
            return (await ReadAsync<Account>("accounts"))
                .FirstOrDefault(x => string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Account> GetAccountByUidAsync(uint uid)
        {
            return (await ReadAsync<Account>("accounts")).FirstOrDefault(x => x.EntityID == uid);
        }

        public async Task<Account> SaveAccountAsync(Account account, bool createIfMissing)
        {
            await _lock.WaitAsync();
            try
            {
                var accounts = await ReadUnlockedAsync<Account>("accounts");
                var existing = accounts.FirstOrDefault(x => string.Equals(x.Username, account.Username, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    existing.IP = account.IP;
                    existing.State = account.State;
                    if (!string.IsNullOrEmpty(account.Password))
                        existing.Password = account.Password;
                    await WriteUnlockedAsync("accounts", accounts);
                    return existing;
                }

                if (!createIfMissing)
                    return null;

                account.EntityID = await NextIdUnlockedAsync("accounts", FirstAccountId);
                accounts.Add(account);
                await WriteUnlockedAsync("accounts", accounts);
                return account;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<bool> DeleteAccountAsync(string username)
        {
            await _lock.WaitAsync();
            try
            {
                var accounts = await ReadUnlockedAsync<Account>("accounts");
                int removed = accounts.RemoveAll(x => string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase));
                if (removed > 0)
                    await WriteUnlockedAsync("accounts", accounts);
                return removed > 0;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<List<Configuration>> GetConfigurationsAsync()
        {
            return await ReadAsync<Configuration>("configurations");
        }

        public async Task<Configuration> GetConfigurationAsync(string key)
        {
            return (await ReadAsync<Configuration>("configurations"))
                .FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Configuration> SaveConfigurationAsync(Configuration configuration)
        {
            await _lock.WaitAsync();
            try
            {
                var configurations = await ReadUnlockedAsync<Configuration>("configurations");
                var existing = configurations.FirstOrDefault(x => string.Equals(x.Key, configuration.Key, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    existing.Value = configuration.Value;
                    await WriteUnlockedAsync("configurations", configurations);
                    return existing;
                }

                configuration.Id = await NextIdUnlockedAsync("configurations");
                configurations.Add(configuration);
                await WriteUnlockedAsync("configurations", configurations);
                return configuration;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Online> GetOnlineAsync(string serverName)
        {
            return (await ReadAsync<Online>("online"))
                .FirstOrDefault(x => string.Equals(x.Name, serverName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Online> SaveOnlineAsync(Online online)
        {
            await _lock.WaitAsync();
            try
            {
                var onlines = await ReadUnlockedAsync<Online>("online");
                var existing = onlines.FirstOrDefault(x => string.Equals(x.Name, online.Name, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    existing.OnlineCount = online.OnlineCount;
                    await WriteUnlockedAsync("online", onlines);
                    return existing;
                }

                online.Id = await NextIdUnlockedAsync("online");
                onlines.Add(online);
                await WriteUnlockedAsync("online", onlines);
                return online;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Server> GetServerByNameAsync(string serverName)
        {
            return (await ReadAsync<Server>("servers"))
                .FirstOrDefault(x => string.Equals(x.Name, serverName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<Server>> GetServersAsync()
        {
            return await ReadAsync<Server>("servers");
        }

        public async Task<Server> SaveServerAsync(Server server)
        {
            await _lock.WaitAsync();
            try
            {
                var servers = await ReadUnlockedAsync<Server>("servers");
                var existing = servers.FirstOrDefault(x => x.Id == server.Id || string.Equals(x.Name, server.Name, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    existing.Name = server.Name;
                    existing.IP = server.IP;
                    existing.Port = server.Port;
                    existing.TransferKey = server.TransferKey;
                    existing.TransferSalt = server.TransferSalt;
                    await WriteUnlockedAsync("servers", servers);
                    return existing;
                }

                server.Id = server.Id == 0 ? await NextIdUnlockedAsync("servers") : server.Id;
                servers.Add(server);
                await WriteUnlockedAsync("servers", servers);
                return server;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SetServerNameAsync(uint serverId, string serverName)
        {
            await _lock.WaitAsync();
            try
            {
                var servers = await ReadUnlockedAsync<Server>("servers");
                var existing = servers.FirstOrDefault(x => x.Id == serverId);
                if (existing != null)
                {
                    existing.Name = serverName;
                    await WriteUnlockedAsync("servers", servers);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Vote> GetVoteByUidAsync(uint uid)
        {
            return (await ReadAsync<Vote>("votes")).FirstOrDefault(x => x.EntityID == uid);
        }

        public async Task<Vote> AddVoteAsync(uint uid)
        {
            await _lock.WaitAsync();
            try
            {
                var votes = await ReadUnlockedAsync<Vote>("votes");
                var existing = votes.FirstOrDefault(x => x.EntityID == uid);
                if (existing != null)
                {
                    existing.Votes++;
                    existing.LastVoteDate = DateTime.Now;
                    await WriteUnlockedAsync("votes", votes);
                    return existing;
                }

                var vote = new Vote
                {
                    ID = await NextIdUnlockedAsync("votes"),
                    EntityID = uid,
                    Votes = 1,
                    LastVoteDate = DateTime.Now
                };
                votes.Add(vote);
                await WriteUnlockedAsync("votes", votes);
                return vote;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Vote> RemoveVotesAsync(uint uid, uint votesToRemove)
        {
            await _lock.WaitAsync();
            try
            {
                var votes = await ReadUnlockedAsync<Vote>("votes");
                var existing = votes.FirstOrDefault(x => x.EntityID == uid);
                if (existing == null)
                    return null;

                existing.Votes = existing.Votes >= votesToRemove ? existing.Votes - votesToRemove : 0;
                existing.LastVoteDate = DateTime.Now;
                await WriteUnlockedAsync("votes", votes);
                return existing;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<List<T>> ReadAsync<T>(string name)
        {
            await _lock.WaitAsync();
            try
            {
                return await ReadUnlockedAsync<T>(name);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<List<T>> ReadUnlockedAsync<T>(string name)
        {
            string path = PathFor(name);
            if (!File.Exists(path))
                return new List<T>();

            string json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
        }

        private async Task WriteUnlockedAsync<T>(string name, List<T> values)
        {
            await File.WriteAllTextAsync(PathFor(name), JsonSerializer.Serialize(values, _jsonOptions));
        }

        private async Task<uint> NextIdUnlockedAsync(string name, uint start = 1)
        {
            var counters = await ReadUnlockedAsync<Counter>("counters");
            var counter = counters.FirstOrDefault(x => x.Name == name);
            if (counter == null)
            {
                counter = new Counter { Name = name, Value = start };
                counters.Add(counter);
            }
            else
            {
                counter.Value++;
            }

            await WriteUnlockedAsync("counters", counters);
            return counter.Value;
        }

        private string PathFor(string name) => Path.Combine(_authDir, name + ".json");

        private void MigrateLegacyAuthJson()
        {
            string legacyDir = Path.Combine(AppContext.BaseDirectory, "AuthJson");
            if (!Directory.Exists(legacyDir))
                return;

            foreach (string legacyFile in Directory.GetFiles(legacyDir, "*.json"))
            {
                string targetFile = Path.Combine(_authDir, Path.GetFileName(legacyFile));
                if (!File.Exists(targetFile))
                    File.Copy(legacyFile, targetFile);
            }
        }

        private class Counter
        {
            public string Name { get; set; }
            public uint Value { get; set; }
        }
    }
}
