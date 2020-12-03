<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

var board = ReadBoard();

// Fi3*@2%+
{
	int total = 0;
	for (int y = 0, x = 0; y < board.Height; y++) {
		if (board[x, y] == '#') ++total;
		x += 3;
		x %= board.Width;
	}
	total.DumpClip("Part 1");
}
/*
LXd
")9IY%"!OF
  2u*~
  xZ{
    i;*B!~
    @2%,*+
  F*
*/
{
	long result = 1;
	Direction[] ds = {
		new Direction(1, 1),
		new Direction(3, 1),
		new Direction(5, 1),
		new Direction(7, 1),
		new Direction(1, 2),
	};
	
	foreach (var d in ds) {
		int total = 0;
		for (var p = Position.Origin; p.Y < board.Height; ) {
			if (board[p] == '#') ++total;
			p += d;
			p = new Position(p.X % board.Width, p.Y);
		}
		result *= total;
	}
	result.DumpClip("Part 2");
}
