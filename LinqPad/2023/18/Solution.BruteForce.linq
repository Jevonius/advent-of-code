<Query Kind="Statements">
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Drawing</Namespace>
</Query>

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample1.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

var part1 = true;
bool debug = false;
/*
	Sample data:

R 6 (#70c710)
D 5 (#0dc571)
L 2 (#5713f0)
D 2 (#d2c081)
R 2 (#59c680)
D 2 (#411b91)
L 5 (#8ceee2)
U 2 (#caa173)
L 1 (#1b58a2)
U 2 (#caa171)
R 2 (#7807d2)
U 3 (#a77fa3)
L 2 (#015232)
U 2 (#7a21e3)

Similar to 2023 Day 10? For Part 1 yes, although slow.

*/

var startTime = Util.ElapsedTime;
if (part1)
{
	{
		var data = rawLines.Select(BuildRow1);

		var tiles = new List<Tile>();

		int turnCounter = 0; // at the end, clockwise = >0, anticlockwise = <0
		var direction = data.First().Direction;
		var positionX = 0;
		var positionY = 0;
		foreach (var move in data)
		{
			var turnDirection = GetTurn(direction, move.Direction);
			turnCounter += (int)turnDirection;

			direction = move.Direction;
			// Origin top-left
			var xAdjust = direction == Direction.Left ? -1 : direction == Direction.Right ? 1 : 0;
			var yAdjust = direction == Direction.Up ? -1 : direction == Direction.Down ? 1 : 0;
			for (int i = 0; i < move.Length; i++)
			{
				positionX += xAdjust;
				positionY += yAdjust;
				tiles.Add(new Tile(new Point(positionX, positionY), move.Direction));
			}
		}

		// Trace inside tiles
		var clockwise = turnCounter > 0;
		var insideTilesHash = new HashSet<Point>();
		for (var loopOffset = 0; loopOffset < tiles.Count; loopOffset++)
		{
			var move = tiles[loopOffset];
			var nextMove = tiles[(loopOffset + 1) % tiles.Count];
			var turn = GetTurn(move.Direction, nextMove.Direction);

			var adjacents = FindAdjacents(clockwise, move, turn);
			foreach (var adjacent in adjacents)
			{
				insideTilesHash.Add(adjacent);
			}
		}
		var loopPoints = tiles.Select(t => t.Position).ToList();
		var insideTiles = insideTilesHash.Except(loopPoints).ToList();


		// flood fill
		for (int insideOffset = 0; insideOffset < insideTiles.Count; insideOffset++)
		{
			var tile = insideTiles[insideOffset];
			Flood(tile, loopPoints, insideTiles, Direction.Up);
			Flood(tile, loopPoints, insideTiles, Direction.Down);
			Flood(tile, loopPoints, insideTiles, Direction.Left);
			Flood(tile, loopPoints, insideTiles, Direction.Right);
		}

		var total = tiles.Count + insideTiles.Count;
		Console.WriteLine($"Challenge 18.1 - Result: {total}");
	}
	(Util.ElapsedTime - startTime).Dump();
	Console.WriteLine();
}

startTime = Util.ElapsedTime;
{
	var data = rawLines.Select(BuildRow2).ToList();

	var currentX = 0;
	var currentY = 0;
	
	var points = new List<Point>();
	foreach (var move in data)
	{
		switch (move.Direction)
		{
			case Direction.Up:
				currentY -= move.Length;
				break;
			case Direction.Down:
				currentY += move.Length;
				break;
			case Direction.Left:
				currentX -= move.Length;
				break;
			case Direction.Right:
				currentX += move.Length;
				break;
		}
		points.Add(new Point(currentX, currentY));
	}
	
	if (debug)
	{
		points.Dump();
	}
	
	var area = CalculateArea(data, points);
	
	Console.WriteLine($"Challenge 18.2 - Result: {area}");
}
(Util.ElapsedTime - startTime).Dump();

TurnDirection GetTurn(Direction previousFacing, Direction currentFacing)
{
	switch (previousFacing)
	{
		case Direction.Up:
			if (currentFacing == Direction.Left) return TurnDirection.Left;
			if (currentFacing == Direction.Right) return TurnDirection.Right;
			return TurnDirection.None;
		case Direction.Down:
			if (currentFacing == Direction.Right) return TurnDirection.Left;
			if (currentFacing == Direction.Left) return TurnDirection.Right;
			return TurnDirection.None;
		case Direction.Left:
			if (currentFacing == Direction.Down) return TurnDirection.Left;
			if (currentFacing == Direction.Up) return TurnDirection.Right;
			return TurnDirection.None;
		case Direction.Right:
			if (currentFacing == Direction.Up) return TurnDirection.Left;
			if (currentFacing == Direction.Down) return TurnDirection.Right;
			return TurnDirection.None;
		default:
			throw new InvalidOperationException("Invalid turn data");
	}
}

