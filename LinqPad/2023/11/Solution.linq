<Query Kind="Statements">
  <Namespace>System.Drawing</Namespace>
</Query>

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample1.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

var part2Expand = 1000000;
//part2Expand = 2;


bool debug = false;
/*
	Sample data:

...#......
.......#..
#.........
..........
......#...
.#........
.........#
..........
.......#..
#...#.....

*/


var startTime = Util.ElapsedTime;
{
	var rawData = rawLines.ToList();

	// Expand vertically
	for (int horOffset = 1; horOffset < rawData[0].Length; horOffset++)
	{
		if (rawData.All(rl => rl[horOffset] == '.'))
		{
			for (int verOffset = 0; verOffset < rawData.Count; verOffset++)
			{
				rawData[verOffset] = rawData[verOffset].Insert(horOffset, ".");
			}
			horOffset += 1;
		}
	}
	// Expand horizontally
	var width = rawData[0].Length;
	for (int verOffset = 1; verOffset < rawData.Count(); verOffset++)
	{
		if (rawData[verOffset].All(c => c == '.'))
		{
			rawData.Insert(verOffset, new string('.', width));
			verOffset += 1;
		}
	}

	if (debug)
	{
		rawData.Dump();
	}

	var galaxies = new List<Point>();
	for (int verOffset = 0; verOffset < rawData.Count; verOffset++)
	{
		var rawLine = rawData[verOffset];
		for (int horOffset = 0; horOffset < rawLine.Length; horOffset++)
		{
			if (rawLine[horOffset] == '#')
			{
				galaxies.Add(new Point(horOffset, verOffset));
			}
		}
	}

	var manhatten = galaxies.SelectMany(g => galaxies.Except(new[] { g}).Select( g2 => Math.Abs(g.X - g2.X) + Math.Abs(g.Y - g2.Y)));
	
	Console.WriteLine($"Challenge 11.1 - Total: {manhatten.Sum()/2}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	var rawData = rawLines.ToList();
	
	var galaxies = new List<Point>();
	for (int verOffset = 0; verOffset < rawData.Count; verOffset++)
	{
		var rawLine = rawData[verOffset];
		for (int horOffset = 0; horOffset < rawLine.Length; horOffset++)
		{
			if (rawLine[horOffset] == '#')
			{
				galaxies.Add(new Point(horOffset, verOffset));
			}
		}
	}

	// Detect X blanks
	var xExpand = new List<int>();
	for (int horOffset = 1; horOffset < rawData[0].Length; horOffset++)
	{
		if (rawData.All(rl => rl[horOffset] == '.'))
		{
			xExpand.Add(horOffset);
		}
	}
	
	// Detect Y blanks
	var yExpand = new List<int>();
	for (int verOffset = 1; verOffset < rawData.Count(); verOffset++)
	{
		if (rawData[verOffset].All(c => c == '.'))
		{
			yExpand.Add(verOffset);
		}
	}
	
	if (debug)
	{
		xExpand.Dump();
		yExpand.Dump();
	}
	
	var manhattens = new List<long>();
	for (int g1Offset = 0; g1Offset < galaxies.Count - 1; g1Offset++)
	{
		var g1 = galaxies[g1Offset];
		for (int g2Offset = g1Offset + 1; g2Offset < galaxies.Count; g2Offset++)
		{
			var g2 = galaxies[g2Offset];

			var minX = (long)Math.Min(g1.X, g2.X);
			var maxX = (long)Math.Max(g1.X, g2.X);

			var minY = Math.Min(g1.Y, g2.Y);
			var maxY = Math.Max(g1.Y, g2.Y);

			var xExpandCount = xExpand.Count(x => minX < x && x < maxX);
			var yExpandCount = yExpand.Count(y => minY < y && y < maxY);
			var xMan = maxX - minX;
			var yMan = maxY - minY;
			manhattens.Add(((xMan - xExpandCount) + (xExpandCount * part2Expand)) + ((yMan - yExpandCount) + (yExpandCount * part2Expand)));
		}
	}
	Console.WriteLine($"Challenge 11.2 - Total: {manhattens.Sum()}");
}
(Util.ElapsedTime - startTime).Dump();
