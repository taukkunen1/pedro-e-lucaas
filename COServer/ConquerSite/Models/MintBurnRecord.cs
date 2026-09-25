using System;

namespace ConquerSite.Models
{
    public class MintBurnRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public uint AccountUid { get; set; }
        public string AccountName { get; set; }
        public uint CharacterUid { get; set; }
        public string CharacterName { get; set; }
        public uint ItemUid { get; set; }
        public uint ItemId { get; set; }
        public string Status { get; set; } = MintBurnStatus.Minted;
        public uint MintPower { get; set; }
        public byte Plus { get; set; }
        public byte Bless { get; set; }
        public ushort SocketOne { get; set; }
        public ushort SocketTwo { get; set; }
        public ushort Position { get; set; }
        public DateTime MintedAt { get; set; } = DateTime.UtcNow;
        public DateTime? BurnedAt { get; set; }
    }

    public static class MintBurnStatus
    {
        public const string Minted = "Minted";
        public const string Burned = "Burned";
    }
}
