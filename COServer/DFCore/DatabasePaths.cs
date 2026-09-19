using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Core
{
    /// <summary>
    /// Database fica sempre em TrinityConquer\COServer\Database5700 (caminho embutido no build).
    /// Se esse caminho nao existir (ex.: VPS), usa o valor configurado / variavel TRINITY_DATABASE.
    /// </summary>
    public static class DatabasePaths
    {
        public static string BuiltInDir
        {
            get
            {
                var meta = typeof(DatabasePaths).Assembly
                    .GetCustomAttributes<AssemblyMetadataAttribute>()
                    .FirstOrDefault(a => a.Key == "DatabaseDir");
                return meta?.Value;
            }
        }

        public static string Resolve(string configured)
        {
            string env = Environment.GetEnvironmentVariable("TRINITY_DATABASE");
            if (!string.IsNullOrWhiteSpace(env) && Directory.Exists(env)) return env;
            string built = BuiltInDir;
            if (!string.IsNullOrWhiteSpace(built) && Directory.Exists(built)) return built;
            return configured;
        }

        /// <summary>Arquivo dentro da Database (ex.: BadMsg.txt).</summary>
        public static string File(string configuredDb, string name) => Path.Combine(Resolve(configuredDb), name);
    }
}
