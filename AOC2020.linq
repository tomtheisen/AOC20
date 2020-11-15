<Query Kind="Program">
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Numerics</Namespace>
  <Namespace>System.Drawing</Namespace>
</Query>

#nullable enable

void Main() {
	"foobar".BatchBy(3).Transpose().Dump("batch", 0);
	Permutations("abcd".ToArray()).Select(x => string.Concat(x)).Dump("permutations", 0);
	Choose("abcde", 2).Select(x => string.Concat(x)).Dump("choose", 0);

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
		.SetDumpContainer(new DumpContainer{ DumpDepth=1 }.Dump(0))
		.Search();
		
	foreach (var step in path!) 
		maze = maze
			.With(start = start + step, '.')
			//.Materialize()
			;
	maze.Materialize().Dump("DFS",0);
	
	new Board()
		.With(-5, 0, 'W')
		.With(0, -5, 'N')
		.With(5, 0, 'E')
		.With(0, 5, 'S')
		.Dump("Negative extent test");
	
	const int SomeNumber = 12345;
	var binary = BreadthFirst.Create(0, new[] {0,1})
		.SetGoal(SomeNumber.Equals)
		.SetTransition((val, bit) => 2 * val + bit)
		.Search();
	string.Concat(binary!).Dump("Binary: " + SomeNumber);

	Console.WriteLine("Priority Queue");
	var q = new PriorityQueue<int>(n => n, direction: -1, Enumerable.Repeat(new Random(), 100).Select(rng => rng.Next(100)));
	while (q.Count > 0) Console.WriteLine(q.Pop());
	
	new Board(".").With(0, -1, '#').With(0, 1, '#').Materialize().With(-1, 0, '.').Dump();
	
	Dijkstra.Create(1u, s => BitOperations.PopCount(s), new[] {0u,1u})
		.SetTransition ((state, act) => 2 * state + act)
		.DetectLoops()
		.SetGoal(state => state == 7)
		.SetDumpContainer()
		.Search()
		.Dump("how to make 7");
}

struct Direction : IEquatable<Direction> {
	public int DX { get; }
	public int DY { get; }
	
	public Direction(int dx, int dy) => (DX, DY) = (dx, dy);

	public override int GetHashCode() => (int)BitOperations.RotateRight((uint)DX, 16) ^ DY;
	public bool Equals(Direction other) => other.DX == DX && other.DY == DY;

	public static Direction Zero = new(0, 0);
	public static Direction N = new(0, -1), E = new(1, 0), S = new(0, 1), W = new(-1, 0);
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
	
	public static readonly Position Origin = new(0, 0);

	public override int GetHashCode() => Face.GetHashCode()
		^ (int)BitOperations.RotateRight((uint)X, 8)
		^ (int)BitOperations.RotateRight((uint)X, 24);
	public bool Equals(Position other)
		=> other.X == X && other.Y == Y && other.Face.Equals(Face);

	// make a new position one step in the facing direction
	public Position Step() => new Position(X + Face.DX, Y + Face.DY, Face);
	public Position Up() => this + Direction.N;
	public Position Right() => this + Direction.E;
	public Position Down() => this + Direction.S;
	public Position Left() => this + Direction.W;

	public Position FaceTo(Direction face) => new(X, Y, face);
	public Position CW() => this.FaceTo(Face.CW());
	public Position CCW() => this.FaceTo(Face.CCW());
	public Position Reverse() => this.FaceTo(Face.Reverse());
	public Position Stop() => this.FaceTo(Direction.Zero);

	public static Direction operator -(Position a, Position b) => new(a.X - b.X, a.Y - b.Y);
	public static Position operator +(Position p, Direction d) => new(p.X + d.DX, p.Y + d.DY, p.Face);
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
	public int Left { get; }
	public int Top { get; }
	public int Right => Left + Width;
	public int Bottom => Top + Height;
	
	private char[,]? Cells;
	private Board? OriginalBoard;
	private Position? ChangedPosition;
	private char? NewChar;
	private int Misses = 0;
	
	public Board(char[,] cells) {
		Cells = cells;
		Top = Left = 0;
		Width = cells.GetUpperBound(0) + 1;
		Height = cells.GetUpperBound(1) + 1;
	}
	
