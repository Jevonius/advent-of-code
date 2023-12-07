<Query Kind="Statements" />

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

/*
	Sample data:
	
	Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green
	Game 2: 1 blue, 2 green; 3 green, 4 blue, 1 red; 1 green, 1 blue
	Game 3: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red
	Game 4: 1 green, 3 red, 6 blue; 3 green, 6 red; 3 green, 15 blue, 14 red
	Game 5: 6 red, 1 blue, 3 green; 2 blue, 1 red, 2 green
	
	only 12 red cubes, 13 green cubes, and 14 blue cubes
*/

var maxRed = 12;
var maxGreen = 13;
var maxBlue = 14;

var games = rawLines.Select(rl => {
	var numberStart = rl.IndexOf(' ') + 1;
	var numberEnd = rl.IndexOf(':');
	var number = int.Parse( rl.Substring(numberStart, numberEnd - numberStart));
	
	var rawDraws = rl.Substring(numberEnd + 2);
	var draws = rawDraws.Split("; ").Select( rd =>
		new Draw( rd.Split(", ").Select(rc => {
			var gap = rc.IndexOf(' ');
			var count = int.Parse(rc.Substring(0, gap));
			var colour = (Colour) Enum.Parse(typeof(Colour), rc.Substring(gap+1), true);
			return new CubeCount(colour, count);
		}).ToList())
	).ToList();
	return new Game( number, draws);
});

var startTime = Util.ElapsedTime;
{
	var valid = games.Where(g => g.Draws.All(d => d.Cubes.All( c => c.Count <= c.Colour switch {
		Colour.Blue => maxBlue,
		Colour.Green => maxGreen,
		Colour.Red => maxRed,
		_ => throw new InvalidOperationException("What colour?!")
	})));
	
	var sum = valid.Sum( g => g.Number);

	Console.WriteLine($"Challenge 2.1 - Sum: {sum}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	var powers = games.Select(g =>
	{
		var red = g.Draws.Max(d => d.Cubes.Where(c => c.Colour == Colour.Red).Any() ? d.Cubes.Where(c => c.Colour == Colour.Red).Max(c => c.Count) : 0);
		var green = g.Draws.Max(d => d.Cubes.Where(c => c.Colour == Colour.Green).Any() ? d.Cubes.Where(c => c.Colour == Colour.Green).Max(c => c.Count) : 0);
		var blue = g.Draws.Max(d => d.Cubes.Where(c => c.Colour == Colour.Blue).Any() ? d.Cubes.Where(c => c.Colour == Colour.Blue).Max(c => c.Count) : 0);
		return red * green * blue;
	});
	
	var sum = powers.Sum();

	Console.WriteLine($"Challenge 2.2 - Total Score: {sum}");
}
(Util.ElapsedTime - startTime).Dump();

record Game(int Number, List<Draw> Draws);
record Draw(List<CubeCount> Cubes);
record CubeCount(Colour Colour, int Count);


enum Colour
{
	Red,
	Green,
	Blue
}
