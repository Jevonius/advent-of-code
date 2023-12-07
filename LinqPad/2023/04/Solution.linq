<Query Kind="Statements" />

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

/*
	Sample data:
	
	Card 1: 41 48 83 86 17 | 83 86  6 31 17  9 48 53
	Card 2: 13 32 20 16 61 | 61 30 68 82 17 32 24 19
	Card 3:  1 21 53 59 44 | 69 82 63 72 16 21 14  1
	Card 4: 41 92 73 84 69 | 59 84 76 51 58  5 54 83
	Card 5: 87 83 26 28 32 | 88 30 70 12 93 22 82 36
	Card 6: 31 18 13 56 72 | 74 77 10 23 35 67 36 11

*/
var games = rawLines.Select(rl =>
{
	var parts = rl.Split(new[] { "Card ", ": ", " | " }, StringSplitOptions.RemoveEmptyEntries);
	return new Game(
		int.Parse(parts[0]),
		parts[1].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToList(),
		parts[2].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToList()
	);
})
	.OrderBy(g => g.Number)
	.ToDictionary(g => g.Number);

var startTime = Util.ElapsedTime;
{
	var sum = games.Values.Sum(g =>
	{
		var matches = g.Winners.Intersect(g.Plays);
		if( matches.Any() ) {
			return (int) Math.Pow(2, matches.Count() - 1);
		}
		return 0;
	});
	Console.WriteLine($"Challenge 4.1 - Sum: {sum}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	foreach (var game in games)
	{
		var winners = game.Value.Winners.Intersect(game.Value.Plays).Count();
		var copies = game.Value.Copies;
		for (int gameOffset = 1; gameOffset <= winners; gameOffset++)
		{
			games[game.Key + gameOffset].Copies += copies;
		}
	}
	
	var totalCopies = games.Values.Sum(g => g.Copies);
	Console.WriteLine($"Challenge 4.2 - Total: {totalCopies}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

record Game(int Number, List<int> Winners, List<int> Plays)
{
	public int Copies { get; set; } = 1;
};