	public Board(int width, int height, char fillChar = ' ') {
		Top = Left = 0;
		Width = width;
		Height = height;
		Cells = new char[width, height];
		for (int x = 0; x < width; x++)
			for (int y = 0; y < height; y++) Cells[x, y] = fillChar;
	}
	
	public Board() : this(0, 0) { }
	
	public Board(Board oldBoard, Position changedPosition, char newChar) {
		OriginalBoard = oldBoard;
		Left = Min(oldBoard.Left, changedPosition.X);
		Top = Min(oldBoard.Top, changedPosition.Y);
		Width = Max(oldBoard.Right, changedPosition.X + 1) - Left;
		Height = Max(oldBoard.Bottom, changedPosition.Y + 1) - Top;
		(ChangedPosition, NewChar) = (changedPosition, newChar);
	}
	
	public Board(string cells) : this(Regex.Split(cells, @"\r?\n")) { }
	
	public Board(IEnumerable<string> lines) {
		var linesList = lines.ToList();
		Top = Left = 0;
		Height = linesList.Count;
		Width = linesList.DefaultIfEmpty().Max(l => l?.Length ?? 0);
		Cells = new char[Width, Height];
		
		for (int y = 0; y < Height; y++)
			for (int x = 0; x < Width; x++)
				Cells[x, y] = (x < linesList[y].Length) ? linesList[y][x] : ' ';
	}
	
