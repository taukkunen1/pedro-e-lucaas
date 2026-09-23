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

        public static void RunSelfTest()
        {
            if (ClampLevel(129) != 129 || ClampLevel(130) != 130 || ClampLevel(131) != 130 || ClampLevel(140) != 130)
                throw new System.InvalidOperationException("Era 1 level cap self-test failed.");

            if (ExtraRebirthAttributePoints(120, false) != 0
                || ExtraRebirthAttributePoints(121, false) != 1
                || ExtraRebirthAttributePoints(129, false) != 45
                || ExtraRebirthAttributePoints(130, false) != 55)
                throw new System.InvalidOperationException("Era 1 standard rebirth attribute table self-test failed.");

            if (ExtraRebirthAttributePoints(110, true) != 0
                || ExtraRebirthAttributePoints(112, true) != 1
                || ExtraRebirthAttributePoints(120, true) != 15
                || ExtraRebirthAttributePoints(129, true) != 45
                || ExtraRebirthAttributePoints(130, true) != 55)
                throw new System.InvalidOperationException("Era 1 Water Taoist attribute table self-test failed.");

            System.Console.WriteLine("ERA1 PROGRESSION SELFTEST PASS");
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
