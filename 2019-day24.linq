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
	HashSet<int> seen = new();
	var board = ReadBoard().Dump("Input");
	
	while (true) {
		var original = board;
		foreach (var pos in board) {
			int count = pos.Adjacent().Count(p => original[p] == '#');
			char cell = count == 1 || count == 2 && original[pos] != '#' ? '#' : '.';
			board = board.With(pos, cell);
		}
		
		int rating = board.Select((pos, i) => board[pos] == '#' ? 1 << i : 0).Sum();
		if (!seen.Add(rating)) {
			rating.Dump("Part 1");
			break;
		}
	}
	
	PlutonianGrid grid = new(){ Cells = string.Concat(ReadLines()).ToCharArray() };
	for (int i = 0; i < 200; i++) grid = grid.GetTopSuccessor();
	
	grid.CountBugs().Dump("Part 2");
}

class PlutonianGrid {
	private static char[] DefaultCells = new char[25];

	public char[] Cells { get; init; }
	public PlutonianGrid? InnerGrid { get; init; }
		
	public PlutonianGrid GetSuccessor(char above, char right, char below, char left) {
		var newCells = new char[25];
		newCells[12] = '?';
		
		for (int i = 0; i < 25; i++) {
			if (i == 12) continue;
			
			int neighbors = 0;
			if (i <= 4) neighbors += above == '#' ? 1 : 0;
			else if (i >= 20) neighbors += below == '#' ? 1 : 0;
			
			if (i % 5 == 0) neighbors += left == '#' ? 1 : 0;
			else if (i % 5 == 4) neighbors += right == '#' ? 1 : 0;
			
			if (i == 7) neighbors += InnerGrid?.Cells[..5].Count(b => b == '#') ?? 0;
			else if (i == 11) neighbors += InnerGrid?.Cells.BatchBy(5).Count(c => c.First() == '#') ?? 0;
			else if (i == 13) neighbors += InnerGrid?.Cells.BatchBy(5).Count(c => c.Last() == '#') ?? 0;
			else if (i == 17) neighbors += InnerGrid?.Cells[^5..].Count(b => b == '#') ?? 0;
			
			neighbors += i >= 5 && Cells[i - 5] == '#' ? 1 : 0;
			neighbors += i % 5 > 0 && Cells[i - 1] == '#' ? 1 : 0;
			neighbors += i % 5 < 4 && Cells[i + 1] == '#' ? 1 : 0;
			neighbors += i < 20 && Cells[i + 5] == '#' ? 1 : 0;
			
			char newtile = '.';
			if (neighbors == 1 || neighbors == 2 && Cells[i] != '#') newtile = '#';
			newCells[i] = newtile;
		}
		
		PlutonianGrid? newInnerGrid = null;
		if (Cells[7] == '#' || Cells[11] == '#' || Cells[13] == '#' || Cells[17] == '#' || InnerGrid is not null) {
			newInnerGrid = InnerGrid ?? new(){ Cells = DefaultCells };
			newInnerGrid = newInnerGrid.GetSuccessor(Cells[7], Cells[13], Cells[17], Cells[11]);
		}
		
		return new() { Cells = newCells, InnerGrid = newInnerGrid };
	}
	
	public PlutonianGrid GetTopSuccessor() {
		var result = new PlutonianGrid { Cells = DefaultCells, InnerGrid = this }
			.GetSuccessor('.', '.', '.', '.');
		while (result.Cells.All(c => c != '#')) result = result.InnerGrid;
		return result;
	}
	
	public object ToDump() {
		StringBuilder sb = new();
		for (int row = 0; row < 5; row++) {
			string lineout = "";
			for (var curr = this; curr is not null; curr = curr.InnerGrid) {
				lineout += string.Concat(curr.Cells.Skip(row * 5).Take(5)) + "  ";
			}
			sb.AppendLine(lineout.PadCenter(80));
		}
		return sb.ToString();
	}
	
	public int CountBugs() => Cells.Count(c => c == '#') + (InnerGrid?.CountBugs() ?? 0);
}
