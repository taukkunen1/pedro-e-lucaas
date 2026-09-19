using System;

namespace GameServer.Game.MsgNpc
{
    public class NpcAttribute : Attribute
    {
        public static readonly Func<NpcAttribute, NpcID> Translator = (a) => a.Type;
        public NpcID Type { get; private set; }
        public uint MapId { get; private set; }
        public ushort MapX { get; private set; }
        public ushort MapY { get; private set; }
        public Role.Flags.NpcType ActionType { get; private set; }
        public ushort Mesh { get; private set; }
        public string Name { get; private set; }
        public NpcAttribute(NpcID type, uint mapId = 0, ushort mapX = 0, ushort mapY = 0, Role.Flags.NpcType actionType = Role.Flags.NpcType.Talker, ushort mesh = 4450, string name = "")
        {
            this.Type = type;
            this.MapId = mapId;
            this.MapX = mapX;
            this.MapY = mapY;
            this.ActionType = actionType;
            this.Mesh = mesh;
            this.Name = name;
        }
    }
}
