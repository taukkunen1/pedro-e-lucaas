using GameServer.Role;
using Poker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.MsgServer.AttackHandler.Algoritms
{
	public unsafe class RangeMove
	{
		public class Coords
		{
			public int X;
			public int Y;
		}
		public List<Coords> MoveCoords(ushort X, ushort Y, ushort XX, ushort YY, byte useDistance = 0)
		{
			List<Coords> lis = new List<Coords>();
			Flags.ConquerAngle angle = GetAngle(X, Y, XX, YY);
			byte distance = (byte)GetDistance(X, Y, XX, YY);
			if (useDistance != 0)
				distance = useDistance;
			int dx = XX - X, dy = YY - Y, steps, k;
			float xincrement, yincrement, x = X, y = Y;

			if (Math.Abs(dx) > Math.Abs(dy))
				steps = Math.Abs(dx);
			else
				steps = Math.Abs(dy);
			xincrement = dx / (float)steps;
			yincrement = dy / (float)steps;
			lis.Add(new Coords() { X = (int)Math.Round(x), Y = (int)Math.Round(y) });
			for (k = 0; k < distance; k++)
			{
				x += xincrement;
				y += yincrement;
				lis.Add(new Coords() { X = (int)Math.Round(x), Y = (int)Math.Round(y) });
			}
			return lis;
		}
		public Flags.ConquerAngle GetAngle(ushort X, ushort Y, ushort X2, ushort Y2)
		{
			double direction = 0;
			double AddX = X2 - X;
			double AddY = Y2 - Y;
			double r = (double)Math.Atan2(AddY, AddX);
			if (r < 0) r += (double)Math.PI * 2;
			direction = 360 - (r * 180 / (double)Math.PI);
			byte Dir = (byte)((7 - (Math.Floor(direction) / 45 % 8)) - 1 % 8);
			return (Flags.ConquerAngle)(byte)((int)Dir % 8);
		}
		public short GetDistance(ushort X, ushort Y, ushort X2, ushort Y2)
		{
			short x = 0;
			short y = 0;
			if (X >= X2) x = (short)(X - X2);
			else if (X2 >= X) x = (short)(X2 - X);
			if (Y >= Y2) y = (short)(Y - Y2);
			else if (Y2 >= Y) y = (short)(Y2 - Y);
			if (x > y)
				return x;
			else
				return y;
		}
		public bool InRange(ushort X, ushort Y, byte Range, List<Coords> bas)
		{
			foreach (Coords line in bas)
			{
				byte distance = (byte)GetDistance((ushort)X, (ushort)Y, (ushort)line.X, (ushort)line.Y);
				if (distance <= Range)
					return true;
			}
			return false;
		}
	}
}
