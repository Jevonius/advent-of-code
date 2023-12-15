<Query Kind="Statements">
  <Namespace>System.Drawing</Namespace>
</Query>

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample1.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

bool debug = false;

var part2Loops = 1000000000;
/*
	Sample data:

OOOO.#.O..
OO..#....#
OO..O##..O
O..#.OO...
........#.
..#....#.#
..O..#.O.O
..O.......
#....###..
#....#....

*/

var startTime = Util.ElapsedTime;
{
	var data = GetData();

	DisplayData(data);
	DoTiltUp(data);
	DisplayData(data);
	
	var sum = GetSum(data);
	Console.WriteLine($"Challenge 14.1 - Total: {sum}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	var data = GetData();
	
	var seen = new Dictionary<string, (int Sum, int Index)>();
	(int Sum, int Index) repeatmatch;
	
	while (true)
	{
		DoTiltUp(data);
		DoTiltLeft(data);
		DoTiltDown(data);
		DoTiltRight(data);

		var boardHash = GetHash(data);
		if (seen.ContainsKey(boardHash))
		{
			repeatmatch = seen[boardHash];
			break;
		}
		seen.Add(boardHash, new (GetSum(data), seen.Count ));
	}

	var firstOffset = repeatmatch.Index;
	var loopLength = seen.Count - firstOffset;

	var loopCountAdditional = ((part2Loops - 1 - firstOffset) % loopLength);
	
	var match = seen.First(kv => kv.Value.Index == loopCountAdditional + firstOffset);

	Console.WriteLine($"Challenge 14.2 - Total: {match.Value.Sum}");
}
(Util.ElapsedTime - startTime).Dump();

string GetHash(Data data)
{
	var hash = new StringBuilder();
	foreach( var value in data.Board ){
		hash.Append((int) value);
	}
	return hash.ToString();
}

int GetSum(Data data) {
	var sum = 0;
	for (int y = 0; y < data.Height; y++)
	{
		for (int x = 0; x < data.Width; x++)
		{
			if( data.Board[x, y] == PositionType.Round ) {
				sum += data.Height - y;
			}
		}
	};
	return sum;
}

void DoTiltUp(Data data)
{
	var board = data.Board;
	for (int y = 1; y < data.Height; y++)
	{
		for (int x = 0; x < data.Width; x++)
		{
			if( board[x, y] == PositionType.Round ) {
				var swapY = y;
				while (swapY > 0 && board[x, swapY - 1] == PositionType.Space)
				{
					swapY--;
				}
				if (swapY < y)
				{
					board[x, y] = PositionType.Space;
					board[x, swapY] = PositionType.Round;
				}
			}
		}
	}
}
void DoTiltDown(Data data)
{
	var board = data.Board;
	for (int y = data.Height - 1; y >= 0; y--)
	{
		for (int x = 0; x < data.Width; x++)
		{
			if( board[x, y] == PositionType.Round ) {
				var swapY = y;
				while (swapY < data.Height - 1 && board[x, swapY + 1] == PositionType.Space)
				{
					swapY++;
				}
				if (swapY > y)
				{
					board[x, y] = PositionType.Space;
					board[x, swapY] = PositionType.Round;
				}
			}
		}
	}
}
void DoTiltLeft(Data data)
{
	var board = data.Board;
	for (int y = 0; y < data.Height; y++)
	{
		for (int x = 1; x < data.Width; x++)
		{
			if (board[x, y] == PositionType.Round)
			{
				var swapX = x;
				while (swapX > 0 && board[swapX - 1, y] == PositionType.Space)
				{
					swapX--;
				}
				if (swapX < x)
				{
					board[x, y] = PositionType.Space;
					board[swapX, y] = PositionType.Round;
				}
			}
		}
	}
}
void DoTiltRight(Data data)
{
	var board = data.Board;
	for (int y = 0; y < data.Height; y++)
	{
		for (int x = data.Width - 1; x >= 0; x--)
		{
			if (board[x, y] == PositionType.Round)
			{
				var swapX = x;
				while (swapX < data.Width - 1 && board[swapX + 1, y] == PositionType.Space)
				{
					swapX++;
				}
				if (swapX > x)
				{
					board[x, y] = PositionType.Space;
					board[swapX, y] = PositionType.Round;
				}
			}
		}
	}
}

Data GetData()
{
	var width = rawLines[0].Length;
	var height = rawLines.Length;
	var board = new PositionType[width, height];

	for (int y = 0; y < height; y++)
	{
		var line = rawLines[y];
		for (int x = 0; x < width; x++)
		{
			board[x, y] = line[x] switch
			{
				'O' => PositionType.Round,
				'#' => PositionType.Cube,
				_ => PositionType.Space
			};
		}
	}

	return new Data
	{
		Width = width,
		Height = height,
		Board = board
	};
}

void DisplayData(Data data)
{
	if (debug)
	{
		for (int y = 0; y < data.Height; y++)
		{
			for (int x = 0; x < data.Width; x++)
			{
				Console.Write(data.Board[x,y] switch
				{
					PositionType.Round => 'O',
					PositionType.Cube => '#',
					_ => '.'
				});
			}
			Console.WriteLine();
		}
		Console.WriteLine();
	}
}

class Data
{
	public int Width { get; set; }
	public int Height { get; set; }
	public PositionType[,] Board { get; init; }
}

enum PositionType
{
	Space,
	Round,
	Cube
}