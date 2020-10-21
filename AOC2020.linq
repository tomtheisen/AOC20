<Query Kind="Program">
  <Namespace>static System.Math</Namespace>
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
		
	var start = maze.Find("@").Dump("start");
	
	var path = DepthFirst.Create(start, Direction.Cardinal)
		.SetGoal(pos => maze[pos] == '!')
		.SetTransition((pos, move) => pos + move)
		.AddStateFilter(maze.Contains)
		.AddStateFilter(pos => maze[pos] != 'X')
		.DetectLoops()
		.Search();
		
	foreach (var step in path!) 
		maze = maze
			.With(start = start + step, '.')
			//.Materialize()
			;
	maze.Materialize().Dump();
	
	const int SomeNumber = 12345;
	var binary = BreadthFirst.Create(0, new[] {0,1})
		.SetGoal(SomeNumber.Equals)
		.SetTransition((val, bit) => 2 * val + bit)
		.Search();
	string.Concat(binary!).Dump("Binary: " + SomeNumber);

	var q = new PriorityQueue<int>(n => n, Enumerable.Repeat(new Random(), 100).Select(rng => rng.Next(100)));
	while (q.Count > 0) Console.WriteLine(q.Pop());
}

struct Direction : IEquatable<Direction> {
	public int DX { get; }
	public int DY { get; }
	
	public Direction(int dx, int dy) => (DX, DY) = (dx, dy);

	public override int GetHashCode() => (int)BitOperations.RotateRight((uint)DX, 16) ^ DY;
	public bool Equals(Direction other) => other.DX == DX && other.DY == DY;

	public static Direction Zero = new Direction(0, 0);
	public static Direction N = new Direction(0, -1), E = new Direction(1, 0), S = new Direction(0, 1), W = new Direction(-1, 0);
	public static Direction[] Cardinal = { N, E, S, W };
	public static Direction NW = N + W, NE = N + E, SW = S + W, SE = S + E;
	public static Direction[] InterCardinal = { N, NE, E, SE, S, SW, W, NW };

	public Direction CW() => new Direction(-DY, DX);
	public Direction CCW() => new Direction(DY, -DX);
	public Direction Reverse() => new Direction(-DX, -DY);

	public int Manhattan() => Abs(DX) + Abs(DY);
	public int SquareDistance() => Max(Abs(DX), Abs(DY));

	public override string ToString() => $"d({DX},{DY})";
	public string ToDump() => ToString();
	
	public void Deconstruct(out int dx, out int dy) => (dx, dy) = (DX, DY);
	
	public static Direction operator +(Direction a, Direction b) => new Direction(a.DX + b.DX, a.DY + b.DY);
	public static bool operator ==(Direction a, Direction b) => a.DX == b.DX && a.DY == b.DY;
	public static bool operator !=(Direction a, Direction b) => !(a == b);
	public override bool Equals(object? obj) => obj is Direction d && this == d;
}

struct Position : IEquatable<Position> {
	public int X { get; }
	public int Y { get; }
	public Direction Face { get; }

	public Position(int x, int y) => (X, Y, Face) = (x, y, default);
	public Position(int x, int y, Direction face) => (X, Y, Face) = (x, y, face);
	
	public static readonly Position Origin = new Position(0, 0);

	public override int GetHashCode() => Face.GetHashCode()
		^ (int)BitOperations.RotateRight((uint)X, 8)
		^ (int)BitOperations.RotateRight((uint)X, 24);
	public bool Equals(Position other)
		=> other.X == X && other.Y == Y && other.Face.Equals(Face);

	public Position Step() => new Position(X + Face.DX, Y + Face.DY, Face);
	public Position Up() => this + Direction.N;
	public Position Right() => this + Direction.E;
	public Position Down() => this + Direction.S;
	public Position Left() => this + Direction.W;

