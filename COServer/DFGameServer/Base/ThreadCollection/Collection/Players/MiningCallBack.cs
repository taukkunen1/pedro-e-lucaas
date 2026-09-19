using static GameServer.Base.Mining.Process;
namespace GameServer.Threading
{
	/// <summary>
	/// Controller for the player thread.
	/// </summary>
	public static class MiningCallBack
	{
		/// <summary>
		/// Handles the thread.
		/// </summary>
		public static void Handle(Client.GameClient player, int time)
		{
			//PlayerCollection
			//	.ForEach(player =>
			//	{
			//		try
			//		{
			Handler(player);
			//		}
			//		catch
			//		{
			//UmbralForm.GUI.WriteLine("PlayerCollection > ItemsCallBack Thread Failure. [" + player.Player.Name + "]");
			//player.Socket.Disconnect();
			//		}
			//	});
		}
	}
}
