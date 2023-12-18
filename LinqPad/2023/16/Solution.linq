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
	var testPaths = new List<Beam>();
	for (int x = 0; x < data.Width; x++)
	{
		testPaths.Add(new Beam(BeamDirection.Down, new Point(x, 0)));
		testPaths.Add(new Beam(BeamDirection.Up, new Point(x, data.Height - 1)));
	}
	for (int y = 0; y < data.Height; y++)
	{
		testPaths.Add(new Beam(BeamDirection.Right, new Point(0, y)));
		testPaths.Add(new Beam(BeamDirection.Left, new Point(data.Width - 1, y)));
	}

	var cache = new Dictionary<Beam, BeamPath>();
	testPaths.ForEach(tp => BuildBeamPath(data, tp, cache));
	var flatten = new Dictionary<Beam, List<Point>>();

	var max = testPaths.Max(tp => Flatten(flatten, cache, tp).Count);

	Console.WriteLine($"Challenge 16.2 - Result: {max}");
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

		var nextHeads = GetNext(data, head).ToList();

		foreach (var nextHead in GetNext(data, head))
		{
			if (!beams.Contains(nextHead))
			{
				beams.Add(nextHead);
				heads.Enqueue(nextHead);
			}
		}

		DisplayData(data, beams.Select(b => b.Point));
	}
	
	return beams;
}

IEnumerable<Beam> GetNext(Data data, Beam head)
{
	var position = data.Board[head.Point.X, head.Point.Y];

	// Do above going up
	if (head.Point.Y > 0 && (
		(head.Direction == BeamDirection.Up && position == PositionType.Space)
		|| (head.Direction != BeamDirection.Down && position == PositionType.UpDownSplit)
		|| (head.Direction == BeamDirection.Right && position == PositionType.UpRightBottomLeftMirror)
		|| (head.Direction == BeamDirection.Left && position == PositionType.UpLeftBottomRightMirror)
		))
	{
		yield return new Beam(BeamDirection.Up, new Point(head.Point.X, head.Point.Y - 1));
	};
	// Do below going down
	if (head.Point.Y < data.Height - 1 && (
		(head.Direction == BeamDirection.Down && position == PositionType.Space)
		|| (head.Direction != BeamDirection.Up && position == PositionType.UpDownSplit)
		|| (head.Direction == BeamDirection.Left && position == PositionType.UpRightBottomLeftMirror)
		|| (head.Direction == BeamDirection.Right && position == PositionType.UpLeftBottomRightMirror)
		))
	{
		yield return new Beam(BeamDirection.Down, new Point(head.Point.X, head.Point.Y + 1));
	};
	// Do left going left
	if (head.Point.X > 0 && (
		(head.Direction == BeamDirection.Left && position == PositionType.Space)
		|| (head.Direction != BeamDirection.Right && position == PositionType.LeftRightSplit)
		|| (head.Direction == BeamDirection.Down && position == PositionType.UpRightBottomLeftMirror)
		|| (head.Direction == BeamDirection.Up && position == PositionType.UpLeftBottomRightMirror)
		))
	{
		yield return new Beam(BeamDirection.Left, new Point(head.Point.X - 1, head.Point.Y));
	};
	// Do right going right
	if (head.Point.X < data.Width - 1 && (
		(head.Direction == BeamDirection.Right && position == PositionType.Space)
		|| (head.Direction != BeamDirection.Left && position == PositionType.LeftRightSplit)
		|| (head.Direction == BeamDirection.Up && position == PositionType.UpRightBottomLeftMirror)
		|| (head.Direction == BeamDirection.Down && position == PositionType.UpLeftBottomRightMirror)
		))
	{
		yield return new Beam(BeamDirection.Right, new Point(head.Point.X + 1, head.Point.Y));
	};
}

void BuildBeamPath(Data data, Beam source, Dictionary<Beam, BeamPath> cache)
{
	if (!cache.TryGetValue(source, out var path))
	{
		path = new BeamPath(source);
		path.Path.Add(source);
		cache.Add(source, path);
		FillPath(data, path, cache);
	}
}

void FillPath(Data data, BeamPath path, Dictionary<Beam, BeamPath> cache)
{
	var position = path.Path.Last();
	var current = data.Board[position.Point.X, position.Point.Y];
	while (current == PositionType.Space)
	{
		var next = GetNext(data, position).ToList();
		if (next.Count == 0)
		{
			// Hit an edge
			return;
		}
		// Can move
		position = next.First();
		if( !cache.ContainsKey(position) ) {
			cache.Add(position, path);
		}
		path.Path.Add(position);
		current = data.Board[position.Point.X, position.Point.Y];
	}
	if (current != PositionType.Space)
	{
		// Didn't go off an edge
		path.Next.AddRange(GetNext(data, position));
		foreach (var next in path.Next)
		{
			BuildBeamPath(data, next, cache);
		}
	}
}

List<Point> Flatten(Dictionary<Beam, List<Point>> flattenCache, Dictionary<Beam, BeamPath> beamCache, Beam beam, Stack<Beam> seen = null)
{
	// `isLoop` is a hack, there is an issue with flattenCache I can't quite trace but this fixes it (but is slow).
	seen = seen ?? new();
	var isLoop = seen.Contains(beam);
	seen.Push(beam);

	if (!flattenCache.TryGetValue(beam, out var points))
	{
		points = new List<Point>();
		var path = beamCache[beam];
		points.AddRange(path.Path.Select(p => p.Point));
		var end = path.Path.Last();
		var endPath = beamCache[end];

		if (isLoop)
		{
			flattenCache[beam] = points;
		}

		foreach (var next in endPath.Next)
		{
			points.AddRange(Flatten(flattenCache, beamCache, next, seen).Except(points));
		}
	}

	seen.Pop();
	return points;
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

void DisplayData(Data data, IEnumerable<Point> point = null, bool forceDebug = false)
{
	if (debug || forceDebug)
	{
		point = point ?? new List<Point>();
		for (int y = 0; y < data.Height; y++)
		{
			for (int x = 0; x < data.Width; x++)
			{
				if( point.Any( b => b.X == x && b.Y == y) ) {
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
record BeamPath(Beam Source)
{
	public List<Beam> Path { get; } = new();
	public List<Beam> Next { get;} = new();
	//public Beam Target { get; set; }
}

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