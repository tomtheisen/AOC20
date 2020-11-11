<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\IntCode" 
#load ".\AOC2020"

static bool IsDoor(char tile) => tile >= 'A' && tile <= 'Z';
static bool IsKey(char tile) => tile >= 'a' && tile <= 'z';
static bool IsDeadEnd(Board b, Position p)
	=> b[p]=='.' | IsDoor(b[p]) && Direction.Cardinal.Count(c => b[p.Move(c)] == '#') == 3;

void Main() {
	var board = ReadBoard();
	var boardContainer = new DumpContainer(board);
	
	bool modified;
	do {
		modified = false;
		foreach (var p in board) {
			if (IsDeadEnd(board, p)) {
				modified = true;
				board = board.With(p, '#');
			}
		}
	} while (modified);
	board.ToLazy().Dump("board");
	
	var pos = board.Find("@");
	boardContainer.Content = board = board.With(pos, '.');
	
	int goalKeys = board.Aggregate(0, 
		(acc, pos) => IsKey(board[pos]) ? (1 << board[pos] - 'a' | acc) : acc)
		.Dump("Goal Keys Mask");

	//*
	var part1Path = BreadthFirst.Create(new { Position = pos, Keys = 0 }, Direction.Cardinal)
		.AddStateFilter(state => {
			char tile = board[state.Position];
			if (tile == '#') return false;
			if (IsDoor(tile)) return (state.Keys >> tile - 'A' & 1) > 0;
			return true;
		})
		.SetGoal(state => state.Keys == goalKeys)
		.SetTransition ((state, dir) => {
			var newPos = state.Position.Move(dir);
			var tile = board[newPos];
			int newKeys = state.Keys;
			if (IsKey(tile)) newKeys |= 1<< tile - 'a';
			return new { Position = newPos, Keys = newKeys };
		})
		.DetectLoops()
		.SetDumpContainer(new DumpContainer{ DumpDepth = 3 }.Dump("BFS"))
		.Search();
	
	part1Path.Count().Dump("Part 1");
	//*/
	
	{
		/*
		var p = pos;
		var keyOrder = "";
		int steps = 0;
		foreach (var d in part1Path) {
			p = p.Move(d);
			++steps;
			if (board[p] >= 'a' && board[p] <= 'z' && !keyOrder.Contains(board[p])) keyOrder += board[p] + " " + steps + "\n";
		}
		Console.WriteLine(keyOrder);
		//*/
	}
	
	//*
	var doors = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Where(e => board.FindAll(e).Any()).ToDictionary(e => e, e => board.Find(e));
	var keys = "abcdefghijklmnopqrstuvwxyz".Where(e => board.FindAll(e).Any()).ToDictionary(e => e, e => board.Find(e));
	
	var psearch = Dijkstra.Create((pos, board, keys: 0, moves: 0), state => state.moves, "abcdefghijklmnopqrstuvwxyz".ToCharArray())
		.SetDumpContainer()
		.SetGoal(state => state.keys == goalKeys)
		.AddActFilter ((state, act) => ((state.keys >> act - 'a') & 1) == 0)
		.SetTransition ((state, act) => {
			var path = BreadthFirst.Create(state.pos, Direction.Cardinal)
				.SetTransition((s, d) => s.Move(d))
				.AddStateFilter(s => state.board[s] == '.' || state.board[s] == act)
				.SetGoal(s => s.Equals(keys[act]))
				.DetectLoops()
				.Search();
			if (path is null) return default;
			
			var newPos = state.pos;
			foreach (var step in path) newPos = newPos.Move(step);
			
			var newBoard = state.board.With(newPos, '.');
			if (doors.TryGetValue(char.ToUpper(act), out var doorPos)) newBoard = newBoard.With(doorPos, '.');
			
			return (newPos, newBoard, state.keys | (1 << act - 'a'), state.moves + path.Length);
		})
		.AddStateFilter(state => state.board is object)
		.DetectLoops(state => new { state.pos, state.keys });
		
		psearch.Search().Dump();
	//*/	

	return;
	
	board = board.With(pos, '#');
	foreach (var dir in Direction.Cardinal) board = board.With(pos.Move(dir), '#');
	
	var start = new Quad { 
		Position1 = pos.Up().Left(),
		Position2 = pos.Up().Right(),
		Position3 = pos.Down().Left(),
		Position4 = pos.Down().Right(),
		Board = board,
	};
	int bestKeys = 0;
	var bestKeysContainer = new DumpContainer().Dump("Best Keys");
	BreadthFirst.Create(start, "1234NSEW".ToCharArray())
		.AddActFilter ((state, act) => {
			if (act < '9') {
				if (state.ActiveIndex == 0) return true;
				if (state.GotKey) return true;
				return false;
			}
			else {
				if (state.ActiveIndex == 0) return false;
				return true;
			}
		})
		.SetTransition ((state, act) => {
			if (act < '9') {
				state.ActiveIndex = act - '0';
				return state;
			}
			var dir = act switch { 'N' => Direction.N, 'E' => Direction.E, 'W' => Direction.W, 'S' => Direction.S };
			var newPos = state.ActivePosition.Move(dir);
			if (IsDeadEnd(state.Board, state.ActivePosition)) {
				state.Board = state.Board.With(state.ActivePosition, '#');
			}
			
			state.GotKey = false;
			var destTile = state.Board[newPos];
			if (IsKey(destTile)) {
				state.Keys |= 1 << destTile - 'a';
				state.KeyCount = BitOperations.PopCount((uint)state.Keys);
				state.GotKey = true;
				state.Board = state.Board.With(newPos, '.');
				foreach (var doorPos in state.Board.FindAll(char.ToUpper(destTile))) {
					state.Board = state.Board.With(doorPos, '.'); // open door
				}
				
				if (state.KeyCount > bestKeys) bestKeysContainer.Content = bestKeys = state.KeyCount;
			}
			state.ActivePosition = newPos;
			return state;
		})
		.AddStateFilter(state => {
			var tile = state.Board[state.ActivePosition];
			if (tile == '#' || IsDoor(tile)) return false;
			return true;
		})
		.AddStateFilter(state => state.KeyCount >= bestKeys - 5)
		.SetGoal(state => state.Keys == goalKeys)
		.SetDumpContainer()
		.DetectLoops()
		.Search()
		.Count(char.IsLetter)
		.Dump("Part 2");
}

struct Quad {
	public Position Position1;
	public Position Position2;
	public Position Position3;
	public Position Position4;
	public int Keys;
	public int KeyCount;
	public int ActiveIndex;
	public Board Board;
	public bool GotKey;
	
	public Position ActivePosition {
		get => ActiveIndex switch { 1=>Position1, 2=>Position2, 3=>Position3, 4=>Position4 };
		set {
			switch (ActiveIndex) {
				case 1: Position1 = value; break;
				case 2: Position2 = value; break;
				case 3: Position3 = value; break;
				case 4: Position4 = value; break;
			}
		}
	}
	
	public override int GetHashCode() 
		=> Hash(Position1, Position2, Position3, Position4, Keys, ActiveIndex);

	public override bool Equals(object? obj) => obj is Quad q 
		&& Position1.Equals(q.Position1) 
		&& Position2.Equals(q.Position2) 
		&& Position3.Equals(q.Position3) 
		&& Position4.Equals(q.Position4)
		&& Keys == q.Keys
		&& ActiveIndex == q.ActiveIndex;
}
