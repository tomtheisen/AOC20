<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

#load ".\IntCode" 
#load ".\AOC2020"

var board = ReadBoard();

var boardContainer = new DumpContainer(board);
board.ToLazy().Dump("board");

var pos = board.Find("@").Dump("@ Position");

boardContainer.Content = board = board.With(pos, '.');

int goalKeys = board.Aggregate(0, 
	(acc, pos) => board[pos] >= 'a' && board[pos] <= 'z' ? (1 << board[pos] - 'a' | acc) : acc)
	.Dump("Goal Keys Mask");

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
	
	