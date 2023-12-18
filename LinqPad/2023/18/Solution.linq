<Query Kind="Statements">
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Drawing</Namespace>
</Query>

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample1.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

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

Area of a rectilinear polygon.

*/

var startTime = Util.ElapsedTime;
{
	var moves = rawLines.Select(BuildRow1).ToList();
	var points = GetPoints(moves);

	if (debug)
	{
		points.Dump();
	}

	var area = CalculateArea(moves, points);
	Console.WriteLine($"Challenge 18.1 - Result: {area}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();


startTime = Util.ElapsedTime;
{
	var moves = rawLines.Select(BuildRow2).ToList();
	var points = GetPoints(moves);	

	if (debug)
	{
		points.Dump();
	}
	
	var area = CalculateArea(moves, points);
	
	Console.WriteLine($"Challenge 18.2 - Result: {area}");
}
(Util.ElapsedTime - startTime).Dump();

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

List<Point> GetPoints(List<Move> moves)
{
	var currentX = 0;
	var currentY = 0;
	var points = new List<Point>();
	foreach (var move in moves)
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
	return points;
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

enum Direction {
	Up = 3,
	Down = 1,
	Left = 2,
	Right = 0
}