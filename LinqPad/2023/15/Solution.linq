<Query Kind="Statements" />

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample1.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

bool debug = false;

/*
	Sample data:

Determine the ASCII code for the current character of the string.
Increase the current value by the ASCII code you just determined.
Set the current value to itself multiplied by 17.
Set the current value to the remainder of dividing itself by 256.

rn=1,cm-,qp=3,cm=2,qp-,pc=4,ot=9,ab=5,pc-,pc=6,ot=7

*/

var startTime = Util.ElapsedTime;
{
	var values = rawLines[0].Split(',').Select(Hash);
	
	Console.WriteLine($"Challenge 15.1 - Total: {values.Sum()}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	var operations = rawLines[0].Split(',').Select(GetOperation);

	var boxes = new Box[256];
	for (int boxNumber = 0; boxNumber < 256; boxNumber++)
	{
		boxes[boxNumber] = new Box(boxNumber + 1);
	}

	foreach (var operation in operations)
	{
		var box = boxes[operation.Box];
		if (operation.IsUpsert)
		{
			var existing = box.Lenses.FirstOrDefault(l => l.Label == operation.Lens.Label);
			if (existing == null)
			{
				box.Lenses.Add(operation.Lens);
			}
			else
			{
				existing.FocalLength = operation.Lens.FocalLength;
			}
		}
		else
		{
			box.Lenses.RemoveAll(l => l.Label == operation.Lens.Label);
		}
	}

	if (debug)
	{
		boxes.Dump();
	}
	
	var powers = boxes.Where(b => b.Lenses.Count > 0).SelectMany(b => b.FocalPowers);

	if (debug)
	{
		operations.Dump();
	}
	
	Console.WriteLine($"Challenge 15.2 - Power: {powers.Sum()}");
}
(Util.ElapsedTime - startTime).Dump();

int Hash(string sequence) => sequence.Aggregate(0, (currentValue, character) =>
{
		currentValue += (int)character;
		currentValue *= 17;
		currentValue %= 256;
		return currentValue;
});

Operation GetOperation(string step)
{
	var parts = step.Split(new[] { '=', '-'}, StringSplitOptions.RemoveEmptyEntries);
	var box = Hash(parts[0]);
	if (parts.Length == 2)
	{
		return new Operation(true, box, new Lens { Label = parts[0], FocalLength = int.Parse(parts[1]) });
	}
	else
	{
		return new Operation(false, box, new Lens { Label = parts[0], FocalLength = 0 });
	}
}

record Box(int Number)
{
	public List<Lens> Lenses { get; } = new();

	public IEnumerable<int> FocalPowers => Lenses.Count == 0 ? new[] { 0 } : Lenses.Select((lens, offset) => Number * (offset + 1) * lens.FocalLength); 
}

record Operation( bool IsUpsert, int Box, Lens Lens);

class Lens
{
	public string Label { get; init; }
	public int FocalLength { get; set; }
}