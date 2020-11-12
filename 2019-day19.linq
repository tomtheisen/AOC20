<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

var machine = new IntCodeMachine{ Output = _ => {} };

{
	long total = 0;
	var board = new Board();
	for (int x = 0; x < 50; x++) {
		for (int y = 0; y < 50; y++) {
			machine.Reset();
			machine.TakeInput(x, y);
			long output = machine.RunToNextOutputOrThrow();
			total += output;
			board = board.With(x, y, output > 0 ? '#' : ' ');
		}
	}
	total.Dump("Part 1");
	new{board}.Dump();
}

for (long x = 0, y = 100; ; y++) {
	for (;; x++) {
		machine.Reset();
		machine.TakeInput(x, y);
		if (machine.RunToNextOutputOrThrow() == 1) break;
	}
	
	machine.Reset();
	machine.TakeInput(x + 99, y - 99);
	if (machine.RunToNextOutput() == 1) {
		(x * 10_000 + y - 99).Dump("Part 2");
		break;
	}
}
