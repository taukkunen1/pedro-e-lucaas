namespace ConquerSite.Models
{
    public class Character
    {
        public uint UID { get; set; }
        public ushort Body { get;set; }
        public ushort Face { get; set; }
        public string Name { get; set; }
        public ushort Class { get; set; }
        public ushort Level { get; set; }
        public ushort Reborn { get; set; }
        public uint RacePoints { get; set; }
    }
}
