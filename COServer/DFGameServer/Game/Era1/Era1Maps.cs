namespace GameServer.Game.Era1
{
    /// <summary>
    /// Mapas que nao existiam no Patch 5017. O cliente 5695 ainda tem os arquivos,
    /// entao o bloqueio e' do lado do servidor: ninguem teleporta para eles e quem
    /// estiver salvo dentro volta para Twin City.
    /// </summary>
    public static class Era1Maps
    {
        public const uint SafeMap = 1002;
        public const ushort SafeX = 428;
        public const ushort SafeY = 378;

        public static bool IsBlocked(uint mapId)
        {
            switch (mapId)
            {
                // Frozen Grotto: andares 1-2 no Patch 5155 (jul/2009), 3-6 no Patch 5250 (abr/2010).
                case 1762:
                case 1926:
                case 1927:
                case 1999:
                case 2054:
                case 2055:
                case 2056:
                // Horse Racing (Patch 5212, fev/2010).
                case 1950:
                    return true;
            }
            return false;
        }

        public static void RunSelfTest()
        {
            if (!IsBlocked(1762) || !IsBlocked(2056) || !IsBlocked(1950) || IsBlocked(1002) || IsBlocked(1015))
                throw new System.InvalidOperationException("Era 1 map gate self-test failed.");
            System.Console.WriteLine("ERA1 MAPS SELFTEST PASS");
        }
    }
}
