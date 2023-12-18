<Query Kind="Statements" />

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample1.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

bool debug = false;
/*
	Sample data:

.|...\....
|.-.\.....
.....|-...
........|.
..........
.........\
..../.\\..
.-.-/..|..
.|....-|.\
..//.|....

*/

var data = GetData();

var startTime = Util.ElapsedTime;
{
	DisplayData(data);

	var beam = new Beam(BeamDirection.Right, new Point(0, 0));
	var beams = FillBeams(data, beam);
	
	Console.WriteLine($"Challenge 16.1 - Result: {beams.Select(b => b.Point).Distinct().Count()}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	var maxFill = 0;
	for (int x = 0; x < data.Width; x++)
	{
		var beams = FillBeams(data, new Beam(BeamDirection.Down, new Point(x, 0)));
		var fill = beams.Select(b => b.Point).Distinct().Count();
		maxFill = Math.Max(maxFill, fill);

		beams = FillBeams(data, new Beam(BeamDirection.Up, new Point(x, data.Height - 1)));
		fill = beams.Select(b => b.Point).Distinct().Count();
		maxFill = Math.Max(maxFill, fill);
	}
	for (int y = 0; y < data.Height; y++)
	{
		var beams = FillBeams(data, new Beam(BeamDirection.Right, new Point(0, y)));
		var fill = beams.Select(b => b.Point).Distinct().Count();
		maxFill = Math.Max(maxFill, fill);

		beams = FillBeams(data, new Beam(BeamDirection.Left, new Point(data.Width - 1, y)));
		fill = beams.Select(b => b.Point).Distinct().Count();
		maxFill = Math.Max(maxFill, fill);
	}
	Console.WriteLine($"Challenge 16.2 - Result: {maxFill}");
}
(Util.ElapsedTime - startTime).Dump();

List<Beam> FillBeams(Data data, Beam initialBeam)
{
	var beams = new List<Beam>();
	var heads = new Queue<Beam>();

	beams.Add(initialBeam);
	heads.Enqueue(initialBeam);

	while (heads.Any())
	{
		var head = heads.Dequeue();
		var position = data.Board[head.Point.X, head.Point.Y];

		// Do above going up
		if (head.Point.Y > 0 && (
			(head.Direction == BeamDirection.Up && position == PositionType.Space)
			|| (head.Direction != BeamDirection.Down && position == PositionType.UpDownSplit)
			|| (head.Direction == BeamDirection.Right && position == PositionType.UpRightBottomLeftMirror)
			|| (head.Direction == BeamDirection.Left && position == PositionType.UpLeftBottomRightMirror)
			))
		{
			var newHead = new Beam(BeamDirection.Up, new Point(head.Point.X, head.Point.Y - 1));
			if (!beams.Contains(newHead))
			{
				beams.Add(newHead);
				heads.Enqueue(newHead);
			}
		};
		// Do below going down
		if (head.Point.Y < data.Height - 1 && (
			(head.Direction == BeamDirection.Down && position == PositionType.Space)
			|| (head.Direction != BeamDirection.Up && position == PositionType.UpDownSplit)
			|| (head.Direction == BeamDirection.Left && position == PositionType.UpRightBottomLeftMirror)
			|| (head.Direction == BeamDirection.Right && position == PositionType.UpLeftBottomRightMirror)
			))
		{
			var newHead = new Beam(BeamDirection.Down, new Point(head.Point.X, head.Point.Y + 1));
			if (!beams.Contains(newHead))
			{
				beams.Add(newHead);
				heads.Enqueue(newHead);
			}
		};
		// Do left going left
		if (head.Point.X > 0 && (
			(head.Direction == BeamDirection.Left && position == PositionType.Space)
			|| (head.Direction != BeamDirection.Right && position == PositionType.LeftRightSplit)
			|| (head.Direction == BeamDirection.Down && position == PositionType.UpRightBottomLeftMirror)
			|| (head.Direction == BeamDirection.Up && position == PositionType.UpLeftBottomRightMirror)
			))
		{
			var newHead = new Beam(BeamDirection.Left, new Point(head.Point.X - 1, head.Point.Y));
			if (!beams.Contains(newHead))
			{
				beams.Add(newHead);
				heads.Enqueue(newHead);
			}
		};
		// Do right going right
		if (head.Point.X < data.Width - 1 && (
			(head.Direction == BeamDirection.Right && position == PositionType.Space)
			|| (head.Direction != BeamDirection.Left && position == PositionType.LeftRightSplit)
			|| (head.Direction == BeamDirection.Up && position == PositionType.UpRightBottomLeftMirror)
			|| (head.Direction == BeamDirection.Down && position == PositionType.UpLeftBottomRightMirror)
			))
		{
			var newHead = new Beam(BeamDirection.Right, new Point(head.Point.X + 1, head.Point.Y));
			if (!beams.Contains(newHead))
			{
				beams.Add(newHead);
				heads.Enqueue(newHead);
			}
		};

		DisplayData(data, beams);
	}
	
	return beams;
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
				'|' => PositionType.UpDownSplit,
				'-' => PositionType.LeftRightSplit,
				'\\' => PositionType.UpLeftBottomRightMirror,
				'/' => PositionType.UpRightBottomLeftMirror,
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

void DisplayData(Data data, List<Beam> beams = null)
{
	if (debug)
	{
		beams = beams ?? new();
		for (int y = 0; y < data.Height; y++)
		{
			for (int x = 0; x < data.Width; x++)
			{
				if( beams.Any( b => b.Point.X == x && b.Point.Y == y) ) {
					Console.Write('#');
					continue;
				}
				Console.Write(data.Board[x, y] switch
				{
					PositionType.UpDownSplit => '|',
					PositionType.LeftRightSplit => '-',
					PositionType.UpLeftBottomRightMirror => '\\',
					PositionType.UpRightBottomLeftMirror => '/',
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

record Beam(BeamDirection Direction, Point Point);
record Point(int X, int Y);

enum BeamDirection {
	Up,
	Down,
	Left,
	Right
}

enum PositionType
{
	Space,
	UpDownSplit,
	LeftRightSplit,
	UpLeftBottomRightMirror,
	UpRightBottomLeftMirror
}