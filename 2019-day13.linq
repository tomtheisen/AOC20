<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

var machine = new IntCodeMachine();
{
	var output = new List<long>();
	machine.OutputAction = output.Add;
	machine.Run();
	
	output.BatchBy(3)
		.Where(o => o[2] == 2)
		.Distinct(new ListComparer<long>())
		.Count()
		.Dump();
}

machine.OutputAction = nm => {};
machine.Reset();
machine.Memory[0] = 2;

var board = new Board();
board = null;
var scoreContainer = new DumpContainer().Dump("Score");
var boardContainer = new DumpContainer().Dump("Board");

long paddlePos = 0, ballPos = 0;
machine.InputOverride = () => {
	if (board is object) Thread.Sleep(50);
	return ballPos.CompareTo(paddlePos);
};
while (true) {
	long? x = machine.RunToNextOutput();
	if (x == null) break;
	long y = machine.RunToNextOutputOrThrow(), 
		tileId = machine.RunToNextOutputOrThrow();
	
	if (x == -1 && y == 0) scoreContainer.Content = tileId;
	else {
		boardContainer.Content = board = board?.With((int)x.Value, (int)y, " █◙─•"[(int)tileId]);
		switch (tileId) {
			case 3:
				paddlePos = x.Value;
				break;
			case 4:
				ballPos = x.Value;
				break;
		}
	}
}
