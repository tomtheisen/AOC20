<Query Kind="Statements">
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

var input = ReadLines();

Direction GetDirection(string instr) => instr[0] switch {
	'U' => Direction.N,
	'R' => Direction.E,
	'D' => Direction.S,
	'L' => Direction.W,
};

{
	var seen = new HashSet<Position>();
	var pos = Position.Origin;
	
	foreach (var instr in input[0].Split(',')) {
		for (int n = int.Parse(instr[1..]); n-- > 0; ) {
			seen.Add(pos = pos.Move(GetDirection(instr)));
		}
	}
	
	pos = Position.Origin;
	int result = int.MaxValue;
	foreach (var instr in input[1].Split(',')) {
		for (int n = int.Parse(instr[1..]); n-- > 0; ) {
			if (seen.Contains(pos = pos.Move(GetDirection(instr)))) {
				result = Min(result, Position.Origin.Manhattan(pos));
			}
		}
	}

	Console.WriteLine(result);
}

{
	var dist = new Dictionary<Position, int>();
	var pos = Position.Origin;
	int time = 0;
	foreach (var instr in input[0].Split(',')) {
		for (int n = int.Parse(instr[1..]); n-- > 0; ) {
			pos = pos.Move(GetDirection(instr));
			++time;
			if (!dist.ContainsKey(pos)) dist.Add(pos, time);
		}
	}
	
	pos = Position.Origin;
	time = 0;
	int result = int.MaxValue;
	foreach (var instr in input[1].Split(',')) {
		for (int n = int.Parse(instr[1..]); n-- > 0; ) {
			pos = pos.Move(GetDirection(instr));
			++time;
			if (dist.ContainsKey(pos)) {
				result = Min(result, time + dist[pos]);
			}
		}
	}
	Console.WriteLine(result);
}