	public char this[int x, int y] { 
		get {
			if (!Contains(x, y)) return ' ';
			if (Cells is object) return Cells[x - Left, y - Top];
			if (x == ChangedPosition?.X && y == ChangedPosition?.Y) return NewChar!.Value;
			if (++Misses > Width * Height) {
				Materialize();
				return Cells![x - Left, y - Top];
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
		
		for (int i = Left; i < Right; i++)
			for (int j = Top; j < Bottom; j++)
				Cells[i - Left, j - Top] = board.Contains(i, j) 
					? board.Cells![i - board.Left, j - board.Top] 
					: ' ';
		
		mods.Reverse();
		foreach (var mod in mods) {
			var pos = mod.ChangedPosition!.Value;
			Cells[pos.X - Left, pos.Y - Top] = mod.NewChar!.Value;
		}
		
		OriginalBoard = null;
		ChangedPosition = null;
		NewChar = null;
		
		return this;
	}
	
	public bool Contains(Position p) => Contains(p.X, p.Y);
	public bool Contains(int x, int y) => x >= Left && x < Right && y >= Top && y < Bottom;

	public Board With(int x, int y, char c) => new Board(this, new Position(x, y), c);

	public Board With(Position p, char c) => With(p.X, p.Y, c);
    
    public Position Center() => new Position(Left + Right >> 1, Top + Bottom >> 1);

	public IEnumerator<Position> GetEnumerator() {
		for (int y = Top; y < Bottom; y++)
			for (int x = Left; x < Right; x++)
				yield return new Position(x, y);
	}

	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
	
	public IEnumerable<Position> FindAll(string needles) => this.Where(t => needles.Contains(this[t]));
	public IEnumerable<Position> FindAll(char needle) => this.Where(t => needle == this[t]);
	public Position Find(string needles) => this.First(t => needles.Contains(this[t]));
	public Position Find(char needle) => this.First(t => needle == this[t]);
	
	public string ToDump() {
		var sb = new StringBuilder();
		for (int y = Top; y < Bottom; y++, sb.Append('\n'))
			for (int x = Left; x < Right; x++) sb.Append(this[x, y]);
		return sb.ToString();
	}

    public override string ToString() => $"Board ({Width} x {Height})";
}

public class History<T> : IEnumerable<T> {
	public static History<T> Empty { get; } = new();
	
	public readonly T Last;
	public readonly History<T> Prefix;
	private readonly bool IsEmpty;
	public readonly int Length;
	
	public History() => (IsEmpty, Prefix, Last) = (true, this, default!);
	public History(T start) => (Prefix, Last) = (new History<T>(), start);
	public History(History<T> prefix, T last) => (Prefix, Last, Length) = (prefix, last, prefix.Length + 1);
	
	public History<T> AndThen(T state) => new(this, state);

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
	public List<Func<TState, TAct, bool>> ActFilters { get; } = new();
	public List<Predicate<TState>> StateFilters { get; } = new();
	public Func<TState, TAct, TState> Transition { get; set; }
	public IMembership<TState>? Seen { get; set; }
	public TState Start { get; set; }
	public DumpContainer? DumpContainer { get; set; }
	public int StatesEvaluated { get; protected set; }
	public Stopwatch Timer { get; } = new();

	public SearchBase(TState start, IList<TAct> acts) {
		this.Start = start;
		this.Acts = acts;
		this.Goal = _ => true;
		this.Transition = (_, _) => throw new("Transition not specified");
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
		this.Seen = new CollectionMembershipAdapter<TState>(new HashSet<TState>());
		return this;
	}
	
	public SearchBase<TState, TAct> DetectLoops<TDupe>(Func<TState, TDupe> dupeMap) {
		this.Seen = new MappedMembership<TState, TDupe>(dupeMap);
		return this;
	}

	public SearchBase<TState, TAct> SetDumpContainer(DumpContainer? dumpContainer = null) {
		this.DumpContainer = dumpContainer ?? new DumpContainer().Dump();
		return this;
	}
	
	public History<TAct>? Search(out TState finalState) {
		this.Timer.Start();
		return this.SearchCore(out finalState);
	}
	public History<TAct>? Search() => this.Search(out _);
	public abstract History<TAct>? SearchCore(out TState finalState);
}

public static class DepthFirst {
	public static DepthFirst<TState, TAct> Create<TState, TAct>(TState start, IList<TAct> acts) => new(start, acts);
}

public class DepthFirst<TState, TAct>: SearchBase<TState, TAct> {
	private TimeSpan LastReport = TimeSpan.Zero;
	private int LongestHistory = 0;

	public DepthFirst(TState start, IList<TAct> acts) : base(start, acts) {}
	
	public override History<TAct>? SearchCore(out TState finalState) => SearchCore(Start, new History<TAct>(), out finalState);
	
	private List<double> StatesRateHistory = new();
	private List<int> LongestHistoryLengthHistory = new();
	private List<double> ElapsedHistory = new();
	private void DumpState(bool forceShow, TState state, History<TAct> history) {
		if (DumpContainer is object) {
			if (forceShow || this.Timer.Elapsed - LastReport > TimeSpan.FromMilliseconds(500)) {
				StatesRateHistory.Add(StatesEvaluated / this.Timer.Elapsed.TotalSeconds);
				LongestHistoryLengthHistory.Add(LongestHistory);
				ElapsedHistory.Add(this.Timer.Elapsed.TotalSeconds);
				DumpContainer.Content = new { 
					State = state, 
					this.StatesEvaluated,
					HistoryLength = history.Length, 
					LastReport = LastReport = this.Timer.Elapsed,
					Status = ElapsedHistory.Chart()
						.AddYSeries(LongestHistoryLengthHistory, name: "Longest Branch Explored", seriesType: Util.SeriesType.Column)
						.AddYSeries(StatesRateHistory, name: "States / Sec", seriesType: Util.SeriesType.Line, useSecondaryYAxis: true)
						.ToXRoundedBitmap(600, 400),
				};
			}
		}
	}
	
	public History<TAct>? SearchCore(TState state, History<TAct> history, out TState finalState) {
		if (Seen?.Add(state) == false) {
            finalState = default;
            return null;
        }

		bool goal = Goal(state);
		StatesEvaluated++;
		LongestHistory = Max(LongestHistory, history.Length);
		DumpState(goal, state, history);
		if (goal) {
            finalState = state;
            return history;
        }
		
		foreach (var act in Acts) {
			if (ActFilters.Any(af => !af(state, act))) continue;
			var newState = Transition(state, act);
			if (newState is null) continue;
			if (StateFilters.Any(sf => !sf(newState))) continue;
			var newHistory = history.AndThen(act);
			if (SearchCore(newState, newHistory, out finalState) is History<TAct> result) return result;
		}
        finalState = default;
		return null;
	}
}

public static class BreadthFirst {
	public static BreadthFirst<TState, TAct> Create<TState, TAct>(TState start, IList<TAct> acts) => new(start, acts);
}

public class BreadthFirst<TState, TAct>: SearchBase<TState, TAct> {
	private TimeSpan LastReport = TimeSpan.Zero;

	public BreadthFirst(TState start, IList<TAct> acts) : base(start, acts) {}
	
	private List<double> StatesRateHistory = new();
	private List<int> HistoryLengthHistory = new();
	private List<int> QueueDepthHistory = new();
	private List<double> ElapsedHistory = new();
	private void DumpState(bool forceShow, TState state, History<TAct> history, int queueDepth) {
		if (DumpContainer is object) {
			if (forceShow || this.Timer.Elapsed - LastReport > TimeSpan.FromMilliseconds(500)) {
				StatesRateHistory.Add(StatesEvaluated / this.Timer.Elapsed.TotalSeconds);
				HistoryLengthHistory.Add(history.Length);
				QueueDepthHistory.Add(queueDepth);
				ElapsedHistory.Add(this.Timer.Elapsed.TotalSeconds);
				
				DumpContainer.Content = new { 
					State = state, 
					StatesEvaluated,
					HistoryLength = history.Length,
					QueueDepth = queueDepth,
					LastReport = LastReport = this.Timer.Elapsed,
					Status = ElapsedHistory.Chart()
						.AddYSeries(QueueDepthHistory, name: "Queue Depth", seriesType: Util.SeriesType.Line)
						.AddYSeries(HistoryLengthHistory, name: "History Length", seriesType: Util.SeriesType.Line)
						.AddYSeries(StatesRateHistory, name: "States / Sec", seriesType: Util.SeriesType.Line, useSecondaryYAxis: true)
						.ToXRoundedBitmap(600, 400)
				};
			}
		}
	}
	
	public override History<TAct>? SearchCore(out TState finalState) {
		Queue<(TState, History<TAct>)> queue = new();
		
		for (queue.Enqueue((Start, History<TAct>.Empty)); queue.TryDequeue(out var element); ) {
			var (state, history) = element;
			if (Seen?.Add(state) == false) continue;

			bool goal = Goal(state);
			this.StatesEvaluated++;
			DumpState(goal, state, history, queue.Count);
			if (goal) {
                finalState = state;
                return history;
            }
			
			foreach (var act in Acts) {
				if (ActFilters.Any(af => !af(state, act))) continue;
				var newState = Transition(state, act);
				if (newState is null) continue;
				if (StateFilters.Any(sf => !sf(newState))) continue;
				var newHistory = history.AndThen(act);
				queue.Enqueue((newState, newHistory));
			}
		}
		finalState = default;
		return null;
	}
}

public static class Dijkstra {
	public static Dijkstra<TState, TAct> Create<TState, TAct>(TState start, Func<TState, IComparable> getCost, IList<TAct> acts) => new(start, getCost, acts);
}

public class Dijkstra<TState, TAct> : SearchBase<TState, TAct> {
	private TimeSpan LastReport = TimeSpan.Zero;
	private Func<TState, IComparable> GetCost;

	public Dijkstra(TState start, Func<TState, IComparable> getCost, IList<TAct> acts) 
		: base(start, acts) => GetCost = getCost;

	public override History<TAct>? SearchCore(out TState finalState) {
		PriorityQueue<(TState State, History<TAct> History)> agenda = new(t => GetCost(t.State), direction: -1, (Start, History<TAct>.Empty));

		while (agenda.Count > 0) {
			var (state, history) = agenda.Pop();
			if (Seen?.Add(state) == false) continue;
			
			bool goal = Goal(state);
			this.StatesEvaluated++;
			DumpState(goal, state, agenda.Count);
			if (goal) {
                finalState = state;
                return history;
            }
			
			foreach (var act in Acts) {
				if (ActFilters.Any(af => !af(state, act))) continue;
				var newState = Transition(state, act);
				if (newState is null) continue;
				if (StateFilters.Any(sf => !sf(newState))) continue;
				agenda.Add((newState, history.AndThen(act)));
			}
		}
        finalState = default;
		return null;
	}

	private List<double> StatesRateHistory = new();
	private List<int> AgendaSizeHistory = new();
	private List<double> ElapsedHistory = new();
	private void DumpState(bool forceShow, TState state, int agendaSize) {
		if (DumpContainer is object) {
			if (forceShow || this.Timer.Elapsed - LastReport > TimeSpan.FromMilliseconds(500)) {
				StatesRateHistory.Add(StatesEvaluated / this.Timer.Elapsed.TotalSeconds);
				AgendaSizeHistory.Add(agendaSize);
				ElapsedHistory.Add(this.Timer.Elapsed.TotalSeconds);
				
				DumpContainer.Content = new {
					State = state,
					StatesEvaluated,
					AgendaSize = agendaSize,
					LastReport = LastReport = this.Timer.Elapsed,
					Status = ElapsedHistory.Chart()
						.AddYSeries(AgendaSizeHistory, name: "Agenda Size", seriesType: Util.SeriesType.Line)
						.AddYSeries(StatesRateHistory, name: "States / Sec", seriesType: Util.SeriesType.Line, useSecondaryYAxis: true)
						.ToXRoundedBitmap(600, 400),
				};
			}
		}
	}
}

public class PriorityQueue<T> {
	private List<(T Element, IComparable Priority)> Items = new();
	private readonly Func<T, IComparable> GetPriority;
	
	public int Direction { get; }
	
	public int Count => Items.Count;
	
	public PriorityQueue(Func<T, IComparable> priority, int direction = 1) {
		GetPriority = priority;
		Direction = direction;
	}

	public PriorityQueue(Func<T, IComparable> priority, int direction, IEnumerable<T> elements) 
		: this(priority, direction) => AddAll(elements);

	public PriorityQueue(Func<T, IComparable> priority, int direction, params T[] elements) 
		: this(priority, direction) => AddAll(elements);

	public void Add(T element) {
		Items.Add((element, GetPriority(element)));
		
		for (int child = Count - 1, parent; child > 0; child = parent) {
			parent = child - 1 >> 1;
			if (Items[child].Priority.CompareTo(Items[parent].Priority) * Direction <= 0) break;
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
			if (child < Count - 1 && Items[child].Priority.CompareTo(Items[child + 1].Priority) * Direction < 0) ++child;
			Items[parent] = Items[child];
		}
		
		Items[parent] = Items[Count - 1];
		Items.RemoveAt(Count - 1);
		if (parent == Count) return result;
		
		for (child = parent; child > 0; child = parent) {
			parent = child - 1 >> 1;
			if (Items[child].Priority.CompareTo(Items[parent].Priority) * Direction <= 0) break;
			(Items[child], Items[parent]) = (Items[parent], Items[child]);
		}
		return result;
	}
	
	public bool TryPop(out T result) {
		if (this.Count == 0) {
			result = default;
			return false;
		}
		result = Pop();
		return true;
	}
	
	public T Peek() => Items[0].Element;
}

static string ReadString() {
	string filename = Util.CurrentQueryPath.Replace(".linq", ".txt");
	return File.ReadAllText(filename);
}

static string[] ReadLines() {
	string filename = Util.CurrentQueryPath.Replace(".linq", ".txt");
	return File.ReadAllLines(filename);
}

public static int[] ReadDigits() => ReadLines()[0].Select(e => e - '0').ToArray();

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

static int Hash(params object[] objects) {
	int hash = 0;
	foreach (var obj in objects) hash = hash * 37 ^ obj.GetHashCode();
	return hash;
}

int GCD(int a, int b) => a == 0 ? Abs(b) : GCD(Abs(b) % a, Abs(a));
long GCD(long a, long b) => a == 0 ? Abs(b) : GCD(Abs(b) % a, Abs(a));
BigInteger GCD(BigInteger a, BigInteger b) => a == 0 ? BigInteger.Abs(b) : GCD(BigInteger.Abs(b) % a, BigInteger.Abs(a));

public class ListComparer<T> : IEqualityComparer<IReadOnlyList<T>> {
	public bool Equals(IReadOnlyList<T>? x, IReadOnlyList<T>? y)
		=> x?.SequenceEqual(y) ?? false;

	public int GetHashCode(IReadOnlyList<T> obj)
		=> obj.Aggregate (0, (x, y) => unchecked(x * 37 + (y?.GetHashCode() ?? 0)));
}

static IEnumerable<T[]> Permutations<T>(IEnumerable<T> values) => Permutations(values.ToArray());
static IEnumerable<T[]> Permutations<T>(params T[] values) {
	Array.Reverse(values);
	var inputs = new List<T>();
	int limit = Enumerable.Range(1, values.Length).Aggregate ((x, y) => x * y);
	for (int i = 0; i < limit; i++) {
		inputs.AddRange(values);
		T[] result = new T[values.Length];
		for (int j = values.Length, p = i; j > 0; p /= j--) {
			result[j - 1] = inputs[p % j];
			inputs.RemoveAt(p % j);
		}
		yield return result;
	}
}

static IEnumerable<T[]> Choose<T>(IEnumerable<T> values, int choose) {
	if (choose == 0) {
		yield return Array.Empty<T>();
		yield break;
	}
	var arr = values.ToArray();
	if (arr.Count() == choose) {
		yield return arr;
		yield break;
	}
	if (arr.Count() < choose) yield break;
	
	for (int i = 0; i < arr.Length; i++) {
		foreach (var choice in Choose(values.Skip(i + 1), choose - 1)) {
			yield return choice.Prepend(arr[i]).ToArray();
		}
	}
	foreach (var choice in Choose(values.Skip(1), choose)) yield return choice;
}

public interface IMembership<T> {
	bool Add(T item);
	bool Contains(T item);
	void Clear();
}

public class CollectionMembershipAdapter<T> : IMembership<T> {
	private ISet<T> Collection;
	
	public CollectionMembershipAdapter(ISet<T> collection) {
		Collection = collection;
	}

	public bool Add(T item) => Collection.Add(item);
	public bool Contains(T item) => Collection.Contains(item);
	public void Clear() => Collection.Clear();
}

public class MappedMembership<T, TMap> : IMembership<T> {
	private HashSet<TMap> MapSet = new HashSet<TMap>();
	private Func<T, TMap> MapFunc;
	
	public MappedMembership(Func<T, TMap> map) {
		MapFunc = map;
	}

	public bool Add(T item) => MapSet.Add(MapFunc(item));
	public void Clear() => MapSet.Clear();
	public bool Contains(T item) => MapSet.Contains(MapFunc(item));
}

public static class Extensions {
	public static Bitmap ToXRoundedBitmap(this LINQPad.LINQPadChart @this, int width, int height) {
		var chart = @this.ToWindowsChart();
				
		chart.ChartAreas[0].AxisX.RoundAxisValues();
		chart.Size = new Size(width, height);
		
		Bitmap bitmap = new(width, height);
		chart.DrawToBitmap(bitmap, new Rectangle(0, 0, width, height));
		return bitmap;
	}

	public static IEnumerable<List<T>> BatchBy<T>(this IEnumerable<T> @this, int size) {
		var current = new List<T>();
		foreach (var element in @this) {
			current.Add(element);
			if (current.Count == size) {
				yield return current;
				current = new List<T>();
			}
		}
		if (current.Count > 0) yield return current;
	}
	
	public static T MinBy<T>(this IEnumerable<T> @this, Func<T, IComparable> keyFunc) {
		bool first = true;
		T bestT = default;
		IComparable? bestKey = default;
		foreach (var element in @this) {
			IComparable key = keyFunc(element);
			if (first || key.CompareTo(bestKey) < 0) {
				(bestT, bestKey, first) = (element, key, false);
			}
		}
		return bestT ?? throw new ArgumentOutOfRangeException("Empty sequence");
	}
	
	public static T MaxBy<T>(this IEnumerable<T> @this, Func<T, IComparable> keyFunc) {
		bool first = true;
		T bestT = default;
		IComparable? bestKey = default;
		foreach (var element in @this) {
			IComparable key = keyFunc(element);
			if (first || key.CompareTo(bestKey) > 0) {
				(bestT, bestKey, first) = (element, key, false);
			}
		}
		return bestT ?? throw new ArgumentOutOfRangeException("Empty sequence");
	}
	
	public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> @this, Func<T, TKey> keyFunc) {
		var seen = new HashSet<TKey>();
		foreach (var element in @this) {
			if (seen.Add(keyFunc(element))) yield return element;
		}
	}
	
	public static int Count<T>(this IEnumerable<T> @this, T search) 
		where T: IEquatable<T> => @this.Count(search.Equals);
		
	public static IEnumerable<List<T>> Transpose<T>(this IEnumerable<IEnumerable<T>> @this) {
		var enumerators = @this.Select(t => t.GetEnumerator()).ToList();
		
		while (true) {
			enumerators.RemoveAll(e => !e.MoveNext());
			if (enumerators.Count == 0) yield break;
			yield return enumerators.Select(e => e.Current).ToList();
		}
	}
	
	public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> @this) 
		where TKey: notnull => new(@this);
		
	public static Lazy<T> ToLazy<T>(this T @this) => new(@this);
	
	public static T? ToNullable<T>(this T @this) where T : struct => @this;
}
