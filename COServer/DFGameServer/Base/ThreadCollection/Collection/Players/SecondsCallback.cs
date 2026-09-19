using static GameServer.Client.PoolProcesses;
namespace GameServer.Threading
{
	/// <summary>
	/// Controller for the player thread.
	/// </summary>
	public static class SecondsCallback
	{
		/// <summary>
		/// Handles the thread.
		/// </summary>
		public static void Handle(Client.GameClient player, int time)
		{
			//PlayerCollection
			//	.ForEach(player =>
			//	{
			//	try
			//	{
			SecondsCallback(player);
			//	}
			//	catch
			//	{
			//		UmbralForm.GUI.WriteLine("PlayerCollection > SecondsCallback Thread Failure. [" + player.Player.Name + "]");
			//		player.Socket.Disconnect();
			//	}
			//});
		}
	}
}
