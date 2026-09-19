using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameServer.Client.PoolProcesses;
namespace GameServer.Threading
{
	/// <summary>
	/// Controller for the player thread.
	/// </summary>
	public static class BufferCallback
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
			BuffersCallback(player);
			//	}
			//	catch
			//	{
			//		UmbralForm.GUI.WriteLine("PlayerCollection > BufferCallback Thread Failure. [" + player.Player.Name + "]");
			//		player.Socket.Disconnect();
			//	}
			//});
		}
	}
}