	public Position FaceTo(Direction face) => new Position(X, Y, face);
	public Position CW() => this.FaceTo(Face.CW());
	public Position CCW() => this.FaceTo(Face.CCW());
	public Position Reverse() => this.FaceTo(Face.Reverse());

	public static Direction operator -(Position a, Position b) => new Direction(a.X - b.X, a.Y - b.Y);
	public static Position operator +(Position p, Direction d) => new Position(p.X + d.DX, p.Y + d.DY, p.Face);
	public static bool operator ==(Position a, Position b) => a.X == b.X && a.Y == b.Y && a.Face == b.Face;
	public static bool operator !=(Position a, Position b) => !(a == b);
	public override bool Equals(object? obj) => obj is Position p && this == p;
	
	public Position Move(Direction d) => this + d;

	public int Manhattan(Position other) => (this - other).Manhattan();
	public int SquareDistance(Position other) => (this - other).SquareDistance();

	public void Deconstruct(out int x, out int y) => (x, y) = (X, Y);
	public void Deconstruct(out int x, out int y, out Direction face) => (x, y, face) = (X, Y, Face);

	public Position[] Adjacent() => Array.ConvertAll(Direction.Cardinal, Move);
	public Position[] Neighbors() => Array.ConvertAll(Direction.InterCardinal, Move);
	
	public override string ToString() => $"({X}, {Y})";
	public string ToDump() => ToString();
}

class Board : IEnumerable<Position> {
	public int Width { get; }
	public int Height { get; }
	private char[,]? Cells;
	private Board? OriginalBoard;
	private Position? ChangedPosition;
	private char? NewChar;
	private int Misses = 0;
	
	public Board(char[,] cells) {
		Cells = cells;
		Width = cells.GetUpperBound(0) + 1;
		Height = cells.GetUpperBound(1) + 1;
	}
	
	public Board(Board oldBoard, Position changedPosition, char newChar) {
		(OriginalBoard, Width, Height) = (oldBoard, oldBoard.Width, oldBoard.Height);
		(ChangedPosition, NewChar) = (changedPosition, newChar);
	}
	
	public Board(string cells) {
		string[] lines = Regex.Split(cells, @"\r?\n");
		Height = lines.Length;
		Width = lines.Max(l => l.Length);
		Cells = new char[Width, Height];
		
		for (int y = 0; y < Height; y++)
			for (int x = 0; x < lines[y].Length; x++) Cells[x, y] = lines[y][x];
	}
	
	public char this[int x, int y] { 
		get {
			if (Cells is object) return Cells[x, y];
			if (x == ChangedPosition?.X && y == ChangedPosition?.Y) return NewChar!.Value;
			if (++Misses > Width * Height) {
				Materialize();
				return Cells[x, y];
			}
			return OriginalBoard![x, y];
		}
	}
	public char this[Position p] => this[p.X, p.Y];
	
	public Board Materialize() {
		if (Cells is object) return this;
		
		Cells = new char[Width, Height];
		
		var board = this;
		var mods = new List<Board>();
		while (board.OriginalBoard is Board current) {
			mods.Add(board);
			board = current;
		}
		
		for (int i = 0; i < Width; i++)
			for (int j = 0; j < Height; j++) Cells[i, j] = board.Cells![i, j];
		
		mods.Reverse();
		foreach (var mod in mods) {
			var pos = mod.ChangedPosition!.Value;
			Cells[pos.X, pos.Y] = mod.NewChar!.Value;
		}
		
		OriginalBoard = null;
		ChangedPosition = null;
		NewChar = null;
		
		return this;
	}
	
