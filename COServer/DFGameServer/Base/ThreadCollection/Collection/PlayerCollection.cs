using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Threading
{
	public static class PlayerCollection
	{
		/// <summary>
		/// Performs an iteration of all players.
		/// </summary>
		/// <param name="iterate">The iteration action.</param>
		public static void ForEach(Action<global::GameServer.Client.GameClient> iterate)
		{
			try
			{
				if (iterate != null)
				{
					foreach (var player in Pool.GamePoll.Values)
					{
						iterate(player);
					}
				}
			}
			catch
			{
				Console.WriteLine("Player Collection Failure.");
			}
		}
	}
}
