<Query Kind="Statements">
  <Namespace>System.Drawing</Namespace>
</Query>

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample1.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

bool debug = false;

var part2Loops = 1000000000;
/*
	Sample data:

O....#....
O.OO#....#
.....##...
OO.#O....O
.O.....O#.
O.#..O.#.#
..O..#O..O
.......O..
#....###..
#OO..#....

*/

var startTime = Util.ElapsedTime;
{
	var data = GetData();
	var moved = false;
	DisplayData(data);
	do
	{
		moved = false;
		foreach (var rock in data.Rocks.Where(IsRound))
		{
			if (rock.Y > 1 && !data.Rocks.Any(r => r.X == rock.X && r.Y == rock.Y - 1))
			{
				rock.Y -= 1;
				moved = true;
			}
		}
	} while (moved);
	DisplayData(data);
	var sum = data.Rocks.Where(IsRound).Sum(r => 1 + data.Height - r.Y);
	Console.WriteLine($"Challenge 14.1 - Total: {sum}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	var data = GetData();
	var moved = false;
	
	var seen = new List<string>();;
	var matchFound = false;
	while (!matchFound)
	{
		DoTilt(Direction.Up, data);
		DoTilt(Direction.Left, data);
		DoTilt(Direction.Down, data);
		DoTilt(Direction.Right, data);

		var positions = string.Join('|', data.Rocks.Where(IsRound).OrderBy(r => r.Y).ThenBy(r => r.X).Select(r => $"{{{r.X},{r.Y}}}"));
		if (seen.Contains(positions))
		{
			matchFound = true;
		}
		seen.Add(positions);
	}
	
	var firstOffset = seen.IndexOf(seen.Last());
	var loopLength = seen.Count - firstOffset - 1;
	
	var loopCountAdditional = ((part2Loops - 1 - firstOffset) % loopLength);
	
	var match = seen[loopCountAdditional + firstOffset];

	// USe Range???
	var newData = match.Split('|').Select(pair => 1 + data.Height - int.Parse(pair.Substring(pair.IndexOf(',') + 1, pair.IndexOf('}') - (pair.IndexOf(',') + 1))));

	if (debug)
	{
		seen.Select(s => s.Split('|').Select(pair => 1 + data.Height - int.Parse(pair.Substring(pair.IndexOf(',') + 1, pair.IndexOf('}') - (pair.IndexOf(',') + 1))))).Select(s => s.Sum()).Dump();
	}
	var sum = newData.Sum();
	Console.WriteLine($"Challenge 14.2 - Total: {sum}");
}
(Util.ElapsedTime - startTime).Dump();

void DoTilt(Direction direction, Data data)
{
	var moveable = data.Rocks.Where(IsRound);

	var sorted = direction switch
	{
		Direction.Up => moveable.OrderBy(r => r.Y),
		Direction.Down => moveable.OrderByDescending(r => r.Y),
		Direction.Left => moveable.OrderBy(r => r.X),
		Direction.Right => moveable.OrderByDescending(r => r.X),
		_ => throw new InvalidDataException()
	};

	foreach (var rock in sorted)
	{
		while (
			(direction == Direction.Up && rock.Y > 1 && !data.Rocks.Any(r => r.X == rock.X && r.Y == rock.Y - 1))
			|| (direction == Direction.Down && rock.Y < data.Height && !data.Rocks.Any(r => r.X == rock.X && r.Y == rock.Y + 1))
			|| (direction == Direction.Left && rock.X > 1 && !data.Rocks.Any(r => r.X == rock.X - 1 && r.Y == rock.Y))
			|| (direction == Direction.Right && rock.X < data.Width && !data.Rocks.Any(r => r.X == rock.X + 1 && r.Y == rock.Y))
		)
		{
			switch (direction)
			{
				case Direction.Up:
					rock.Y -= 1;
					break;
				case Direction.Down:
					rock.Y += 1;
					break;
				case Direction.Left:
					rock.X -= 1;
					break;
				case Direction.Right:
					rock.X += 1;
					break;
			}
		}
	}
}

Data GetData() {
	var width = rawLines[0].Length;
	var height = rawLines.Length;
	var data = new Data {
		Width = width,
		Height = height
	};
	for (int y = 0; y < height; y++)
	{
		var line = rawLines[y];
		for (int x = 0; x < width; x++)
		{
			var thing = line[x];
			if (thing == 'O')
			{
				data.Rocks.Add(new Rock
				{
					X = x + 1,
					Y = y + 1,
					Shape = RockShape.Round
				});
			} else if (thing == '#')
			{
				data.Rocks.Add(new Rock
				{
					X = x + 1,
					Y = y + 1,
					Shape = RockShape.Cube
				});
			}
		}
	}	
	return data;
}

void DisplayData(Data data)
{
	if (debug)
	{
		for (int y = 1; y <= data.Height; y++)
		{
			for (int x = 1; x <= data.Width; x++)
			{
				var rock = data.Rocks.FirstOrDefault(r => r.X == x && r.Y == y);
				Console.Write(rock?.Shape switch
				{
					RockShape.Round => 'O',
					RockShape.Cube => '#',
					_ => '.'
				});
			}
			Console.WriteLine();
		}
		Console.WriteLine();
	}
}

bool IsRound(Rock rock) => rock.Shape == RockShape.Round;

class Data
{
	public int Width { get; set; }
	public int Height { get; set; }
	public List<Rock> Rocks { get; } = new();
}

class Rock
{
	public RockShape Shape { get; set; }
	public int X { get; set; }
	public int Y { get; set; }
}

enum RockShape
{
	Round,
	Cube
}

enum Direction
{
	Up,
	Down,
	Left,
	Right
}

record Position(int X, int Y);