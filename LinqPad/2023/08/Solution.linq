<Query Kind="Statements" />

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample1.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample2.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample3.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

bool doPart1 = true;
bool debug = false;
/*
	Sample data:

RL

AAA = (BBB, CCC)
BBB = (DDD, EEE)
CCC = (ZZZ, GGG)
DDD = (DDD, DDD)
EEE = (EEE, EEE)
GGG = (GGG, GGG)
ZZZ = (ZZZ, ZZZ)

*/

var directions = rawLines[0];
var totalDirections = directions.Length;
var map = rawLines.SkipWhile(rl => rl != string.Empty).Skip(1).Select(rl => {
	var parts = rl.Split(new [] { " = (", ", ", ")" }, StringSplitOptions.RemoveEmptyEntries);
	return new Direction(parts[0], parts[1], parts[2]);
}).ToDictionary(d => d.Position);

//map.Dump();

var startTime = Util.ElapsedTime;
if (doPart1)
{
	{
		var position = map["AAA"];
		var steps = 0;
		while (position.Position != "ZZZ")
		{
			var directionOffset = steps++ % totalDirections;
			var direction = directions[directionOffset];
			position = map[direction == 'L' ? position.Left : position.Right];
		}
		Console.WriteLine($"Challenge 8.1 - Total: {steps}");
	}
	(Util.ElapsedTime - startTime).Dump();
	Console.WriteLine();
}

startTime = Util.ElapsedTime;
{
	var ghosts = map.Where(kv => kv.Key.Last() == 'A').Select(kv => new Ghost { Position = kv.Value }).ToList();
	var unloopedGhosts = ghosts.ToList();
	var steps = 0L;
	while (unloopedGhosts.Count > 0)
	{
		var directionOffset = (int)(steps % totalDirections);
		var direction = directions[directionOffset];
		var ghostOffset = 0;
		while (ghostOffset < unloopedGhosts.Count)
		{
			var ghost = unloopedGhosts[ghostOffset];
			ghost.ProcessMove(steps, directionOffset, map[direction == 'L' ? ghost.Position.Left : ghost.Position.Right]);
			if (ghost.Looped)
			{
				if (debug)
				{
					Console.WriteLine($"Found Loop for {unloopedGhosts.Count}");
				}
				unloopedGhosts.RemoveAt(ghostOffset);
			}
			else
			{
				ghostOffset += 1;
			}
		}
		steps += 1;
	}
	if (debug)
	{
		ghosts.Select(g => new
		{
			Position = g.Position.Position,
			LoopLength = g.LoopLength,
			FirstStep = g.FirstLoopStep,
			ZOffsets = g.ZOffsets
		}).Dump();
	}
		
	var longestLoopGhost = ghosts.OrderByDescending(g => g.LoopLength).ThenBy(g => g.ZOffsets.Count()).First();
	
	var loopLength = longestLoopGhost.LoopLength;
	
	var curStep = longestLoopGhost.FirstLoopStep;
	var found = false;
	var foundStep = curStep;
	while (!found)
	{
		foreach (var zOffset in longestLoopGhost.ZOffsets)
		{
			var testStep = curStep + zOffset;
			found = ghosts.All(g => g.IsZ(testStep));
			if (found)
			{
				foundStep = testStep;
				break;
			}
		}
		if (!found)
		{
			curStep += loopLength;
		}
	}
	
	Console.WriteLine($"Challenge 8.2 - Step: {foundStep + 1}");
}
(Util.ElapsedTime - startTime).Dump();

record Direction(string Position, string Left, string Right);

record RouteStep(Direction Position, int DirectionOffset);

class Ghost
{
	public Direction Position { get; set; }

	public long FirstLoopStep { get; set; } = 0;
	public List<RouteStep> RouteSteps { get; set;} = new();
	public bool Looped { get; set; } = false;
	public int LoopLength => RouteSteps.Count;

	public int[] ZOffsets { get; set;}

	public void ProcessMove(long step, int directionOffset, Direction position)
	{
		if (Looped)
		{
			return;
		}
		
		Position = position;
		var routeStep = new RouteStep( position, directionOffset );

		if (!RouteSteps.Contains(routeStep))
		{
			RouteSteps.Add(routeStep);
			return;
		}
		
		// We've found a loop.
		// Trim the start
		RouteSteps = RouteSteps.SkipWhile(rs => rs != routeStep).ToList();
		FirstLoopStep = step - RouteSteps.Count; // - 1? + 1?
		Looped = true;

		ZOffsets = RouteSteps.Select((rs, offset) => rs.Position.Position.Last() == 'Z' ? offset : -1)
			.Where(e => e >= 0)
			.ToArray();
	}

	public bool IsZ(long step) => ZOffsets.Contains((int)((step - FirstLoopStep) % LoopLength));
}