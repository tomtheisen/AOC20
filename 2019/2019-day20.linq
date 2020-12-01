<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

var board = ReadBoard();
board.ToLazy().Dump("Board");
var labelList = new List<(string name, Position position)>();

// left side
labelList.AddRange(
	from pos in board
	let t1 = board[pos]
	let t2 = board[pos.Right()]
	let portal = pos.Right().Right()
	where board[portal] == '.'
	where char.IsLetter(t1) 
	where char.IsLetter(t2)
	select ("" + t1 + t2, portal)
);

// right side
labelList.AddRange(
	from pos in board
	let t1 = board[pos]
	let t2 = board[pos.Right()]
	let portal = pos.Left()
	where board[portal] == '.'
	where char.IsLetter(t1) 
	where char.IsLetter(t2)
	select ("" + t1 + t2, portal)
);

// top side
labelList.AddRange(
	from pos in board
	let t1 = board[pos]
	let t2 = board[pos.Down()]
	let portal = pos.Down().Down()
	where board[portal] == '.'
	where char.IsLetter(t1) 
	where char.IsLetter(t2)
	select ("" + t1 + t2, portal)
);

// bottom side
labelList.AddRange(
	from pos in board
	let t1 = board[pos]
	let t2 = board[pos.Down()]
	let portal = pos.Up()
	where board[portal] == '.'
	where char.IsLetter(t1) 
	where char.IsLetter(t2)
	select ("" + t1 + t2, portal)
);

var labels = labelList.ToLookup(e => e.name, e => e.position).Dump("Labels", 0);

var warps = new Dictionary<Position, Position>();
foreach (var group in labels) {
	if (group.Count() != 2) continue;
	warps[group.First()] = group.Last();
	warps[group.Last()] = group.First();
}

var entrance = labels["AA"].Single().Dump("Entrance");
var exit = labels["ZZ"].Single().Dump("Entrance");

var part1Path = BreadthFirst.Create(entrance, "NSEWX".ToCharArray())
	.AddStateFilter(pos => board[pos] == '.')
	.AddActFilter((state, act) => act != 'X' || warps.ContainsKey(state))
	.SetGoal(pos => pos == exit)
	.DetectLoops()
	.SetTransition((state, act) => act switch {
		'N' => state.Up(),
		'S' => state.Down(),
		'W' => state.Left(),
		'E' => state.Right(),
		'X' => warps[state],
	})
	.SetDumpContainer()
	.Search();
part1Path.Length.Dump("Part 1");

{
	var (solPos, solution) = (entrance, board);
	foreach (var step in part1Path) {
		solution = solution.With(solPos, '@');
		solPos = step switch {
			'N' => solPos.Up(),
			'S' => solPos.Down(),
			'W' => solPos.Left(),
			'E' => solPos.Right(),
			'X' => warps[solPos],
		};
	}
	solution.ToLazy().Dump("Part 1 path");
}

var center = board.Center();
bool IsInnerWarp(Position p) => (p - center).SquareDistance() < board.Width / 3;

var part2Path = BreadthFirst.Create(new { Position = entrance, Depth = 0 }, "NSEWX".ToCharArray())
	.AddStateFilter(state => board[state.Position] == '.')
	.AddStateFilter(state => state.Depth >= 0)
	.AddActFilter((state, act) => act != 'X' || warps.ContainsKey(state.Position))
	.SetGoal(state => state.Depth == 0 && state.Position == exit)
	.DetectLoops()
	.SetTransition((state, act) => act switch {
		'N' => new { Position = state.Position.Up(), state.Depth },
		'S' => new { Position = state.Position.Down(), state.Depth },
		'W' => new { Position = state.Position.Left(), state.Depth },
		'E' => new { Position = state.Position.Right(), state.Depth },
		'X' => new { Position = warps[state.Position], Depth = state.Depth + (IsInnerWarp(state.Position) ? 1 : -1)},
	})
	.SetDumpContainer()
	.Search().Length.Dump("Part 2");
