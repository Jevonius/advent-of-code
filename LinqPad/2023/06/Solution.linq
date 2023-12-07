<Query Kind="Statements" />

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

/*
	Sample data:
	
Time:      7  15   30
Distance:  9  40  200

*/

var startTime = Util.ElapsedTime;
{
	var times = rawLines[0].Split(new[] { "Time:", " " }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
	var distances = rawLines[1].Split(new[] { "Distance:", " " }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
	var races = times.Zip(distances).Select(zipper => new Race(zipper.First, zipper.Second));

	var winners = races.Select(GetHeatDistances);
	//winners.Dump();
	var total = winners.Aggregate(1, (runningTotal, current) => runningTotal * current.Count());
	Console.WriteLine($"Challenge 6.1 - Total: {total}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	var time = long.Parse(rawLines[0].Replace("Time:", "").Replace(" ", ""));
	var distance = long.Parse(rawLines[1].Replace("Distance:", "").Replace(" ", ""));
	var race = new Race(time, distance);

	var winners = GetHeatDistances(race);
	var total = winners.Count();
	Console.WriteLine($"Challenge 6.2 - Total: {total}");
}
(Util.ElapsedTime - startTime).Dump();

IEnumerable<long> GetHeatDistances(Race race)
{
	for (var time = 0; time < race.Time; time++)
	{
		var remaining = race.Time - time;
		var speed = time;
		var dist = speed * remaining;
		if( dist > race.Distance ) {
			yield return dist;
		}
	}
}

record Race(long Time, long Distance);