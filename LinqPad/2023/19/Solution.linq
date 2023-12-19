<Query Kind="Statements" />

var rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "sample1.txt"));
rawLines = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "input.txt"));

bool debug = false;

/*
	Sample data:

px{a<2006:qkq,m>2090:A,rfg}
pv{a>1716:R,A}
lnx{m>1548:A,A}
rfg{s<537:gd,x>2440:R,A}
qs{s>3448:A,lnx}
qkq{x<1416:A,crn}
crn{x>2662:A,R}
in{s<1351:px,qqz}
qqz{s>2770:qs,m<1801:hdj,R}
gd{a>3333:R,R}
hdj{m>838:A,pv}

{x=787,m=2655,a=1222,s=2876}
{x=1679,m=44,a=2067,s=496}
{x=2036,m=264,a=79,s=2244}
{x=2461,m=1339,a=466,s=291}
{x=2127,m=1623,a=2188,s=1013}

*/

var workflows = rawLines.TakeWhile(l => l != string.Empty).Select(BuildWorkflow).ToDictionary(w => w.Name);
var parts = rawLines.SkipWhile(l => l != string.Empty).Skip(1).Select(BuildPart);

if (debug)
{
	workflows.Dump();
	parts.Dump();
}

var startTime = Util.ElapsedTime;
{
	var total = 0;
	var workflowIn = workflows["in"];
	foreach (var part in parts)
	{
		var currentWorkflow = workflowIn;
		while (true)
		{
			var answer = currentWorkflow.Test(part);
			if (answer == "A")
			{
				total += part.Total;
				break;
			}
			else if (answer == "R")
			{
				break;
			}
			currentWorkflow = workflows[answer];
		}
	}
	Console.WriteLine($"Challenge 19.1 - Result: {total}");
}
(Util.ElapsedTime - startTime).Dump();
Console.WriteLine();

startTime = Util.ElapsedTime;
{
	var acceptable = new List<Acceptable>();
	var stack = new List<Rule>();
	foreach (var rule in workflows["in"].Rules)
	{
		DigAccept(acceptable, workflows, rule, stack);
		// Invert the rule, add it to the stack ("else" path).
		stack.Add(Inverted(rule));
	}

	if (debug)
	{
		acceptable
			.OrderBy(a => a.XMin).ThenBy(a => a.XMax)
			.ThenBy(a => a.MMin).ThenBy(a => a.MMax)
			.ThenBy(a => a.AMin).ThenBy(a => a.AMax)
			.ThenBy(a => a.SMin).ThenBy(a => a.SMax)
			.Dump();
	}

	var total = acceptable.Sum(a => a.Total);
	Console.WriteLine($"Challenge 19.2 - Result: {total}");
}
(Util.ElapsedTime - startTime).Dump();

void DigAccept(List<Acceptable> acceptable, Dictionary<string, Workflow> workflows, Rule current, List<Rule> stack)
{
	if( current.Answer == "R" ){
		return;
	}
	stack.Add(current);
	if( current.Answer == "A")
	{
		BuildAcceptable(acceptable, stack);
	}
	else
	{
		var workflowRules = workflows[current.Answer].Rules;
		foreach (var rule in workflowRules)
		{
			DigAccept(acceptable, workflows, rule, stack);
			// Invert the rule, add it to the stack ("else" path).
			stack.Add(Inverted(rule));
		}
		var ruleCount = workflowRules.Count;
		stack.RemoveRange(stack.Count - ruleCount, ruleCount);
	}
	stack.Remove(current);
}

Rule Inverted(Rule rule)
{
	if (rule.Parameter == '-')
	{
		// Nothing to do
		return rule;
	}
	if (rule.IsLessThan){
		// y < 10 => y > 9
		return new Rule(null, rule.Parameter, rule.Value - 1, false, rule.Answer);
	}
	else
	{
		// y > 10 => y < 11
		return new Rule(null, rule.Parameter, rule.Value + 1, true, rule.Answer);
	}
}

