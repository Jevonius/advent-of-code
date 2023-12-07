<Query Kind="Statements" />

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

/*
	Sample data:
	
seeds: 79 14 55 13

seed-to-soil map:
50 98 2
52 50 48

soil-to-fertilizer map:
0 15 37
37 52 2
39 0 15

fertilizer-to-water map:
49 53 8
0 11 42
42 0 7
57 7 4

water-to-light map:
88 18 7
18 25 70

light-to-temperature map:
45 77 23
81 45 19
68 64 13

temperature-to-humidity map:
0 69 1
1 0 69

humidity-to-location map:
60 56 37
56 93 4

*/

var seeds = rawLines.First().Split(new[] { "seeds: ", " " }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse);

var seedToSoil = rawLines.SkipWhile(l => l != "seed-to-soil map:").Skip(1).TakeWhile(l => l != "").Select(BuildMap).OrderBy(m => m.SourceStart);
var soilToFert = rawLines.SkipWhile(l => l != "soil-to-fertilizer map:").Skip(1).TakeWhile(l => l != "").Select(BuildMap).OrderBy(m => m.SourceStart);
var fertToWater = rawLines.SkipWhile(l => l != "fertilizer-to-water map:").Skip(1).TakeWhile(l => l != "").Select(BuildMap).OrderBy(m => m.SourceStart);
var waterToLight = rawLines.SkipWhile(l => l != "water-to-light map:").Skip(1).TakeWhile(l => l != "").Select(BuildMap).OrderBy(m => m.SourceStart);
var lightToTemp = rawLines.SkipWhile(l => l != "light-to-temperature map:").Skip(1).TakeWhile(l => l != "").Select(BuildMap).OrderBy(m => m.SourceStart);
var tempToHumid = rawLines.SkipWhile(l => l != "temperature-to-humidity map:").Skip(1).TakeWhile(l => l != "").Select(BuildMap).OrderBy(m => m.SourceStart);
var humidToLoca = rawLines.SkipWhile(l => l != "humidity-to-location map:").Skip(1).TakeWhile(l => l != "").Select(BuildMap).OrderBy(m => m.SourceStart);

var startTime = Util.ElapsedTime;
{
	var locations = seeds.Select(MapSeedToLocation);

	Console.WriteLine($"Challenge 5.1 - Lowest Location: {locations.Min()}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	var seedRanges = seeds.Where((x, i) => i % 2 == 0).Zip(seeds.Where((x, i) => i % 2 == 1)).Select(pair => new Range(pair.First, pair.First + pair.Second - 1));
	var locations = MapSeedRangesToLocation(seedRanges);

	Console.WriteLine($"Challenge 5.2 - Lowest Location: {locations.Min(l => l.Lower)}");
}
(Util.ElapsedTime - startTime).Dump();

Mapping BuildMap(string rawLine)
{
	var parts = rawLine.Split(" ").Select(long.Parse).ToArray();
	return new Mapping(parts[1], parts[1] + parts[2] - 1, parts[0] - parts[1]);
}

long MapSeedToLocation(long seed)
{
	var soil = DoMapping(seed, seedToSoil);
	var fert = DoMapping(soil, soilToFert);
	var water = DoMapping(fert, fertToWater);
	var light = DoMapping(water, waterToLight);
	var temp = DoMapping(light, lightToTemp);
	var humid = DoMapping(temp, tempToHumid);
	var loca = DoMapping(humid, humidToLoca);
	//$"Seed {seed}, soil {soil}, fertilizer {fert}, water {water}, light {light}, temperature {temp}, humidity {humid}, location {loca}".Dump();
	return loca;
}

IEnumerable<Range> MapSeedRangesToLocation(IEnumerable<Range> seeds)
{
	//seeds.Dump();
	//seedToSoil.Dump();
	var soils = DoRangeMapping(seeds, seedToSoil).OrderBy(r => r.Lower);
	//soils.Dump();
	var ferts = DoRangeMapping(soils, soilToFert).OrderBy(r => r.Lower);
	var waters = DoRangeMapping(ferts, fertToWater).OrderBy(r => r.Lower);
	var lights = DoRangeMapping(waters, waterToLight).OrderBy(r => r.Lower);
	var temps = DoRangeMapping(lights, lightToTemp).OrderBy(r => r.Lower);
	var humids = DoRangeMapping(temps, tempToHumid).OrderBy(r => r.Lower);
	var locas = DoRangeMapping(humids, humidToLoca).OrderBy(r => r.Lower);
	return locas;
}

long DoMapping(long source, IEnumerable<Mapping> mappings) {
	var mapping = mappings.FirstOrDefault(m => m.SourceStart <= source && m.SourceEnd >= source);
	if( mapping == null ) {
		return source;
	}
	return source + mapping.Offset;
}

IEnumerable<Range> DoRangeMapping(IEnumerable<Range> sources, IEnumerable<Mapping> mappings)
{
	foreach (var source in sources)
	{
		var matchingMappings = mappings.SkipWhile(m => m.SourceEnd <= source.Lower).TakeWhile(m => m.SourceStart <= source.Upper);
		var newRanges = new List<Range>();
		if (!mappings.Any())
		{
			yield return source;
		}

		var lower = source.Lower;
		var upper = source.Upper;
		foreach (var mapping in matchingMappings)
		{
			if (lower < mapping.SourceStart)
			{
				// Before mapping
				yield return new Range(lower, mapping.SourceStart - 1);
				lower = mapping.SourceStart;
			}

			if (upper > mapping.SourceEnd)
			{
				yield return new Range(lower + mapping.Offset, mapping.SourceEnd + mapping.Offset);
				lower = mapping.SourceEnd + 1;
			}
			else
			{
				yield return new Range(lower + mapping.Offset, upper + mapping.Offset);
				lower = upper;
			}
		}
		if (lower < upper)
		{
			// After mappings
			yield return new Range(lower, upper);
		}
	}
}

record Mapping( long SourceStart, long SourceEnd, long Offset );
record Range(long Lower, long Upper);