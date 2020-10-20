<Query Kind="Program">
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

void Main() {
	var maze = new Board(@"
		XXXXXXXXXXXXXXXX
		X           X  X
		X  XXXXXXXX  X X
		X          X X X
		X  XXXXXXX X   X
		X        X XXX X
		XXXXXX   X    XX
		X    X XXXXX   X
		X X  X X   XXX X
		X@X        X!  X
		XXXXXXXXXXXXXXXX"[2..]);
		
	maze.Dump();
	var start = maze.Find("@").Dump("start");
	
	// TODO
	// BreadthFirst.Create(start, Direction.Cardinal)
	var path = DepthFirst.Create(start, Direction.Cardinal)
		.SetGoal(pos => maze[pos] == '!')
		.SetTransition((pos, move) => pos.Move(move))
		.AddStateFilter(maze.Contains)
		.AddStateFilter(pos => maze[pos] != 'X')
		.DetectLoops()
		.Search();
		
	foreach (var step in path!) {
		start = start.Move(step);
		maze = maze.With(start, '.');
	}
	maze.Dump();
}

struct Direction : IEquatable<Direction> {
	public int DX { get; }
	public int DY { get; }
	
	public Direction(int dx, int dy) => (DX, DY) = (dx, dy);

	public override int GetHashCode() => (int)BitOperations.RotateRight((uint)DX, 16) ^ DY;
	public bool Equals(Direction other) => other.DX == DX && other.DY == DY;

	public static Direction Up = new Direction(0, -1);
	public static Direction Right = new Direction(1, 0);
	public static Direction Down = new Direction(0, 1);
	public static Direction Left = new Direction(-1, 0);
	public static Direction[] Cardinal = { Up, Right, Down, Left };

	public Direction CW() => new Direction(-DY, DX);
	public Direction CCW() => new Direction(DY, -DX);
	public Direction Reverse() => new Direction(-DX, -DY);

	public override string ToString() => $"d({DX},{DY})";
	public string ToDump() => ToString();
	
	public void Deconstruct(out int dx, out int dy) => (dx, dy) = (DX, DY);
}

struct Position : IEquatable<Position> {
	public int X { get; }
	public int Y { get; }
	public Direction Face { get; }

	public Position(int x, int y) => (X, Y, Face) = (x, y, default);
	public Position(int x, int y, Direction face) => (X, Y, Face) = (x, y, face);

	public override int GetHashCode() => Face.GetHashCode()
		^ (int)BitOperations.RotateRight((uint)X, 8)
		^ (int)BitOperations.RotateRight((uint)X, 24);
	public bool Equals(Position other)
		=> other.X == X && other.Y == Y && other.Face.Equals(Face);

	public Position Step() => new Position(X + Face.DX, Y + Face.DY, Face);
	public Position Move(Direction move) => new Position(X + move.DX, Y + move.DY, Face);
	public Position Up() => this.Move(Direction.Up);
	public Position Right() => this.Move(Direction.Right);
	public Position Down() => this.Move(Direction.Down);
	public Position Left() => this.Move(Direction.Left);

	public Position FaceTo(Direction face) => new Position(X, Y, face);
	public Position CW() => this.FaceTo(Face.CW());
	public Position CCW() => this.FaceTo(Face.CCW());
	public Position Reverse() => this.FaceTo(Face.Reverse());

	public override string ToString() => $"({X}, {Y})";
	public string ToDump() => ToString();

	public void Deconstruct(out int x, out int y) => (x, y) = (X, Y);
	public void Deconstruct(out int x, out int y, out Direction face) => (x, y, face) = (X, Y, Face);

	public Position[] Neighbors() {
		var self = this;
		return Array.ConvertAll(Direction.Cardinal, d => self.Move(d));
	}
}

class Board : IEnumerable<Position> {
	private readonly char[,] _Cells;
	public int Width { get; }
	public int Height { get; }
	
	public Board(char[,] cells) {
		_Cells = cells;
		Width = cells.GetUpperBound(0) + 1;
		Height = cells.GetUpperBound(1) + 1;
	}
	
	public Board(string cells) {
		string[] lines = Regex.Split(cells, @"\r?\n");
		Height = lines.Length;
		Width = lines.Max(l => l.Length);
		_Cells = new char[Width, Height];
		
		for (int y = 0; y < Height; y++) {
			for (int x = 0; x < lines[y].Length; x++) {
				_Cells[x, y] = lines[y][x];
			}
		}
	}
	
	public char this[int x, int y] => _Cells[x, y];
	public char this[Position p] => _Cells[p.X, p.Y];
	
	public bool Contains(Position p) => Contains(p.X, p.Y);
	public bool Contains(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

	public Board With(int x, int y, char c) {
		var newCells = (char[,])_Cells.Clone();
		newCells[x, y] = c;
		return new Board(newCells);
	}
	public Board With(Position p, char c) => With(p.X, p.Y, c);

	public IEnumerator<Position> GetEnumerator() {
		for (int x = 0; x < this.Width; x++) {
			for (int y = 0; y < this.Height; y++) {
				yield return new Position(x, y);
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
	
	public IEnumerable<Position> FindAll(string needles) => this.Where(t => needles.Contains(this[t]));
	public Position Find(string needles) => this.First(t => needles.Contains(this[t]));
	
	public string ToDump() {
		var sb = new StringBuilder();
		for (int y = 0; y < this.Height; y++) {
			for (int x = 0; x < this.Width; x++) {
				sb.Append(this[x, y]);
			}
			sb.Append('\n');
		}
		return sb.ToString();
	}
}

public class History<T> : IEnumerable<T> {
	public readonly T Last;
	public readonly History<T> Prefix;
	private readonly bool IsEmpty;
	
	public History() {
		IsEmpty = true;
		Prefix = this;
		Last = default!;
	}
	
	public History(T start) {
		Prefix = new History<T>();
		Last = start;
	}
	
	public History(History<T> prefix, T last) {
		Prefix = prefix;
		Last = last;
	}
	
	public static History<T> Empty { get; } = new History<T>();
	
	public History<T> AndThen(T state) => new History<T>(this, state);

	public IEnumerator<T> GetEnumerator() {
		if (IsEmpty) yield break;
		foreach (var element in Prefix) yield return element;
		yield return Last;
	}

	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}

public delegate IEnumerable<(TState, TTransition)> StateTransition<TState, TTransition>(TState state);

public abstract class SearchBase<TState, TAct> {
	public Predicate<TState> Goal { get; set; }
	public IList<TAct> Acts { get; set; }
	public List<Func<TState, TAct, bool>> ActFilters { get; } = new List<Func<TState, TAct, bool>>();
	public List<Predicate<TState>> StateFilters { get; } = new List<Predicate<TState>>();
	public Func<TState, TAct, TState> Transition { get; set; }
	public HashSet<TState>? Seen { get; set; }
	public TState Start { get; set; }
	public DumpContainer? DumpContainer { get; set; }

	public SearchBase(TState start, IList<TAct> acts) {
		this.Start = start;
		this.Acts = acts;
		this.Goal = _ => true;
		this.Transition = (_, _) => throw new Exception("Transition not specified");
	}
	
	public SearchBase<TState, TAct> AddActFilter(Func<TState, TAct, bool> legalAct) {
		this.ActFilters.Add(legalAct);
		return this;
	}
	
	public SearchBase<TState, TAct> AddStateFilter(Predicate<TState> legalState) {
		this.StateFilters.Add(legalState);
		return this;
	}
	
	public SearchBase<TState, TAct> SetGoal(Predicate<TState> goal) {
		this.Goal = goal;
		return this;
	}
	
	public SearchBase<TState, TAct> SetTransition(Func<TState, TAct, TState> transition) {
		this.Transition = transition;
		return this;
	}
	
	public SearchBase<TState, TAct> DetectLoops() {
		this.Seen = new HashSet<TState>();
		return this;
	}

	public SearchBase<TState, TAct> SetDumpContainer(DumpContainer dumpContainer) {
		this.DumpContainer = dumpContainer;
		return this;
	}
	
	public abstract IEnumerable<TAct>? Search();
}

public static class DepthFirst {
	public static DepthFirst<TState, TAct> Create<TState, TAct>(TState start, IList<TAct> acts)
		=> new DepthFirst<TState, TAct>(start, acts);
}

public class DepthFirst<TState, TAct>: SearchBase<TState, TAct> {
	public DepthFirst(TState start, IList<TAct> acts) : base(start, acts) {}
	
	public override IEnumerable<TAct>? Search() => Search(Start, new History<TAct>());
	
	public IEnumerable<TAct>? Search(TState state, History<TAct> history) {
		if (Goal(state)) return history;
		
		if (Seen?.Contains(state) == true) return null;
		Seen?.Add(state);
		if (DumpContainer is object) DumpContainer.Content = state;
		
		foreach (var act in Acts) {
			if (ActFilters.Any(af => !af(state, act))) continue;
			var newState = Transition(state, act);
			if (StateFilters.Any(sf => !sf(newState))) continue;
			var newHistory = history.AndThen(act);
			if (Search(newState, newHistory) is IEnumerable<TAct> result) return result;
		}
		return null;
	}
}

public static class BreadthFirst {
	public static BreadthFirst<TState, TAct> Create<TState, TAct>(TState start, IList<TAct> acts)
		=> new BreadthFirst<TState, TAct>(start, acts);
}

public class BreadthFirst<TState, TAct>: SearchBase<TState, TAct> {
	public BreadthFirst(TState start, IList<TAct> acts) : base(start, acts) {}
	
	public override IEnumerable<TAct>? Search() {
		var queue = new Queue<(TState, History<TAct>)>();
		
		for (queue.Enqueue((Start, History<TAct>.Empty)); queue.TryDequeue(out var element); ) {
			var (state, history) = element;
			if (Goal(state)) return history;

			if (Seen?.Contains(state) == true) return null;
			Seen?.Add(state);
			if (DumpContainer is object) DumpContainer.Content = state;
			
			foreach (var act in Acts) {
				if (ActFilters.Any(af => !af(state, act))) continue;
				var newState = Transition(state, act);
				if (StateFilters.Any(sf => !sf(newState))) continue;
				var newHistory = history.AndThen(act);
				queue.Enqueue((newState, newHistory));
			}
		}
		
		return null;
	}
}
