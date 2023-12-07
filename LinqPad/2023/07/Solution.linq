<Query Kind="Statements" />

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

/*
	Sample data:
	
32T3K 765
T55J5 684
KK677 28
KTJJT 220
QQQJA 483

*/

const string Strength1 = "23456789TJQKA";
const string Strength2 = "J23456789TQKA";

var startTime = Util.ElapsedTime;
{
	var hands = rawLines.Select(rl =>
	{
		var parts = rl.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		return new Hand(parts[0], long.Parse(parts[1]), GetType(parts[0]));
	});

	var rankHands = RankHands(hands, Strength1);
	var scores = rankHands.Select((hand, offset) => hand.Rank * (offset + 1));
	Console.WriteLine($"Challenge 7.1 - Total: {scores.Sum()}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	var hands = rawLines.Select(rl =>
	{
		var parts = rl.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		return new Hand(parts[0], long.Parse(parts[1]), GetType(parts[0], true));
	});
	//hands.Where( h => h.Cards.IndexOf('J') >= 0 ).Dump();

	var rankHands = RankHands(hands, Strength2);
	var scores = rankHands.Select((hand, offset) => hand.Rank * (offset + 1));
	Console.WriteLine($"Challenge 7.2 - Total: {scores.Sum()}");
}
(Util.ElapsedTime - startTime).Dump();

IOrderedEnumerable<Hand> RankHands(IEnumerable<Hand> hands, string strength) => hands
	.OrderBy(h => (int)h.Type)
	.ThenBy(h => strength.IndexOf(h.Cards[0]))
	.ThenBy(h => strength.IndexOf(h.Cards[1]))
	.ThenBy(h => strength.IndexOf(h.Cards[2]))
	.ThenBy(h => strength.IndexOf(h.Cards[3]))
	.ThenBy(h => strength.IndexOf(h.Cards[4]));

HandType GetType(string hand, bool jokerEnabled = false)
{
	var groups = hand.GroupBy(c => c);
	var jCount = jokerEnabled ? hand.Count(c => c == 'J') : 0;

	if (groups.Count() == 1)
	{
		return HandType.FiveKind;
	}
	else if (groups.Count() == 2)
	{
		if (jCount > 0)
		{
			return HandType.FiveKind;
		}

		var firstCount = groups.First().Count();

		if (firstCount == 4 || firstCount == 1)
		{
			return HandType.FourKind;
		}
		return HandType.FullHouse;
	}
	else if (groups.Count() == 3)
	{
		if (groups.Any(g => g.Count() == 3))
		{
			if (jCount > 0)
			{
				return HandType.FourKind;
			}
			return HandType.ThreeKind;
		}

		if (jCount == 2)
		{
			return HandType.FourKind;
		}
		else if (jCount == 1)
		{
			return HandType.FullHouse;
		}
		return HandType.TwoPair;
	}
	else if (groups.Count() == 4)
	{
		if (jCount > 0)
		{
			return HandType.ThreeKind;
		}
		return HandType.OnePair;
	}
	else
	{
		if (jCount > 0)
		{
			return HandType.OnePair;
		}
		return HandType.HighCard;
	}
}

enum HandType
{
	FiveKind = 7,
	FourKind = 6,
	FullHouse = 5,
	ThreeKind = 4,
	TwoPair = 3,
	OnePair = 2,
	HighCard = 1
};

record Hand( string Cards, long Rank, HandType Type );