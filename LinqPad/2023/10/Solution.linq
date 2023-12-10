<Query Kind="Statements">
  <Namespace>System.Drawing</Namespace>
</Query>

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample1.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample2.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample3.4.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample4.8.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample5.10.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

bool debug = false;
/*
	Sample data:

| is a vertical pipe connecting north and south.
- is a horizontal pipe connecting east and west.
L is a 90-degree bend connecting north and east.
J is a 90-degree bend connecting north and west.
7 is a 90-degree bend connecting south and west.
F is a 90-degree bend connecting south and east.
. is ground; there is no pipe in this tile.
S is the starting position of the animal; there is a pipe on this tile, but your sketch doesn't show what shape the pipe has.

*/

var width = rawLines.First().Length;
var height = rawLines.Length;

var data = new Tile[width, height];
Tile start = null;

for (int h = 0; h < height; h++)
{
	var line = rawLines[h];
	for (int w = 0; w < width; w++)
	{
		var character = line[w];
		var tile = new Tile(
			new Point(w, h),
			character == 'S',
			character == '|' || character == 'L' || character == 'J',
			character == '|' || character == '7' || character == 'F',
			character == '-' || character == 'J' || character == '7',
			character == '-' || character == 'L' || character == 'F'
		);
		if (tile.IsStart)
		{
			start = tile;
		}
		data[w, h] = tile;
	}
}

if( start == null ) {
	throw new InvalidOperationException("Start not found");
}

// Get start directions
bool pipeNorth = start.Position.Y > 0 && data[start.Position.X, start.Position.Y - 1].PipeSouth;
bool pipeSouth = start.Position.Y < height - 1 && data[start.Position.X, start.Position.Y + 1].PipeNorth;
bool pipeWest = start.Position.X > 0 && data[start.Position.X - 1, start.Position.Y].PipeEast;
bool pipeEast = start.Position.X < width - 1 && data[start.Position.X + 1, start.Position.Y].PipeWest;

start = new Tile( start.Position, true, pipeNorth, pipeSouth, pipeWest, pipeEast );
data[start.Position.X, start.Position.Y] = start;