void BuildAcceptable(List<Acceptable> acceptable, List<Rule> stack)
{
	var minX = 1;
	var maxX = 4000;

	var minM = 1;
	var maxM = 4000;

	var minA = 1;
	var maxA = 4000;

	var minS = 1;
	var maxS = 4000;

	foreach (var rule in stack)
	{
		if (debug)
		{
			Console.Write($"{{{rule.Answer}}}");
		}
		switch (rule.Parameter)
		{
			case '-':
				// Ignore
				break;
			case 'x':
				if (rule.IsLessThan)
				{
					maxX = Math.Min(maxX, rule.Value - 1);
				}
				else
				{
					minX = Math.Max(minX, rule.Value + 1);
				}
				break;
			case 'm':
				if (rule.IsLessThan)
				{
					maxM = Math.Min(maxM, rule.Value - 1);
				}
				else
				{
					minM = Math.Max(minM, rule.Value + 1);
				}
				break;
			case 'a':
				if (rule.IsLessThan)
				{
					maxA = Math.Min(maxA, rule.Value - 1);
				}
				else
				{
					minA = Math.Max(minA, rule.Value + 1);
				}
				break;
			case 's':
				if (rule.IsLessThan)
				{
					maxS = Math.Min(maxS, rule.Value - 1);
				}
				else
				{
					minS = Math.Max(minS, rule.Value + 1);
				}
				break;
			default: throw new InvalidDataException();
		}
	}

	if (
		(1 + maxX - minX < 0)
		|| (1 + maxM - minM < 0)
		|| (1 + maxA - minA < 0)
		|| (1 + maxS - minS < 0)
	)
	{
		if (debug) Console.WriteLine(" - Not Valid");
		return;
	}

	var accept = new Acceptable(minX, maxX, minM, maxM, minA, maxA, minS, maxS);
	if (debug) Console.WriteLine(" - " + accept.ToString());
	acceptable.Add(accept);
}

Workflow BuildWorkflow(string rawLine)
{
	// ex{x>10:one,m<20:two,a>30:R,A}
	var ruleStart = rawLine.IndexOf('{');
	var name = rawLine.Substring(0, ruleStart);
	var rules = rawLine.Substring(ruleStart + 1, rawLine.Length - ruleStart - 2).Split(',').Select(BuildRule).ToList();
	return new Workflow(name, rules);
}

Rule BuildRule(string rawRule)
{
	var colon = rawRule.IndexOf(':');
	if (colon >= 0)
	{
		var parameter = rawRule.First();
		var isLessThan = rawRule[1] == '<';
		var value = int.Parse(rawRule.Substring(2, colon - 2));
		var answer = rawRule.Substring(colon + 1);
		return new Rule((Part part) =>
		{
			var parValue = parameter switch
			{
				'x' => part.X,
				'm' => part.M,
				'a' => part.A,
				's' => part.S,
				_ => throw new InvalidDataException()
			};
			if (isLessThan)
			{
				return parValue < value ? answer : string.Empty;
			}
			else
			{
				return parValue > value ? answer : string.Empty;
			}
		}, parameter, value, isLessThan, answer);
	}
	else
	{
		return new Rule((Part part) => rawRule,'-', 0, false, rawRule);
	}
}

Part BuildPart(string rawLine)
{
	var parts = rawLine.Split(new[] { '{', 'x', 'm', 'a', 's', ',', '=', '}'}, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
	return new Part(parts[0], parts[1],parts[2],parts[3]);
}

record Part(int X, int M, int A, int S)
{
	public int Total => X + M + A + S;
}
record Workflow(string Name, List<Rule> Rules)
{
	public string Test(Part part)
	{
		foreach (var rule in Rules)
		{
			var answer = rule.Test(part);
			if (answer != string.Empty)
			{
				return answer;
			}
		}
		throw new InvalidOperationException();
	}
}
record Rule(Func<Part, string> Test, char Parameter, int Value, bool IsLessThan, string Answer);
record Acceptable(int XMin, int XMax, int MMin, int MMax, int AMin, int AMax, int SMin, int SMax)
{
	public long Total =>
		(1L + XMax - XMin)
		* (1L + MMax - MMin)
		* (1L + AMax - AMin)
		* (1L + SMax - SMin);
};