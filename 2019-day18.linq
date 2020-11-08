<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\IntCode" 
#load ".\AOC2020"

void Main() {
	var board = ReadBoard();
	
	var boardContainer = new DumpContainer(board);
	
	bool modified;
	do {
		modified = false;
		foreach (var p in board) {
			if (board[p]=='.' && Direction.Cardinal.Count(c => board[p.Move(c)] == '#') == 3) {
				modified = true;
				board = board.With(p, '#');
			}
		}
	} while (!modified);
	
	board.Dump("board");
	
	var pos = board.Find("@");
	boardContainer.Content = board = board.With(pos, '.');
	
	int goalKeys = board.Aggregate(0, 
		(acc, pos) => board[pos] >= 'a' && board[pos] <= 'z' ? (1 << board[pos] - 'a' | acc) : acc)
		.Dump("Goal Keys Mask");

	//*
	BreadthFirst.Create(new { Position = pos, Keys = 0 }, Direction.Cardinal)
		.AddStateFilter(state => {
			char tile = board[state.Position];
			if (tile == '#') return false;
			if (tile >= 'A' && tile <= 'Z') return (state.Keys >> tile - 'A' & 1) > 0;
			return true;
		})
		.SetGoal(state => state.Keys == goalKeys)
		.SetTransition ((state, dir) => {
			var newPos = state.Position.Move(dir);
			var tile = board[newPos];
			int newKeys = state.Keys;
			if (tile >= 'a' && tile <= 'z') newKeys |= 1<< tile - 'a';
			return new { Position = newPos, Keys = newKeys };
		})
		.DetectLoops()
		.SetDumpContainer(new DumpContainer{ DumpDepth = 3 }.Dump("BFS"))
		.Search()
		.Count()
		.Dump("Part 1");
	//*/
	
	board = board.With(pos, '#');
	foreach (var dir in Direction.Cardinal) board = board.With(pos.Move(dir), '#');
	
	var start = new Quad { 
		Position1 = pos.Up().Left(),
		Position2 = pos.Up().Right(),
		Position3 = pos.Down().Left(),
		Position4 = pos.Down().Right(),
	};
	int bestKeys = 0;
	var bestKeysContainer = new DumpContainer().Dump("Best Keys");
	BreadthFirst.Create(start, "1234NSEW".ToCharArray())
		.AddActFilter ((state, act) => {
			if (act < '9') {
				if (state.ActiveIndex == 0) return true;
				if (board[state.ActivePosition] is char c && c >= 'a' && c <= 'z') return true;
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
			var destTile = board[newPos];
			if (destTile >= 'a' && destTile <= 'z') {
				state.Keys |= 1 << destTile - 'a';
				state.KeyCount = BitOperations.PopCount((uint)state.Keys);
				if (state.KeyCount > bestKeys) bestKeysContainer.Content = bestKeys = state.KeyCount;
			}
			state.ActivePosition = newPos;
			return state;
		})
		.AddStateFilter(state => {
			var tile = board[state.ActivePosition];
			if (tile == '#') return false;
			if (tile >= 'A' && tile <= 'Z') return (state.Keys >> tile - 'A' & 1) > 0;
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
}
