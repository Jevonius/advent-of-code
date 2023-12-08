<Query Kind="Statements" />

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample1.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample2.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample3.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

bool doPart1 = false;
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
	var positions = map.Where(kv => kv.Key.Last() == 'A').Select(kv => kv.Value).ToArray();
	var steps = 0L;
	while (positions.Any(p => p.Position.Last() != 'Z'))
	{
		var directionOffset = (int) (steps++ % totalDirections);
		var direction = directions[directionOffset];
		positions = positions.Select(p => map[direction == 'L' ? p.Left : p.Right]).ToArray();
	}
	Console.WriteLine($"Challenge 8.2 - Total: {steps}");
}
(Util.ElapsedTime - startTime).Dump();

if(false)
{
	var ghosts = map.Where(kv => kv.Key.Last() == 'A').Select(kv => new Ghost { Position = kv.Value}).ToList();
	var steps = 0L;
	while (ghosts.Any(g => !g.Looped))
	{
		var directionOffset = (int) (steps++ % totalDirections);
		var direction = directions[directionOffset];
		for (int i = 0; i < ghosts.Count(); i++)
		{
			if (!ghosts[i].Looped)
			{
				ProcessMove( ghosts[i], steps, map[direction == 'L' ? ghosts[i].Position.Left : ghosts[i].Position.Right]);
			}
		}
	}
	Console.WriteLine($"Challenge 8.2 - Total: {steps}");
}
//(Util.ElapsedTime - startTime).Dump();

void ProcessMove( Ghost ghost, long step, Direction nextPosition){
	if(ghost.Looped){
		return;
	}
	ghost.Position = nextPosition;
	if( nextPosition.Left != nextPosition.Right ) {
		ghost.LastPositionSameSides = false;
		ghost.FirstPositionSameSides = null;
		ghost.SameSidesLoopCount = 0;
		return;
	}
	if (ghost.LastPositionSameSides)
	{
		ghost.SameSidesLoopCount += 1;
		if (nextPosition == ghost.FirstPositionSameSides)
		{
			ghost.Looped = true;
			return;
		}
	}
	if (ghost.FirstPositionSameSides == null)
	{
		ghost.FirstPositionSameSides = nextPosition;
		ghost.LastPositionSameSides = true;
		ghost.FirstLoopStep = step;
	}
}

record Direction(string Position, string Left, string Right);

class Ghost
{
	public Direction Position { get; set; }

	public bool LastPositionSameSides { get; set; } = false;
	public Direction FirstPositionSameSides { get; set; } = null;
	public int SameSidesLoopCount { get; set; } = 0;
	public long FirstLoopStep { get; set; } = 0;

	public bool Looped { get; set; } = false;
}