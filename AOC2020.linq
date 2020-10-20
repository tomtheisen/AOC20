<Query Kind="Program">
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#nullable enable

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
	var pos = maze.Find("@").Dump();
	maze[pos].Dump();
	
	maze.Find("!").Dump();
	
	var path = DFS(pos, 
		s => maze[s] == '!', 
		s => Direction.Cardinal
			.Select(d => (s.Move(d), d))
			.Where(d => maze[d.Item1] != 'X'), 
		detectLoops: true).Dump();
		
	foreach (var step in path!) {
		pos = pos.Move(step);
		maze = maze.With(pos, '.');
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
	private readonly bool Empty;
	
	public History(History<T> prefix, T last) {
		Prefix = prefix;
		Last = last;
	}
	
	public History() {
		Empty = true;
		Prefix = this;
	}
	
	public History(T start) {
		Prefix = new History<T>();
		Last = start;
	}
	
	public History<T> AndThen(T state) => new History<T>(this, state);

	public IEnumerator<T> GetEnumerator() {
		if (Empty) yield break;
		foreach (var element in Prefix) yield return element;
		yield return Last;
	}

	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}

public delegate IEnumerable<(TState, TTransition)> StateTransition<TState, TTransition>(TState state);

public IEnumerable<THistory>? DFS<TState, THistory>(TState start, Predicate<TState> goal, StateTransition<TState, THistory> next, bool detectLoops = false)
	=> DFS(start, goal, next, detectLoops ? new HashSet<TState>() : null, new History<THistory>());
	
public IEnumerable<THistory>? DFS<TState, THistory>(TState start, Predicate<TState> goal, StateTransition<TState, THistory> next, HashSet<TState>? seen, History<THistory> history) {
	if (goal(start)) 
		return history;
	
	if (seen?.Contains(start) == true) return null;
	seen?.Add(start);
	
	foreach (var (state, transition) in next(start)) {
		if (DFS(state, goal, next, seen, history.AndThen(transition)) is IEnumerable<THistory> result) return result;
	}
	return null;
}

public class DepthFirst {
	public int MyProperty { get; set; }
}

public IEnumerable<THistory>? BFS<TState, THistory>(TState start, Predicate<TState> goal, StateTransition<TState, THistory> next, bool detectLoops = false) {
	var seen = new HashSet<TState>();
	var queue = new Queue<(TState state, History<THistory> history)>();
	
	for(queue.Enqueue((start, new History<THistory>())); queue.Count > 0;) {
		var (state, history) = queue.Dequeue();
		
		if (goal(state)) return history;
		
		if (detectLoops) {
			if (seen.Contains(state)) continue;
			seen.Add(state);
		}
		
		foreach (var (successor, transition) in next(state)) {
			queue.Enqueue((successor, history.AndThen(transition)));
		}
	}
	
	return null;
}
