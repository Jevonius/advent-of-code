<Query Kind="Statements" />

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample1.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample2.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

var startTime = Util.ElapsedTime;
{

	var sum = rawLines.Sum(l =>
		10 * int.Parse(l.First(c => char.IsNumber(c)).ToString())
		+ int.Parse(l.Last(c => char.IsNumber(c)).ToString())
	);
	Console.WriteLine($"Challenge 1.1 - Sum: {sum}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	var searchArray = new Dictionary<string, int>
	{
		{ "0", 0 },
		{ "1", 1 },
		{ "2", 2 },
		{ "3", 3 },
		{ "4", 4 },
		{ "5", 5 },
		{ "6", 6 },
		{ "7", 7 },
		{ "8", 8 },
		{ "9", 9 },
		{ "zero", 0 },
		{ "one", 1 },
		{ "two", 2 },
		{ "three", 3 },
		{ "four", 4 },
		{ "five", 5 },
		{ "six", 6 },
		{ "seven", 7 },
		{ "eight", 8 },
		{ "nine", 9 }
	};

	var sum = rawLines.Sum(l =>
		10 * searchArray.Select(sa => new { Index = l.IndexOf(sa.Key), Value = sa.Value }).OrderBy(r => r.Index).First(r => r.Index >= 0).Value
		+ searchArray.Select(sa => new { Index = l.LastIndexOf(sa.Key), Value = sa.Value }).OrderBy(r => r.Index).Last(r => r.Index >= 0).Value
	);

	Console.WriteLine($"Challenge 2.1 - Sum: {sum}");
}
(Util.ElapsedTime - startTime).Dump();
