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
	=> b[p]=='.' | IsDoor(b[p]) && p.Adjacent().Count(a => b[a] == '#') == 3;

void Main() {
	var board = ReadBoard();
	board.ToLazy().Dump("board");
	
	var pos = board.Find("@");
	
	int goalKeys = board.Aggregate(0, 
		(acc, pos) => IsKey(board[pos]) ? (1 << board[pos] - 'a' | acc) : acc)
		.Dump("Goal Keys Mask");

	// Priority search over key-seeking
    //
	var doorPos = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Where(e => board.FindAll(e).Any()).ToDictionary(e => e, e => board.Find(e));
	var keyPos = "abcdefghijklmnopqrstuvwxyz".Where(e => board.FindAll(e).Any()).ToDictionary(e => e, e => board.Find(e));

    // Priority lookups over key-seeking
    var paths = new Dictionary<(char, char), (int Distance, int KeysNeeded)>();
    for (char startKey = '@'; startKey <= 'z'; startKey++, startKey |= ' ') {
        var startPos = board.Find(startKey);
        BreadthFirst.Create((pos: startPos, moves: 0, keysNeeded: 0), Direction.Cardinal)
            .DetectLoops(state => state.pos)
            .SetGoal(_ => false)
            .AddStateFilter(state => board[state.pos] != '#')
            .SetTransition((state, act) => {
                int newKeysNeeded = state.keysNeeded;
                char tile = board[state.pos];
                if (IsDoor(tile)) newKeysNeeded |= 1 << tile - 'A';
                else if (IsKey(tile) && tile != startKey) paths[(startKey, tile)] = (state.moves, state.keysNeeded);
                
                // moving past a key is inefficient unless you already have that key
                if (IsKey(tile) && tile != startKey) newKeysNeeded |= 1 << tile - 'a'; 
                
                return (state.pos.Move(act), state.moves + 1, newKeysNeeded);
            })
			.AddStateFilter(state => state.moves >= 0)
            .Search();
    }

	Dijkstra.Create((pos, keys: 0, moves: 0), state => state.moves, "abcdefghijklmnopqrstuvwxyz".ToCharArray())
		.SetDumpContainer(new DumpContainer().Dump("Pre-moved Dijkstra search"))
		.SetGoal(state => state.keys == goalKeys)
		.AddActFilter ((state, act) => ((state.keys >> act - 'a') & 1) == 0)
		.SetTransition ((state, act) => {
			if (!paths.TryGetValue((board[state.pos], act), out var path)) return (default, -1, -1);
            if ((path.KeysNeeded & state.keys) != path.KeysNeeded) return (default, -1 , -1);
			return (keyPos[act], state.keys | (1 << act - 'a'), state.moves + path.Distance);
		})
		.AddStateFilter(state => state.moves >= 0)
		.DetectLoops(state => new { state.pos, state.keys })
		.Search(out var p1state);
    p1state.moves.Dump("Dijkstra pre-moves Part 1");
	
	var start = new Quad { 
		Position1 = pos,
		Position2 = pos,
		Position3 = pos,
		Position4 = pos,
		Moves = -8, // to get to start positions from center
	};

	Dijkstra.Create(start, state => state.Moves, "abcdefghijklmnopqrstuvwxyz".ToCharArray())
		.AddActFilter ((state, act) => ((state.Keys >> act - 'a') & 1) == 0)
		.SetTransition ((state, act) => {
			var newPos = keyPos[act];
			int quadrant = state.GetQuadrantForPosition(newPos);
			var originalPos = state.GetPosition(quadrant);
			
			if (!paths.TryGetValue((board[originalPos], act), out var path)) return new Quad { Moves = int.MinValue };
			var (distance, keysNeeded) = path;
            if ((path.KeysNeeded & state.Keys) != path.KeysNeeded) return new Quad { Moves = int.MinValue };
			
			state.Keys |= 1 << act - 'a';
			state.SetPosition(quadrant, newPos);
			state.Moves += distance;
			return state;
		})
		.AddStateFilter(state => state.Moves > int.MinValue)
		.SetGoal(state => state.Keys == goalKeys)
		.SetDumpContainer(new DumpContainer().Dump("Dijkstra part 2"))
		.DetectLoops()
		.Search(out var p2state);
		p2state.Moves.Dump("Part 2");
}

struct Quad {
	public Position Position1;
	public Position Position2;
	public Position Position3;
	public Position Position4;
	public int Keys;
	public int Moves;
	
	public int GetQuadrantForPosition(Position p) {
		var (x, y) = p;
		if (x <= 40 && y <= 40) return 1;
		if (x >= 40 && y <= 40) return 2;
		if (x <= 40 && y >= 40) return 3;
		if (x >= 40 && y >= 40) return 4;
		return 0;
	}
	
	public Position GetPosition(int quadrant) 
		=> quadrant switch { 1=>Position1, 2=>Position2, 3=>Position3, 4=>Position4 };
		
	public void SetPosition(int quadrant, Position p) {
		switch (quadrant) {
			case 1: Position1 = p; break;
			case 2: Position2 = p; break;
			case 3: Position3 = p; break;
			case 4: Position4 = p; break;
			default: throw new ArgumentOutOfRangeException();
		}
	}
	
	public override int GetHashCode() 
		=> Hash(Position1, Position2, Position3, Position4, Keys);

	public override bool Equals(object? obj) => obj is Quad q 
		&& Position1.Equals(q.Position1) 
		&& Position2.Equals(q.Position2) 
		&& Position3.Equals(q.Position3) 
		&& Position4.Equals(q.Position4)
		&& Keys == q.Keys;
}
