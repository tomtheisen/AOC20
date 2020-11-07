<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

string b = "";
var machine = new IntCodeMachine { Output = n => b += (char)n };

machine.Run();

var board = new Board(b);

var crosses = board
	.Where(bo => Direction.Cardinal.All(dir => board[bo.Move(dir)] == '#'));
	
int total = 0;
foreach (var cross in crosses) {
	total += cross.X * cross.Y;
}

board.Dump();
total.Dump("Part 1");

var pos = board.Find("^").FaceTo(Direction.N).Dump("Starting position");

var instructions = "";
while (true) {
	if (board[pos.CCW().Step()] == '#') {
		instructions += "L,";
		pos = pos.CCW();
	}
	else if (board[pos.CW().Step()] == '#') {
		instructions += "R,";
		pos = pos.CW();
	}
	else break;
	
	int steps = 0;
	while (board[pos.Step()] == '#') {
		pos = pos.Step();
		++steps;
	}
	instructions += steps + ",";
}
instructions = instructions[..^1].Dump("Complete instructions");

// L,10,R,10,L,10,L,10,R,10,R,12,L,12,L,10,R,10,L,10,L,10,R,10,R,12,L,12,R,12,L,12,R,6,R,12,L,12,R,6,R,10,R,12,L,12,L,10,R,10,L,10,L,10,R,10,R,12,L,12,R,12,L,12,R,6
/*
A L,10,R,10,L,10,L,10,
B R,10,R,12,L,12,
A L,10,R,10,L,10,L,10,
B R,10,R,12,L,12,
C R,12,L,12,R,6,
C R,12,L,12,R,6,
B R,10,R,12,L,12,
A L,10,R,10,L,10,L,10,
B R,10,R,12,L,12,
C R,12,L,12,R,6
*/

machine.Reset();
machine.Memory[0] = 2;
machine.Output = e => {
	if (e < 127) Console.Write((char)e);
	else e.Dump("Part 2");
};

machine.TakeInput("A,B,A,B,C,C,B,A,B,C\n");
machine.TakeInput("L,10,R,10,L,10,L,10\n");
machine.TakeInput("R,10,R,12,L,12\n");
machine.TakeInput("R,12,L,12,R,6\n");
machine.TakeInput("n\n");

machine.Run();
