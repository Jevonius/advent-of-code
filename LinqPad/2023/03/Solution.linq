<Query Kind="Statements" />

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

/*
	Sample data:
	
	467..114..
	...*......
	..35..633.
	......#...
	617*......
	.....+.58.
	..592.....
	......755.
	...$.*....
	.664.598..

*/
var maxHeight = rawLines.Count();
var maxWidth = rawLines.First().Length;
var data = new Block[maxWidth, maxHeight];
for (int row = 0; row < maxHeight; row++)
{
	var line = rawLines[row];
	for (int col = 0; col < maxWidth; col++)
	{
		var character = line[col];
		BlockType type = BlockType.Symbol;
		if( character == '.' ) {
			type = BlockType.Ignore;
		} else if (char.IsNumber(character)){
			type = BlockType.Number;
		}
		data[col, row] = new Block( type, character );
	}
}

maxHeight -= 1;
maxWidth -= 1;

var startTime = Util.ElapsedTime;
{
	var curNumber = string.Empty;
	var foundSymbol = false;
	var numbers = new List<int>();

	for (int row = 0; row <= maxHeight; row++)
	{
		for (int col = 0; col <= maxWidth; col++)
		{
			var block = data[col, row];
			foundSymbol = foundSymbol
				|| (
					(row > 0 && data[col, row - 1].BlockType == BlockType.Symbol)
					|| block.BlockType == BlockType.Symbol
					|| (row < maxHeight && data[col, row + 1].BlockType == BlockType.Symbol)
				);

			if (block.BlockType == BlockType.Number)
			{
				if (curNumber == string.Empty)
				{
					// Check previous col for symbols
					foundSymbol = foundSymbol
						|| (
							col > 0
							&& (
								(row > 0 && data[col - 1, row - 1].BlockType == BlockType.Symbol)
								|| data[col - 1, row].BlockType == BlockType.Symbol
								|| (row < maxHeight && data[col - 1, row + 1].BlockType == BlockType.Symbol)
							)
						);
				}
				curNumber += block.Character;
			}
			else
			{
				if (curNumber != string.Empty && foundSymbol)
				{
					numbers.Add(int.Parse(curNumber));
				}
					curNumber = string.Empty;
					foundSymbol = false;
			}
		}

		if(curNumber != string.Empty && foundSymbol ){
			numbers.Add(int.Parse(curNumber));
			curNumber = string.Empty;
			foundSymbol = false;
		}
	}
	
	var sum = numbers.Sum();
	Console.WriteLine($"Challenge 3.1 - Sum: {sum}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	long total = 0;
	for (int row = 0; row <= maxHeight; row++)
	{
		for (int col = 0; col <= maxWidth; col++)
		{
			var block = data[col, row];
			if (block.Character == '*')
			{
				var numbers = new List<int>();
				// Find surrounding numbers
				if (row > 0)
				{
					// Look above
					var aboveLeft = (col > 0) && data[col - 1, row - 1].BlockType == BlockType.Number;
					var above = (col > 0) && data[col, row - 1].BlockType == BlockType.Number;
					var aboveRight = (col < maxWidth) && data[col + 1, row - 1].BlockType == BlockType.Number;
					
					if( aboveLeft ) {
						numbers.Add(FindNumber(data, row - 1, col - 1));
					}
					if (above && !aboveLeft)
					{
						numbers.Add(FindNumber(data, row - 1, col));
					}
					if (aboveRight && !above)
					{
						numbers.Add(FindNumber(data, row - 1, col + 1));
					}
				}
				if (col > 0 && data[col - 1, row].BlockType == BlockType.Number)
				{
					numbers.Add(FindNumber(data, row, col - 1));
				}
				if (col < maxWidth && data[col + 1, row].BlockType == BlockType.Number)
				{
					numbers.Add(FindNumber(data, row, col + 1));
				}
				if (row < maxHeight)
				{
					// Look below
					var belowLeft = (col > 0) && data[col - 1, row + 1].BlockType == BlockType.Number;
					var below = (col > 0) && data[col, row + 1].BlockType == BlockType.Number;
					var belowRight = (col < maxWidth) && data[col + 1, row + 1].BlockType == BlockType.Number;

					if (belowLeft)
					{
						numbers.Add(FindNumber(data, row + 1, col - 1));
					}
					if (below && !belowLeft)
					{
						numbers.Add(FindNumber(data, row + 1, col));
					}
					if (belowRight && !below)
					{
						numbers.Add(FindNumber(data, row + 1, col + 1));
					}
				}
				if( numbers.Count == 2 ) {
					total += numbers[0] * numbers[1];
				}
			}
		}
	}
	Console.WriteLine($"Challenge 3.2 - Total: {total}");
}
(Util.ElapsedTime - startTime).Dump();

int FindNumber(Block[,] data, int row, int col)
{
	var myBlock = data[col, row];
	var prefix = (col > 0) ? FindNumberPrefix(data, row, col - 1) : string.Empty;
	var suffix = (col < maxWidth) ? FindNumberSuffix(data, row, col + 1) : string.Empty;
	return int.Parse(prefix + myBlock.Character + suffix);
}

string FindNumberPrefix(Block[,] data, int row, int col)
{
	var myBlock = data[col, row];
	if (myBlock.BlockType != BlockType.Number)
	{
		return string.Empty;
	}
	else if (col > 0)
	{
		return FindNumberPrefix(data, row, col - 1) + myBlock.Character;
	}
	else
	{
		return myBlock.Character.ToString();
	}
}
string FindNumberSuffix(Block[,] data, int row, int col)
{
	var myBlock = data[col, row];
	if (myBlock.BlockType != BlockType.Number)
	{
		return string.Empty;
	}
	else if (col < maxWidth)
	{
		return myBlock.Character + FindNumberSuffix(data, row, col + 1);
	}
	else
	{
		return myBlock.Character.ToString();
	}
}

record Block(BlockType BlockType, char Character);
enum BlockType
{
	Ignore,
	Symbol,
	Number
}