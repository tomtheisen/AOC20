<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

var cont = new DumpContainer().Dump("state");

{
	var board = ReadBoard();
	while (true) {
		var next = board;
		foreach (var pos in board) {
			if (next[pos] == '.') continue;
			int neighbors = pos.Neighbors().Count(n => board[n] == '#');
			if (neighbors == 0 && next[pos] != '#') next = next.With(pos, '#');
			else if (neighbors >= 4 && next[pos] != 'L') next = next.With(pos, 'L');
		}
		if (next == board) break;
		cont.Content = board = next;
	}
	board.Count(p => board[p] == '#').Dump("Part 1");
}

{
	var board = ReadBoard();
	while (true) {
		var next = board;
		foreach (var pos in board) {
			if (next[pos] == '.') continue;
			int neighbors = 0;
			foreach (var dir in Direction.InterCardinal) {
				var npos = pos;
				do npos += dir; while (board[npos] == '.');
				if (board[npos] == '#') neighbors += 1;
			}
			
			if (neighbors == 0 && next[pos] != '#') next = next.With(pos, '#');
			else if (neighbors >= 5 && next[pos] != 'L') next = next.With(pos, 'L');
		}
		if (next == board) break;
		cont.Content = board = next;
	}
	board.Count(p => board[p] == '#').Dump("Part 2");
}
