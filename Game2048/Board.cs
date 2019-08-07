using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game2048
{
	public class Board : ICloneable
	{
		private readonly Random _random = new Random();
		private readonly ulong[,] _board;
		public int NumOfRows => _board.GetLength(0);
		public int NumOfColumns => _board.GetLength(1);
		public Board(int numOfRows, int numOfCols)
		{
			_board = new ulong[numOfRows, numOfCols];
		}

		public void SetValue(int x, int y, ulong value)
		{
			_board[x, y] = value;
		}

		public ulong GetValue(int x, int y)
		{
			return _board[x, y];
		}
		public void PutNewValue()
		{
			// Find all empty slots
			var emptySlots = new List<Tuple<int, int>>();
			for (var iRow = 0; iRow < NumOfRows; iRow++)
			{
				for (var iCol = 0; iCol < NumOfColumns; iCol++)
				{
					if (this.GetValue(iRow, iCol) == 0)
					{
						emptySlots.Add(new Tuple<int, int>(iRow, iCol));
					}
				}
			}

			// We should have at least 1 empty slot. Since we know the user is not dead
			var iSlot = _random.Next(0, emptySlots.Count); // randomly pick an empty slot
			var value = _random.Next(0, 100) < 95 ? (ulong)2 : (ulong)4; // randomly pick 2 (with 95% chance) or 4 (rest of the chance)
			this.SetValue(emptySlots[iSlot].Item1, emptySlots[iSlot].Item2, value);
		}

		public bool Move(Direction direction, out ulong score)
		{
			bool hasUpdated = false;
			score = 0;
			bool isAlongRow = direction == Direction.Left || direction == Direction.Right;

			// Should we process inner dimension in increasing index order?
			bool isIncreasing = direction == Direction.Left || direction == Direction.Up;

			int outterCount = isAlongRow ? NumOfRows : NumOfColumns;
			int innerCount = isAlongRow ? NumOfColumns : NumOfRows;
			int innerStart = isIncreasing ? 0 : innerCount - 1;
			int innerEnd = isIncreasing ? innerCount - 1 : 0;

			Func<int, int> drop = isIncreasing
				? new Func<int, int>(innerIndex => innerIndex - 1)
				: new Func<int, int>(innerIndex => innerIndex + 1);

			Func<int, int> reverseDrop = isIncreasing
				? new Func<int, int>(innerIndex => innerIndex + 1)
				: new Func<int, int>(innerIndex => innerIndex - 1);

			Func<int, int, ulong> getValue = isAlongRow
				? new Func<int, int, ulong>(GetValue)
				: new Func<int, int, ulong>((i, j) => GetValue(j, i));

			Action<int, int, ulong> setValue = isAlongRow
				? new Action<int, int, ulong>(SetValue)
				: new Action<int, int, ulong>((i, j, v) => SetValue(j, i, v));

			Func<int, bool> innerCondition = index => Math.Min(innerStart, innerEnd) <= index && index <= Math.Max(innerStart, innerEnd);

			for (var i = 0; i < outterCount; i++)
			{
				for (var j = innerStart; innerCondition(j); j = reverseDrop(j))
				{
					if (getValue(i, j) == 0)
					{
						continue;
					}

					int newJ = j;
					do
					{
						newJ = drop(newJ);
					}
					// Continue probing along as long as we haven't hit the boundary and the new position isn't occupied
					while (innerCondition(newJ) && getValue(i, newJ) == 0);


					if (innerCondition(newJ) && getValue(i, newJ) == getValue(i, j))
					{
						// We did not hit the canvas boundary (we hit a node) AND no previous merge occurred AND the nodes' values are the same
						// Let's merge
						ulong newValue = getValue(i, newJ) * 2;
						setValue(i, newJ, newValue);
						setValue(i, j, 0);

						hasUpdated = true;
						score += newValue;
					}
					else
					{
						// Reached the boundary OR...
						// we hit a node with different value OR...
						// we hit a node with same value BUT a prevous merge had occurred
						//
						// Simply stack along
						newJ = reverseDrop(newJ); // reverse back to its valid position
						if (newJ != j)
						{
							hasUpdated = true;
						}

						ulong value = getValue(i, j);
						setValue(i, j, 0);
						setValue(i, newJ, value);
					}
				}
			}

			return hasUpdated;
		}

		public object Clone()
		{
			var board = new Board(NumOfRows, NumOfColumns);
			for (var i = 0; i < NumOfRows; i++)
			{
				for (var j = 0; j < NumOfColumns; j++)
				{
					board.SetValue(i, j, GetValue(i, j));
				}
			}

			return board;
		}
	}
}
