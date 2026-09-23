namespace GameServer.Game.Era1
{
    /// <summary>
    /// Structural progression rules for the 5017-inspired Era 1.
    /// Keep level-cap checks centralized so later-version 5695 limits cannot leak back in.
    /// </summary>
    public static class Era1Progression
    {
        public const ushort MaxLevel = 130;

        public static ushort ClampLevel(ushort level)
        {
            return level > MaxLevel ? MaxLevel : level;
        }

        public static byte ExtraRebirthAttributePoints(byte level, bool waterTaoist)
        {
            if (waterTaoist)
            {
                if (level <= 110) return 0;
                switch (level)
                {
                    case 112: return 1;
                    case 114: return 3;
                    case 116: return 6;
                    case 118: return 10;
                    case 120:
                    case 121: return 15;
                    case 122:
                    case 123: return 21;
                    case 124:
                    case 125: return 28;
                    case 126:
                    case 127: return 36;
                    case 128:
                    case 129: return 45;
                    default: return 55;
                }
            }

            if (level <= 120) return 0;
            switch (level)
            {
                case 121: return 1;
                case 122: return 3;
                case 123: return 6;
                case 124: return 10;
                case 125: return 15;
                case 126: return 21;
                case 127: return 28;
                case 128: return 36;
                case 129: return 45;
                default: return 55;
            }
        }
    }
}