var startTime = Util.ElapsedTime;
{
	var position1 = GetNext( data, new Move( Direction.North, start) );
	var position2 = GetNext( data, new Move( (Direction) (int) (position1.Facing + 1 ), start));
	
	var distance = 1;
	while( position1.Tile != position2.Tile ) {
		position1 = GetNext( data, position1);
		position2 = GetNext( data, position2);
		distance += 1;
	}
	Console.WriteLine($"Challenge 10.1 - Distance: {distance}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	// build loop
	var loop = new List<Move>();
	
	var previousPosition = GetNext(data, new Move(Direction.North, start));
	var loopStart = previousPosition.Tile;
	int turnCounter = 0; // at the end, clockwise = >0, anticlockwise = <0
	
	do{
		loop.Add(previousPosition);
		var nextPosition = GetNext(data, previousPosition);
		var turn = GetTurn(previousPosition.Facing, nextPosition.Facing);
		turnCounter += (int) turn;
		previousPosition = nextPosition;
	}while ( previousPosition.Tile != loopStart );
	
	// Start tile has ended up at the end.
	var startMove = loop.Last();
	loop.Remove(startMove);
	loop.Insert(0, startMove);	
	
	DumpLoop(loop);

	// trace inside tiles
	var clockwise = turnCounter > 0;
	var loopTiles = loop.Select( l => l.Tile).ToList();
	var insideTilesHash = new HashSet<Tile>();
	
	for (var loopOffset = 0; loopOffset < loop.Count; loopOffset++)
	{
		var move = loop[loopOffset];
		var nextMove = loop[(loopOffset + 1) % loop.Count];
		var turn = GetTurn(move.Facing, nextMove.Facing);

		var adjacents = FindAdjacents(data, clockwise, move, turn);
		foreach (var adjacent in adjacents)
		{
			insideTilesHash.Add(adjacent);
		}
	}
	var insideTiles = insideTilesHash.Except(loopTiles).ToList();

	// flood fill
	for (int insideOffset = 0; insideOffset < insideTiles.Count; insideOffset++)
	{
		var tile = insideTiles[insideOffset];
		Flood(data, tile, loopTiles, insideTiles, Direction.North);
		Flood(data, tile, loopTiles, insideTiles, Direction.South);
		Flood(data, tile, loopTiles, insideTiles, Direction.West);
		Flood(data, tile, loopTiles, insideTiles, Direction.East);
	}

	var count = insideTiles.Count;

	DumpLoop(loop, insideTiles);
	
	Console.WriteLine($"Challenge 10.2 - Count: {count}");
}
(Util.ElapsedTime - startTime).Dump();

Move GetNext(Tile[,] data, Move previous)
{
	if (previous.Facing != Direction.South && previous.Tile.PipeNorth)
	{
		return new Move(Direction.North, data[previous.Tile.Position.X, previous.Tile.Position.Y - 1]);
	}
	else if (previous.Facing != Direction.North && previous.Tile.PipeSouth)
	{
		return new Move(Direction.South, data[previous.Tile.Position.X, previous.Tile.Position.Y + 1]);
	}
	else if (previous.Facing != Direction.East && previous.Tile.PipeWest)
	{
		return new Move(Direction.West, data[previous.Tile.Position.X - 1, previous.Tile.Position.Y]);
	}
	else if (previous.Facing != Direction.West && previous.Tile.PipeEast)
	{
		return new Move( Direction.East, data[previous.Tile.Position.X + 1, previous.Tile.Position.Y] );
	}
	throw new InvalidOperationException("This shouldn't happen...");
}

TurnDirection GetTurn(Direction previousFacing, Direction currentFacing) {
	switch( previousFacing ) {
		case Direction.North:
			if( currentFacing == Direction.West ) return TurnDirection.Left;
			if( currentFacing == Direction.East ) return TurnDirection.Right;
			return TurnDirection.None;
		case Direction.South:
			if (currentFacing == Direction.East) return TurnDirection.Left;
			if (currentFacing == Direction.West) return TurnDirection.Right;
			return TurnDirection.None;
		case Direction.West:
			if (currentFacing == Direction.South) return TurnDirection.Left;
			if (currentFacing == Direction.North) return TurnDirection.Right;
			return TurnDirection.None;
		case Direction.East:
			if (currentFacing == Direction.North) return TurnDirection.Left;
			if (currentFacing == Direction.South) return TurnDirection.Right;
			return TurnDirection.None;
		default:
			throw new InvalidOperationException("Invalid turn data");
	}
}

IEnumerable<Tile> FindAdjacents(Tile[,] data, bool clockwise, Move move, TurnDirection turn)
{
	var position = move.Tile.Position;

	if (turn == TurnDirection.None)
	{
		switch (move.Facing)
		{
			case Direction.North:
				yield return data[position.X + (clockwise ? 1 : -1), position.Y];
				break;
			case Direction.South:
				yield return data[position.X + (clockwise ? -1 : 1), position.Y];
				break;
			case Direction.West:
				yield return data[position.X, position.Y + (clockwise ? -1 : 1)];
				break;
			case Direction.East:
				yield return data[position.X, position.Y + (clockwise ? 1 : -1)];
				break;
		}
	}
	else if ((clockwise && turn == TurnDirection.Left) || (!clockwise && turn == TurnDirection.Right))
	{
		// 3 Potential tiles - inner corner - obtuse
		switch (move.Facing)
		{
			case Direction.North:
				yield return data[position.X + (clockwise ? 1 : -1), position.Y];
				yield return data[position.X + (clockwise ? 1 : -1), position.Y + (clockwise ? 1 : -1)];
				yield return data[position.X, position.Y + (clockwise ? 1 : -1)];
				break;
			case Direction.South:
				yield return data[position.X + (clockwise ? -1 : 1), position.Y];
				yield return data[position.X + (clockwise ? -1 : 1), position.Y + (clockwise ? -1 : 1)];
				yield return data[position.X, position.Y + (clockwise ? -1 : 1)];
				break;
			case Direction.West:
				yield return data[position.X + (clockwise ? 1 : -1), position.Y];
				yield return data[position.X + (clockwise ? 1 : -1), position.Y + (clockwise ? -1 : -1)];
				yield return data[position.X, position.Y + (clockwise ? -1 : 1)];
				break;
			case Direction.East:
				yield return data[position.X + (clockwise ? -1 : 1), position.Y];
				yield return data[position.X + (clockwise ? -1 : 1), position.Y + (clockwise ? 1 : -1)];
				yield return data[position.X, position.Y + (clockwise ? 1 : -1)];
				break;
		}
	}
	else
	{
		// No inside tile with this turn - outer corner - acute
	}
}

void Flood(Tile[,] data, Tile tile, List<Tile> loopTiles, List<Tile> insideTiles, Direction direction)
{
	var adjacent = direction switch
	{
		Direction.North => data[tile.Position.X, tile.Position.Y - 1],
		Direction.South => data[tile.Position.X, tile.Position.Y + 1],
		Direction.West => data[tile.Position.X - 1, tile.Position.Y],
		Direction.East => data[tile.Position.X + 1, tile.Position.Y],
		_ => throw new InvalidOperationException("Bad direction")
	};

	if (!loopTiles.Contains(adjacent) && !insideTiles.Contains(adjacent))
	{
		insideTiles.Add(adjacent);
	}
}

void DumpLoop(List<Move> loop, List<Tile> inside = null)
{
	if (!debug)
	{
		return;
	}

	inside = inside ?? new ();
	
	var loopStart = loop.First().Tile;
	var loopEnd = loop.Last().Tile;

	var builder = new StringBuilder();
	builder.AppendLine();
	for (int y = 0; y < height; y++)
	{
		for (int x = 0; x < width; x++)
		{
			var tile = data[x, y];
			if (loop.Any(l => l.Tile == tile))
			{
				if( tile == loopStart) builder.Append('S');
				else if (tile.PipeNorth && tile.PipeSouth) builder.Append('│');
				else if (tile.PipeWest && tile.PipeEast) builder.Append('─');
				else if (tile.PipeNorth && tile.PipeWest) builder.Append('┘');
				else if (tile.PipeNorth && tile.PipeEast) builder.Append('└');
				else if (tile.PipeSouth && tile.PipeWest) builder.Append('┐');
				else if (tile.PipeSouth && tile.PipeEast) builder.Append('┌');
			}
			else builder.Append( inside.Contains(tile) ? 'I' : ' ');
		}
		builder.AppendLine();
	}

	builder.ToString().Dump();
}

record Tile(Point Position, bool IsStart, bool PipeNorth, bool PipeSouth, bool PipeWest, bool PipeEast);

record Move(Direction Facing, Tile Tile);

enum Direction
{
	North,
	South,
	West,
	East
}

enum TurnDirection
{
	Left = -1,
	None = 0,
	Right = 1
}
