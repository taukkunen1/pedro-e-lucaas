using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Core.Features
{
    public enum FeatureOrigin { Original5017, Posterior, Custom, Verify, Base }
    public enum FeatureDecision { Keep, Remove, Review }

    /// <summary>Marca uma classe/metodo como pertencente a uma feature do registro (Features5017.json).</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class FeatureAttribute : Attribute
    {
        public string Id { get; }
        public FeatureAttribute(string id) { Id = id; }
    }

    public class FeatureInfo
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("origin")] public string Origin { get; set; }
        [JsonProperty("confidence")] public string Confidence { get; set; }
        [JsonProperty("decision")] public string Decision { get; set; }
        [JsonProperty("paths")] public List<string> Paths { get; set; } = new();
        [JsonProperty("dbFiles")] public List<string> DbFiles { get; set; } = new();
        [JsonProperty("types")] public List<string> Types { get; set; } = new();
        [JsonProperty("npcs")] public List<string> Npcs { get; set; } = new();
        [JsonProperty("items")] public List<uint> Items { get; set; } = new();
        [JsonProperty("quests")] public List<uint> Quests { get; set; } = new();
        [JsonProperty("note")] public string Note { get; set; }

        public FeatureOrigin OriginValue => Enum.TryParse(Origin, true, out FeatureOrigin o) ? o : FeatureOrigin.Verify;
        public FeatureDecision DecisionValue => Enum.TryParse(Decision, true, out FeatureDecision d) ? d : FeatureDecision.Review;
    }

    /// <summary>
    /// Registro de sistemas x versao 5017, lido de Database\Features5017.json.
    /// Consulta: FeatureRegistry.Get("progression.chi"), IsOriginal5017(id), IsKept(id).
    /// Nao altera comportamento sozinho: serve para identificar e para gates explicitos no codigo.
    /// </summary>
    public static class FeatureRegistry
    {
        public const string FileName = "Features5017.json";
        private static Dictionary<string, FeatureInfo> _byId = new(StringComparer.OrdinalIgnoreCase);
        private static HashSet<string> _blockedTypes = new();
        private static HashSet<string> _blockedNpcs = new(StringComparer.OrdinalIgnoreCase);
        private static HashSet<uint> _blockedItems = new();
        private static HashSet<uint> _blockedQuests = new();
        public static bool Loaded { get; private set; }
        public static IReadOnlyCollection<FeatureInfo> All => _byId.Values;

        private class Doc { [JsonProperty("features")] public List<FeatureInfo> Features { get; set; } }

        public static bool Load(string databaseDir)
        {
            try
            {
                string path = Path.Combine(databaseDir, FileName);
                if (!File.Exists(path)) return false;
                var doc = JsonConvert.DeserializeObject<Doc>(File.ReadAllText(path));
                _byId = doc.Features.ToDictionary(f => f.Id, StringComparer.OrdinalIgnoreCase);
                RebuildBlocked();
                Loaded = true;
                return true;
            }
            catch { return false; }
        }

        private static void RebuildBlocked()
        {
            var removed = _byId.Values.Where(f => f.DecisionValue == FeatureDecision.Remove).ToList();
            _blockedTypes = new HashSet<string>(removed.SelectMany(f => f.Types));
            _blockedNpcs = new HashSet<string>(removed.SelectMany(f => f.Npcs), StringComparer.OrdinalIgnoreCase);
            _blockedItems = new HashSet<uint>(removed.SelectMany(f => f.Items));
            _blockedQuests = new HashSet<uint>(removed.SelectMany(f => f.Quests));
        }

        /// <summary>true se o tipo (Type.FullName; classes aninhadas incluidas) pertence a uma feature marcada como "remove".
        /// Usado no registro de handlers (pacotes/NPCs): handler bloqueado nem e' registrado.</summary>
        public static bool IsBlockedType(string fullName)
        {
            if (_blockedTypes.Count == 0 || fullName == null) return false;
            int plus = fullName.IndexOf('+');
            return _blockedTypes.Contains(plus < 0 ? fullName : fullName.Substring(0, plus));
        }
        /// <summary>true se o NPC (nome no enum NpcID) pertence a uma feature "remove".</summary>
        public static bool IsBlockedNpc(string npcName) => _blockedNpcs.Count > 0 && npcName != null && _blockedNpcs.Contains(npcName);

        /// <summary>true se o item (ItemType id) pertence a uma feature "remove": nao entra no inventario, nao e' usado e sai das lojas.</summary>
        public static bool IsBlockedItem(uint itemId) => _blockedItems.Count > 0 && _blockedItems.Contains(itemId);
        /// <summary>true se a quest (id) pertence a uma feature "remove": nao pode ser aceita.</summary>
        public static bool IsBlockedQuest(uint questId) => _blockedQuests.Count > 0 && _blockedQuests.Contains(questId);

        /// <summary>Aplica a decisao de uma feature em memoria (testes / comando de admin). Nao grava o JSON.</summary>
        public static bool SetDecision(string id, FeatureDecision d)
        {
            var f = Get(id); if (f == null) return false;
            f.Decision = d.ToString().ToLowerInvariant(); RebuildBlocked(); return true;
        }

        public static FeatureInfo Get(string id) => _byId.TryGetValue(id, out var f) ? f : null;
        public static bool IsOriginal5017(string id) => Get(id)?.OriginValue == FeatureOrigin.Original5017;
        /// <summary>false apenas se a decisao for "remove". Feature desconhecida = mantida.</summary>
        public static bool IsKept(string id) => Get(id)?.DecisionValue != FeatureDecision.Remove;

        public static string Summary()
        {
            if (!Loaded) return "Features5017.json not found - feature registry disabled";
            string by = string.Join(", ", _byId.Values.GroupBy(f => f.OriginValue).OrderBy(g => g.Key)
                .Select(g => $"{g.Key}={g.Count()}"));
            int rem = _byId.Values.Count(f => f.DecisionValue == FeatureDecision.Remove);
            int rev = _byId.Values.Count(f => f.DecisionValue == FeatureDecision.Review);
            return $"Features: {_byId.Count} ({by}) | remove={rem} review={rev}";
        }
    }
}