IEnumerable<Point> FindAdjacents(bool clockwise, Tile move, TurnDirection turn)
{
	var position = move.Position;

	if (turn == TurnDirection.None)
	{
		switch (move.Direction)
		{
			case Direction.Up:
				yield return new Point(position.X + (clockwise ? 1 : -1), position.Y);
				break;
			case Direction.Down:
				yield return new Point(position.X + (clockwise ? -1 : 1), position.Y);
				break;
			case Direction.Left:
				yield return new Point(position.X, position.Y + (clockwise ? -1 : 1));
				break;
			case Direction.Right:
				yield return new Point(position.X, position.Y + (clockwise ? 1 : -1));
				break;
		}
	}
	else if ((clockwise && turn == TurnDirection.Left) || (!clockwise && turn == TurnDirection.Right))
	{
		// 3 Potential tiles - inner corner - obtuse
		switch (move.Direction)
		{
			case Direction.Up:
				yield return new Point(position.X + (clockwise ? 1 : -1), position.Y);
				yield return new Point(position.X + (clockwise ? 1 : -1), position.Y + (clockwise ? 1 : -1));
				yield return new Point(position.X, position.Y + (clockwise ? 1 : -1));
				break;
			case Direction.Down:
				yield return new Point(position.X + (clockwise ? -1 : 1), position.Y);
				yield return new Point(position.X + (clockwise ? -1 : 1), position.Y + (clockwise ? -1 : 1));
				yield return new Point(position.X, position.Y + (clockwise ? -1 : 1));
				break;
			case Direction.Left:
				yield return new Point(position.X + (clockwise ? 1 : -1), position.Y);
				yield return new Point(position.X + (clockwise ? 1 : -1), position.Y + (clockwise ? -1 : -1));
				yield return new Point(position.X, position.Y + (clockwise ? -1 : 1));
				break;
			case Direction.Right:
				yield return new Point(position.X + (clockwise ? -1 : 1), position.Y);
				yield return new Point(position.X + (clockwise ? -1 : 1), position.Y + (clockwise ? 1 : -1));
				yield return new Point(position.X, position.Y + (clockwise ? 1 : -1));
				break;
		}
	}
	else
	{
		// No inside tile with this turn - outer corner - acute
	}
}

void Flood(Point tile, List<Point> loopTiles, List<Point> insideTiles, Direction direction)
{
	var adjacent = direction switch
	{
		Direction.Up => new Point(tile.X, tile.Y - 1),
		Direction.Down => new Point(tile.X, tile.Y + 1),
		Direction.Left => new Point(tile.X - 1, tile.Y),
		Direction.Right => new Point(tile.X + 1, tile.Y),
		_ => throw new InvalidOperationException("Bad direction")
	};

	if (!loopTiles.Contains(adjacent) && !insideTiles.Contains(adjacent))
	{
		insideTiles.Add(adjacent);
	}
}

Move BuildRow1(string rawLine)
{
	var parts = rawLine.Split(new[] { " ", "(", ")" }, StringSplitOptions.RemoveEmptyEntries);
	return new Move(
		parts[0] switch
		{
			"U" => Direction.Up,
			"D" => Direction.Down,
			"L" => Direction.Left,
			"R" => Direction.Right,
			_ => throw new InvalidDataException()
		},
		int.Parse(parts[1])
	);
}

Move BuildRow2(string rawLine)
{
	var parts = rawLine.Split(new[] { " ", "(", ")" }, StringSplitOptions.RemoveEmptyEntries);
	
	var hexStart = rawLine.IndexOf('#') + 1;
	var distance = int.Parse(rawLine.Substring(hexStart, 5), NumberStyles.HexNumber);
	var direction = (Direction) int.Parse(rawLine.Substring(hexStart + 5, 1));
	return new Move(
		direction,
		distance
	);
}

long CalculateArea(List<Move> moves, List<Point> points)
{
	var areaOfPolygon = ShoelaceFormula(points);
	var areaOfOuterTrench = (moves.Sum(m => m.Length) / 2); // Divide by 2 as logic of formula is inclusive on 2 sides, and exclusive on the other.
	var neo = 1; // Also causes a stray 1 to be required to take into account the 2 opposing corners where inclusive meets exclusive (and are excluded).
	return areaOfPolygon + areaOfOuterTrench + neo;
}

long ShoelaceFormula(List<Point> points)
{
    var area = 0L;
    for (var pointOffset = 0; pointOffset < points.Count - 1; pointOffset++)
    {
		var current = points[pointOffset];
		var next = points[pointOffset + 1];
        area += current.X * next.Y - next.X * current.Y;
    }
	var last = points.Last();
	var first = points.First();
    area += last.X * first.Y - first.X * last.Y;
    return Math.Abs(area / 2);
}

record Move(Direction Direction, int Length);
record Point(long X, long Y);
record Tile(Point Position, Direction Direction);

enum Direction {
	Up = 3,
	Down = 1,
	Left = 2,
	Right = 0
}

enum TurnDirection
{
	Left = -1,
	None = 0,
	Right = 1
}