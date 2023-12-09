<Query Kind="Statements" />

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample1.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

var sources = rawLines.Select(rl => rl.Split(' ', StringSplitOptions.TrimEntries).Select(int.Parse)).ToList();

bool debug = true;
/*
	Sample data:

0 3 6 9 12 15
1 3 6 10 15 21
10 13 16 21 30 45

*/

var startTime = Util.ElapsedTime;
{
	var numbers = sources.Select(GetNext);
	
	Console.WriteLine($"Challenge 9.1 - Total: {numbers.Sum()}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	var numbers = sources.Select(GetPrev);

	Console.WriteLine($"Challenge 9.2 - Total: {numbers.Sum()}");
}
(Util.ElapsedTime - startTime).Dump();

int GetNext(IEnumerable<int> initial) => GetDiffsToZeroNext(initial).Aggregate(0, (total, current) => total + current.Last());
int GetPrev(IEnumerable<int> initial) => GetDiffsToZeroPrev(initial).Aggregate(0, (total, current) => total + current.First());

List<IEnumerable<int>> GetDiffsToZeroNext(IEnumerable<int> initial) => GetDiffsToZero(initial, GetDiffsNext);
List<IEnumerable<int>> GetDiffsToZeroPrev(IEnumerable<int> initial) => GetDiffsToZero(initial, GetDiffsPrev);

List<IEnumerable<int>> GetDiffsToZero(IEnumerable<int> initial, Func<IEnumerable<int>, IEnumerable<int>> getDiffs)
{
	var diffs = new List<IEnumerable<int>> { initial };
	var last = getDiffs(initial).ToList();

	while (!last.All(v => v == 0))
	{
		diffs.Insert(0, last);
		last = getDiffs(last).ToList();
	}
	return diffs;
}

IEnumerable<int> GetDiffsNext(IEnumerable<int> numbers) => numbers.Zip(numbers.Skip(1), (first, second) => second - first);
IEnumerable<int> GetDiffsPrev(IEnumerable<int> numbers) => numbers.Zip(numbers.Skip(1), (first, second) => first - second);