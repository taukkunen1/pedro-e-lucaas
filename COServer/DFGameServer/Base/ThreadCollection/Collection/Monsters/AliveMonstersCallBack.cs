using static GameServer.Game.MsgMonster.PoolProcesses;
namespace GameServer.Threading
{
    /// <summary>
    /// Controller for the player thread.
    /// </summary>
    public static class AliveMonstersCallback
    {
        /// <summary>
        /// Handles the thread.
        /// </summary>
        public static void Handle(Client.GameClient player, int time)
        {
            //PlayerCollection
            //    .ForEach(player =>
            //    {
            //        try
            //        {
            AliveMonstersCallback(player);
            //    }
            //    catch
            //    {
            //        UmbralForm.GUI.WriteLine("MonsterCollection > AliveMonstersCallback Thread Failure. [" + player.Player.Name + "]");
            //        player.Socket.Disconnect();
            //    }
            //});
        }
    }
}