	public bool Contains(Position p) => Contains(p.X, p.Y);
	public bool Contains(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

	public Board With(int x, int y, char c) => new Board(this, new Position(x, y), c);

	public Board With(Position p, char c) => With(p.X, p.Y, c);

	public IEnumerator<Position> GetEnumerator() {
		for (int x = 0; x < this.Width; x++)
			for (int y = 0; y < this.Height; y++)
				yield return new Position(x, y);
	}

	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
	
	public IEnumerable<Position> FindAll(string needles) => this.Where(t => needles.Contains(this[t]));
	public Position Find(string needles) => this.First(t => needles.Contains(this[t]));
	
	public string ToDump() {
		var sb = new StringBuilder();
		for (int y = 0; y < this.Height; y++, sb.Append('\n'))
			for (int x = 0; x < this.Width; x++) sb.Append(this[x, y]);
		return sb.ToString();
	}
}

public class History<T> : IEnumerable<T> {
	public static History<T> Empty { get; } = new History<T>();
	
	public readonly T Last;
	public readonly History<T> Prefix;
	private readonly bool IsEmpty;
	
	public History() => (IsEmpty, Prefix, Last) = (true, this, default!);
	public History(T start) => (Prefix, Last) = (new History<T>(), start);
	public History(History<T> prefix, T last) => (Prefix, Last) = (prefix, last);
	
	public History<T> AndThen(T state) => new History<T>(this, state);

	public IEnumerator<T> GetEnumerator() {
		if (IsEmpty) yield break;
		foreach (var element in Prefix) yield return element;
		yield return Last;
	}

	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

	public override string ToString() => string.Join(' ', this);
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

			if (Seen?.Contains(state) == true) continue;
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

public class PriorityQueue<T> {
	private List<(T Element, IComparable Priority)> Items = new List<(T, IComparable)>();
	private readonly Func<T, IComparable> GetPriority;
	
	public int Count => Items.Count;
	
	public PriorityQueue(Func<T, IComparable> priority) => GetPriority = priority;

	public PriorityQueue(Func<T, IComparable> priority, IEnumerable<T> elements) 
		: this(priority) => AddAll(elements);

	public void Add(T element) {
		Items.Add((element, GetPriority(element)));
		
		for (int child = Count - 1, parent; child > 0; child = parent) {
			parent = child - 1 >> 1;
			if (Items[child].Priority.CompareTo(Items[parent].Priority) <= 0) break;
			(Items[child], Items[parent]) = (Items[parent], Items[child]);
		}
	}
	
	public void AddAll(IEnumerable<T> elements) {
		foreach (var element in elements) Add(element);
	}
	
	public T Pop() {
		T result = Items[0].Element;
		
		int child = 1, parent = 0;
		for (; child < Count; parent = child, parent = child, child = 2 * child + 1) {
			if (child < Count - 1 && Items[child].Priority.CompareTo(Items[child + 1].Priority) < 0) ++child;
			Items[parent] = Items[child];
		}
		
		Items[parent] = Items[Count - 1];
		Items.RemoveAt(Count - 1);
		if (parent == Count) return result;
		
		for (child = parent; child > 0; child = parent) {
			parent = child - 1 >> 1;
			if (Items[child].Priority.CompareTo(Items[parent].Priority) <= 0) break;
			(Items[child], Items[parent]) = (Items[parent], Items[child]);
		}
		return result;
	}
}

static string ReadString() {
	string filename = Util.CurrentQueryPath.Replace(".linq", ".txt");
	return File.ReadAllText(filename);
}

static string[] ReadLines() {
	string filename = Util.CurrentQueryPath.Replace(".linq", ".txt");
	return File.ReadAllLines(filename);
}

static int[] ReadInts() {
	string filename = Util.CurrentQueryPath.Replace(".linq", ".txt");
	return File.ReadLines(filename).Select(int.Parse).ToArray();
}

static long[] ReadLongs() {
	string filename = Util.CurrentQueryPath.Replace(".linq", ".txt");
	return File.ReadLines(filename).Select(long.Parse).ToArray();
}

static Board ReadBoard() {
	string filename = Util.CurrentQueryPath.Replace(".linq", ".txt");
	return new Board(File.ReadAllText(filename));
}

