<Query Kind="Statements" />

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample1.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

bool debug = false;
/*
	Sample data:

???.### 1,1,3
.??..??...?##. 1,1,3
?#?#?#?#?#?#?#? 1,3,1,6
????.#...#... 4,1,1
????.######..#####. 1,6,5
?###???????? 3,2,1

*/

var startTime = Util.ElapsedTime;
{
	var data = GetData(1);
	var pos = data.Select(r => new { Row = r, Possibilities = BruteForce(r) });

	if (debug)
	{
		pos.Select(p => new { Field = p.Row.Field, Arrangement = string.Join(',', p.Row.Arrangement), Possibilities = p.Possibilities }).Dump();
	}
	Console.WriteLine($"Challenge 12.1 - Total: {pos.Sum(pos => pos.Possibilities)}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	var data = GetData(5);
	var pos = data.Select(r => new { Row = r, Possibilities = Iterative(r, 0, 0, r.Arrangement.ToList(), new Dictionary<Remain, long>()) }).ToList();
	if (debug)
	{
		pos.Select(p => new { Field = p.Row.Field, Arrangement = string.Join(',', p.Row.Arrangement), Possibilities = p.Possibilities } ).Dump();
	}
	Console.WriteLine($"Challenge 12.2 - Total: {pos.Sum(pos => pos.Possibilities)}");
}
(Util.ElapsedTime - startTime).Dump();

int BruteForce(Row row)
{
	var possibilityCount = 0;

	var minStartGap = row.Field.IndexOfAny(new[] { '?', '#' });
	var minEndGap = row.Field.Length - row.Field.LastIndexOfAny(new[] { '?', '#' }) - 1;
	
	var springs = row.Arrangement;

	var gaps = springs.Zip(springs.Skip(1), (first, second) => 1).ToList();
	gaps.Insert(0, minStartGap);
	var endGap = row.Field.Length - (gaps.Sum() + springs.Sum());
	gaps.Add(endGap);

	var lastGapOffset = gaps.Count - 1;

	var moved = true;

	var rightmostOffset = springs.Count - 1;
	while (moved)
	{
		// Test this arrangement
		var arrangement = gaps.Zip(springs).Aggregate(string.Empty, (arrangement, pair) => arrangement + (new string('.', pair.First)) + (new string('#', pair.Second))) + new string('.', gaps[lastGapOffset]);
		var test = arrangement.Zip(row.Field).All((test => (test.First == '.' && test.Second != '#') || (test.First == '#' && test.Second != '.')));

		if (test)
		{
			possibilityCount += 1;
		}
		var springPos = new List<int>();

		// Move the gaps
		moved = false;
		if (gaps[lastGapOffset] > minEndGap)
		{
			// Move the last one
			gaps[rightmostOffset] += 1;
			gaps[lastGapOffset] -= 1;
			moved = true;
		}
		else
		{
			// Need to reset up to point of first block that can move
			var leftmostMover = rightmostOffset;
			while (!moved && leftmostMover >= 0)
			{
				gaps[leftmostMover] += 1;
				for (var mover = leftmostMover + 1; mover < lastGapOffset; mover++)
				{
					gaps[mover] = 1;
				}
				endGap = row.Field.Length - (gaps.SkipLast(1).Sum() + springs.Sum());
				if (endGap >= minEndGap)
				{
					gaps[lastGapOffset] = endGap;
					moved = true;
				}
				else
				{
					leftmostMover -= 1;
				}
			}
		}
	}
	return possibilityCount;
}

long Iterative(Row row, int currentLength, int initialGap, List<int> remainingSprings, Dictionary<Remain, long> shortcuts)
{
	var shortcutKey = new Remain(currentLength, remainingSprings.Count);
	if (shortcuts.ContainsKey(shortcutKey))
	{
		return shortcuts[shortcutKey];
	}
	else if (remainingSprings.Count == 0)
	{
		if (!row.Field.Skip(currentLength).Any(c => c == '#'))
		{
			// valid
			return 1;
		}
		else
		{
			return 0;
		}
	}

	var remainingSpace = row.Field.Length - currentLength - (remainingSprings.Count - 1 + remainingSprings.Skip(1).Sum());
	if (remainingSpace < 0)
	{
		return 0;
	}

	var spring = remainingSprings[0];
	remainingSprings.RemoveAt(0);

	var options = 0L;

	var cutout = remainingSpace - spring;

	var gap = initialGap;
	while (gap <= cutout)
	{
		var testPart = row.Field.Skip(currentLength);
		if (
			testPart.Take(gap).All(t => t != '#')
			&& testPart.Skip(gap).Take(spring).All(t => t != '.')
		)
		{
			// This spring is valid, try the next...
			options += Iterative(row, currentLength + gap + spring, 1, remainingSprings, shortcuts);
		}
		gap += 1;
	}

	remainingSprings.Insert(0, spring);
	shortcuts.Add(shortcutKey, options);
	return options;
}

IEnumerable<Row> GetData(int repeats)
{
	var data = rawLines.Select(rl =>
	{
		var parts = rl.Split(' ');
		
		var field = string.Join('?', Enumerable.Repeat(parts[0], repeats));
		var arrangement = string.Join(',', Enumerable.Repeat(parts[1], repeats));
		return new Row(field, arrangement.Split(',').Select(int.Parse).ToList());
	});
	return data.ToList();
}

record Row(string Field, List<int> Arrangement);
record Remain(int CurrentLength, int RemainingSpringCount);

class RowTest
{
	public int ArrangementOffset { get; set; }
	public int PositionOffset { get; set; }
}