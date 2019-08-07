using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Game2048
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Game game = new Game();
			game.Run();
		}
	}

	internal class Game
	{
		public ulong Score { get; private set; }
		public Board Board { get; private set; }

		private readonly int nRows;
		private readonly int nCols;

		public Game()
		{
			Board = new Board(4, 4);
			nRows = Board.NumOfRows;
			nCols = Board.NumOfColumns;
			Score = 0;
		}

		public void Run()
		{
			bool hasUpdated = true;
			do
			{
				if (hasUpdated)
				{
					Board.PutNewValue();
				}

				Display();

				if (IsDead())
				{
					using (new ColorOutput(ConsoleColor.Red))
					{
						Console.WriteLine("YOU ARE DEAD!!!");
						break;
					}
				}

				Console.WriteLine("Use arrow keys to move the tiles. Press Ctrl-C to exit.");
				ConsoleKeyInfo input = Console.ReadKey(true); // BLOCKING TO WAIT FOR INPUT
				Console.WriteLine(input.Key.ToString());

				switch (input.Key)
				{
					case ConsoleKey.UpArrow:
						hasUpdated = Update(Direction.Up);
						break;

					case ConsoleKey.DownArrow:
						hasUpdated = Update(Direction.Down);
						break;

					case ConsoleKey.LeftArrow:
						hasUpdated = Update(Direction.Left);
						break;

					case ConsoleKey.RightArrow:
						hasUpdated = Update(Direction.Right);
						break;

					default:
						hasUpdated = false;
						break;
				}
			}
			while (true); // use CTRL-C to break out of loop

			Console.WriteLine("Press any key to quit...");
			Console.Read();
		}

		private static ConsoleColor GetNumberColor(ulong num)
		{
			switch (num)
			{
				case 0:
					return ConsoleColor.DarkGray;

				case 2:
					return ConsoleColor.Cyan;

				case 4:
					return ConsoleColor.Magenta;

				case 8:
					return ConsoleColor.Red;

				case 16:
					return ConsoleColor.Green;

				case 32:
					return ConsoleColor.Yellow;

				case 64:
					return ConsoleColor.Yellow;

				case 128:
					return ConsoleColor.DarkCyan;

				case 256:
					return ConsoleColor.Cyan;

				case 512:
					return ConsoleColor.DarkMagenta;

				case 1024:
					return ConsoleColor.Magenta;

				default:
					return ConsoleColor.Red;
			}
		}

		private static bool Update(Board board, Direction direction, out ulong score)
		{
			score = 0;
			return board.Move(direction, out score);
		}

		private bool Update(Direction dir)
		{
			ulong score;
			bool isUpdated = Game.Update(this.Board, dir, out score);
			this.Score += score;
			return isUpdated;
		}

		private bool IsDead()
		{
			ulong score;
			foreach (Direction dir in new Direction[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right })
			{
				var clone = (Board)Board.Clone();
				if (Game.Update(clone, dir, out score))
				{
					return false;
				}
			}

			// tried all directions. none worked.
			return true;
		}

		private void Display()
		{
			Console.Clear();
			Console.WriteLine();
			for (int i = 0; i < nRows; i++)
			{
				for (int j = 0; j < nCols; j++)
				{
					using (new ColorOutput(Game.GetNumberColor(Board.GetValue(i, j))))
					{
						Console.Write($"{Board.GetValue(i, j),6}");
					}
				}

				Console.WriteLine();
				Console.WriteLine();
			}

			Console.WriteLine("Score: {0}", this.Score);
			Console.WriteLine();
		}

		#region Utility Classes



		private class ColorOutput : IDisposable
		{
			public ColorOutput(ConsoleColor fg, ConsoleColor bg = ConsoleColor.Black)
			{
				Console.ForegroundColor = fg;
				Console.BackgroundColor = bg;
			}

			public void Dispose()
			{
				Console.ResetColor();
			}
		}

		#endregion Utility Classes
	}
}