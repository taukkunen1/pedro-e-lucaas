using Core.Interfaces.GameServer;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Core.Models.GameServer
{
    public class VendorShop : IVendorShop
    {
        public uint UID { get; set; }
        public ushort Mesh { get; set; } = 100;
        public string Name { get; set; }
        public ushort Map { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public string ItemsJson { get; set; }
        public VendorActionMode CostType { get; set; }
        [NotMapped]
        public List<string> Items
        {
            get => string.IsNullOrEmpty(ItemsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(ItemsJson);
            set => ItemsJson = value == null
                ? null
                : JsonSerializer.Serialize(value);
        }
    }
}
