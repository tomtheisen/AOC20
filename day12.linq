<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

var input = ReadLines();

{
	var pos = Position.Origin.FaceTo(Direction.E);
	foreach (var instr in input) {
		int amount = int.Parse(instr[1..]);
		
		pos = instr[0] switch {
			'N' => pos.Move(Direction.N * amount),
			'S' => pos.Move(Direction.S * amount),
			'E' => pos.Move(Direction.E * amount),
			'W' => pos.Move(Direction.W * amount),
			'L' => Enumerable.Repeat(0, amount / 90).Aggregate (pos, (p, _) => p.CCW()),
			'R' => Enumerable.Repeat(0, amount / 90).Aggregate (pos, (p, _) => p.CW()),
			'F' => pos.Move(pos.Face * amount),
		};
	}
	pos.Manhattan(Position.Origin).Dump("Part 1");
}

{
	var waypoint = new Position(10, -1);
	var ship = Position.Origin;
	
	foreach (var instr in input) {
		int amount = int.Parse(instr[1..]);
		
		switch (instr[0]) {
			case 'N': waypoint = waypoint.Move(Direction.N * amount); break;
			case 'S': waypoint = waypoint.Move(Direction.S * amount); break;
			case 'E': waypoint = waypoint.Move(Direction.E * amount); break;
			case 'W': waypoint = waypoint.Move(Direction.W * amount); break;
			case 'L': for (; amount > 0; amount -= 90) waypoint = new(waypoint.Y, -waypoint.X); break;
			case 'R': for (; amount > 0; amount -= 90) waypoint = new(-waypoint.Y, waypoint.X); break;
			case 'F': ship = ship.Move(new Direction(waypoint.X, waypoint.Y) * amount); break;
		};
	}
	ship.Manhattan(Position.Origin).Dump("Part 2");
}