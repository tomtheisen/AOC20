<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

for (int i = 0; i < 2; i++) {
	var machine = new IntCodeMachine();
	HashSet<Position> white = new HashSet<Position>(), painted = new HashSet<Position>();
	if (i == 1) white.Add(Position.Origin);
	var pos = new Position(0, 0, Direction.N);
	
	var outputs = new BlockingCollection<long>();
	machine.Output = outputs.Add;
	
	var cancelSource = new CancellationTokenSource();
	var run = Task.Run(machine.Run).ContinueWith(t => cancelSource.Cancel());
	try {	        
		while (!run.IsCompleted) {
			machine.TakeInput(white.Contains(pos.Stop()) ? 1 : 0);
			
			painted.Add(pos.Stop());
			switch (outputs.Take(cancelSource.Token)) {
				case 0: white.Remove(pos.Stop()); break;
				case 1: white.Add(pos.Stop());    break;
			}
			
			pos = outputs.Take(cancelSource.Token) switch {
				0 => pos.CCW().Step(), 1 => pos.CW().Step()
			};
		}
	}
	catch (OperationCanceledException) {
		painted.Count.Dump("painted cell total");
		int xmin = white.Min(w => w.X), xmax = white.Max(w => w.X);
		int ymin = white.Min(w => w.Y), ymax = white.Max(w => w.Y);
		
		for (int y = ymin; y <= ymax; y++, Console.WriteLine())
			for (int x = xmin; x <= xmax; x++)
				Console.Write(white.Contains(new Position(x, y)) ? '*' : ' ');
	}
}
