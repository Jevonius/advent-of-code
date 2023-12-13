<Query Kind="Statements" />

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample1.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

bool debug = true;
/*
	Sample data:

#.##..##.
..#.##.#.
##......#
##......#
..#.##.#.
..##..##.
#.#.##.#.

#...##..#
#....#..#
..##..###
#####.##.
#####.##.
..##..###
#....#..#

*/

var data = new List<Mirror>();
{
	var mirror = new Mirror();
	data.Add(mirror);
	for (int rlOffset = 0; rlOffset < rawLines.Length; rlOffset++)
	{
		if (rawLines[rlOffset] == "")
		{
			mirror = new Mirror();
			data.Add(mirror);
			continue;
		}
		mirror.Horizontals.Add(rawLines[rlOffset]);
	}
}
foreach( var mirror in data) {
	// Build verticals
	for (int i = 0; i < mirror.Horizontals[0].Length; i++)
	{
		var vertical = string.Join( string.Empty, mirror.Horizontals.Select(h => h[i]));
		mirror.Verticals.Add(vertical);
	}
	// Test
	var offset = SearchReflect(mirror.Horizontals);
	if( offset >= 0 ) {
		mirror.MatchIsHorzontal = true;
		mirror.MatchNumber = offset;
		continue;
	}
	offset = SearchReflect(mirror.Verticals);
	if( offset >= 0 ) {
		mirror.MatchIsHorzontal = false;
		mirror.MatchNumber = offset;
		continue;
	}
	throw new InvalidDataException();
}

var startTime = Util.ElapsedTime;
{
	var sum = data.Sum(m => (m.MatchIsHorzontal ? 100 : 1) * m.MatchNumber);
	Console.WriteLine($"Challenge 13.1 - Total: {sum}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{

	foreach (var mirror in data)
	{
		// Test
		var offset = SearchSmudge(mirror.Horizontals);
		if (offset >= 0)
		{
			mirror.MatchIsHorzontal = true;
			mirror.MatchNumber = offset;
			continue;
		}
		offset = SearchSmudge(mirror.Verticals);
		if (offset >= 0)
		{
			mirror.MatchIsHorzontal = false;
			mirror.MatchNumber = offset;
			continue;
		}
		throw new InvalidDataException();
	}
	var sum = data.Sum(m => (m.MatchIsHorzontal ? 100 : 1) * m.MatchNumber);
	Console.WriteLine($"Challenge 13.2 - : {sum}");
}
(Util.ElapsedTime - startTime).Dump();

int SearchReflect(List<string> lines){
	var maxOffset = lines.Count;
	for (int offset = 1; offset < maxOffset; offset++)
	{
		var mirror = lines.Take(offset).Reverse().Zip(lines.Skip(offset)).All(pair => pair.First == pair.Second);
		if(mirror) {
			return offset;
		}
	}
	return -1;
}

int SearchSmudge(List<string> lines)
{
	var maxOffset = lines.Count;
	for (int offset = 1; offset < maxOffset; offset++)
	{
		var zipped = lines.Take(offset).Reverse().Zip(lines.Skip(offset));
		var differences = zipped.Sum(pair => pair.First.Zip(pair.Second).Count(p => p.First != p.Second));
		if (differences == 1)
		{
			return offset;
		}
	}
	return -1;
}
class Mirror
{
	public List<string> Verticals { get; set; } = new();
	public List<string> Horizontals { get; set; } = new();
	public int MatchNumber { get; set; }
	public bool MatchIsHorzontal { get; set; }
}