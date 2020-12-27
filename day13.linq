<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

var input = ReadLines();
{
	int start = int.Parse(input[0]);
	var buses = input[1]
		.Split(",x".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
		.Select(int.Parse)
		.ToList()
		.Dump("buses");
	
	var best = buses
		.Select(b => new { Wait = ((start + b - 1) / b * b - start), Bus = b})
		.MinBy(t => t.Wait).Dump();
		
	(best.Bus * best.Wait).DumpClip("Part 1");
}

{
	var buses = input[1].Split(',');
	long start = 0, phase = long.Parse(buses[0]);
	for (int i = 1; i < buses.Length; i++) checked {
		if (buses[i] == "x") continue;
		long bus = long.Parse(buses[i]);
		while ((start + i) % bus != 0) start += phase;
		phase = LCM(phase, bus);
	}
	
	start.DumpClip("Part 2");
}
