<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

var board = new Board();
var machine = new IntCodeMachine() { Output = _ => {} };
var seenPos = new HashSet<Position> { Position.Origin };

var boardContainer = new DumpContainer(board).Dump("Board");

BreadthFirst
	.Create((machine, position: Position.Origin, status: -1L, steps: 0), new[] {
		(Direction.N, 1),
		(Direction.S, 2),
		(Direction.W, 3),
		(Direction.E, 4),
	})
	.SetTransition((state, act) => {
		var (machine, pos, lastStatus, steps) = state;
		var (dir, dirCode) = act;
		
		machine = machine.Clone();
		machine.TakeInput(dirCode);
		
		var status = machine.RunToNextOutputOrThrow();
		if (status == 2) (steps + 1).Dump("Found Module");
		//boardContainer.Content = 
		board = board.With(pos.Move(dir), "#.S"[(int)status]);
		//Thread.Sleep(1);
		
		return (machine, status > 0 ? pos.Move(dir) : pos, status, steps + 1);
	})
	.AddStateFilter(state => seenPos.Add(state.position))
	.SetGoal(state => false)
	.Search();

BreadthFirst.Create(board.Find("S") , Direction.Cardinal)
	.SetTransition ((position, direction) => {
		//boardContainer.Content = 
		board = board.With(position, 'O');
		//Thread.Sleep(1);
		return position.Move(direction);
	})
	.AddStateFilter(position => board[position] == '.')
	.SetGoal(position => board.Count(p => board[p] == '.') == 1)
	.Search()
	.Count()
	.Dump("Oxygenated");